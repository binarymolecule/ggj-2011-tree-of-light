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

        public List<PlayerSprite> fairies = new List<PlayerSprite>();

        public FairyCollection(Game1 game)
        {
            this.game = game;
            PlayerSprite sprite = new PlayerSprite(game, new Vector3(0, 0, 0), game.playerTexture);
        }

        public void Load(Texture2D t)
        {
            content = GameServices.GetService<ContentManager>();
            this.texture = t;
        }
        public void Update(GameTime time)
        {
            foreach (var fairy in fairies)
            {
                fairy.Move(0,0.1f);
                fairy.Update(time);
            }
        }

        public void Draw(SpriteBatch sb, GameTime time)
        {
            
            foreach (var fairy in fairies)
            {
                fairy.Draw(time);
            }
        }

        internal void SpawnFairy(Vector3 v)
        {
            fairies.Add(new PlayerSprite(game, v, texture));
        }
    }
}