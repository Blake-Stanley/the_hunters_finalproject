using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace the_hunters_finalproject;

public class Rabbit
{
    // position and movement
    public Vector2 Position;
    private Vector2 _velocity;
    private float _speed = 120f;
    public bool IsAlive = true;

    // animations
    private float _legPhase = 0f;
    private float _earPhase = 0f;
    private bool _facingRight = true;

    // lifespan
    private float _lifeTimer = 0f;
    private float _lifespan;

    // hunger — rabbit dies if it goes too long without eating food
    private float _hungerTimer = 0f;
    private float _hungerLimit;

    // reproduction
    public bool WantsToReproduce = false;
    private float _reprodCooldown = 0f;
    private float _reprodInterval;
    private const float ReprodRange = 50f;
    private const float ReprodSafeRange = 180f;

    // textures
    private Texture2D _bodyTex;
    private Texture2D _earTex;
    private Texture2D _legTex;

    // hierarchy offsets relative to the root
    private static readonly Vector2 EarOffset   = new Vector2(-4f, -10f);
    private static readonly Vector2 LegOffsetL  = new Vector2(-6f,   8f);
    private static readonly Vector2 LegOffsetR  = new Vector2( 2f,   8f);

    public Rabbit(Texture2D bodyTex, Texture2D earTex, Texture2D legTex, Vector2 position,
                  float reprodInterval = 12f, float lifespan = 60f, float hungerLimit = 35f)
    {
        _bodyTex = bodyTex;
        _earTex  = earTex;
        _legTex  = legTex;
        Position = position;
        _reprodInterval = reprodInterval;
        _lifespan = lifespan;
        _hungerLimit = hungerLimit;

        // random initial velocity, makes rabbits all move in different directions
        float angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        _velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    // Spawn rabbits at random spots within the scene
    public static Rabbit SpawnRandom(Texture2D bodyTex, Texture2D earTex, Texture2D legTex,
                                     int screenWidth, int screenHeight,
                                     float reprodInterval = 12f, float lifespan = 60f,
                                     float hungerLimit = 35f)
    {
        float x = 40 + (float)Random.Shared.NextDouble() * (screenWidth  - 80);
        float y = 40 + (float)Random.Shared.NextDouble() * (screenHeight - 80);
        return new Rabbit(bodyTex, earTex, legTex, new Vector2(x, y), reprodInterval, lifespan, hungerLimit);
    }

    public void Update(GameTime gameTime, List<Fox> foxes, List<Rabbit> rabbits, List<FoodItem> foodItems,
                       bool fleeModeOn, int screenWidth, int screenHeight,
                       IReadOnlyList<Vector2> grassZones, float speedMult = 1f)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _lifeTimer   += dt;
        _hungerTimer += dt;

        if (_lifeTimer   >= _lifespan)   { IsAlive = false; return; }
        if (_hungerTimer >= _hungerLimit) { IsAlive = false; return; }

        _legPhase += dt * 8f;
        _earPhase += dt * 3f;

        // eat any food item within reach
        foreach (var food in foodItems)
        {
            if (food.IsEaten) continue;
            if (Vector2.Distance(Position, food.Position) < 15f)
            {
                food.IsEaten  = true;
                _hungerTimer  = 0f;
                break;
            }
        }

        // wander randomly every frame
        float turnAmount   = (float)(Random.Shared.NextDouble() - 0.5) * 4f;
        float currentAngle = MathF.Atan2(_velocity.Y, _velocity.X);
        currentAngle += turnAmount * dt;
        _velocity = new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle));

        // flee from nearby foxes; when safe, forage toward food or grass zones
        if (fleeModeOn)
        {
            Vector2 fleeForce = Vector2.Zero;
            foreach (var fox in foxes)
            {
                if (!fox.IsAlive) continue;
                Vector2 diff = Position - fox.Position;
                float dist = diff.Length();
                if (dist < 150f && dist > 0.01f)
                    fleeForce += Vector2.Normalize(diff) * (150f - dist);
            }
            if (fleeForce.LengthSquared() > 0.01f)
                _velocity = Vector2.Normalize(fleeForce);
            else
                ApplyForageAttraction(foodItems, grassZones);
        }
        else
        {
            ApplyForageAttraction(foodItems, grassZones);
        }

        Position += _velocity * _speed * speedMult * dt;

        // bounce off edges of the screen
        if (Position.X < 20f)              { Position.X = 20f;              _velocity.X =  MathF.Abs(_velocity.X); }
        if (Position.X > screenWidth  - 20f) { Position.X = screenWidth  - 20f; _velocity.X = -MathF.Abs(_velocity.X); }
        if (Position.Y < 20f)              { Position.Y = 20f;              _velocity.Y =  MathF.Abs(_velocity.Y); }
        if (Position.Y > screenHeight - 20f) { Position.Y = screenHeight - 20f; _velocity.Y = -MathF.Abs(_velocity.Y); }

        _facingRight = _velocity.X >= 0;

        // reproduction: two nearby rabbits with no fox close will spawn an offspring
        _reprodCooldown -= dt;
        if (_reprodCooldown <= 0f)
        {
            bool foxNearby = false;
            foreach (var fox in foxes)
                if (fox.IsAlive && Vector2.Distance(Position, fox.Position) < ReprodSafeRange)
                { foxNearby = true; break; }

            if (!foxNearby)
            {
                foreach (var other in rabbits)
                {
                    if (other == this || !other.IsAlive) continue;
                    if (Vector2.Distance(Position, other.Position) < ReprodRange)
                    {
                        WantsToReproduce = true;
                        _reprodCooldown  = _reprodInterval;
                        break;
                    }
                }
            }
        }
    }

    // prefer nearest food; fall back to nearest grass zone
    private void ApplyForageAttraction(List<FoodItem> foodItems, IReadOnlyList<Vector2> grassZones)
    {
        FoodItem nearestFood = null;
        float nearestFoodDist = float.MaxValue;
        foreach (var food in foodItems)
        {
            if (food.IsEaten) continue;
            float d = Vector2.Distance(Position, food.Position);
            if (d < nearestFoodDist) { nearestFoodDist = d; nearestFood = food; }
        }

        const float FoodAttractRadius = 220f;
        if (nearestFood != null && nearestFoodDist < FoodAttractRadius && nearestFoodDist > 5f)
        {
            Vector2 toFood = Vector2.Normalize(nearestFood.Position - Position);
            _velocity = Vector2.Normalize(_velocity + toFood * 0.45f);
            return;
        }

        ApplyGrassAttraction(grassZones);
    }

    private void ApplyGrassAttraction(IReadOnlyList<Vector2> grassZones)
    {
        if (grassZones == null || grassZones.Count == 0) return;
        Vector2 nearest = grassZones[0];
        float nearestDist = Vector2.Distance(Position, nearest);
        for (int i = 1; i < grassZones.Count; i++)
        {
            float d = Vector2.Distance(Position, grassZones[i]);
            if (d < nearestDist) { nearestDist = d; nearest = grassZones[i]; }
        }
        const float AttractRadius = 120f;
        if (nearestDist < AttractRadius && nearestDist > 5f)
        {
            Vector2 toGrass = Vector2.Normalize(nearest - Position);
            _velocity = Vector2.Normalize(_velocity + toGrass * 0.15f);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive) return;

        // tint orange-red as hunger grows
        float hungerFrac = MathHelper.Clamp(_hungerTimer / _hungerLimit, 0f, 1f);
        Color tint = Color.Lerp(Color.White, new Color(230, 80, 10), hungerFrac * hungerFrac);

        SpriteEffects flip = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // level 1 (root): body
        Vector2 bodyOrigin = new Vector2(_bodyTex.Width / 2f, _bodyTex.Height / 2f);
        spriteBatch.Draw(_bodyTex, Position, null, tint, 0f, bodyOrigin, 1f, flip, 0.5f);

        // level 2 (child): ears — bob on earPhase
        float earBob = MathF.Sin(_earPhase) * 2f;
        Vector2 earPos = Position + ((_facingRight ? 1 : -1) * EarOffset) + new Vector2(0, earBob);
        Vector2 earOrigin = new Vector2(_earTex.Width / 2f, _earTex.Height);
        spriteBatch.Draw(_earTex, earPos, null, tint, 0f, earOrigin, 1f, flip, 0.4f);

        // level 2 (child): legs — alternate on legPhase
        float legL = MathF.Sin(_legPhase) * 3f;
        float legR = MathF.Sin(_legPhase + MathHelper.Pi) * 3f;

        Vector2 legOrigin = new Vector2(_legTex.Width / 2f, 0f);

        Vector2 legLPos = Position + LegOffsetL + new Vector2(0, legL);
        spriteBatch.Draw(_legTex, legLPos, null, tint, 0f, legOrigin, 1f, flip, 0.6f);

        Vector2 legRPos = Position + LegOffsetR + new Vector2(0, legR);
        spriteBatch.Draw(_legTex, legRPos, null, tint, 0f, legOrigin, 1f, flip, 0.6f);
    }
}
