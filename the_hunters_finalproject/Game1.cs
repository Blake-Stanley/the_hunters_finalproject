using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace the_hunters_finalproject;

public enum GameState { MainMenu, Playing, Paused, GameOver }

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int ScreenWidth   = 1600;
    private const int ScreenHeight  = 1000;
    private const int MaxRabbits    = 60;
    private const int MaxFoxes      = 20;

    // state machine
    private GameState _state = GameState.MainMenu;

    // config and stats
    private SimConfig _config;
    private float _sessionTime      = 0f;
    private int   _sessionKills     = 0;
    private int   _totalKills       = 0;   // accumulated across resets in this run
    private float _bestSessionTime  = 0f;
    private int   _bestSessionKills = 0;

    // entities
    private List<Rabbit>   _rabbits   = new();
    private List<Fox>      _foxes     = new();
    private List<FoodItem> _foodItems = new();

    // grass zones
    private List<Vector2> _grassZones = new();
    private Texture2D     _grassZoneTex;
    private const int     GrassZoneRadius = 70;

    // food respawn
    private Texture2D _foodTex;
    private float     _foodRespawnTimer   = 0f;
    private const float FoodRespawnInterval = 2.5f;
    private const int   InitialFoodPerZone  = 8;
    private const int   ScatteredFoodCount  = 5;

    // game over
    private Texture2D _overlayTex;
    private bool   _isNewRecord     = false;
    private string _extinctSpecies  = "";

    // simulation settings
    private bool  _fleeModeOn = true;
    private float _speedMult  = 1f;
    private bool  _soundMuted = false;

    // keyboard edge detection
    private KeyboardState _prevKeys;

    // textures (built from colored pixels — no external sprites needed)
    private Texture2D _foxBodyTex, _foxTailTex, _foxLegTex;
    private Texture2D _rabbitBodyTex, _rabbitEarTex, _rabbitLegTex;

    private Hud        _hud;
    private MainMenu   _menu;
    private SpriteFont _font;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Exiting += (_, _) => PersistStats();
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth  = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();
        _prevKeys = Keyboard.GetState();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");
        _hud  = new Hud(_font);

        _config = ConfigManager.Load();
        _speedMult        = _config.DefaultSpeed;
        _totalKills       = _config.LifetimeKills;
        _bestSessionKills = _config.BestSessionKills;
        _bestSessionTime  = _config.BestSurvivalSeconds;

        // seed Sydney's menu controls with saved config values
        _menu = new MainMenu(_font, GraphicsDevice);
        _menu.FoxCount             = _config.InitialFoxCount;
        _menu.RabbitCount          = _config.InitialRabbitCount;
        _menu.Speed                = _config.DefaultSpeed;
        _menu.FoxHungerLimit       = _config.FoxHungerLimit;
        _menu.FoxReproInterval     = _config.FoxReproInterval;
        _menu.FoxLifespan          = _config.FoxLifespan;
        _menu.RabbitReproInterval  = _config.RabbitReproInterval;
        _menu.RabbitLifespan       = _config.RabbitLifespan;
        _menu.RabbitHungerLimit    = _config.RabbitHungerLimit;
        _menu.GrassZoneCount       = _config.GrassZoneCount;

        // fox: orange body, lighter tail, darker legs
        _foxBodyTex = MakeTex(24, 16, new Color(210, 110, 40));
        _foxTailTex = MakeTex(12, 10, new Color(230, 160, 70));
        _foxLegTex  = MakeTex(5,  10, new Color(180,  80, 25));

        // rabbit: light gray body, pink-tinted ears, gray legs
        _rabbitBodyTex = MakeTex(18, 14, new Color(215, 215, 215));
        _rabbitEarTex  = MakeTex(4,  12, new Color(240, 200, 200));
        _rabbitLegTex  = MakeTex(5,   8, new Color(195, 195, 195));

        _grassZoneTex = MakeCircleTex(GrassZoneRadius, new Color(40, 110, 40, 160));
        _foodTex      = MakeWheatTex();
        _overlayTex   = MakeTex(1, 1, new Color(0, 0, 0, 210));
    }

    private Texture2D MakeTex(int w, int h, Color color)
    {
        var tex  = new Texture2D(GraphicsDevice, w, h);
        var data = new Color[w * h];
        for (int i = 0; i < data.Length; i++) data[i] = color;
        tex.SetData(data);
        return tex;
    }

    // wheat/grass food sprite: oval grain head with a green stem
    private Texture2D MakeWheatTex()
    {
        const int w = 10, h = 18;
        var tex  = new Texture2D(GraphicsDevice, w, h);
        var data = new Color[w * h];
        for (int i = 0; i < data.Length; i++) data[i] = Color.Transparent;

        Color grain = new Color(220, 195, 55);
        Color stem  = new Color(80,  150, 35);
        int cx = w / 2;

        // grain head: ellipse — cx=5, cy=5, rx=3, ry=5
        for (int y = 0; y <= 10; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - 5f;
            if (dx * dx / 9f + dy * dy / 25f <= 1f)
                data[y * w + x] = grain;
        }

        // stem: 2 px wide from just below the head to the bottom
        for (int y = 9; y < h; y++)
        {
            if (cx - 1 >= 0) data[y * w + cx - 1] = stem;
            data[y * w + cx] = stem;
        }

        tex.SetData(data);
        return tex;
    }

    private Texture2D MakeCircleTex(int radius, Color color)
    {
        int size = radius * 2;
        var tex  = new Texture2D(GraphicsDevice, size, size);
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - radius, dy = y - radius;
            data[y * size + x] = (dx * dx + dy * dy <= (float)radius * radius)
                ? color : Color.Transparent;
        }
        tex.SetData(data);
        return tex;
    }

    protected override void Update(GameTime gameTime)
    {
        var keys = Keyboard.GetState();

        if (Pressed(keys, Keys.Escape))
        {
            if (_state == GameState.MainMenu)
                SaveAndExit();
            else
            {
                CommitSessionBests();
                _state = GameState.MainMenu;
            }
            _prevKeys = keys;
            return;
        }

        switch (_state)
        {
            case GameState.MainMenu:
                _menu.Update();
                if (Pressed(keys, Keys.Enter)) StartSession();
                break;

            case GameState.Playing:
                UpdatePlaying(gameTime, keys);
                break;

            case GameState.Paused:
                if (Pressed(keys, Keys.P)) _state = GameState.Playing;
                break;

            case GameState.GameOver:
                if (Pressed(keys, Keys.R)) StartSession();
                break;
        }

        _prevKeys = keys;
        base.Update(gameTime);
    }

    private void UpdatePlaying(GameTime gameTime, KeyboardState keys)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _sessionTime += dt * _speedMult;

        // keyboard actions (edge-triggered)
        if (Pressed(keys, Keys.P)) _state = GameState.Paused;
        if (Pressed(keys, Keys.B)) _fleeModeOn = !_fleeModeOn;
        if (Pressed(keys, Keys.M)) _soundMuted = !_soundMuted;
        if (Pressed(keys, Keys.R)) { CommitSessionBests(); StartSession(); return; }

        if (Pressed(keys, Keys.S))
        {
            _foxes.Add(Fox.SpawnRandom(_foxBodyTex, _foxTailTex, _foxLegTex, ScreenWidth, ScreenHeight,
                _config.FoxHungerLimit, _config.FoxReproInterval, _config.FoxLifespan));
            _rabbits.Add(Rabbit.SpawnRandom(_rabbitBodyTex, _rabbitEarTex, _rabbitLegTex, ScreenWidth, ScreenHeight,
                _config.RabbitReproInterval, _config.RabbitLifespan, _config.RabbitHungerLimit));
        }

        if (Pressed(keys, Keys.OemPlus)  || Pressed(keys, Keys.Add))
            _speedMult = MathHelper.Clamp(_speedMult + 0.25f, 0.25f, 3f);
        if (Pressed(keys, Keys.OemMinus) || Pressed(keys, Keys.Subtract))
            _speedMult = MathHelper.Clamp(_speedMult - 0.25f, 0.25f, 3f);

        // snapshot before foxes run so kills this frame are counted correctly
        int deadBefore = DeadCount(_rabbits);

        foreach (var fox in _foxes)
            fox.Update(gameTime, _rabbits, _foxes, ScreenWidth, ScreenHeight, _speedMult);

        foreach (var rabbit in _rabbits)
            rabbit.Update(gameTime, _foxes, _rabbits, _foodItems, _fleeModeOn, ScreenWidth, ScreenHeight, _grassZones, _speedMult);

        int newKills = DeadCount(_rabbits) - deadBefore;

        // remove eaten food then periodically regrow one near a random grass zone
        _foodItems.RemoveAll(f => f.IsEaten);
        if (_grassZones.Count > 0)
        {
            _foodRespawnTimer += dt * _speedMult;
            if (_foodRespawnTimer >= FoodRespawnInterval)
            {
                _foodRespawnTimer = 0f;
                var zone = _grassZones[Random.Shared.Next(_grassZones.Count)];
                _foodItems.Add(FoodItem.SpawnNearZone(_foodTex, zone, GrassZoneRadius, ScreenWidth, ScreenHeight));
            }
        }
        _sessionKills += newKills;
        _totalKills   += newKills;

        // prune dead entities so lists stay clean
        _rabbits.RemoveAll(r => !r.IsAlive);
        _foxes.RemoveAll(f => !f.IsAlive);

        // rabbit reproduction
        var newRabbits = new List<Rabbit>();
        foreach (var rabbit in _rabbits)
        {
            if (rabbit.WantsToReproduce)
            {
                rabbit.WantsToReproduce = false;
                if (_rabbits.Count + newRabbits.Count < MaxRabbits)
                    newRabbits.Add(Rabbit.SpawnRandom(_rabbitBodyTex, _rabbitEarTex, _rabbitLegTex,
                        ScreenWidth, ScreenHeight, _config.RabbitReproInterval, _config.RabbitLifespan,
                        _config.RabbitHungerLimit));
                else
                    _rabbits[Random.Shared.Next(_rabbits.Count)].IsAlive = false; // cull a random rabbit
            }
        }
        _rabbits.AddRange(newRabbits);

        // fox reproduction
        var newFoxes = new List<Fox>();
        foreach (var fox in _foxes)
        {
            if (fox.WantsToReproduce)
            {
                fox.WantsToReproduce = false;
                if (_foxes.Count + newFoxes.Count < MaxFoxes)
                    newFoxes.Add(Fox.SpawnRandom(_foxBodyTex, _foxTailTex, _foxLegTex,
                        ScreenWidth, ScreenHeight, _config.FoxHungerLimit, _config.FoxReproInterval,
                        _config.FoxLifespan));
                else
                    _foxes[Random.Shared.Next(_foxes.Count)].IsAlive = false; // cull a random fox
            }
        }
        _foxes.AddRange(newFoxes);

        // extinction check — freeze clock and transition to game over
        if (_rabbits.Count == 0 || _foxes.Count == 0)
        {
            _extinctSpecies = _rabbits.Count == 0 ? "rabbits" : "foxes";
            _isNewRecord    = _sessionTime > _bestSessionTime;
            CommitSessionBests();
            PersistStats();
            _state = GameState.GameOver;
        }
    }

    private static int DeadCount(List<Rabbit> list)
    {
        int n = 0;
        foreach (var r in list) if (!r.IsAlive) n++;
        return n;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(28, 75, 28));

        if (_state == GameState.MainMenu)
        {
            _spriteBatch.Begin();
            _menu.Draw(_spriteBatch);
            DrawMenuStats();
            _spriteBatch.End();
        }
        else
        {
            // grass zones drawn beneath everything
            _spriteBatch.Begin();
            foreach (var zone in _grassZones)
                _spriteBatch.Draw(_grassZoneTex, zone - new Vector2(GrassZoneRadius), Color.White);
            _spriteBatch.End();

            // food items and entities with per-part depth sorting
            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack);
            foreach (var food   in _foodItems) food.Draw(_spriteBatch);
            foreach (var fox    in _foxes)     fox.Draw(_spriteBatch);
            foreach (var rabbit in _rabbits)   rabbit.Draw(_spriteBatch);
            _spriteBatch.End();

            // HUD or game-over overlay on top
            _spriteBatch.Begin();
            if (_state == GameState.GameOver)
                DrawGameOver();
            else
                _hud.Draw(_spriteBatch, _rabbits, _foxes,
                    _sessionKills, _speedMult, _fleeModeOn,
                    _state == GameState.Paused, _soundMuted,
                    _sessionTime, _bestSessionTime);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    // persistent stats and keybindings drawn on top of the menu layout
    private void DrawMenuStats()
    {
        float sx = MainMenu.StatsX;
        float sy = MainMenu.StatsY;
        float sg = MainMenu.StatsGap;
        DrawMenuStr($"Survival:  {FormatTime(_bestSessionTime)}", new Vector2(sx, sy),        Color.LightGray, 1.5f);
        DrawMenuStr($"Kills:     {_bestSessionKills}",            new Vector2(sx, sy + sg),   Color.LightGray, 1.5f);
        DrawMenuStr($"Lifetime:  {_config.LifetimeKills}",        new Vector2(sx, sy + sg*2), Color.Orange,    1.5f);
        DrawMenuStr("[S] Spawn  [B] Flee  [+/-] Speed  [M] Mute  [R] Reset  [ESC] Menu",
                    new Vector2(375, MainMenu.KeybindingsY), new Color(120, 180, 120), 1.05f);
    }

    private void DrawMenuStr(string text, Vector2 pos, Color color, float scale)
        => _spriteBatch.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void StartSession()
    {
        // pull configuration values the menu may have changed
        _config.InitialFoxCount       = _menu.FoxCount;
        _config.InitialRabbitCount    = _menu.RabbitCount;
        _config.DefaultSpeed          = _menu.Speed;
        _config.FoxHungerLimit        = _menu.FoxHungerLimit;
        _config.FoxReproInterval      = _menu.FoxReproInterval;
        _config.FoxLifespan           = _menu.FoxLifespan;
        _config.RabbitReproInterval   = _menu.RabbitReproInterval;
        _config.RabbitLifespan        = _menu.RabbitLifespan;
        _config.RabbitHungerLimit     = _menu.RabbitHungerLimit;
        _config.GrassZoneCount        = _menu.GrassZoneCount;

        _rabbits.Clear();
        _foxes.Clear();
        _grassZones.Clear();
        _foodItems.Clear();
        _foodRespawnTimer = 0f;
        _sessionTime  = 0f;
        _sessionKills = 0;
        _speedMult    = _config.DefaultSpeed;
        _fleeModeOn   = true;
        _soundMuted   = !_menu.SoundOn;

        for (int i = 0; i < _config.GrassZoneCount; i++)
            _grassZones.Add(new Vector2(
                120 + (float)Random.Shared.NextDouble() * (ScreenWidth  - 240),
                120 + (float)Random.Shared.NextDouble() * (ScreenHeight - 240)));

        // spawn wheat/food concentrated in grass zones with a few scattered extras
        foreach (var zone in _grassZones)
            for (int i = 0; i < InitialFoodPerZone; i++)
                _foodItems.Add(FoodItem.SpawnNearZone(_foodTex, zone, GrassZoneRadius, ScreenWidth, ScreenHeight));
        for (int i = 0; i < ScatteredFoodCount; i++)
            _foodItems.Add(FoodItem.SpawnRandom(_foodTex, ScreenWidth, ScreenHeight));

        for (int i = 0; i < _config.InitialFoxCount; i++)
            _foxes.Add(Fox.SpawnRandom(_foxBodyTex, _foxTailTex, _foxLegTex, ScreenWidth, ScreenHeight,
                _config.FoxHungerLimit, _config.FoxReproInterval, _config.FoxLifespan));
        for (int i = 0; i < _config.InitialRabbitCount; i++)
            _rabbits.Add(Rabbit.SpawnRandom(_rabbitBodyTex, _rabbitEarTex, _rabbitLegTex, ScreenWidth, ScreenHeight,
                _config.RabbitReproInterval, _config.RabbitLifespan, _config.RabbitHungerLimit));

        _state = GameState.Playing;
    }

    // preserve best stats from the just-ended session before resetting
    private void CommitSessionBests()
    {
        if (_sessionKills > _bestSessionKills) _bestSessionKills = _sessionKills;
        if (_sessionTime  > _bestSessionTime)  _bestSessionTime  = _sessionTime;
    }

    private void PersistStats()
    {
        CommitSessionBests();
        if (_bestSessionKills > _config.BestSessionKills)    _config.BestSessionKills    = _bestSessionKills;
        if (_bestSessionTime  > _config.BestSurvivalSeconds) _config.BestSurvivalSeconds = _bestSessionTime;
        _config.LifetimeKills = _totalKills;
        ConfigManager.Save(_config);
    }

    private void SaveAndExit()
    {
        PersistStats();
        Exit();
    }

    private bool Pressed(KeyboardState current, Keys key)
        => current.IsKeyDown(key) && !_prevKeys.IsKeyDown(key);

    private void DrawGameOver()
    {
        const int panelW = 620, panelH = 370;
        int panelX = (ScreenWidth  - panelW) / 2;
        int panelY = (ScreenHeight - panelH) / 2;
        float cx   = ScreenWidth  / 2f;

        _spriteBatch.Draw(_overlayTex, new Rectangle(panelX, panelY, panelW, panelH), Color.White);

        Str("- GAME OVER -",                   cx, panelY +  35, new Color(220, 55, 55),   2.2f);
        Str($"All {_extinctSpecies} have died.", cx, panelY + 105, Color.LightGray,          1.2f);

        Str($"Time:  {FormatTime(_sessionTime)}",    cx, panelY + 150, Color.White, 1.6f);
        Str($"Best:  {FormatTime(_bestSessionTime)}", cx, panelY + 188, Color.Gold,  1.6f);

        if (_isNewRecord)
            Str("**  NEW RECORD!  **", cx, panelY + 238, new Color(255, 228, 40), 1.75f);

        float promptY = panelY + (_isNewRecord ? 305 : 265);
        Str("[R] Play Again          [ESC] Main Menu", cx, promptY, new Color(120, 200, 120), 1.1f);
    }

    // draw text centered on cx
    private void Str(string text, float cx, float y, Color color, float scale)
    {
        float w = _font.MeasureString(text).X * scale;
        _spriteBatch.DrawString(_font, text, new Vector2(cx - w / 2f, y),
            color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private static string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return $"{m:D2}:{s:D2}";
    }
}
