using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds
{
    class GameOverGameScreen : GameScreen
    {
        private int totalTime;
        public GameOverGameScreen(Game1 game, Texture2D tex, float duration)
            : base(game, tex, duration)
        {
            
        }

        public void Reset(GameTime gameTime)
        {
            base.Reset();
            totalTime = (int)(gameTime.TotalGameTime.TotalSeconds - game.startTime);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);

            float c = fadeTime / GameScreen.MAX_FADE_TIME;
            //Color color = new Color(c, c, c);
            game.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (var p in effect.CurrentTechnique.Passes)
            {
                p.Apply();
                //game.spriteBatch.Draw(screenTexture, new Rectangle(0, 0, Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT), color);

                var msg = "All Life has been consumed and\nthe Tree of Light is gone.\nYou helped it to resist for\n\n" + totalTime + " years (tree seconds).\n\n";
                var msgMeasure = game.spriteFont.MeasureString(msg);

                var contMsg = "Press Start to Reboot";
                var contMsgMeasure = game.spriteFont.MeasureString(contMsg);

                game.spriteBatch.DrawString(game.spriteFont, msg, new Vector2(Game1.SCREEN_WIDTH / 2 - msgMeasure.X / 2, 100), Color.White);

                game.spriteBatch.DrawString(game.spriteFont, contMsg, new Vector2(Game1.SCREEN_WIDTH / 2 - contMsgMeasure.X / 2, Game1.SCREEN_HEIGHT - 100), Color.White);
                //game.spriteBatch.Draw(screenTexture, new Rectangle(0, 0, Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT), Color.White);
                game.spriteBatch.End();
            }
            base.Update(gameTime);
        }
    }
}
