using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace the_hunters_finalproject;

public class MainMenu
{
    private SpriteFont _font;
    private Texture2D  _pixel;

    public int   FoxCount            = 3;
    public int   RabbitCount         = 10;
    public float Speed               = 1f;
    public bool  SoundOn             = true;
    public float FoxHungerLimit      = 20f;
    public float FoxReproInterval    = 15f;
    public float RabbitReproInterval = 12f;
    public float RabbitLifespan      = 60f;
    public int   GrassZoneCount      = 5;

    // y positions for stats that Game1.DrawMenuStats() needs to match
    public const float StatsY         = 498f;
    public const float StatsGap       = 45f;
    public const float KeybindingsY   = 716f;

    public MainMenu(SpriteFont font, GraphicsDevice gd)
    {
        _font  = font;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Update() { }

    public void Draw(SpriteBatch sb)
    {
        // background panel
        DrawRect(sb, new Rectangle(350, 40, 900, 930), new Color(10, 40, 10));
        DrawRect(sb, new Rectangle(350, 40,  900,  2), new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(350, 968, 900,  2), new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(350, 40,    2, 930), new Color(80, 160, 80));
        DrawRect(sb, new Rectangle(1248, 40,   2, 930), new Color(80, 160, 80));

        // title
        DrawStr(sb, "THE HUNTERS",         new Vector2(470,  58), Color.Yellow,     2.6f);
        DrawStr(sb, "Predator / Prey Sim", new Vector2(490, 148), Color.LightGreen, 1.4f);

        Divider(sb, 188);

        // layout constants
        const float sy   = 202f;
        const float gap  = 52f;
        const float scL  = 1.6f;  // left column (spawn settings)
        const float scR  = 1.4f;  // right column (sim tuning — slightly smaller to fit labels)

        const float lLab = 375f;  const float lVal = 545f;
        const float rLab = 680f;  const float rVal = 1140f;

        Color hdr = new Color(140, 200, 140);
        Color val = new Color(220, 220, 120);

        // --- left column: spawn settings ---
        DrawStr(sb, "--- Spawn Settings ---", new Vector2(lLab, sy), hdr, 1.2f);
        DrawStr(sb, "Foxes:",    new Vector2(lLab, sy + gap),     Color.White, scL);
        DrawStr(sb, $"{FoxCount}",  new Vector2(lVal, sy + gap),  val, scL);
        DrawStr(sb, "Rabbits:",  new Vector2(lLab, sy + gap * 2), Color.White, scL);
        DrawStr(sb, $"{RabbitCount}", new Vector2(lVal, sy + gap * 2), val, scL);
        DrawStr(sb, "Speed:",    new Vector2(lLab, sy + gap * 3), Color.White, scL);
        DrawStr(sb, $"{Speed:F2}", new Vector2(lVal, sy + gap * 3), val, scL);
        DrawStr(sb, "Sound:",    new Vector2(lLab, sy + gap * 4), Color.White, scL);
        DrawStr(sb, SoundOn ? "ON" : "OFF", new Vector2(lVal, sy + gap * 4), val, scL);

        // left col sub-divider → session records section below spawn settings
        DrawRect(sb, new Rectangle(370, (int)(sy + gap * 4 + 38), 280, 1), new Color(50, 100, 50));
        DrawStr(sb, "Session Records", new Vector2(lLab, sy + gap * 4 + 50), new Color(180, 180, 180), 1.3f);
        // values for Survival / Kills / Lifetime drawn by Game1.DrawMenuStats() at StatsY / StatsGap

        // --- right column: sim tuning ---
        DrawStr(sb, "--- Sim Tuning ---",      new Vector2(rLab, sy), hdr, 1.2f);
        DrawStr(sb, "Fox Hunger Limit:",        new Vector2(rLab, sy + gap),     Color.White, scR);
        DrawStr(sb, $"{FoxHungerLimit:F0}s",    new Vector2(rVal, sy + gap),     val, scR);
        DrawStr(sb, "Fox Repro Every:",         new Vector2(rLab, sy + gap * 2), Color.White, scR);
        DrawStr(sb, $"{FoxReproInterval:F0}s",  new Vector2(rVal, sy + gap * 2), val, scR);
        DrawStr(sb, "Rabbit Repro Every:",      new Vector2(rLab, sy + gap * 3), Color.White, scR);
        DrawStr(sb, $"{RabbitReproInterval:F0}s", new Vector2(rVal, sy + gap * 3), val, scR);
        DrawStr(sb, "Rabbit Lifespan:",         new Vector2(rLab, sy + gap * 4), Color.White, scR);
        DrawStr(sb, $"{RabbitLifespan:F0}s",    new Vector2(rVal, sy + gap * 4), val, scR);
        DrawStr(sb, "Grass Zones:",             new Vector2(rLab, sy + gap * 5), Color.White, scR);
        DrawStr(sb, $"{GrassZoneCount}",        new Vector2(rVal, sy + gap * 5), val, scR);

        // global divider after both columns, ENTER, second divider
        Divider(sb, 630);
        DrawStr(sb, "Press  ENTER  to  Start", new Vector2(430, 644), Color.Yellow, 1.9f);
        Divider(sb, 702);
        // keybindings hint drawn by Game1.DrawMenuStats() at KeybindingsY
    }

    private void DrawStr(SpriteBatch sb, string text, Vector2 pos, Color color, float scale)
        => sb.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        => sb.Draw(_pixel, r, c);

    private void Divider(SpriteBatch sb, int y)
        => DrawRect(sb, new Rectangle(390, y, 820, 2), new Color(60, 120, 60));
}
