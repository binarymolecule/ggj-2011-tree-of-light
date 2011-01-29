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
    public enum TreeType
    {
        BASE,
        FIGHTER,
        MOTHER,
        PAWN,
        TANK,
    };

    public class TreeCollection
    {
        public List<Tree> trees = new List<Tree>();

        public Game1 game;

        public ContentManager content;

        public Texture2D texture;
        public Texture2D fruitTexture;

        public TreeCollection(Game1 game)
        {
            this.game = game;
        }

        public void Load()
        {
            content = GameServices.GetService<ContentManager>();
            texture = content.Load<Texture2D>("textures/treeDummy");
            fruitTexture = content.Load<Texture2D>("textures/glowing");
        }

        public void Update(GameTime gameTime)
        {
            List<Tree> treesToRemove = new List<Tree>();

            foreach(var tree in trees)
            {
                tree.Update(gameTime);

                if (tree.RemoveOnNextUpdate)
                {
                    treesToRemove.Add(tree);
                }
            }

            foreach (var tree in treesToRemove)
            {
                trees.Remove(tree);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var tree in trees)
            {
                tree.Draw(sb);
            }
        }

        internal Tree CreateTree(Vector3 seedPos, TreeType type, bool isBlueprint)
        {
            Tree newTree = new Tree(this, seedPos, type, isBlueprint);
            trees.Add(newTree);
            return newTree;
        }

        public bool HasTreeAtPosition(float posX)
        {
            foreach (Tree tree in trees)
            {
                if (tree.OccupiesPosition(posX))
                    return true;
            }
            return false;
        }
    }
}
