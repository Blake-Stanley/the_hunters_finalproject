using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace the_hunters_finalproject;

public class Fox
{
    // position and movement
    public Vector2 Position;
    private Vector2 _velocity;
    private float _speedChase = 90f;
    private float _speedWander = 45f;
    public bool IsAlive = true;
    
    // animations
    private float _legPhase = 0f;
    private float _tailPhase = 0f;
    private bool _facingRight = true;
    
    // texutes
    private Texture2D _bodyTex;
    private Texture2D _tailTex;
    private Texture2D _legTex;
    
    // hierarchy offsets relative to the root
    private static readonly Vector2 TailOffset = new Vector2(-10f, -3f);
    private static readonly Vector2 LegOffsetL = new Vector2(-7f, 10f);
    private static readonly Vector2 LegOffsetR = new Vector2(7f, 10f);

    public Fox(Texture2D bodyTex, Texture2D tailTex, Texture2D legTex, Vector2 position)
    {
        _bodyTex = bodyTex;
        _tailTex = tailTex;
        _legTex = legTex;
        Position = position;
        
        // random initial velocity, makes rabbits all move in different directions
        float angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        _velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
    
    // Spawn foxes at random spots within the scene
    public static Fox SpawnRandom(Texture2D bodyTex, Texture2D tailTex, Texture2D legTex,
        int screenWidth, int screenHeight)
    {
        float x = 40 + (float)Random.Shared.NextDouble() * (screenWidth - 80);
        float y = 40 + (float)Random.Shared.NextDouble() * (screenHeight - 80);
        return new Fox(bodyTex, tailTex, legTex, new Vector2(x, y));
    }

    public void Update(GameTime gameTime, List<Rabbit> rabbits, int screenWidth,
        int screenHeight)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _legPhase += dt * 12f;
        _tailPhase += dt * 2f;
        
        Rabbit? closestRabbit = null;
        float closestDist = float.MaxValue;
        
        // search for the closest rabbit from fox
        foreach (Rabbit rabbit in rabbits)
        {
            if (rabbit.IsAlive)
            {
                float dist = Vector2.Distance(Position, rabbit.Position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestRabbit = rabbit;
                }
            }
        }

        // chase rabbit after noticing the existence of closest rabbit
        if (closestRabbit != null)
        {
            Vector2 diff = closestRabbit.Position - Position;
            if (diff.LengthSquared() > 0.01f)
            {
                _velocity = Vector2.Normalize(diff);
            }
            Position += _velocity * _speedChase * dt;

            // catch rabbit and kill
            if (Vector2.Distance(Position, closestRabbit.Position) < 5f)
            {
                closestRabbit.IsAlive = false;
            }
        }
        // wander if there's no rabbit around fox
        else
        { 
            Position += _velocity * _speedWander * dt;
        }
        
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
        Vector2 bodyOrigin = new Vector2(_bodyTex.Width / 2f, _bodyTex.Height / 2);
        spriteBatch.Draw(_bodyTex, Position, null, Color.White, 0f, bodyOrigin, 1f,
            flip, 0.5f);
        
        // level 2 (child) : tail - sway on tailPhase
        float tailSway = MathF.Sin(_tailPhase) * 0.2f;
        Vector2 tailPos = Position + new Vector2((_facingRight ? 1 : -1) * TailOffset.X, TailOffset.Y);
        Vector2 tailOrigin = new Vector2(_tailTex.Width, _tailTex.Height / 2f);
        spriteBatch.Draw(_tailTex, tailPos, null, Color.White, tailSway, tailOrigin, 1f,
            flip, 0.4f);

        // level 3 (child) : legs - alternate on legPhase
        float legRotL = MathF.Sin(_legPhase) * 0.6f;
        float legRotR = MathF.Sin(_legPhase + MathHelper.Pi) * 0.6f;

        Vector2 legOrigin = new Vector2(_legTex.Width / 2f, 0f);

        Vector2 legLPos = Position + new Vector2((_facingRight ? 1 : -1) * LegOffsetL.X, LegOffsetL.Y);
        spriteBatch.Draw(_legTex, legLPos, null, Color.White, legRotL, legOrigin, 1f,
            flip, 0.6f);
        
        Vector2 legRPos = Position + new Vector2((_facingRight ? 1 : -1) * LegOffsetR.X, LegOffsetR.Y);
        spriteBatch.Draw(_legTex, legRPos, null, Color.White, legRotR, legOrigin, 1f,
            flip, 0.6f);
    }
}