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
        BASE = 0,
        FIGHTER,
        MOTHER,
        PAWN,
        TANK
    };

    public class TreeCollection
    {
        public List<Tree> trees = new List<Tree>();

        public Random randomizer;

        public Game1 game;

        public ContentManager content;

        public Texture2D[] textures;
        public Texture2D fruitTexture;

        public TreeCollection(Game1 game)
        {
            this.game = game;
        }

        public void Load()
        {
            content = GameServices.GetService<ContentManager>();
            textures = new Texture2D[4*3 + 1];
            textures[0] = content.Load<Texture2D>("Tree Previews/Treeoflife_Small");
            textures[1] = content.Load<Texture2D>("Trees/Fighter Tree/Fighter_Tree_tex");
            textures[2] = content.Load<Texture2D>("Trees/Mother Tree/Mother_tree_tex");
            textures[3] = content.Load<Texture2D>("Trees/Pawn Tree/Pawn_Tree_tex");
            textures[4] = content.Load<Texture2D>("Trees/Tank Tree/Tank_Tree_tex");
            textures[5] = content.Load<Texture2D>("Trees/Fighter Tree/Fighter_Tree_noleafs");
            textures[6] = content.Load<Texture2D>("Trees/Mother Tree/Mother_tree_noleafs");
            textures[7] = content.Load<Texture2D>("Trees/Pawn Tree/Pawn_Tree_noleafs");
            textures[8] = content.Load<Texture2D>("Trees/Tank Tree/Tank_Tree_noleafs");
            textures[9] = content.Load<Texture2D>("Trees/Fighter Tree/Fighter_Tree_xray");
            textures[10] = content.Load<Texture2D>("Trees/Mother Tree/Mother_tree_xray");
            textures[11] = content.Load<Texture2D>("Trees/Pawn Tree/Pawn_Tree_xray");
            textures[12] = content.Load<Texture2D>("Trees/Tank Tree/Tank_Tree_xray");
            fruitTexture = content.Load<Texture2D>("textures/glowing");

            randomizer = new Random();
        }

        public void Reset()
        {
            // remove all trees
            trees.Clear();
            // create base tree
            CreateTree(Vector3.Zero, TreeType.BASE, false, "");
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

        internal Tree CreateTree(Vector3 seedPos, TreeType type, bool isBlueprint, String name)
        {
            Tree newTree = new Tree(this, seedPos, type, isBlueprint, name);
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

        public Tree FindTreeAtPosition(float posX)
        {
            foreach (Tree tree in trees)
            {
                if (tree.OccupiesPosition(posX))
                    return tree;
            }
            return null;
        }
    }
}
