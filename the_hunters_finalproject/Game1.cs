using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace the_hunters_finalproject;

public enum GameState { MainMenu, Playing, Paused }

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int ScreenWidth  = 800;
    private const int ScreenHeight = 600;

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
    private List<Rabbit> _rabbits = new();
    private List<Fox>    _foxes   = new();

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
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth  = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();
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
        _menu = new MainMenu(_font);
        _menu.FoxCount    = _config.InitialFoxCount;
        _menu.RabbitCount = _config.InitialRabbitCount;
        _menu.Speed       = _config.DefaultSpeed;

        // fox: orange body, lighter tail, darker legs
        _foxBodyTex = MakeTex(24, 16, new Color(210, 110, 40));
        _foxTailTex = MakeTex(12, 10, new Color(230, 160, 70));
        _foxLegTex  = MakeTex(5,  10, new Color(180,  80, 25));

        // rabbit: light gray body, pink-tinted ears, gray legs
        _rabbitBodyTex = MakeTex(18, 14, new Color(215, 215, 215));
        _rabbitEarTex  = MakeTex(4,  12, new Color(240, 200, 200));
        _rabbitLegTex  = MakeTex(5,   8, new Color(195, 195, 195));
    }

    private Texture2D MakeTex(int w, int h, Color color)
    {
        var tex  = new Texture2D(GraphicsDevice, w, h);
        var data = new Color[w * h];
        for (int i = 0; i < data.Length; i++) data[i] = color;
        tex.SetData(data);
        return tex;
    }

    protected override void Update(GameTime gameTime)
    {
        var keys = Keyboard.GetState();

        if (Pressed(keys, Keys.Escape))
        {
            SaveAndExit();
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
        }

        _prevKeys = keys;
        base.Update(gameTime);
    }

    private void UpdatePlaying(GameTime gameTime, KeyboardState keys)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _sessionTime += dt;

        // keyboard actions (edge-triggered)
        if (Pressed(keys, Keys.P)) _state = GameState.Paused;
        if (Pressed(keys, Keys.B)) _fleeModeOn = !_fleeModeOn;
        if (Pressed(keys, Keys.M)) _soundMuted = !_soundMuted;
        if (Pressed(keys, Keys.R)) { CommitSessionBests(); StartSession(); return; }

        if (Pressed(keys, Keys.S))
        {
            _foxes.Add(Fox.SpawnRandom(_foxBodyTex, _foxTailTex, _foxLegTex, ScreenWidth, ScreenHeight));
            _rabbits.Add(Rabbit.SpawnRandom(_rabbitBodyTex, _rabbitEarTex, _rabbitLegTex, ScreenWidth, ScreenHeight));
        }

        if (Pressed(keys, Keys.OemPlus)  || Pressed(keys, Keys.Add))
            _speedMult = MathHelper.Clamp(_speedMult + 0.25f, 0.25f, 3f);
        if (Pressed(keys, Keys.OemMinus) || Pressed(keys, Keys.Subtract))
            _speedMult = MathHelper.Clamp(_speedMult - 0.25f, 0.25f, 3f);

        // update foxes first so they can mark rabbits dead this frame
        foreach (var fox in _foxes)
            fox.Update(gameTime, _rabbits, ScreenWidth, ScreenHeight, _speedMult);

        // count kills produced this frame
        int deadBefore = DeadCount(_rabbits);
        foreach (var rabbit in _rabbits)
            rabbit.Update(gameTime, _foxes, _fleeModeOn, ScreenWidth, ScreenHeight, _speedMult);
        int newKills = DeadCount(_rabbits) - deadBefore;
        _sessionKills += newKills;
        _totalKills   += newKills;
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
            // entities with per-part depth sorting
            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack);
            foreach (var fox in _foxes)      fox.Draw(_spriteBatch);
            foreach (var rabbit in _rabbits) rabbit.Draw(_spriteBatch);
            _spriteBatch.End();

            // HUD drawn on top in a separate deferred pass
            _spriteBatch.Begin();
            _hud.Draw(_spriteBatch, _rabbits, _foxes,
                _sessionKills, _speedMult, _fleeModeOn,
                _state == GameState.Paused, _soundMuted);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    // persistent stats overlay drawn below Sydney's main menu layout
    private void DrawMenuStats()
    {
        _spriteBatch.DrawString(_font, $"Best Survival:  {FormatTime(_bestSessionTime)}", new Vector2(200, 360), Color.LightGray);
        _spriteBatch.DrawString(_font, $"Best Kills:     {_bestSessionKills}",            new Vector2(200, 390), Color.LightGray);
        _spriteBatch.DrawString(_font, $"Lifetime Kills: {_config.LifetimeKills}",        new Vector2(200, 420), Color.Orange);
        _spriteBatch.DrawString(_font, "[S] Spawn  [B] Flee  [+/-] Speed  [M] Mute  [R] Reset", new Vector2(60, 470), Color.DarkGray);
    }

    private void StartSession()
    {
        // pull configuration values the menu may have changed
        _config.InitialFoxCount    = _menu.FoxCount;
        _config.InitialRabbitCount = _menu.RabbitCount;
        _config.DefaultSpeed       = _menu.Speed;

        _rabbits.Clear();
        _foxes.Clear();
        _sessionTime  = 0f;
        _sessionKills = 0;
        _speedMult    = _config.DefaultSpeed;
        _fleeModeOn   = true;
        _soundMuted   = !_menu.SoundOn;

        for (int i = 0; i < _config.InitialFoxCount; i++)
            _foxes.Add(Fox.SpawnRandom(_foxBodyTex, _foxTailTex, _foxLegTex, ScreenWidth, ScreenHeight));
        for (int i = 0; i < _config.InitialRabbitCount; i++)
            _rabbits.Add(Rabbit.SpawnRandom(_rabbitBodyTex, _rabbitEarTex, _rabbitLegTex, ScreenWidth, ScreenHeight));

        _state = GameState.Playing;
    }

    // preserve best stats from the just-ended session before resetting
    private void CommitSessionBests()
    {
        if (_sessionKills > _bestSessionKills) _bestSessionKills = _sessionKills;
        if (_sessionTime  > _bestSessionTime)  _bestSessionTime  = _sessionTime;
    }

    private void SaveAndExit()
    {
        CommitSessionBests();
        if (_bestSessionKills > _config.BestSessionKills)    _config.BestSessionKills    = _bestSessionKills;
        if (_bestSessionTime  > _config.BestSurvivalSeconds) _config.BestSurvivalSeconds = _bestSessionTime;
        _config.LifetimeKills = _totalKills;
        ConfigManager.Save(_config);
        Exit();
    }

    private bool Pressed(KeyboardState current, Keys key)
        => current.IsKeyDown(key) && !_prevKeys.IsKeyDown(key);

    private static string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return $"{m:D2}:{s:D2}";
    }
}
