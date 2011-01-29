using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds
{
    
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PlayerSprite : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;

        public int index;
        
        Vector3 position;

        Texture2D texture;
        public const float XBOUNDARY = 180.0f;
        public const float YBOUNDARY = 50.0f;

        public const float WOBBLEBPM = 60.0f;
        public const float WOBBLYNESS = 3.0f;
        public const float MAXVELOCITY_X = 25.0f;
        public const float MAXVELOCITY_Y = 10.0f;
        public const float ACCELERATION = 100.0f;
        public float wobbleHeight = 2.0f;
        
        public float currentXAcceleration = 0.0f;
        public float currentYAcceleration = 0.0f;
        public float xVelocity = 0.0f;
        public float yVelocity = 0.0f;

        Random random = new Random();

        public Color color;

        public int collectedSeeds;

        public float scale = 1f;
        public float currentScale = 1f;
        public float scaleTransition = 1f;

        public Vector2 offset;
        private float oldScale = 1f;

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
            offset = new Vector2(-0.5f * texture.Width, -0.5f * texture.Height);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateXPosition(gameTime);
            UpdateYPosition(gameTime);

            scaleTransition += (float)gameTime.ElapsedGameTime.TotalSeconds * 6f;

            if(scaleTransition >= 1) {
                oldScale = scale;
                scale = 0.8f + (float)random.NextDouble() * 0.2f;
                scaleTransition = 0;
            }

            currentScale = oldScale + scaleTransition * (scale - oldScale);

            base.Update(gameTime);
        }

        public void Respawn()
        {
            this.position.X = 0;
        }

        private void UpdateXPosition(GameTime gameTime)
        {
            
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            //movement
            if (position.X < XBOUNDARY && position.X > -XBOUNDARY)
            {
                xVelocity += currentXAcceleration * timeFactor;
                if (xVelocity > MAXVELOCITY_X)
                    xVelocity = MAXVELOCITY_X;
                if (xVelocity < -MAXVELOCITY_X)
                    xVelocity = -MAXVELOCITY_X;
            }
            
            //backbounce
            if (position.X < -XBOUNDARY)
                xVelocity += 2 * ACCELERATION * timeFactor;
            if (position.X > XBOUNDARY)
                xVelocity -= 2 * ACCELERATION * timeFactor;
            
            //slowdown
            if (currentXAcceleration == 0)
            {
                if (xVelocity > 0)
                    xVelocity -= ACCELERATION * timeFactor;
                if (xVelocity < 0)
                    xVelocity += ACCELERATION * timeFactor;
                if (Math.Abs(xVelocity) <= ACCELERATION * timeFactor)
                    xVelocity = 0;
            }
            
            position.X += xVelocity * timeFactor;
            
        }
        public void UpdateYPosition(GameTime gameTime)
        {
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            //movement
            if (position.Y < YBOUNDARY)
            {
                yVelocity += currentYAcceleration * timeFactor;
                if (yVelocity > MAXVELOCITY_Y)
                    yVelocity = MAXVELOCITY_Y;
                if (yVelocity < -MAXVELOCITY_Y)
                    yVelocity = -MAXVELOCITY_Y;
            }

            //backbounce
            if (position.Y > YBOUNDARY)
                yVelocity -= 2 * ACCELERATION * timeFactor;
            float diff = game.world.getHeigth(position.X) + 3.0f - position.Y;
            if (diff > 0 && yVelocity < MAXVELOCITY_Y / 2)
                yVelocity += 4 * diff * ACCELERATION * timeFactor;


            //slowdown
            if (currentYAcceleration == 0)
            {
                if (yVelocity > 0)
                    yVelocity -= ACCELERATION * timeFactor;
            }

            position.Y += yVelocity * timeFactor;
            
        }

        public override void Draw(GameTime gameTime)
        {
            var x = screenPosition.X + 2* (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 120);
            var y = WobblyPosition(screenPosition.Y, wobbleHeight, gameTime);
            game.spriteBatch.Draw(texture, new Vector2(x,y) + offset, null, color, 0, Vector2.Zero, currentScale, SpriteEffects.None, 0);
            base.Draw(gameTime);
        }

        private float WobblyPosition(float pos, float modifier, GameTime gameTime)
        {
            var sin1 = modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 60 + index * Math.PI);
            var sin2 = 0.7f * modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 97);
            var sin3 = 0.3f * modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 23);
            return pos + sin1 + sin2 + sin3 ;
        }

        public void Move(float x, float y)
        {
            currentXAcceleration = x * ACCELERATION;
            currentYAcceleration = y * ACCELERATION;
        }
    }
}
