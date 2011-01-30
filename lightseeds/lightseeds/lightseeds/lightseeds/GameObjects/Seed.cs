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
        public Vector2 offset;
        public bool RemoveOnNextUpdate = false;
        public bool collected = false;
        private SeedCollection parentCollection;


        public Seed(SeedCollection collection)
        {
            parentCollection = collection;
            offset = new Vector2(-0.5f * parentCollection.texture.Width, -0.5f * parentCollection.texture.Height);
        }


        public void Update()
        {
            if (collected)
            {
                return;
            }

            foreach (var player in parentCollection.game.players)
            {
                float distance = (player.worldPosition - position).Length();
                if (distance < 1.5f)
                {
                  Random r = new Random();
                  switch(r.Next(1, 6))
                  {
                    case 1:
                      GameServices.GetService<MusicManager>().Play("PickUpOne");
                      break;
                    case 2:
                      GameServices.GetService<MusicManager>().Play("PickUpTwo");
                      break;
                    case 3:
                      GameServices.GetService<MusicManager>().Play("PickUpThree");
                      break;
                    case 4:
                      GameServices.GetService<MusicManager>().Play("PickUpFour");
                      break;
                    case 5:
                      GameServices.GetService<MusicManager>().Play("PickUpFive");
                      break;
                    case 6:
                      GameServices.GetService<MusicManager>().Play("PickUpSix");
                      break;
                    }
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
            spriteBatch.Draw(parentCollection.texture, (Vector3.Transform(position, parentCollection.game.worldToScreen)).ToVector2() + offset, Color.White);
        }
    }
}
