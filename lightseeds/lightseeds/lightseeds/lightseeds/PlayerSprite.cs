using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace lightseeds
{
    public enum Direction
    {
        LEFT, RIGHT, NONE
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
        public float wobbleSpeed = 1.0f;
        public float wobbleHeight = 2.0f;
        public float XVelocity = 0.0f;
        public float MAXVELOCITY = 0.3f;
        public float ACCELERATION = 1.0f;
        public Direction currentDirection = Direction.NONE;

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
            this.texture = tex;
            this.position = new Vector3(0.0f, 5.0f, 1.0f);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            if (XVelocity < MAXVELOCITY && Direction.RIGHT == currentDirection)
                XVelocity += ACCELERATION * timeFactor;
            if (XVelocity > -MAXVELOCITY && Direction.LEFT == currentDirection)
                XVelocity -= ACCELERATION * timeFactor;
            if (Direction.NONE == currentDirection && XVelocity > 0)
                XVelocity -= ACCELERATION * timeFactor;
            if (Direction.NONE == currentDirection && XVelocity < 0)
                XVelocity += ACCELERATION * timeFactor;
            
            position.X += XVelocity;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.spriteBatch.Draw(texture, new Vector2(screenPosition.X, getWobblyPosition(gameTime)), Color.White);
            base.Draw(gameTime);
        }

        private float getWobblyPosition(GameTime gameTime)
        {
            var sin = wobbleHeight * (float)Math.Sin((gameTime.TotalGameTime.TotalMilliseconds % (360.0f * 95.0f * wobbleSpeed)) / (95.0f * wobbleSpeed));
            var cos = wobbleHeight * (float)Math.Cos((gameTime.TotalGameTime.TotalMilliseconds%(360.0f*120.0f*wobbleSpeed))/(120.0f*wobbleSpeed));
            return position.Y + cos + sin;
        }

        public void move(Direction d)
        {
            currentDirection = d;
        }
        
    }
}
