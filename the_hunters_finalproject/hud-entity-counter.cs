using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace the_hunters_finalproject;

public class Hud
{
    private SpriteFont _font;
    private const int ScreenWidth  = 1600;
    private const int ScreenHeight = 1000;

    public Hud(SpriteFont font)
    {
        _font = font;
    }

    public void Draw(SpriteBatch spriteBatch, List<Rabbit> rabbits, List<Fox> foxes,
        int sessionKills, float speedMult, bool fleeModeOn, bool paused, bool muted,
        float sessionTime, float bestSessionTime)
    {
        int rabbitsAlive = 0;
        foreach (var r in rabbits)
            if (r.IsAlive) rabbitsAlive++;

        int foxesAlive = 0;
        foreach (var f in foxes)
            if (f.IsAlive) foxesAlive++;

        // top-left stats
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

        // top-right: session time and best survival record
        string timeStr = $"Time:  {FormatTime(sessionTime)}";
        string bestStr = $"Best:  {FormatTime(bestSessionTime)}";
        float timeX = ScreenWidth - 10 - _font.MeasureString(timeStr).X;
        float bestX = ScreenWidth - 10 - _font.MeasureString(bestStr).X;
        spriteBatch.DrawString(_font, timeStr, new Vector2(timeX, 10), Color.White);
        spriteBatch.DrawString(_font, bestStr, new Vector2(bestX, 35), Color.Gold);

        // bottom controls bar
        const string controls = "[P] Pause   [B] Flee   [S] Spawn   [+/-] Speed   [M] Mute   [R] Reset   [ESC] Menu";
        float controlsX = (ScreenWidth - _font.MeasureString(controls).X) / 2f;
        spriteBatch.DrawString(_font, controls, new Vector2(controlsX, ScreenHeight - 30), new Color(120, 200, 120));
    }

    private static string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return $"{m:D2}:{s:D2}";
    }
}
