using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace the_hunters_finalproject;

public class MainMenu
{
    private SpriteFont _font;
    private Texture2D  _pixel;

    public int   FoxCount    = 3;
    public int   RabbitCount = 10;
    public float Speed       = 1f;
    public bool  SoundOn     = true;

    public MainMenu(SpriteFont font, GraphicsDevice gd)
    {
        _font  = font;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Update()
    {
        // button logic placeholder
    }

    public void Draw(SpriteBatch sb)
    {
        // background panel
        DrawRect(sb, new Rectangle(350, 40, 900, 930), new Color(10, 40, 10));
        DrawRect(sb, new Rectangle(350, 40, 900,  2),  new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(350, 968, 900, 2),  new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(350, 40,   2, 930), new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(1248, 40,  2, 930), new Color(80, 160, 80));

        // title
        DrawStr(sb, "THE HUNTERS",              new Vector2(460,  80), Color.Yellow,     3.0f);
        DrawStr(sb, "Predator / Prey Sim",      new Vector2(460, 180), Color.LightGreen, 1.6f);

        Divider(sb, 240);

        // config values
        const float bx    = 430f;
        const float bscale = 2.0f;
        const float gap    = 70f;
        DrawStr(sb, $"Foxes:   {FoxCount}",                   new Vector2(bx, 270),       Color.White, bscale);
        DrawStr(sb, $"Rabbits: {RabbitCount}",                new Vector2(bx, 270 + gap), Color.White, bscale);
        DrawStr(sb, $"Speed:   {Speed:F1}",                   new Vector2(bx, 270 + gap * 2), Color.White, bscale);
        DrawStr(sb, $"Sound:   {(SoundOn ? "ON" : "OFF")}",  new Vector2(bx, 270 + gap * 3), Color.White, bscale);

        Divider(sb, 560);

        DrawStr(sb, "Press  ENTER  to  Start", new Vector2(400, 590), Color.Yellow, 2.0f);

        Divider(sb, 680);

        // persistent stats
        DrawStr(sb, "Session Records",          new Vector2(bx, 710), new Color(180, 180, 180), 1.4f);
    }

    private void DrawStr(SpriteBatch sb, string text, Vector2 pos, Color color, float scale)
        => sb.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        => sb.Draw(_pixel, r, c);

    private void Divider(SpriteBatch sb, int y)
        => DrawRect(sb, new Rectangle(390, y, 820, 2), new Color(60, 120, 60));
}
