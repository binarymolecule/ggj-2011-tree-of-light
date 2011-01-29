using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using lightseeds.Helpers;
using Microsoft.Xna.Framework.Content;

namespace lightseeds.GameObjects
{
    public class Seed
    {
        public Vector3 position;
        public bool RemoveOnNextUpdate = false;
        public bool collected = false;
        private SeedCollection parentCollection;

        public Seed(SeedCollection collection)
        {
            parentCollection = collection;
        }


        public void Update()
        {
            if (collected)
            {
                return;
            }

            foreach (var player in parentCollection.game.players)
            {
                var distance = (player.worldPosition - position).Length();
                distance = Math.Abs(player.worldPosition.X - position.X);
                if (distance < 0.2f)
                {
                    RemoveOnNextUpdate = true;
                    collected = true;
                    player.collectedSeeds += 1;
                    parentCollection.collectedSeedCount += 1;
                    return;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(parentCollection.texture, (Vector3.Transform(position, parentCollection.game.worldToScreen)).ToVector2(), Color.White);
        }
    }
}
