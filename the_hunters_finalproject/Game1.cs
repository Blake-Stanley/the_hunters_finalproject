using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace the_hunters_finalproject;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private List<Rabbit> _rabbits = new();
    private List<Fox> _foxes = new();
    private bool _fleeModeOn = true;
    private Hud _hud;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        SpriteFont font = Content.Load<SpriteFont>("DefaultFont");
        _hud = new Hud(font);
        
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape)) Exit();

        if (kb.IsKeyDown(Keys.B)) _fleeModeOn = !_fleeModeOn;

        foreach (var rabbit in _rabbits)
            rabbit.Update(gameTime, _foxes, _fleeModeOn, 800, 600);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGreen);
        _spriteBatch.Begin();
        _hud.Draw(_spriteBatch, _rabbits, _foxes);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}