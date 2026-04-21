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

    // textures
    private Texture2D _bodyTex;
    private Texture2D _earTex;
    private Texture2D _legTex;

    // hierarchy offsets relative to the root
    private static readonly Vector2 EarOffset = new Vector2(-4f, -10f);
    private static readonly Vector2 LegOffsetL = new Vector2(-6f, 8f);
    private static readonly Vector2 LegOffsetR = new Vector2(2f, 8f);

    public Rabbit(Texture2D bodyTex, Texture2D earTex, Texture2D legTex, Vector2 position)
    {
        _bodyTex = bodyTex;
        _earTex = earTex;
        _legTex = legTex;
        Position = position;

        // random initial velocity, makes rabbits all move in different directions
        float angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        _velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    // Spawn rabbits at random spots within the scene
    public static Rabbit SpawnRandom(Texture2D bodyTex, Texture2D earTex, Texture2D legTex,
                                     int screenWidth, int screenHeight)
    {
        float x = 40 + (float)Random.Shared.NextDouble() * (screenWidth - 80);
        float y = 40 + (float)Random.Shared.NextDouble() * (screenHeight - 80);
        return new Rabbit(bodyTex, earTex, legTex, new Vector2(x, y));
    }

    public void Update(GameTime gameTime, List<Fox> foxes, bool fleeModeOn,
                       int screenWidth, int screenHeight, float speedMult = 1f)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _legPhase += dt * 8f;
        _earPhase += dt * 3f;

        // wander randomly every frame
        float turnAmount = (float)(Random.Shared.NextDouble() - 0.5) * 4f;
        float currentAngle = MathF.Atan2(_velocity.Y, _velocity.X);
        currentAngle += turnAmount * dt;
        _velocity = new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle));

        // flee behavior toggled when foxes nearby
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
        }

        Position += _velocity * _speed * speedMult * dt;

        // bounce off edges of the screen
        if (Position.X < 20f)  { Position.X = 20f;  _velocity.X =  MathF.Abs(_velocity.X); }
        if (Position.X > screenWidth  - 20f) { Position.X = screenWidth  - 20f; _velocity.X = -MathF.Abs(_velocity.X); }
        if (Position.Y < 20f)  { Position.Y = 20f;  _velocity.Y =  MathF.Abs(_velocity.Y); }
        if (Position.Y > screenHeight - 20f) { Position.Y = screenHeight - 20f; _velocity.Y = -MathF.Abs(_velocity.Y); }

        _facingRight = _velocity.X >= 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive) return;

        SpriteEffects flip = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // level 1 (root): body
        Vector2 bodyOrigin = new Vector2(_bodyTex.Width / 2f, _bodyTex.Height / 2f);
        spriteBatch.Draw(_bodyTex, Position, null, Color.White, 0f, bodyOrigin, 1f, flip, 0.5f);

        // level 2 (child): ears — bob on earPhase
        float earBob = MathF.Sin(_earPhase) * 2f;
        Vector2 earPos = Position + ((_facingRight ? 1 : -1) * EarOffset) + new Vector2(0, earBob);
        Vector2 earOrigin = new Vector2(_earTex.Width / 2f, _earTex.Height);
        spriteBatch.Draw(_earTex, earPos, null, Color.White, 0f, earOrigin, 1f, flip, 0.4f);

        // level 2 (child): legs — alternate on legPhase
        float legL = MathF.Sin(_legPhase) * 3f;
        float legR = MathF.Sin(_legPhase + MathHelper.Pi) * 3f;

        Vector2 legOrigin = new Vector2(_legTex.Width / 2f, 0f);

        Vector2 legLPos = Position + LegOffsetL + new Vector2(0, legL);
        spriteBatch.Draw(_legTex, legLPos, null, Color.White, 0f, legOrigin, 1f, flip, 0.6f);

        Vector2 legRPos = Position + LegOffsetR + new Vector2(0, legR);
        spriteBatch.Draw(_legTex, legRPos, null, Color.White, 0f, legOrigin, 1f, flip, 0.6f);
    }
}