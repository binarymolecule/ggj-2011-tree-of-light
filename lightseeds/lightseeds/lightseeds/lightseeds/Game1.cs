#define SHOW_INTRO
//#define TEST_GAMEOVER
//#define DEBUG_CONTROLS

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

        public Texture2D playerTexture;
        Texture2D backgroundTexture;
        public SpriteFont spriteFont;

        Texture2D menuButtons;

        RenderTarget2D[] splitScreens;
        public Vector2[] splitScreenPositions;

        RenderTarget2D joinedScreen;
        public Vector2 joinedScreenPosition;
        public Vector2 joinedScreenBottomPosition;
        public GameCamera joinedCamera;

        MapPanel mapPanel;

        public TreeCollection treeCollection;
        public SeedCollection seedCollection;
        public FairyCollection fairyCollection;

        public List<TheVoid> voids = new List<TheVoid>();

        static public int SCREEN_WIDTH = 1280;
        static public int SCREEN_HEIGHT = 720;
        static public bool FULLSCREEN = false;
        static public int SPLIT_SCREEN_WIDTH = SCREEN_WIDTH;
        static public int SPLIT_SCREEN_HEIGHT = SCREEN_HEIGHT / 2;
        static public int WORLD_SCREEN_WIDTH = 24;
        static public int WORLD_SCREEN_HEIGHT = 9;
        const int NUM_INITIAL_SEEDS = 8;

        public ParticleCollection particleCollection;
        public GameState State;
        private Texture2D backgroundTexture2;
        private Texture2D backgroundTexture3;
        private Color fadeColor = Color.Black;
        public Random random = new Random();

        public double startTime;
        private SpriteFont headlineFont;

        GameScreen introScreen;
        GameOverGameScreen gameoverScreen;

        int storyProgress = -1;
        float storyTime = 0.0f;
        float[] storyTimeIntervalls = { 7.0f, 3.0f, 4.0f, 7.0f, 4.0f, 6.0f,
                                        4.0f, 12.0f, 4.0f, 8.0f };

        bool splitScreenMode = true;
        private SpriteFont scriptFont;
        private ParallaxCollection parallaxCollection;
        private Texture2D black;
        private System.Text.StringBuilder timeTest;

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
            
            joinedScreenPosition = Vector2.Zero;
            joinedScreenBottomPosition = new Vector2(0.0f, (float)SCREEN_HEIGHT - 12.0f);
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
            introScreen = new GameScreen(this, Content.Load<Texture2D>("textures/titleScreen"), 10.0f);
            gameoverScreen = new GameOverGameScreen(this, Content.Load<Texture2D>("textures/gameoverScreen"), 3.0f);

            // Create main game related stuff
            world = new World();
            world.Load();
            // load textures
            playerTexture = Content.Load<Texture2D>("textures/playerTexture");
            backgroundTexture = Content.Load<Texture2D>("Background/Background_1");
            backgroundTexture2 = Content.Load<Texture2D>("Background/Background_2");
            backgroundTexture3 = Content.Load<Texture2D>("Background/Background_3");
            this.black = Content.Load<Texture2D>("black");
            this.noise = Content.Load<Texture2D>("noise");

            this.parallaxCollection = new ParallaxCollection(this);

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

            joinedCamera = new GameCamera(this, 2, new Vector3(0.0f, 16.0f, 1.0f));

            // create game screens
            splitScreens = new RenderTarget2D[2];
            splitScreens[0] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);
            splitScreens[1] = new RenderTarget2D(GraphicsDevice, SPLIT_SCREEN_WIDTH, SPLIT_SCREEN_HEIGHT);
            
            joinedScreen = new RenderTarget2D(GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT);

            // load GUI related stuff
            spriteFont = Content.Load<SpriteFont>("fonts/Geo");
            headlineFont = Content.Load<SpriteFont>("fonts/headline");
            scriptFont = Content.Load<SpriteFont>("fonts/script");
            menuButtons = Content.Load<Texture2D>("textures/menuButtons");

            // create tree collection
            treeCollection = new TreeCollection(this);
            treeCollection.Load();
            treeCollection.Reset();

            // create seed collection
            seedCollection = new SeedCollection(this);
            seedCollection.Load();
            seedCollection.Reset();
            CreateRandomSeeds(NUM_INITIAL_SEEDS, treeCollection.trees[0]);

            fairyCollection = new FairyCollection(this);
            fairyCollection.Load(playerTexture);

            const int NUM_FAIRIES = 40;
            for (int i = NUM_FAIRIES - 1; i >= 0; i--)
            {
                float sx = random.Next(30);
                float sy = random.Next(30);
                fairyCollection.SpawnFairy(new Vector3(sx-15.0f, sy+5.0f, 1.0f));
            }
            
            
            /*
             spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                               GameCamera.CurrentCamera.screenTransform);
                
             foreground.Draw(this, gameTime, i);
            */            

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
#if TEST_GAMEOVER
                this.gameoverScreen.Reset(gameTime);
                this.State = GameState.GAMEOVER;
