using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using lightseeds.Helpers;

namespace lightseeds.GameObjects
{
    class ParallaxCollection
    {
        private Game1 game;
        private List<Texture2D> textures = new List<Texture2D>();
        private List<ParallaxItem> items = new List<ParallaxItem>();

        public ParallaxCollection(Game1 game)
        {
            this.game = game;
            textures.Add(game.Content.Load<Texture2D>("forground/brunsch1_blur"));
            textures.Add(game.Content.Load<Texture2D>("forground/brunsch2_blur"));
            textures.Add(game.Content.Load<Texture2D>("forground/brunsch3_blur"));

            Random random = new Random();

            for(int i=0;i<50;i++) {
                var x = -250 + random.Next(500);
                items.Add(new ParallaxItem()
                {
                    texture = textures[random.Next(3)],
                    position = new Vector3(x,8,0)
                });
            }
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                   GameCamera.CurrentCamera.screenTransform * game.bgMatrix(0, 25f));

            foreach (var item in items)
            {
                spriteBatch.Draw(item.texture, Vector3.Transform(item.position, game.worldToScreen).ToVector2(), Color.White);
            }
            

            spriteBatch.End();
        }
    }
}
