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
        public Tree[] blueprints = new Tree[2];

        public List<TheVoid> voids = new List<TheVoid>();

        static public int SCREEN_WIDTH = 1024;
        static public int SCREEN_HEIGHT = 768;
        static public int SPLIT_SCREEN_WIDTH = 1024;
        static public int SPLIT_SCREEN_HEIGHT = 384;
        static public int WORLD_SCREEN_WIDTH = 24;
        static public int WORLD_SCREEN_HEIGHT = 9;
        private bool waitForReleaseA;
        private bool waitForReleaseB;
        private bool waitForBPConfirm;
        public ParticleCollection particleCollection;

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
            seedCollection.SpawnSeed(new Vector3(4.0f, world.getHeigth(4.0f) + 2.0f, 0.0f));

            this.particleCollection = new ParticleCollection(this);

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(1, 0, 0),
                horizontalPosition = -World.WorldWidth/5
            });

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(-1, 0, 0),
                horizontalPosition = World.WorldWidth / 5
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

            particleCollection.Update(gameTime);

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

                BlendState bs = new BlendState() {
                    AlphaBlendFunction = BlendFunction.Subtract,
                    ColorBlendFunction = BlendFunction.Subtract,
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.One,
                    AlphaDestinationBlend = Blend.One,
                    AlphaSourceBlend = Blend.One
                };
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                  cameras[i].screenTransform);
                voids.ForEach((v) => v.Draw(spriteBatch));
                particleCollection.Draw(gameTime, spriteBatch);
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
            if (blueprints[0] != null)
            {
                var y = 0;
                foreach (var descriptionLine in blueprints[0].descriptionLines)
                {
                    spriteBatch.DrawString(spriteFont, String.Format("{0}", descriptionLine), new Vector2(SCREEN_WIDTH-180, y), Color.White);
                    y += 20;
                }
            }
            
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
            // buttons

            if (waitForBPConfirm)
            {
                if (players[0].xVelocity == 0)
                    showBlueprint(players[0]);
                players[0].Move(0, 0);
                if (gamepadState.IsButtonDown(Buttons.A) && !waitForReleaseA)
                {
                    treeCollection.trees.Remove(blueprints[0]);
                    blueprints[0] = null;
                    createTree(players[0]);
                    waitForBPConfirm = false;
                    waitForReleaseA = true;
                }
                if (gamepadState.IsButtonDown(Buttons.B) && !waitForReleaseB)
                {
                    treeCollection.trees.Remove(blueprints[0]);
                    blueprints[0] = null;
                    waitForBPConfirm = false;
                    waitForReleaseB = true;
                }
                if (gamepadState.ThumbSticks.Left.X < 0.0f)
                {
                    ;//prev blueprint
                }
                if (gamepadState.ThumbSticks.Left.X > 0.0f)
                {
                    ;//next blueprint
                }
                
                    
            } else {
                players[0].Move(stick.X, stick.Y);
                
                if (gamepadState.IsButtonDown(Buttons.A) && !waitForReleaseA)
                {
                    waitForBPConfirm = true;
                    waitForReleaseA = true;
                }
            }
            if (gamepadState.IsButtonUp(Buttons.A))
                waitForReleaseA = false;
            if (gamepadState.IsButtonUp(Buttons.B))
                waitForReleaseB = false;
        }

        private void showBlueprint(PlayerSprite player)
        {

            float posX = player.worldPosition.X;
            if (!treeCollection.HasTreeAtPosition(posX))
            {
                if (player.index == 0)
                    blueprints[0] = treeCollection.CreateTree(player.worldPosition, TreeType.PAWN, true);
                if (player.index == 1)
                    blueprints[1] = treeCollection.CreateTree(player.worldPosition, TreeType.PAWN, true);
            }
        }

        public void createTree(PlayerSprite player)
        {
            // check and decrease seeds of players here
            float posX = player.worldPosition.X;
            if (seedCollection.collectedSeedCount > 0 && !treeCollection.HasTreeAtPosition(posX))
            {
                treeCollection.CreateTree(player.worldPosition, TreeType.PAWN, false);
                seedCollection.collectedSeedCount--;
            }
        }
    }
}
