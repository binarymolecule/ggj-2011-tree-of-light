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

        MusicManager musicManager = new MusicManager(20, 20);

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

        MapPanel mapPanel;

        public TreeCollection treeCollection;
        public SeedCollection seedCollection;

        public List<TheVoid> voids = new List<TheVoid>();

        static public int SCREEN_WIDTH = 1280;
        static public int SCREEN_HEIGHT = 720;
        static public bool FULLSCREEN = false;
        static public int SPLIT_SCREEN_WIDTH = SCREEN_WIDTH;
        static public int SPLIT_SCREEN_HEIGHT = SCREEN_HEIGHT / 2;
        static public int WORLD_SCREEN_WIDTH = 24;
        static public int WORLD_SCREEN_HEIGHT = 9;

        public ParticleCollection particleCollection;
        public GameState State;
        private Texture2D backgroundTexture2;
        private Texture2D backgroundTexture3;
        private Color fadeColor = Color.Black;

        public double startTime;
        private SpriteFont headlineFont;

        GameScreen introScreen, gameoverScreen;

        int storyProgress = -1;
        float storyTime = 0.0f;
        float[] storyTimeIntervalls = { 7.0f, 3.0f, 4.0f, 6.0f, 4.0f, 3.0f };

        public enum GameState
        {
            INTRO,
            STORY,
            RUNNING,
            CLOSING,
            GAMEOVER
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
          
            InitGraphicsMode(SCREEN_WIDTH, SCREEN_HEIGHT, FULLSCREEN);
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
            GameServices.AddService(musicManager);

            base.Initialize();

        }


        private bool InitGraphicsMode(int iWidth, int iHeight, bool FullScreen)
        {
          foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            if ((dm.Width == iWidth) && (dm.Height == iHeight))
            {
              graphics.PreferredBackBufferWidth = iWidth;
              graphics.PreferredBackBufferHeight = iHeight;
              graphics.IsFullScreen = FullScreen;
              graphics.ApplyChanges();
              return true;
            }

          return false;
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create title and gameover screens
            introScreen = new GameScreen(this, Content.Load<Texture2D>("textures/titleScreen"));
            gameoverScreen = new GameScreen(this, Content.Load<Texture2D>("textures/gameoverScreen"));

            // Create main game related stuff
            world = new World();
            world.Load();

            // load textures
            playerTexture = Content.Load<Texture2D>("textures/playerTexture");
            backgroundTexture = Content.Load<Texture2D>("Background/Background_1");
            backgroundTexture2 = Content.Load<Texture2D>("Background/Background_2");
            backgroundTexture3 = Content.Load<Texture2D>("Background/Background_3");

            // create map panel
            mapPanel = new MapPanel(this, Content.Load<Texture2D>("textures/centerBar"),
                                    Content.Load<Texture2D>("textures/darkBar"),
                                    Content.Load<Texture2D>("textures/mapIcons"));

            // create players
            players = new PlayerSprite[2];
            players[0] = new PlayerSprite(this, 0, new Vector3(2.0f, 7.0f, 1.0f), playerTexture)
            {
                color = new Color(0x48, 0xe6, 0xfe, 255)
            };
            players[1] = new PlayerSprite(this, 1, new Vector3(-2.0f, 6.5f, 1.0f), playerTexture)
            {
                color = new Color(0xf8, 0xfe, 0x4d, 255)
            };

            // create cameras
            cameras = new GameCamera[2];
            cameras[0] = new GameCamera(this, 0, players[0].worldPosition);
            cameras[0].FollowPlayer(players[0]);
            cameras[1] = new GameCamera(this, 1, players[1].worldPosition);
            cameras[1].FollowPlayer(players[1]);

            // createTree split screens
            splitScreens = new RenderTarget2D[2];
            splitScreens[0] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);
            splitScreens[1] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);

            spriteFont = Content.Load<SpriteFont>("fonts/Geo");
            headlineFont = Content.Load<SpriteFont>("fonts/headline");

            treeCollection = new TreeCollection(this);
            treeCollection.Load();
            treeCollection.Reset();

            seedCollection = new SeedCollection(this);
            seedCollection.Load();
            seedCollection.Reset();

            // distribute some random seeds
            Random randomizer = new Random();
            const int NUM_INITIAL_SEEDS = 8;
            for (int i = 0; i < NUM_INITIAL_SEEDS; i++)
            {
                float posX = (float)randomizer.Next(-(World.WorldWidth + WORLD_SCREEN_WIDTH) / 2, (World.WorldWidth - WORLD_SCREEN_WIDTH) / 2);
                float offsetY = (float)randomizer.Next(2, 6);
                seedCollection.SpawnSeed(new Vector3(posX, world.getHeigth(posX) + offsetY, 0.0f));
            }

            // reset "The Void"
            ResetVoids(1.0f);

            this.State = GameState.INTRO;            
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
            if (this.State == GameState.INTRO)
            {
                introScreen.Update(gameTime);
                if (introScreen.Status == GameScreen.ScreenStatus.DONE)
                {
                    //this.State = GameState.STORY;

                    // skip intro for now
                    fadeColor = Color.White;
                    this.State = GameState.RUNNING;
                    startTime = gameTime.TotalGameTime.TotalSeconds;             

                    musicManager.StartLoop();
                }
                return;
            }
            else if (this.State == GameState.GAMEOVER)
            {
                gameoverScreen.Update(gameTime);
                if (gameoverScreen.Status == GameScreen.ScreenStatus.DONE)
                {
                    introScreen.Reset();
                    this.State = GameState.INTRO;
                    Reset();
                }
                return;
            }

            if (Math.Abs(voids[0].horizontalPosition) + Math.Abs(voids[1].horizontalPosition) < 200.0f)
                musicManager.SetNextStage(1);

            float P1VoidDistance = Math.Min(Math.Abs(voids[0].horizontalPosition - players[0].worldPosition.X), Math.Abs(voids[1].horizontalPosition - players[0].worldPosition.X));
            float P2VoidDistance = Math.Min(Math.Abs(voids[1].horizontalPosition - players[1].worldPosition.X), Math.Abs(voids[1].horizontalPosition - players[1].worldPosition.X));

            musicManager.SetVolume(Math.Abs(players[0].worldPosition.X), Math.Min(P1VoidDistance, P2VoidDistance));
            musicManager.Update();

            // story mode
            if (this.State == GameState.STORY)
            {
                UpdateStoryMode(gameTime);
                return;
            }

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

            mapPanel.Update(gameTime);

            if (voids.All((v) => v.IsBehind(0.0f)))
            {
                gameoverScreen.Reset();
                State = GameState.GAMEOVER;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (this.State == GameState.INTRO)
            {
                introScreen.Draw(gameTime);                
                return;
            }
            else if (this.State == GameState.GAMEOVER)
            {
                gameoverScreen.Draw(gameTime);               
                return;
            }

            GraphicsDevice.Clear(new Color(40, 40, 40));
            
            // Draw worlds
            for (int i = 0; i < 2; i++)
            {
                GraphicsDevice.SetRenderTarget(splitScreens[i]);
                GraphicsDevice.Clear(new Color(40, 40, 40));

                GameCamera.CurrentCamera = cameras[i];
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i));
                spriteBatch.Draw(backgroundTexture, new Rectangle((int)(-SCREEN_WIDTH*0.1f), (int)(-SCREEN_HEIGHT*0.1f), (int)(SCREEN_WIDTH * 2.4), (int)(SCREEN_HEIGHT*1.2)), Color.Gray);
                spriteBatch.End();

                //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i, 2f));
                //spriteBatch.Draw(backgroundTexture2, new Rectangle((int)(-SCREEN_WIDTH * 0.1f), (int)(-SCREEN_HEIGHT * 0.1f), (int)(SCREEN_WIDTH * 2.4), (int)(SCREEN_HEIGHT * 1.2)), Color.White);
                //spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i, 4f));
                spriteBatch.Draw(backgroundTexture3, new Rectangle((int)(-SCREEN_WIDTH * 0.1f), (int)(-SCREEN_HEIGHT * 0.1f), (int)(SCREEN_WIDTH * 2.4), (int)(SCREEN_HEIGHT * 1.2)), Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                  cameras[i].screenTransform);
                treeCollection.trees.ForEach((t) => DrawForceField(spriteBatch, t));
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

                if (this.State != GameState.STORY)
                {
                    Tree tree = treeCollection.FindTreeAtPosition(players[i].worldPosition.X);
                    if (tree != null)
                    {
                        // show tree information

                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                          cameras[i].screenTransform);
                        var textPos = Vector3.Transform(tree.worldPosition + new Vector3(1.5f, 1f, 0), worldToScreen).ToVector2() + tree.fruitOffset;

                        var bodyText = tree.GetStatusInfo();
                        var headlineText = tree.status == Tree.TreeStatus.BLUEPRINT ? tree.descriptionLines[0] : tree.name;

                        var headlineMeasure = headlineFont.MeasureString(headlineText);
                        var bodyMeasure = spriteFont.MeasureString(bodyText);

                        textPos.Y = Math.Min(SPLIT_SCREEN_HEIGHT - headlineMeasure.Y - bodyMeasure.Y - 10 - cameras[i].screenTransform.Translation.Y, textPos.Y);

                        spriteBatch.DrawString(headlineFont, headlineText, textPos + new Vector2(2, 2), Color.Black);
                        spriteBatch.DrawString(headlineFont, headlineText, textPos, Color.White);
                        textPos.Y += headlineMeasure.Y;

                        spriteBatch.DrawString(spriteFont, bodyText, textPos + new Vector2(2, 2), Color.Black);
                        spriteBatch.DrawString(spriteFont, bodyText, textPos, Color.White);
                        spriteBatch.End();
                    }
                }
            }
            GraphicsDevice.SetRenderTarget(null);

            // Draw split screens
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            for (int i = 0; i < 2; i++)
                spriteBatch.Draw(splitScreens[i], splitScreenPositions[i], fadeColor);
            spriteBatch.End();

            if (this.State != GameState.STORY)
            {
                spriteBatch.Begin();

                mapPanel.Draw(gameTime);

                spriteBatch.DrawString(spriteFont, String.Format("Seeds: {0:0}", seedCollection.collectedSeedCount), new Vector2(0, -40) + splitScreenPositions[1], Color.Red);

                int totalTime = (int)(gameTime.TotalGameTime.TotalSeconds - startTime);
                spriteBatch.DrawString(spriteFont, String.Format("DEBUG Time: {0:0}:{1:00}", totalTime / 60, totalTime % 60), splitScreenPositions[0], Color.Red);

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        private void DrawForceField(SpriteBatch spriteBatch, Tree tree)
        {
            var a = tree.glowStrength * tree.growth;
            var color = new Color(1f, 1f, 1f, a);
            var scale = tree.glowRange * (tree.texture.Width / playerTexture.Width * 0.85f);
            spriteBatch.Draw(playerTexture, Vector3.Transform(tree.worldPosition, worldToScreen).ToVector2(), null, color, 0, new Vector2(playerTexture.Width/2, playerTexture.Height/2), scale, SpriteEffects.None, 0);
         
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
                p.HandleInput(gamepadState, p.index == 0 ? Keyboard.GetState() : new KeyboardState());

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

        public void createTree(PlayerSprite player, TreeType treeType, String name, int price)
        {
            // check and decrease seeds of players here
            if (seedCollection.collectedSeedCount >= price &&
                !treeCollection.HasTreeAtPosition(player.worldPosition.X))
            {
                treeCollection.CreateTree(player.worldPosition, treeType, false, name);
                seedCollection.collectedSeedCount -= price;
            }
        }

        public Matrix bgMatrix(int index, float factor = 1f)
        {
            Vector3 v = new Vector3(-(players[index].worldPosition.X) * factor, 
                                    (players[index].worldPosition.Y) * factor, 0);
            return Matrix.CreateTranslation(v);
            
        }


        public void Reset()
        {
            treeCollection.Reset();
            seedCollection.Reset();
            ResetVoids(1.0f);
            for (int i = 0; i < 2; i++)
            {
                // reset players
                players[i].Reset();
                // reset cameras
                cameras[i].FollowPlayer(players[i]);
                cameras[i].MoveTo(players[i].worldPosition.ToVector2(), 0.1f);
            }

            // TODO care about music stuff
        }

        public void ResetVoids(float factor)
        {
            particleCollection = new ParticleCollection(this);
            voids.Clear();
            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(1, 0, 0),
                horizontalPosition = -factor * World.WorldWidth / 2
            });
            voids.Add(new TheVoid(this)
            {
                direction = new Vector3(-1, 0, 0),
                horizontalPosition = factor * World.WorldWidth / 2
            });
        }

        public void UpdateStoryMode(GameTime gameTime)
        {
            if (storyProgress < 0)
            {
                // initialization
                storyProgress = 0;
                storyTime = 0.0f;
                cameras[0].Center(new Vector3(0.0f, 40.0f, 1.0f));
                cameras[0].MoveTo(new Vector2(0.0f, 8.0f), storyTimeIntervalls[storyProgress]);
            }

            storyTime += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            switch (storyProgress)
            {
                case 0:
                    {
                        // scroll + fade in
                        cameras[0].Update(gameTime);
                        treeCollection.Update(gameTime);
                        float c = storyTime / storyTimeIntervalls[storyProgress];
                        fadeColor = new Color(c, c, c, 1.0f); 
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            fadeColor = Color.White;
                        }
                    }
                    break;
                case 1:
                    {
                        // wait
                        treeCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            cameras[0].MoveTo(new Vector2(-0.5f * World.WorldWidth + 0.5f * WORLD_SCREEN_WIDTH, 8.0f),
                                              storyTimeIntervalls[storyProgress]);
                        }                        
                    }
                    break;
                case 2:
                    {
                        // scroll left
                        cameras[0].Update(gameTime);
                        treeCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                        }
                    }
                    break;
                case 3:
                    {
                        // wait
                        voids.ForEach((v) => v.Update(gameTime));
                        particleCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            cameras[0].MoveTo(new Vector2(0.0f, 8.0f), storyTimeIntervalls[storyProgress]);
                        }
                    }
                    break;
                case 4:
                    {
                        // scroll right
                        cameras[0].Update(gameTime);
                        treeCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                        }
                    }
                    break;
                case 5:
                    {
                        // wait
                        treeCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                        }
                    }
                    break;
                default:
                    {
                        // start the game
                        this.State = GameState.RUNNING;
                        cameras[0].FollowPlayer(players[0]);
                        startTime = gameTime.TotalGameTime.TotalSeconds;
                    }
                    break;
            }

            // skip intro
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            if (gamepadState.IsButtonDown(Buttons.Start) || keyboardState.IsKeyDown(Keys.Enter))
            {
                // start the game
                fadeColor = Color.White;
                this.State = GameState.RUNNING;
                cameras[0].FollowPlayer(players[0]);
                cameras[0].MoveTo(players[0].worldPosition.ToVector2(), 0.1f);
                startTime = gameTime.TotalGameTime.TotalSeconds;             
            }
        }
    }
}
