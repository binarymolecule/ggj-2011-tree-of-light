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
        public World world;

        public Matrix worldToScreen;
        public PlayerSprite[] players;
        public GameCamera[] cameras;

        Texture2D playerTexture;
        private SpriteFont spriteFont;

        RenderTarget2D[] splitScreens;
        public Vector2[] splitScreenPositions;

        public TreeCollection treeCollection;
        public SeedCollection seedCollection;

        public List<TheVoid> voids = new List<TheVoid>();

        static public int SCREEN_WIDTH = 1024;
        static public int SCREEN_HEIGHT = 768;
        static public int SPLIT_SCREEN_WIDTH = 1024;
        static public int SPLIT_SCREEN_HEIGHT = 384;
        static public int WORLD_SCREEN_WIDTH = 24;
        static public int WORLD_SCREEN_HEIGHT = 9;
        private bool waitForRelease;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;

            worldToScreen = Matrix.CreateScale((float)SPLIT_SCREEN_WIDTH / (float)WORLD_SCREEN_WIDTH,
                                               -(float)SPLIT_SCREEN_HEIGHT / (float)WORLD_SCREEN_HEIGHT, 1.0f) *
                            Matrix.CreateTranslation(0.5f * (float)SPLIT_SCREEN_WIDTH, 0.5f * (float)SPLIT_SCREEN_HEIGHT, 0.0f);
            splitScreenPositions = new Vector2[2];
            splitScreenPositions[0] = Vector2.Zero;
            splitScreenPositions[1] = new Vector2(0.0f, (float)SPLIT_SCREEN_HEIGHT);
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

            playerTexture = Content.Load<Texture2D>("textures/playerTexture");

            players = new PlayerSprite[2];
            players[0] = new PlayerSprite(this, 0, playerTexture);
            players[1] = new PlayerSprite(this, 1, playerTexture);

            cameras = new GameCamera[2];
            cameras[0] = new GameCamera(this, 0, players[0].worldPosition);
            cameras[0].FollowPlayer(players[0]);
            cameras[1] = new GameCamera(this, 1, players[1].worldPosition);
            cameras[1].FollowPlayer(players[1]);

            splitScreens = new RenderTarget2D[2];
            splitScreens[0] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);
            splitScreens[1] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);

            spriteFont = Content.Load<SpriteFont>("fonts/Geo");

            treeCollection = new TreeCollection(this);
            treeCollection.Load();

            seedCollection = new SeedCollection(this);
            seedCollection.Load();
            seedCollection.SpawnSeed(new Vector3(4, 6, 0));

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(1, 0, 0),
                horizontalPosition = -World.WorldWidth/20
            });

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(-1, 0, 0),
                horizontalPosition = World.WorldWidth / 20
            });
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

            treeCollection.Update(gameTime);

            seedCollection.Update();

            world.Update(gameTime);
            
            voids.ForEach((v) => v.Update(gameTime));

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(40,40,40));
            
            // Draw worlds
            for (int i = 0; i < 2; i++)
            {
                GraphicsDevice.SetRenderTarget(splitScreens[i]);
                if (i == 0)
                    GraphicsDevice.Clear(new Color(40, 40, 40));
                else
                    GraphicsDevice.Clear(new Color(40, 40, 80));
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null,
                                  cameras[i].screenTransform);
                treeCollection.Draw(spriteBatch);
                seedCollection.Draw(spriteBatch);
                players[i].Draw(gameTime);
                spriteBatch.End();
                GameCamera.CurrentCamera = cameras[i];
                world.Draw(this, gameTime, -1000, 2000);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null,
                                  cameras[i].screenTransform);
                voids.ForEach((v) => v.Draw(spriteBatch));
                spriteBatch.End();
            }
            GraphicsDevice.SetRenderTarget(null);

            // Draw split screens
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            for (int i = 0; i < 2; i++)
                spriteBatch.Draw(splitScreens[i], splitScreenPositions[i], Color.White);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, String.Format("P1: {0:0.0} / {1:0.0}", players[0].worldPosition.X, players[0].worldPosition.Y), Vector2.Zero, Color.White);
            spriteBatch.DrawString(spriteFont, String.Format("Seeds: {0:0}", seedCollection.collectedSeedCount), new Vector2(0, 20), Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void handleControls()
        {
            var gamepadState = GamePad.GetState(PlayerIndex.One);
            var keyboardState = Keyboard.GetState();
            var stick = gamepadState.ThumbSticks.Left;

            if (keyboardState.IsKeyDown(Keys.Escape) ||
                gamepadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // controls for player 1
            players[0].Move(stick.X, stick.Y);

            // buttons
            if (gamepadState.IsButtonDown(Buttons.X) && !waitForRelease)
            {
                createTree(players[0]);
                waitForRelease = true;
            }

            if (gamepadState.IsButtonUp(Buttons.X))
            {
                waitForRelease = false;
            }             
        }

        public void createTree(PlayerSprite player)
        {
            // check and decrease seeds of players here
            float posX = player.worldPosition.X;
            if (seedCollection.collectedSeedCount > 0 && !treeCollection.HasTreeAtPosition(posX))
            {
                treeCollection.CreateTree(posX, TreeType.PAWN);
                seedCollection.collectedSeedCount--;
            }
        }
    }
}
