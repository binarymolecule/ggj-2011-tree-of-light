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
        LEFT, RIGHT
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

        public float XVelocity = 0.0f;
        public float MAXVELOCITY = 0.1f;
        public float ACCELERATION = 0.01f;

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
            this.position.X += XVelocity;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.spriteBatch.Draw(texture, this.screenPosition, Color.White);

            base.Draw(gameTime);
        }

        public void move(Direction d)
        {

            if (XVelocity < MAXVELOCITY && Direction.RIGHT == d)
                XVelocity += ACCELERATION;
            if (XVelocity > -MAXVELOCITY && Direction.LEFT == d)
                XVelocity -= ACCELERATION;
        }
        public void stop()
        {
            if (XVelocity > 0)
                XVelocity -= ACCELERATION;
            else if (XVelocity < 0)
                XVelocity += ACCELERATION;
        }
    }
}
