using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace the_hunters_finalproject;

public class FoodItem
{
    public Vector2 Position;
    public bool IsEaten = false;

    private readonly Texture2D _tex;

    public FoodItem(Texture2D tex, Vector2 position)
    {
        _tex = tex;
        Position = position;
    }

    public static FoodItem SpawnNearZone(Texture2D tex, Vector2 zoneCenter, float zoneRadius,
                                         int screenWidth, int screenHeight)
    {
        float angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        float dist  = (float)(Random.Shared.NextDouble() * zoneRadius);
        float x = MathHelper.Clamp(zoneCenter.X + MathF.Cos(angle) * dist, 20f, screenWidth  - 20f);
        float y = MathHelper.Clamp(zoneCenter.Y + MathF.Sin(angle) * dist, 20f, screenHeight - 20f);
        return new FoodItem(tex, new Vector2(x, y));
    }

    public static FoodItem SpawnRandom(Texture2D tex, int screenWidth, int screenHeight)
    {
        float x = 40 + (float)Random.Shared.NextDouble() * (screenWidth  - 80);
        float y = 40 + (float)Random.Shared.NextDouble() * (screenHeight - 80);
        return new FoodItem(tex, new Vector2(x, y));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsEaten) return;
        // origin at bottom-center so the stalk base sits on the ground position
        var origin = new Vector2(_tex.Width / 2f, _tex.Height);
        spriteBatch.Draw(_tex, Position, null, Color.White, 0f, origin, 1.3f, SpriteEffects.None, 0.3f);
    }
}
