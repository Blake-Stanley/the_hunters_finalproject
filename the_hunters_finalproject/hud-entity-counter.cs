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

    public void Draw(SpriteBatch spriteBatch, List<Rabbit> rabbits, List<Fox> foxes,
        int sessionKills, float speedMult, bool fleeModeOn, bool paused, bool muted)
    {
        int rabbitsAlive = 0;
        foreach (var r in rabbits)
            if (r.IsAlive) rabbitsAlive++;

        int foxesAlive = 0;
        foreach (var f in foxes)
            if (f.IsAlive) foxesAlive++;

        spriteBatch.DrawString(_font, $"Rabbits: {rabbitsAlive}", new Vector2(10, 10), Color.White);
        spriteBatch.DrawString(_font, $"Foxes:   {foxesAlive}",   new Vector2(10, 35), Color.White);
        spriteBatch.DrawString(_font, $"Kills:   {sessionKills}", new Vector2(10, 60), Color.Orange);
        spriteBatch.DrawString(_font, $"Speed:   {speedMult:F2}x", new Vector2(10, 85), Color.LightBlue);

        if (!fleeModeOn)
            spriteBatch.DrawString(_font, "FLEE OFF", new Vector2(10, 110), Color.Red);
        if (paused)
            spriteBatch.DrawString(_font, "-- PAUSED --", new Vector2(300, 270), Color.Yellow);
        if (muted)
            spriteBatch.DrawString(_font, "MUTED", new Vector2(10, 135), Color.Gray);
    }
}
