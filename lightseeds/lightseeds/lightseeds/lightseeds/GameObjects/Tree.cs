using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds.GameObjects
{
    /*
    class Branch
    {
        Matrix fatherTransform;

        Vector3 localPosition;

        Vector2 pivot;

        float length, orientation, maxRotation;

        float timer;

        Texture2D texture;

        Branch[] branches;

        public Branch(Texture2D texture, Vector2 position, Vector2 size, float orientation)
        {
            this.texture = texture;
            this.localPosition = position;
            this.length = 1.0f;
            this.orientation = orientation;
            this.maxRotation = MathHelper.ToRadians(30.0f);
            this.pivot = new Vector2(0.5f, 1.0f);
            this.timer = 0.0f;
            this.branches = new Branch[0];
        }

        public void AddBranches(Branch[] b)
        {
            branches = b;
        }

        public void Update(GameTime gameTime, Vector2 position)
        {
            foreach (Branch b in branches)
            {
                b.Update(, gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector3 pos = Vector3.Transform(localPosition, fatherTransform);
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), null, Color.White, orientation, pivot, SpriteEffects.None, 1.0f);
        }
    }
    */
    public class Tree
    {
        public Vector3 position;

        public TreeCollection parentCollection;


        #region gameplay properties
        public float growthTime;
        public float growthAmount = 0.0f;
        public float lifeSpan;
        public float resistance;
        //public float fruitsPerMinute;
        public float fruitTime = 2;
        public int price;
        public string name;

        public double currentFruitTime;
        #endregion


        public bool RemoveOnNextUpdate;

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
                Vector3 screenPos = Vector3.Transform(position, parentCollection.game.worldToScreen);
                return new Vector2(screenPos.X, screenPos.Y);
            }
        }

        public Tree(TreeCollection coll, Vector3 position)
        {
            this.parentCollection = coll;
            this.position = position;
        }

        public void Update(GameTime gameTime)
        {
            currentFruitTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (currentFruitTime > this.fruitTime)
            {
                this.parentCollection.game.seedCollection.SpawnSeed(worldPosition + new Vector3(0,2,0));
                currentFruitTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(parentCollection.texture, screenPosition - new Vector2(0.0f, parentCollection.texture.Height), Color.White);
        }
    }
}
