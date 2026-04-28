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
    public float FoxLifespan         = 90f;
    public float RabbitReproInterval = 12f;
    public float RabbitLifespan      = 60f;
    public float RabbitHungerLimit   = 35f;
    public int   GrassZoneCount      = 5;

    // positions used by Game1.DrawMenuStats() to place the stat values
    public const float StatsX       = 130f;
    public const float StatsY       = 560f;
    public const float StatsGap     = 58f;
    public const float KeybindingsY = 845f;

    private KeyboardState _prevKeys;

    public MainMenu(SpriteFont font, GraphicsDevice gd)
    {
        _font  = font;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Update()
    {
        var keys = Keyboard.GetState();

        if (keys.IsKeyDown(Keys.Up)    && !_prevKeys.IsKeyDown(Keys.Up))    FoxCount++;
        if (keys.IsKeyDown(Keys.Down)  && !_prevKeys.IsKeyDown(Keys.Down))  FoxCount--;
        if (keys.IsKeyDown(Keys.Right) && !_prevKeys.IsKeyDown(Keys.Right)) RabbitCount++;
        if (keys.IsKeyDown(Keys.Left)  && !_prevKeys.IsKeyDown(Keys.Left))  RabbitCount--;
        if (keys.IsKeyDown(Keys.M)     && !_prevKeys.IsKeyDown(Keys.M))     SoundOn = !SoundOn;

        _prevKeys = keys;
    }

    public void Draw(SpriteBatch sb)
    {
        // --- outer panel ---
        // box spans x: 80–1520 (1440px wide), y: 12–978 (966px tall)
        const int bx = 80, by = 12, bw = 1440, bh = 966;
        DrawRect(sb, new Rectangle(bx,          by,          bw, bh),  new Color(8, 35, 8));    // fill
        DrawRect(sb, new Rectangle(bx,          by,          bw,  2),  new Color(80, 160, 80)); // top
        DrawRect(sb, new Rectangle(bx,          by + bh - 2, bw,  2),  new Color(80, 160, 80)); // bottom
        DrawRect(sb, new Rectangle(bx,          by,           2, bh),  new Color(80, 160, 80)); // left
        DrawRect(sb, new Rectangle(bx + bw - 2, by,           2, bh),  new Color(80, 160, 80)); // right

        // --- title block ---
        DrawStr(sb, "THE HUNTERS",         new Vector2(473,  30), Color.Yellow,                2.6f);
        DrawStr(sb, "Predator / Prey Sim", new Vector2(520, 116), Color.LightGreen,            1.4f);
        HRule(sb, 158);

        // --- column layout constants ---
        const float sy   = 237f;   // first content row
        const float gapL = 63f;    // left column row gap
        const float gapR = 55f;    // right column row gap (7 rows)
        const float scL  = 1.6f;
        const float scR  = 1.35f;

        const float lLab = 130f;   // left label x
        const float lVal = 375f;   // left value x
        const float rLab = 810f;   // right label x
        const float rVal = 1290f;  // right value x

        Color hdr = new Color(140, 200, 140);
        Color val = new Color(220, 220, 120);

        // column headers (same y, clearly separated)
        DrawStr(sb, "-- Spawn Settings --", new Vector2(lLab, 175), hdr, 1.2f);
        DrawStr(sb, "--  Sim  Tuning  --",  new Vector2(rLab, 175), hdr, 1.2f);

        // vertical separator between the two columns
        DrawRect(sb, new Rectangle(765, 158, 2, 540), new Color(50, 100, 50));

        // --- left column: spawn settings ---
        DrawStr(sb, "Foxes:",               new Vector2(lLab, sy),          Color.White, scL);
        DrawStr(sb, $"{FoxCount}",          new Vector2(lVal, sy),          val, scL);
        DrawStr(sb, "Rabbits:",             new Vector2(lLab, sy + gapL),   Color.White, scL);
        DrawStr(sb, $"{RabbitCount}",       new Vector2(lVal, sy + gapL),   val, scL);
        DrawStr(sb, "Speed:",               new Vector2(lLab, sy + gapL*2), Color.White, scL);
        DrawStr(sb, $"{Speed:F2}",          new Vector2(lVal, sy + gapL*2), val, scL);
        DrawStr(sb, "Sound:",               new Vector2(lLab, sy + gapL*3), Color.White, scL);
        DrawStr(sb, SoundOn ? "ON" : "OFF", new Vector2(lVal, sy + gapL*3), val, scL);

        // session records sub-section (left column, below spawn settings)
        float recDivY = sy + gapL * 3 + 50f;
        DrawRect(sb, new Rectangle((int)lLab, (int)recDivY, 590, 1), new Color(50, 100, 50));
        DrawStr(sb, "Session Records", new Vector2(lLab, recDivY + 12f), new Color(160, 200, 160), 1.3f);
        // Survival / Kills / Lifetime drawn by Game1.DrawMenuStats() at StatsY / StatsGap

        // --- right column: sim tuning ---
        DrawStr(sb, "Fox Hunger Limit:",          new Vector2(rLab, sy),          Color.White, scR);
        DrawStr(sb, $"{FoxHungerLimit:F0}s",      new Vector2(rVal, sy),          val, scR);

        DrawStr(sb, "Fox Repro Every:",           new Vector2(rLab, sy + gapR),   Color.White, scR);
        DrawStr(sb, $"{FoxReproInterval:F0}s",    new Vector2(rVal, sy + gapR),   val, scR);

        DrawStr(sb, "Fox Lifespan:",              new Vector2(rLab, sy + gapR*2), Color.White, scR);
        DrawStr(sb, $"{FoxLifespan:F0}s",         new Vector2(rVal, sy + gapR*2), val, scR);

        DrawStr(sb, "Rabbit Repro Every:",        new Vector2(rLab, sy + gapR*3), Color.White, scR);
        DrawStr(sb, $"{RabbitReproInterval:F0}s", new Vector2(rVal, sy + gapR*3), val, scR);

        DrawStr(sb, "Rabbit Lifespan:",           new Vector2(rLab, sy + gapR*4), Color.White, scR);
        DrawStr(sb, $"{RabbitLifespan:F0}s",      new Vector2(rVal, sy + gapR*4), val, scR);

        DrawStr(sb, "Rabbit Hunger:",             new Vector2(rLab, sy + gapR*5), Color.White, scR);
        DrawStr(sb, $"{RabbitHungerLimit:F0}s",   new Vector2(rVal, sy + gapR*5), val, scR);

        DrawStr(sb, "Grass Zones:",               new Vector2(rLab, sy + gapR*6), Color.White, scR);
        DrawStr(sb, $"{GrassZoneCount}",          new Vector2(rVal, sy + gapR*6), val, scR);

        // --- footer: ENTER prompt and keybindings ---
        // last content: StatsY + StatsGap*2 = 560+116 = 676; right col last = 237+55*6 = 567
        // both clear of HRule at 720
        HRule(sb, 720);
        DrawStr(sb, "Press  ENTER  to  Start", new Vector2(510, 737), Color.Yellow, 1.9f);
        HRule(sb, 820);
        // keybindings hint drawn by Game1.DrawMenuStats() at KeybindingsY
    }

    private void DrawStr(SpriteBatch sb, string text, Vector2 pos, Color color, float scale)
        => sb.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        => sb.Draw(_pixel, r, c);

    private void HRule(SpriteBatch sb, int y)
        => DrawRect(sb, new Rectangle(100, y, 1440, 2), new Color(55, 115, 55));
}
