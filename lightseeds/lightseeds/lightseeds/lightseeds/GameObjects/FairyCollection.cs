using System;
using System.Collections.Generic;
using lightseeds.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds.GameObjects
{
    public class FairyCollection
    {
        private Game1 game;
        public Texture2D texture;
        public ContentManager content;
        private Random random = new Random();
        public List<PlayerSprite> fairies = new List<PlayerSprite>();

        public FairyCollection(Game1 game)
        {
            this.game = game;
        }

        public void Load(Texture2D t)
        {
            texture = t;
        }
        public void Update(GameTime time)
        {
            foreach (var fairy in fairies)
            {

                var sx = random.NextDouble() - 0.5f;
                var sy = random.NextDouble() - 0.5f;
                Vector2 movingVector = new Vector2((float)sx, (float)sy);
                
                if (fairy.xVelocity > PlayerSprite.MAXVELOCITY_X / 2)
                    movingVector.X = -PlayerSprite.MAXVELOCITY_X / 4;
                if (fairy.xVelocity < -PlayerSprite.MAXVELOCITY_X / 2)
                    movingVector.X = PlayerSprite.MAXVELOCITY_X / 4;
                if (fairy.yVelocity < -PlayerSprite.MAXVELOCITY_Y_DOWN / 4)
                    movingVector.Y = PlayerSprite.MAXVELOCITY_Y / 20;
                if (fairy.position.X > (World.WorldWidth/2) / 4)
                    movingVector.X -= PlayerSprite.MAXVELOCITY_X / 20;
                if (fairy.position.X < -(World.WorldWidth / 2) / 4)
                    movingVector.X += PlayerSprite.MAXVELOCITY_X / 20;
                
                fairy.Move(movingVector.X, movingVector.Y);

                fairy.Update(time);
            }
        }

        public void Draw(GameTime time)
        {
            foreach (var fairy in fairies)
            {
                fairy.DrawFairy();
            }
        }

        internal PlayerSprite SpawnFairy(Vector3 v)
        {
            var fairy = new PlayerSprite(game, v, texture);
            fairies.Add(fairy);
            return fairy;
        }

        public void Reset()
        {
            fairies.Clear();
            const int NUM_FAIRIES = 40;
            for (int i = NUM_FAIRIES - 1; i >= 0; i--)
            {
                float sx = random.Next(30);
                float sy = random.Next(30);
                SpawnFairy(new Vector3(sx - 15.0f, sy + 5.0f, 1.0f));
            }
        }
    }
}