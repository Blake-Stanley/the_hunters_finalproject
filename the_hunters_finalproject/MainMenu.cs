using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace the_hunters_finalproject;

public class MainMenu
{
    private SpriteFont _font;

    public int FoxCount = 3;
    public int RabbitCount = 10;
    public float Speed = 1f;
    public bool SoundOn = true;

    public MainMenu(SpriteFont font)
    {
        _font = font;
    }

    public void Update()
    {
        var mouse = Mouse.GetState();

        //  add button logic here soon
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(_font, "Predator-Prey Simulation", new Vector2(200, 50), Color.White);

        spriteBatch.DrawString(_font, $"Foxes: {FoxCount}", new Vector2(200, 150), Color.White);
        spriteBatch.DrawString(_font, $"Rabbits: {RabbitCount}", new Vector2(200, 200), Color.White);
        spriteBatch.DrawString(_font, $"Speed: {Speed:F1}", new Vector2(200, 250), Color.White);
        spriteBatch.DrawString(_font, $"Sound: {(SoundOn ? "ON" : "OFF")}", new Vector2(200, 300), Color.White);

        spriteBatch.DrawString(_font, "[Press ENTER to Start]", new Vector2(200, 400), Color.Yellow);
    }
}