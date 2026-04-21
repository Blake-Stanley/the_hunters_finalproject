using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace the_hunters_finalproject;

public class Hud
{
    private SpriteFont _font;

    public Hud(SpriteFont font)
    {
        _font = font;
    }

    public void Draw(SpriteBatch spriteBatch, List<Rabbit> rabbits, List<Fox> foxes)
    {
        int rabbitsAlive = 0;
        foreach (var r in rabbits)
            if (r.IsAlive) rabbitsAlive++;

        int foxesAlive = 0;
        foreach (var f in foxes)
            if (f.IsAlive) foxesAlive++;

        int killed = rabbits.Count - rabbitsAlive;

        spriteBatch.DrawString(_font, $"Rabbits: {rabbitsAlive}", new Vector2(10, 10), Color.White);
        spriteBatch.DrawString(_font, $"Foxes:   {foxesAlive}",   new Vector2(10, 35), Color.White);
        spriteBatch.DrawString(_font, $"Killed:  {killed}",       new Vector2(10, 60), Color.Orange);
    }
}