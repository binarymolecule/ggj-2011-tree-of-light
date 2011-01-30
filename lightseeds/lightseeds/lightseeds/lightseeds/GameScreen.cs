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
using lightseeds.Helpers;

namespace lightseeds
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GameScreen : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D screenTexture;

        Effect effect;

        public enum ScreenStatus { WAIT, FADE_IN, FADE_OUT, ACTIVE, DONE };

        ScreenStatus screenStatus;

        float fadeTime, waitTime, duration;

        const float MAX_FADE_TIME = 1.0f, MAX_WAIT_TIME = 1.0f;

        Game1 game;

        public ScreenStatus Status { get { return screenStatus; } }

        public GameScreen(Game1 game, Texture2D tex, float duration) : base(game)
        {
            this.game = game;
            screenTexture = tex;
            this.duration = duration;
            var content = GameServices.GetService<ContentManager>();
            effect = content.Load<Effect>("effects/titleEffect");
            Reset();
        }

        public void Reset()
        {
            screenStatus = ScreenStatus.WAIT;
            fadeTime = 0.0f;
            waitTime = 0.0f;
            effect.Parameters["fade"].SetValue(0.0f);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (screenStatus == ScreenStatus.WAIT)
            {
                waitTime += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (waitTime > MAX_WAIT_TIME)
                {
                    waitTime = MAX_WAIT_TIME;
                    screenStatus = ScreenStatus.FADE_IN;
                }
            }
            else if (screenStatus == ScreenStatus.FADE_IN)
            {
                fadeTime += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (fadeTime > MAX_FADE_TIME)
                {
                    fadeTime = MAX_FADE_TIME;
                    screenStatus = ScreenStatus.ACTIVE;
                    waitTime = duration;
                }
                effect.Parameters["fade"].SetValue(fadeTime / MAX_FADE_TIME);
            }
            else if (screenStatus == ScreenStatus.FADE_OUT)
            {
                fadeTime -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (fadeTime < 0.0f)
                {
                    fadeTime = 0.0f;
                    screenStatus = ScreenStatus.DONE;
                }
                effect.Parameters["fade"].SetValue(fadeTime / MAX_FADE_TIME);
            }
            else if (screenStatus == ScreenStatus.ACTIVE)
            {
                GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyboardState = Keyboard.GetState();
                if (gamepadState.IsButtonDown(Buttons.Start) ||
                    gamepadState.IsButtonDown(Buttons.A) ||
                    gamepadState.IsButtonDown(Buttons.B) ||
                    keyboardState.IsKeyDown(Keys.Enter))
                {
                    screenStatus = ScreenStatus.FADE_OUT;
                }
                else if (gamepadState.Buttons.Back == ButtonState.Pressed ||
                         keyboardState.IsKeyDown(Keys.Escape))
                {
                    game.Exit();
                }
                // automatic fade out after given timer (< 0: wait for user input)
                if (waitTime > 0)
                {
                    waitTime -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                    if (waitTime < 0)
                    {
                        waitTime = 0.0f;
                        screenStatus = ScreenStatus.FADE_OUT;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);

            float c = fadeTime / MAX_FADE_TIME;
            //Color color = new Color(c, c, c);
            game.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (var p in effect.CurrentTechnique.Passes)
            {
                p.Apply();
                //game.spriteBatch.Draw(screenTexture, new Rectangle(0, 0, Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT), color);
                game.spriteBatch.Draw(screenTexture, new Rectangle(0, 0, Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT), Color.White);
                game.spriteBatch.End();
            }   
            base.Update(gameTime);
        }
    }
}
