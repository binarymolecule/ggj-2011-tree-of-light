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
using lightseeds.GameObjects;


namespace lightseeds
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MapPanel : Microsoft.Xna.Framework.DrawableGameComponent
    {
        
        Texture2D barTexture, darkTexture, iconTexture;

        Rectangle leftDarkRect, rightDarkRect;

        Vector2 iconOffset, barOffset;
        
        public Vector2 edgePosition;

        int iconSize;

        Game1 game;

        public MapPanel(Game1 game, Texture2D barTex, Texture2D darkTex, Texture2D iconTex) : base(game)
        {
            this.game = game;
            barTexture = barTex;
            darkTexture = darkTex;
            iconTexture = iconTex;
            iconSize = iconTexture.Height;
            iconOffset = new Vector2(-iconSize / 2, iconSize / 2);
            barOffset = new Vector2(0.0f, -barTexture.Height / 2);
            edgePosition = game.splitScreenPositions[1];
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

        int worldToBarCoordinate(float posX)
        {
            return (int)(((posX / (float)World.WorldWidth) + 0.5f) * Game1.SCREEN_WIDTH);
        }

        Rectangle worldToIconRectangle(float posX)
        {
            int barX = worldToBarCoordinate(posX);
            return new Rectangle(barX - iconSize / 2, (int)edgePosition.Y - iconSize / 2, iconSize, iconSize);
        }

        Rectangle iconRectangle(int index)
        {
            return new Rectangle(index * iconSize, 0, iconSize, iconSize);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float leftWidth = -World.WorldWidth, rightWidth = World.WorldWidth;
            foreach (var v in game.voids)
            {
                if (v.direction.X > 0 && v.horizontalPosition > leftWidth)
                    leftWidth = v.horizontalPosition;
                else if (v.horizontalPosition < rightWidth)
                    rightWidth = v.horizontalPosition;
            }
            leftWidth = worldToBarCoordinate(leftWidth);
            rightWidth = worldToBarCoordinate(rightWidth);
            leftDarkRect = new Rectangle(0, (int)edgePosition.Y - 9, (int)leftWidth, 16);
            rightDarkRect = new Rectangle((int)rightWidth, (int)edgePosition.Y - 9,
                                          Game1.SPLIT_SCREEN_WIDTH - (int)rightWidth, 16);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.spriteBatch.Draw(barTexture, edgePosition + barOffset, Color.White);

            // draw dark areas
            game.spriteBatch.Draw(darkTexture, leftDarkRect, Color.White);
            game.spriteBatch.Draw(darkTexture, rightDarkRect, Color.White);
            
            // draw players and stuff
            foreach (var tree in game.treeCollection.trees)
            {
                if (tree.status != Tree.TreeStatus.BLUEPRINT)
                {
                    int i = (tree.lifeSpan < 10.0f) ? 3 : 1;
                    if (tree.status == Tree.TreeStatus.DIED || tree.status == Tree.TreeStatus.KILLED) i = 2;
                    if (tree.treeType == TreeType.BASE) i = 5;
                    game.spriteBatch.Draw(iconTexture, worldToIconRectangle(tree.worldPosition.X), iconRectangle(i), Color.White);
                }
            }
            foreach (var seed in game.seedCollection.seeds)
            {
                game.spriteBatch.Draw(iconTexture, worldToIconRectangle(seed.position.X), iconRectangle(0), Color.White);
            }
            game.spriteBatch.Draw(iconTexture, worldToIconRectangle(game.players[0].worldPosition.X), iconRectangle(4), game.players[0].color);
            game.spriteBatch.Draw(iconTexture, worldToIconRectangle(game.players[1].worldPosition.X), iconRectangle(4), game.players[1].color);

            base.Draw(gameTime);
        }
    }
}
