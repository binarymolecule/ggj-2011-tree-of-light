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
using lightseeds.Helpers;

namespace lightseeds
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private World world;

        public Matrix worldToScreen;
        Matrix[] screenToGlobal;
        PlayerSprite[] players;
        GameCamera[] cameras;

        Texture2D playerTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            
            float screenWidth = 1024.0f;
            float screenHeight = 768.0f;
            float screenWidthWorld = 24.0f;
            float screenHeightWorld = 9.0f;

            worldToScreen = Matrix.CreateScale(screenWidth / screenWidthWorld, -0.5f * screenHeight / screenHeightWorld, 1.0f) *
                               Matrix.CreateTranslation(0.5f * screenWidth, 0.5f * screenHeight, 0.0f);
            screenToGlobal = new Matrix[2];
            screenToGlobal[0] = Matrix.Identity;
            screenToGlobal[1] = Matrix.CreateTranslation(0.0f, 0.5f * screenHeight, 0.0f);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            GameServices.AddService(GraphicsDevice);
            GameServices.AddService(Content);

            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            world = new World();
            world.Load();
       
            playerTexture = Content.Load<Texture2D>("playerTexture");

            players = new PlayerSprite[2];
            players[0] = new PlayerSprite(this, 0, playerTexture);
            players[1] = new PlayerSprite(this, 1, playerTexture);

            cameras = new GameCamera[2];
            cameras[0] = new GameCamera(this, 0, players[0].worldPosition);
            cameras[0].FollowPlayer(players[0]);
            cameras[1] = new GameCamera(this, 1, players[1].worldPosition);
            cameras[1].FollowPlayer(players[1]);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            handleControls();
            
            // Update players
            foreach (PlayerSprite p in players)
                p.Update(gameTime);

            // Update cameras
            foreach (GameCamera c in cameras)
                c.Update(gameTime);

            world.Update(gameTime);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw screen of player 1
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null,
                              screenToGlobal[0] * cameras[0].screenTransform);
            players[0].Draw(gameTime);
            world.Draw(spriteBatch, gameTime, -1000, 2000);
            spriteBatch.End();
       
            // Draw screen of player 2
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null,
                              screenToGlobal[1] * cameras[1].screenTransform);
            players[1].Draw(gameTime);
            world.Draw(spriteBatch, gameTime, -1000, 2000);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void handleControls()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
         
                this.Exit();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                players[0].move(Direction.LEFT);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                players[0].move(Direction.RIGHT);
            if (!Keyboard.GetState().IsKeyDown(Keys.Right) && !Keyboard.GetState().IsKeyDown(Keys.Left))
                players[0].stop();
            
        }
    }
}
