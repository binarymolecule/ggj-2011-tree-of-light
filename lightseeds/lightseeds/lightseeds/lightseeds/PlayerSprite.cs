using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using lightseeds.GameObjects;
using lightseeds.Helpers;

namespace lightseeds
{
    
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PlayerSprite : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;

        public int index;
        
        Vector3 position, initialPosition;

        Texture2D texture;
        public const float XBOUNDARY = 4.0f;
        public const float YBOUNDARY = 40.0f;

        public const float WOBBLEBPM = 60.0f;
        public const float WOBBLYNESS = 3.0f;
        public const float MAXVELOCITY_X = 25.0f;
        public const float MAXVELOCITY_Y = 10.0f;
        public const float MAXVELOCITY_Y_DOWN = 25.0f;
        public const float ACCELERATION = 100.0f;
        public float wobbleHeight = 2.0f;
        
        public float currentXAcceleration = 0.0f;
        public float currentYAcceleration = 0.0f;
        public float xVelocity = 0.0f;
        public float yVelocity = 0.0f;
        public List<Vector3> positions = new List<Vector3>();
        public Vector3 drawingPosition;

        private float stunTimer = 0.0f;
        public bool isStunned = false;

        private bool waitForReleaseA = false;
        private bool waitForReleaseB = false;
        private bool waitForReleaseLeft = false;
        private bool waitForReleaseRight = false;
        private bool waitForBPConfirm = false;


        public Tree blueprint;
        private TreeType lastUsedType = TreeType.PAWN;

        Random random = new Random();

        public Color color;

        public int collectedSeeds;

        public float scale = 1f;
        public float currentScale = 1f;
        public float scaleTransition = 1f;

        public Vector2 offset;
        private float oldScale = 1f;

        public Vector3 worldPosition
        {
            get
            {
                return position;
            }
        }


        public Vector2 screenPosition
        {
            get
            {
                Vector3 screenPos = Vector3.Transform(position, game.worldToScreen);
                return new Vector2(screenPos.X, screenPos.Y);
            }
        }
        
        public PlayerSprite(Game game, int index, Vector3 pos, Texture2D tex) : base(game)
        {
            this.game = (Game1)game;
            this.index = index;
            texture = tex;
            initialPosition = pos;
            position = initialPosition;
            offset = new Vector2(-0.5f * texture.Width, -0.5f * texture.Height);
        }

        public PlayerSprite(Game1 game1, Vector3 vector3, Texture2D playerTexture) : base(game1)
        {
            this.game = (Game1)game;
            texture = playerTexture;
            initialPosition = vector3;
            position = initialPosition;
            offset = new Vector2(-0.5f * texture.Width, -0.5f * texture.Height); 
        }

        public override void Update(GameTime gameTime)
        {
            
            if (isStunned)
            {
                stunTimer -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (stunTimer < 0.0f)
                {
                    stunTimer = 0.0f;
                    isStunned = false;                    
                }
            }
            else
            {
                UpdateXPosition(gameTime);
                UpdateYPosition(gameTime);
            }
            var x = screenPosition.X + 2 * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 120);
            var y = WobblyPosition(screenPosition.Y, wobbleHeight, gameTime);
            positions.Add(new Vector3(x,y,0));
            if (positions.Count > 12)
                positions.RemoveAt(0);
            
            scaleTransition += (float)gameTime.ElapsedGameTime.TotalSeconds * 6f;

            if (scaleTransition >= 1)
            {
               oldScale = scale;
               scale = 0.8f + (float)random.NextDouble() * 0.2f;
               scaleTransition = 0;
            }
          
            currentScale = oldScale + scaleTransition * (scale - oldScale);

