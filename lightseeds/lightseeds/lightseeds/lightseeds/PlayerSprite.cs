using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds
{
    public enum Direction
    {
        Left, Right, None
    }
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PlayerSprite : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;

        int index;
        
        Vector3 position;

        Texture2D texture;
        public const float WOBBLEBPM = 60.0f;
        public const float WOBBLYNESS = 3.0f;
        public const float MAXVELOCITY = 25.0f;
        public const float ACCELERATION = 100.0f;
        
        public Direction currentDirection = Direction.None;
        public float xAcceleration = 0.0f;
        public float xVelocity = 0.0f;
        public float wobbleHeight = 1.0f;
        
        public Vector3 worldPosition
        {
            get
            {
                return position;
            }
        }


        public Vector2 screenPosition
        {
            get
            {
                Vector3 screenPos = Vector3.Transform(position, game.worldToScreen);
                return new Vector2(screenPos.X, screenPos.Y);
            }
        }
        
        public PlayerSprite(Game game, int index, Texture2D tex) : base(game)
        {
            this.game = (Game1)game;
            this.index = index;
            texture = tex;
            position = new Vector3(0.0f, 8.0f, 1.0f);            
        }

        public override void Update(GameTime gameTime)
        {
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            if (xVelocity < MAXVELOCITY && Direction.Right == currentDirection)
                xVelocity += xAcceleration * timeFactor;
            if (xVelocity > -MAXVELOCITY && Direction.Left == currentDirection)
                xVelocity -= xAcceleration * timeFactor;
            if (Direction.None == currentDirection)
            {
                if (xVelocity > 0)
                    xVelocity -= ACCELERATION * timeFactor;
                if (xVelocity < 0)
                    xVelocity += ACCELERATION * timeFactor;
                if (Math.Abs(xVelocity) <= ACCELERATION * timeFactor)
                    xVelocity = 0;
            }   
            
            position.X += xVelocity * timeFactor;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.spriteBatch.Draw(texture, new Vector2(screenPosition.X, WobblyPosition(screenPosition.Y, gameTime)), Color.White);
            base.Draw(gameTime);
        }

        private float WobblyPosition(float pos, GameTime gameTime)
        {
            var sin1 = wobbleHeight * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 60);
            var sin2 = 0.7f * wobbleHeight * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 97);
            var sin3 = 0.2f * wobbleHeight * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 23);
            return pos + sin1 + sin2 + sin3 ;
        }

        public void Move(Direction d, float strength)
        {
            currentDirection = d;
            xAcceleration = ACCELERATION*strength;
        }
    }
}
