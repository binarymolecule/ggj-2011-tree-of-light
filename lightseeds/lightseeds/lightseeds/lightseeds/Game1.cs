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
        Texture2D backgroundTexture;
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

        public ParticleCollection particleCollection;
        public GameState State;

        public enum GameState
        {
            RUNNING,
            CLOSING
        }

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
            backgroundTexture = Content.Load<Texture2D>("Background/Background_1");

            players = new PlayerSprite[2];
            players[0] = new PlayerSprite(this, 0, new Vector3(2.0f, 7.0f, 1.0f), playerTexture)
            {
                color = new Color(220, 220, 255, 255)
            };
            players[1] = new PlayerSprite(this, 1, new Vector3(-2.0f, 6.5f, 1.0f), playerTexture)
            {
                color = new Color(255, 220, 220, 255)
            };

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
            treeCollection.CreateTree(Vector3.Zero, TreeType.BASE, false);

            seedCollection = new SeedCollection(this);
            seedCollection.Load();

            Random randomizer = new Random();
            const int NUM_INITIAL_SEEDS = 8;
            for (int i = 0; i < NUM_INITIAL_SEEDS; i++)
            {
                float posX = (float)randomizer.Next(-(World.WorldWidth + WORLD_SCREEN_WIDTH)/2, (World.WorldWidth- WORLD_SCREEN_WIDTH)/2);
                float offsetY = (float)randomizer.Next(2, 6);
                seedCollection.SpawnSeed(new Vector3(posX, world.getHeigth(posX) + offsetY, 0.0f));
            }

            this.particleCollection = new ParticleCollection(this);

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(1, 0, 0),
                horizontalPosition = -World.WorldWidth/2
            });

            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(-1, 0, 0),
                horizontalPosition = World.WorldWidth / 2
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
                GraphicsDevice.Clear(new Color(40, 40, 40));

                GameCamera.CurrentCamera = cameras[i];
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i));
                spriteBatch.Draw(backgroundTexture, new Rectangle((int)(-SCREEN_WIDTH*0.1f), (int)(-SCREEN_HEIGHT*0.1f), (int)(SCREEN_WIDTH * 2.4), (int)(SCREEN_HEIGHT*1.2)), Color.White);
                spriteBatch.End();
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null,
                                  cameras[i].screenTransform);
                treeCollection.Draw(spriteBatch);
                seedCollection.Draw(spriteBatch);

                foreach(var player in players) {
                  player.Draw(gameTime);   
                }
                spriteBatch.End();

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
            foreach (var p in players)
            {
                spriteBatch.DrawString(spriteFont, String.Format("P{0:0}: {1:0.0} / {2:0.0}", p.index, p.worldPosition.X, p.worldPosition.Y), splitScreenPositions[p.index], Color.White);
                spriteBatch.DrawString(spriteFont, String.Format("Seeds: {0:0}", seedCollection.collectedSeedCount), new Vector2(0, 20) + splitScreenPositions[p.index], Color.Red);
                if (p.blueprint != null)
                {
                    var y = 0;
                    foreach (var descriptionLine in p.blueprint.descriptionLines)
                    {
                        spriteBatch.DrawString(spriteFont, String.Format("{0}", descriptionLine), new Vector2(SPLIT_SCREEN_WIDTH - 180, y) + splitScreenPositions[p.index], Color.White);
                        y += 20;
                    }
                }
            }            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void handleControls()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            foreach (var p in players)
            {
                var gamepadState = GamePad.GetState(p.index == 0 ? PlayerIndex.One : PlayerIndex.Two);
                p.HandleInput(gamepadState);


                // debug input
                if (p.index == 0)
                {
                    if (gamepadState.IsButtonDown(Buttons.RightShoulder))
                    {
                        this.State = GameState.CLOSING;
                    }
                    else if (gamepadState.IsButtonDown(Buttons.LeftShoulder))
                    {
                        seedCollection.collectedSeedCount += 10;
                    }
                }
            }
        }

        public void createTree(PlayerSprite player, TreeType treeType, int price)
        {
            // check and decrease seeds of players here
            if (seedCollection.collectedSeedCount >= price &&
                !treeCollection.HasTreeAtPosition(player.worldPosition.X))
            {
                treeCollection.CreateTree(player.worldPosition, treeType, false);
                seedCollection.collectedSeedCount -= price;
            }
        }
        public Matrix bgMatrix(int index)
        {
            Vector3 v = new Vector3(-(players[index].worldPosition.X), 
                                    (players[index].worldPosition.Y), 0);
            return Matrix.CreateTranslation(v);
            
        }
    }
}