            base.Update(gameTime);
        }

        public void Respawn()
        {
            position = initialPosition;
            stunTimer = 2.0f;
            isStunned = true;

            // reset controls
            waitForReleaseA = false;
            waitForReleaseB = false;
            waitForReleaseLeft = false;
            waitForReleaseRight = false;
            waitForBPConfirm = false;
            blueprint = null;

            currentXAcceleration = 0.0f;
            currentYAcceleration = 0.0f;

            game.cameras[index].MoveTo(worldPosition.ToVector2(), 1.0f);
        }

        public void Reset()
        {
            position = initialPosition;

            // reset stun mode
            stunTimer = 0.0f;
            isStunned = false;

            // reset controls
            waitForReleaseA = false;
            waitForReleaseB = false;
            waitForReleaseLeft = false;
            waitForReleaseRight = false;
            waitForBPConfirm = false;
            blueprint = null;

            currentXAcceleration = 0.0f;
            currentYAcceleration = 0.0f;
        }

        private void UpdateXPosition(GameTime gameTime)
        {
            
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            var leftBound = game.voids[0].horizontalPosition + XBOUNDARY;
            var rightBound = game.voids[1].horizontalPosition - XBOUNDARY;
            //movement
            if (position.X < rightBound && position.X > leftBound)
            {
                
            }

            xVelocity += currentXAcceleration * timeFactor;
            if (xVelocity > MAXVELOCITY_X)
              xVelocity = MAXVELOCITY_X;
            if (xVelocity < -MAXVELOCITY_X)
              xVelocity = -MAXVELOCITY_X;
            
            //backbounce
            /*if (position.X < leftBound)
            {
              xVelocity += 2 * ACCELERATION * timeFactor;
              GameServices.GetService<MusicManager>().Play("Die");
            }
            if (position.X > rightBound)
            {
              xVelocity -= 2 * ACCELERATION * timeFactor;
              GameServices.GetService<MusicManager>().Play("Die");
            }
          */
            //slowdown
            if (currentXAcceleration == 0)
            {
                if (xVelocity > 0)
                    xVelocity -= ACCELERATION * timeFactor;
                if (xVelocity < 0)
                    xVelocity += ACCELERATION * timeFactor;
                if (Math.Abs(xVelocity) <= ACCELERATION * timeFactor)
                    xVelocity = 0;
            }
            
            position.X += xVelocity * timeFactor;
            
        }
        public void UpdateYPosition(GameTime gameTime)
        {
            var timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            //movement
            if (position.Y < YBOUNDARY)
            {
                yVelocity += currentYAcceleration * timeFactor;
                if (yVelocity > MAXVELOCITY_Y)
                    yVelocity = MAXVELOCITY_Y;
                if (yVelocity < -MAXVELOCITY_Y_DOWN)
                    yVelocity = -MAXVELOCITY_Y_DOWN;
            }

            //backbounce
            if (position.Y > YBOUNDARY)
                yVelocity -= 2 * ACCELERATION * timeFactor;
            float diff = game.world.getHeigth(position.X) + 3.0f - position.Y;
            if (diff > 0 && yVelocity < MAXVELOCITY_Y / 2)
                yVelocity += (float)Math.Pow(diff,2) * ACCELERATION * timeFactor;


            //slowdown
            if (currentYAcceleration == 0)
            {
                if (yVelocity > 0)
                    yVelocity -= ACCELERATION * timeFactor;
            }

            position.Y += yVelocity * timeFactor;
            
        }

        public override void Draw(GameTime gameTime)
        {
            if (isStunned)
            {
                if ((int)(stunTimer * 20.0f) % 2 == 0)
                {
                    var pos = positions[11];
                    game.spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y) + offset, null, color, 0, Vector2.Zero, currentScale, SpriteEffects.None, 0);                
                }
            }
            else
            {
                var tempColor = color;
                tempColor.A = 0;
                for (int i = 1; i < positions.Count; i++)
                {
                    tempColor.A += 2;
                    game.spriteBatch.Draw(texture, new Vector2((positions[i].X + positions[i - 1].X) / 2, (positions[i].Y + positions[i - 1].Y) / 2) + offset, null, tempColor, 0, Vector2.Zero,
                                          currentScale, SpriteEffects.None, 0);
                    tempColor.A += 2;
                    game.spriteBatch.Draw(texture, new Vector2(positions[i].X, positions[i].Y) + offset, null, tempColor, 0, Vector2.Zero,
                                          currentScale, SpriteEffects.None, 0);
                }
            }
            base.Draw(gameTime);
        }

        private float WobblyPosition(float pos, float modifier, GameTime gameTime)
        {
            var sin1 = modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 60 + index * Math.PI);
            var sin2 = 0.7f * modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 97);
            var sin3 = 0.3f * modifier * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500 * Math.PI * WOBBLEBPM / 23);
            return pos + sin1 + sin2 + sin3 ;
        }

        public void Move(float x, float y)
        {
            currentXAcceleration = x * ACCELERATION;
            currentYAcceleration = y * ACCELERATION;
        }

        private void showBlueprint()
        {
            if (!game.treeCollection.HasTreeAtPosition(worldPosition.X))
            {
                blueprint = game.treeCollection.CreateTree(worldPosition, TreeType.PAWN, true, "");
            }
        }


        public void HandleInput(GamePadState gamepadState, KeyboardState kbs)
        {
            if (!isStunned)
            {
                var stick = gamepadState.ThumbSticks.Left;

                if (waitForBPConfirm)
                {
                    if (xVelocity == 0)
                        showBlueprint();
                    Move(0, 0);
                    if (gamepadState.IsButtonDown(Buttons.A) && !waitForReleaseA)
                    {
                        game.treeCollection.trees.Remove(blueprint);
                        game.createTree(this, blueprint.treeType, blueprint.name, blueprint.price);
                        lastUsedType = blueprint.treeType;
                        blueprint = null;
                        waitForBPConfirm = false;
                        waitForReleaseA = true;
                        GameServices.GetService<MusicManager>().Play("MenuSelect");
                    }
                    if (gamepadState.IsButtonDown(Buttons.B) && !waitForReleaseB)
                    {
                        game.treeCollection.trees.Remove(blueprint);
                        lastUsedType = blueprint.treeType;
                        blueprint = null;
                        waitForBPConfirm = false;
                        waitForReleaseB = true;
                        GameServices.GetService<MusicManager>().Play("MenuSelect");
                    }
                    if (gamepadState.ThumbSticks.Left.X < 0.0f && !waitForReleaseLeft && !waitForReleaseA)
                    {
                        var type = blueprint.treeType;
                        game.treeCollection.trees.Remove(blueprint);
                        blueprint = game.treeCollection.CreateTree(worldPosition, type.Previous(), true, "");
                        waitForReleaseLeft = true;
                        GameServices.GetService<MusicManager>().Play("MenuSelect");
                    }
                    if (gamepadState.ThumbSticks.Left.X > 0.0f && !waitForReleaseRight)
                    {
                        var type = blueprint.treeType;
                        game.treeCollection.trees.Remove(blueprint);
                        blueprint = game.treeCollection.CreateTree(worldPosition, type.Next(), true, "");
                        waitForReleaseRight = true;
                        GameServices.GetService<MusicManager>().Play("MenuSelect");
                    }

                    
                }
                else
                {
                    float x = stick.X, y = stick.Y;

                    if (kbs != null)
                    {
                        x = kbs.IsKeyDown(Keys.Right) ? 1 : x;
                        x = kbs.IsKeyDown(Keys.Left) ? -1 : x;


                        y = kbs.IsKeyDown(Keys.Up) ? 1 : y;
                        y = kbs.IsKeyDown(Keys.Down) ? -1 : y;
                    }
                    Move(x, y);

                    if (gamepadState.IsButtonDown(Buttons.A) && !waitForReleaseA)
                    {
                        if (!game.treeCollection.HasTreeAtPosition(worldPosition.X))
                        {
                            blueprint = game.treeCollection.CreateTree(worldPosition, lastUsedType, true, "");
                            waitForBPConfirm = true;
                            waitForReleaseA = true;
                        }

                        GameServices.GetService<MusicManager>().Play("MenuSelect");
                    }
                }
                if (gamepadState.IsButtonUp(Buttons.A))
                    waitForReleaseA = false;
                if (gamepadState.IsButtonUp(Buttons.B))
                    waitForReleaseB = false;
                if (gamepadState.ThumbSticks.Left.X == 0)
                {
                    waitForReleaseLeft = false;
                    waitForReleaseRight = false;
                }
            }
        }
    }
}