#endif

                introScreen.Update(gameTime);
                if (introScreen.Status == GameScreen.ScreenStatus.DONE)
                {
#if SHOW_INTRO
                    this.State = GameState.STORY;
#else
                    //skip intro
                    fadeColor = Color.White;
                    this.State = GameState.RUNNING;
                    startTime = gameTime.TotalGameTime.TotalSeconds;             
#endif
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
            joinedCamera.Update(gameTime);

            treeCollection.Update(gameTime);

            seedCollection.Update();

            world.Update(gameTime);
            
            voids.ForEach((v) => v.Update(gameTime));

            particleCollection.Update(gameTime);

            mapPanel.Update(gameTime);

            if (voids.All((v) => v.IsBehind(0.0f)))
            {
                gameoverScreen.Reset(gameTime);
                State = GameState.GAMEOVER;
            }
            fairyCollection.Update(gameTime);

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
            for (int i = (splitScreenMode ? 0 : 2); i < (splitScreenMode ? 2 : 3); i++)
            {
                if (splitScreenMode)
                    GraphicsDevice.SetRenderTarget(splitScreens[i]);
                else
                    GraphicsDevice.SetRenderTarget(joinedScreen);
                GraphicsDevice.Clear(new Color(40, 40, 40));

                GameCamera.CurrentCamera = (splitScreenMode ? cameras[i] : joinedCamera);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i));
                spriteBatch.Draw(backgroundTexture, new Rectangle((int)(-SCREEN_WIDTH*0.1f), (int)(-SCREEN_HEIGHT*0.1f),
                                                                  (int)(SCREEN_WIDTH * 2.4), (int)(SCREEN_HEIGHT * 1.2f)), Color.Gray);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i, 2.0f));
                spriteBatch.Draw(backgroundTexture2, new Rectangle((int)(-SCREEN_WIDTH * 0.1f), (int)(-SCREEN_HEIGHT * 0.1f),
                                                                   (int)(SCREEN_WIDTH * 2.4f), (int)(SCREEN_HEIGHT * 1.2f)), Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, bgMatrix(i, 4.0f));
                spriteBatch.Draw(backgroundTexture3, new Rectangle((int)(-SCREEN_WIDTH * 0.1f), (int)(-SCREEN_HEIGHT * 0.1f),
                                                                   (int)(SCREEN_WIDTH * 2.4f), (int)(SCREEN_HEIGHT * 1.2f)), Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                  GameCamera.CurrentCamera.screenTransform);
                treeCollection.trees.ForEach((t) => DrawForceField(spriteBatch, t));
                treeCollection.Draw(spriteBatch);
                seedCollection.Draw(spriteBatch);
                
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null,
                                  GameCamera.CurrentCamera.screenTransform);
                fairyCollection.Draw(gameTime);
                foreach (var player in players)
                {
                    player.Draw(gameTime);
                }
                spriteBatch.End();
                
                world.Draw(this, gameTime, -1000, 2000);
                /**
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                  worldToScreen * bgMatrix(i, 1.1f));
                foreground.Draw(this, gameTime, i);
                spriteBatch.End();
                **/

                BlendState bs = new BlendState() {
                    AlphaBlendFunction = BlendFunction.Subtract,
                    ColorBlendFunction = BlendFunction.Subtract,
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.One,
                    AlphaDestinationBlend = Blend.One,
                    AlphaSourceBlend = Blend.One
                };

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                  GameCamera.CurrentCamera.screenTransform);
                voids.ForEach((v) => v.Draw(spriteBatch));
                particleCollection.Draw(gameTime, spriteBatch);
                spriteBatch.End();

                this.parallaxCollection.Draw(spriteBatch);

                if (this.State != GameState.STORY)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                      GameCamera.CurrentCamera.screenTransform);
                    foreach (var tree in treeCollection.trees)
                    {
                        var xpos = tree.worldPosition.X;
                        var textPos = Vector3.Transform(new Vector3(xpos, world.getHeigth(xpos) - 1, 0), worldToScreen).ToVector2();
                        var textMeasure = headlineFont.MeasureString(tree.name);
                        spriteBatch.DrawString(headlineFont, tree.name, textPos - textMeasure / 2, new Color(55, 55, 55, 255));
                        //var textMeasure = scriptFont.MeasureString(tree.name);
                        //spriteBatch.DrawString(scriptFont, tree.name, textPos - textMeasure / 2, new Color(55, 55, 55, 255));
                    }
                    spriteBatch.End();
                }


                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                float nx = this.particleCollection.random.Next(100), ny = particleCollection.random.Next(100);
                var nv = new Vector2(-nx, -ny);
                for(int nxi=0; nxi<4; nxi++) {
                    for (int nyi = 0; nyi < 2; nyi++)
                    {
                        spriteBatch.Draw(noise, nv + new Vector2(nxi * noise.Width, nyi * noise.Height), new Color(255, 255, 255, 13));
                    }
                }
                
                spriteBatch.End();

                if (this.State != GameState.STORY)
                {
                    for (int j = (splitScreenMode ? i : 0); j <= (splitScreenMode ? i : 1); j++)
                    {
                        Tree tree = treeCollection.FindTreeAtPosition(players[j].worldPosition.X);
                        if (tree != null)
                        {
                            // show tree information

                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
                                              GameCamera.CurrentCamera.screenTransform);
                            var textPos = Vector3.Transform(tree.worldPosition + new Vector3(1.5f, 1f, 0), worldToScreen).ToVector2();

                            var bodyText = tree.GetStatusInfo();
                            var headlineText = tree.descriptionLines[0];

                            var headlineMeasure = headlineFont.MeasureString(headlineText);
                            var bodyMeasure = spriteFont.MeasureString(bodyText);

                            textPos.Y = Math.Min(SPLIT_SCREEN_HEIGHT - headlineMeasure.Y - bodyMeasure.Y - 10 - GameCamera.CurrentCamera.screenTransform.Translation.Y, textPos.Y);
                            //if (!splitScreenMode && j == 0)
                            //    textPos.Y -= SCREEN_HEIGHT / 2;

                            spriteBatch.DrawString(headlineFont, headlineText, textPos + new Vector2(2, 2), Color.Black);
                            spriteBatch.DrawString(headlineFont, headlineText, textPos, Color.White);
                            textPos.Y += headlineMeasure.Y;

                            spriteBatch.DrawString(spriteFont, bodyText, textPos + new Vector2(2, 2), Color.Black);
                            spriteBatch.DrawString(spriteFont, bodyText, textPos, Color.WhiteSmoke);
                            textPos.Y += headlineMeasure.Y + menuButtons.Height;

                            // draw menu buttons for build menu
                            if (tree.status == Tree.TreeStatus.BLUEPRINT)
                                spriteBatch.Draw(menuButtons, textPos, new Rectangle(0, 0, menuButtons.Width, menuButtons.Height / 8), Color.White);

                            spriteBatch.End();
                        }
                    }
                }
                else
                {
                    // draw story related stuff
                    if (storyProgress == 5)
                    {
                        if (storyTime > 0.25f * storyTimeIntervalls[storyProgress] &&
                            storyTime < 0.75f * storyTimeIntervalls[storyProgress])
                        {
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);                        
                            spriteBatch.Draw(menuButtons, new Vector2(SCREEN_WIDTH / 2 + 24, SCREEN_HEIGHT / 2),
                                             new Rectangle(0, menuButtons.Height / 4, menuButtons.Width, menuButtons.Height / 4), Color.White);
                            spriteBatch.End();
                        }
                    }
                    else if (storyProgress == 7)
                    {
                        if (storyTime < 0.5f * storyTimeIntervalls[storyProgress])
                        {
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                            spriteBatch.Draw(menuButtons, new Vector2(SCREEN_WIDTH / 2 + 24, SCREEN_HEIGHT / 2),
                                             new Rectangle(0, menuButtons.Height / 8, menuButtons.Width, menuButtons.Height / 8), Color.White);
                            spriteBatch.End();
                        }
                    }
                    else if (storyProgress == 9)
                    {
                        if (storyTime > 0.23f * storyTimeIntervalls[storyProgress] &&
                            storyTime < 0.48f * storyTimeIntervalls[storyProgress])
                        {
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                            spriteBatch.Draw(menuButtons, new Vector2(SCREEN_WIDTH / 2 - 136, SCREEN_HEIGHT / 2 - 16),
                                             new Rectangle(0, menuButtons.Height / 2, menuButtons.Width, menuButtons.Height / 4), Color.White);
                            spriteBatch.End();
                        }
                        else if (storyTime > 0.52f * storyTimeIntervalls[storyProgress] &&
                            storyTime < 0.77f * storyTimeIntervalls[storyProgress])
                        {
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                            spriteBatch.Draw(menuButtons, new Vector2(SCREEN_WIDTH / 2 + 24, SCREEN_HEIGHT / 2),
                                             new Rectangle(0, 3 * menuButtons.Height / 4, menuButtons.Width, menuButtons.Height / 4), Color.White);
                            spriteBatch.End();
                        }
                    }
                }
            }
            GraphicsDevice.SetRenderTarget(null);

            // Draw split screens
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            if (splitScreenMode)
            {
                for (int i = 0; i < 2; i++)
                    spriteBatch.Draw(splitScreens[i], splitScreenPositions[i], fadeColor);
            }
            else
            {
                spriteBatch.Draw(joinedScreen, joinedScreenPosition, fadeColor);
            }
            spriteBatch.End();

            if (this.State != GameState.STORY)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                // set position of map panel and info texts
                Vector2 edgePos = (splitScreenMode ? splitScreenPositions[1] : joinedScreenBottomPosition);
                mapPanel.edgePosition = edgePos;
                mapPanel.Draw(gameTime);

                var soulText = String.Format("{0:0} Souls", seedCollection.collectedSeedCount);
                var soulTextDim = spriteFont.MeasureString(soulText);

                var textStart = new Vector2(10, -soulTextDim.Y/2) + splitScreenPositions[1];

                spriteBatch.Draw(black, new Rectangle((int)textStart.X-10, (int)textStart.Y - 5, (int)soulTextDim.X + 20 + (int)soulTextDim.Y + 15, (int)soulTextDim.Y + 10), Color.Black);
                spriteBatch.Draw(seedCollection.texture, new Rectangle((int)textStart.X, (int)textStart.Y, (int)soulTextDim.Y, (int)soulTextDim.Y), Color.White);
                spriteBatch.DrawString(spriteFont, soulText, new Vector2((int)soulTextDim.Y + 5, 0) + textStart, Color.White);

                int totalTime = (int)(gameTime.TotalGameTime.TotalSeconds - startTime);
                var timeText = String.Format("{0:0}:{1:00}", totalTime / 60, totalTime % 60);
                var timeTextDim = spriteFont.MeasureString(timeText);

                var timeTextStart = new Vector2(SPLIT_SCREEN_WIDTH - timeTextDim.X - 10, splitScreenPositions[1].Y - timeTextDim.Y/2);
                spriteBatch.Draw(black, new Rectangle((int)timeTextStart.X - 10, (int)timeTextStart.Y - 5, (int)timeTextDim.X + 20, (int)timeTextDim.Y + 10), Color.Black);
                spriteBatch.DrawString(spriteFont, timeText, timeTextStart, Color.White);

                //int totalTime = (int)(gameTime.TotalGameTime.TotalSeconds - startTime);
                //spriteBatch.DrawString(spriteFont, String.Format("DEBUG Time: {0:0}:{1:00}", totalTime / 60, totalTime % 60),
                //                       (splitScreenMode ? splitScreenPositions[0] : joinedScreenPosition), Color.Red);

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
#if DEBUG_CONTROLS
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
#endif
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

        public Matrix bgMatrix(int index, float factor = 1.0f)
        {
                Vector3 v = new Vector3((GameCamera.CurrentCamera.worldTransform.Translation.X) * factor,
                                    -(GameCamera.CurrentCamera.worldTransform.Translation.Y * 0.2f) * factor, 0);
                return Matrix.CreateTranslation(v);
        }

        public void Reset()
        {
            treeCollection.Reset();
            seedCollection.Reset();
            CreateRandomSeeds(NUM_INITIAL_SEEDS, treeCollection.trees[0]);
            ResetVoids(1.0f);
            for (int i = 0; i < 2; i++)
            {
                // reset players
                players[i].Reset();
                // reset cameras
                cameras[i].FollowPlayer(players[i]);
                cameras[i].MoveTo(players[i].worldPosition.ToVector2(), 0.1f);
            }
            joinedCamera.Center(new Vector3(0.0f, 16.0f, 1.0f));
            splitScreenMode = true;

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

        public void CreateRandomSeeds(int num, Tree tree)
        {
            for (int i = 0; i < num; i++)
            {
                //seedCollection.SpawnSeed(new Vector3(posX, world.getHeigth(posX) + offsetY, 0.0f));
                seedCollection.SpawnSeed(tree.GetNextSeedPosition());
            }
        }

        public void UpdateStoryMode(GameTime gameTime)
        {
            if (storyProgress < 0)
            {
                // initialization
                splitScreenMode = false;
                storyProgress = 0;
                storyTime = 0.0f;
                joinedCamera.Center(new Vector3(0.0f, 48.0f, 1.0f));
                joinedCamera.MoveTo(new Vector2(0.0f, 16.0f), storyTimeIntervalls[storyProgress]);
            }

            storyTime += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            // update tree and players
            treeCollection.Update(gameTime);
            foreach (PlayerSprite p in players)
                p.Update(gameTime);

            switch (storyProgress)
            {
                case 0:
                    {
                        // scroll + fade in
                        joinedCamera.Update(gameTime);
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
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            joinedCamera.MoveTo(new Vector2(-0.5f * World.WorldWidth + 0.5f * WORLD_SCREEN_WIDTH, 16.0f),
                                                storyTimeIntervalls[storyProgress]);
                        }                        
                    }
                    break;
                case 2:
                case 6:
                    {
                        // scroll left
                        joinedCamera.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            if (storyProgress == 7)
                            {
                                treeCollection.CreateTree(new Vector3(-0.5f * World.WorldWidth + WORLD_SCREEN_WIDTH, 16.0f, 1.0f),
                                                          TreeType.MOTHER, false, "");
                            }
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
                            joinedCamera.MoveTo(new Vector2(0.0f, 16.0f), storyTimeIntervalls[storyProgress]);
                        }
                    }
                    break;
                case 4:
                case 8:
                    {
                        // scroll right
                        joinedCamera.Update(gameTime);
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
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            joinedCamera.MoveTo(new Vector2(-0.5f * World.WorldWidth + WORLD_SCREEN_WIDTH, 16.0f),
                                                storyTimeIntervalls[storyProgress]);
                        }
                    }
                    break;
                case 7:
                    {
                        // plant seed and watch tree grow
                        voids.ForEach((v) => v.Update(gameTime));
                        particleCollection.Update(gameTime);
                        if (storyTime > storyTimeIntervalls[storyProgress])
                        {
                            storyTime = 0.0f;
                            storyProgress++;
                            joinedCamera.MoveTo(new Vector2(0.0f, 16.0f), storyTimeIntervalls[storyProgress]);
                        }
                    }
                    break;
                case 9:
                    {
                        // wait for the game to begin
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
                        splitScreenMode = true;
                        joinedCamera.Center(new Vector3(0.0f, 16.0f, 1.0f));
                        startTime = gameTime.TotalGameTime.TotalSeconds;
                        ResetVoids(1.0f);
                    }
                    break;
            }

            // skip intro
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            if (gamepadState.IsButtonDown(Buttons.Start) ||
                gamepadState.IsButtonDown(Buttons.B) || keyboardState.IsKeyDown(Keys.Enter))
            {
                // start the game
                fadeColor = Color.White;
                this.State = GameState.RUNNING;
                splitScreenMode = true;
                joinedCamera.Center(new Vector3(0.0f, 16.0f, 1.0f));
                startTime = gameTime.TotalGameTime.TotalSeconds;
                ResetVoids(1.0f);
            }
            else if (gamepadState.Buttons.Back == ButtonState.Pressed ||
                     keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
        }

        public Texture2D noise { get; set; }
    }
}
