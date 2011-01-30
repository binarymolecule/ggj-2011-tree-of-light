using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using lightseeds.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace lightseeds.GameObjects
{
    public class SeedCollection
    {
        public List<Seed> seeds = new List<Seed>();
        public int collectedSeedCount;
        public Game1 game;
        public Texture2D texture;
        public ContentManager content;

        public void Load()
        {
            content = GameServices.GetService<ContentManager>();
            texture = content.Load<Texture2D>("textures/glowing");
        }

        public SeedCollection(Game1 game)
        {
            this.game = game;
        }


        public void Reset()
        {
            // remove all seeds
            seeds.Clear();
            // clear score
            collectedSeedCount = 0;            
        }

        public void Update()
        {
            List<Seed> seedsToRemove = new List<Seed>();

            foreach(var seed in seeds)
            {
                seed.Update();

                if (seed.RemoveOnNextUpdate)
                {
                    seedsToRemove.Add(seed);
                }
            }

            foreach (var seed in seedsToRemove)
            {
                seeds.Remove(seed);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var seed in seeds)
            {
                seed.Draw(sb);
            }
        }

        internal void SpawnSeed(Vector3 v)
        {
            seeds.Add(new Seed(this)
            {
                position = v
            });
        }
    }
}
