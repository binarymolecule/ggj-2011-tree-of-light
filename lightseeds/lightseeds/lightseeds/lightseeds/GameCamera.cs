using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using lightseeds.GameObjects;

namespace lightseeds
{
    public class GameCamera
    {
        int index;

        Vector2 speed;

        bool isMoving;

        public const float MAX_RANGE_X = 5.0f, MAX_RANGE_Y = 0.5f;

        public const float BRAKE_DIST_X = 5.0f, BRAKE_DIST_Y = 2.0f;

        public const float MIN_DIST = 0.1f;

        public const float MIN_SPEED = 5.0f, MAX_SPEED = 30.0f;

        public const float ACCELERATION = 100.0f, BRAKE_ACCELERATION = 60.0f;

        static public Vector2 WORLD_OFFSET = new Vector2(0.0f, 2.0f);

        //static public Vector2 MIN_COORDS = new Vector2(-180.0f + 0.25f * Game1.WORLD_SCREEN_WIDTH, 0.5f * Game1.WORLD_SCREEN_HEIGHT) + WORLD_OFFSET;
        //static public Vector2 MAX_COORDS = new Vector2(180.0f - 0.25f * Game1.WORLD_SCREEN_WIDTH, 100.0f) + WORLD_OFFSET;

        public Vector2 translation {
            get {
                return _translation;
            }

            set {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                {
                    throw new Exception("nan");
                }
                _translation = value;
                RestrictLocation();
            }
        }

        Vector2 target;

        Game1 game;

        PlayerSprite player;

        public static GameCamera CurrentCamera;

        private bool returnMode = false;

        private float returnFactor, returnDuration;

        private Vector2 returnSource, returnTarget;
        public Vector2 screen;
        private  Vector2 _translation;

        public void MoveTo(Vector2 retTarget, float retDuration)
        {
            if (returnDuration < 0)
            {
                throw new Exception("retDuration smaller than 0");
            }

            returnMode = true;
            returnFactor = 0.0f;
            returnDuration = retDuration;
            returnSource = translation;
            returnTarget = retTarget;
        }

        public Matrix screenTransform
        {
            get
            {
                Vector3 vTo = Vector3.Transform(new Vector3(-translation, 0), game.worldToScreen);
                Vector3 vFrom = Vector3.Transform(Vector3.Zero, game.worldToScreen);

                return Matrix.CreateTranslation(vTo - vFrom);
            }
        }

        public Matrix worldTransform
        {
            get
            {
                return Matrix.CreateTranslation(-(new Vector3(translation.X, translation.Y, 0.0f)));
            }
        }

        public GameCamera(Game1 game, Vector2 screen, int index, Vector3 position)
        {
            this.game = game;
            this.index = index;
            this.player = null;
            this.screen = screen;

            Center(position);
        }

        public void FollowPlayer(PlayerSprite player)
        {
            this.player = player;
            this.speed = Vector2.Zero;
            isMoving = false;
        }

        public void Center(Vector3 position)
        {
            this.player = null;
            this.translation = new Vector2(position.X, position.Y);
            this.target = new Vector2(position.X, position.Y);
            this.speed = Vector2.Zero;
            isMoving = false;
            returnMode = false;
        }

        public void Reset()
        {
            Center(Vector3.Zero);
        }

        /*
        public void MoveTo(Vector3 position)
        {
            this.player = null;
            this.target = new Vector2(position.X, position.Y);
            this.speed = 0.0f;
            isMoving = false;
        }
        */

        public void Update(GameTime gameTime)
        {
            if (returnMode)
            {
                returnFactor += (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / returnDuration;
                if (returnFactor > 1.0f)
                {
                    returnMode = false;
                    translation = returnTarget;
                }
                else
                {
                    translation = returnFactor * returnTarget + (1.0f - returnFactor) * returnSource;
                }
                return;
            }

            bool targetIsMoving = false;
            if (player != null)
            {
                target = new Vector2(player.worldPosition.X, player.worldPosition.Y);
                targetIsMoving = !player.isStunned && (player.currentXAcceleration != 0 || player.currentYAcceleration !=0 );
            }

            // keep close track of target on vertical axis
            translation = new Vector2(translation.X, target.Y);

            Vector2 direction = target - translation;
            float seconds = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
            //float distance = direction.Length();
            float distanceX = Math.Abs(direction.X);
            float distanceY = Math.Abs(direction.Y);

            if (!isMoving)
            {
                if (distanceX > MAX_RANGE_X || distanceY > MAX_RANGE_Y)
                {
                    isMoving = true;
                }
            }

            if (isMoving)
            {
                if (Math.Max(distanceX, distanceY) < MIN_DIST && !targetIsMoving)
                {
                    translation = target;
                    speed = Vector2.Zero;
                    direction = Vector2.Zero;
                    isMoving = false;
                }
                else 
                {
                    if (distanceX < BRAKE_DIST_X && distanceY < BRAKE_DIST_Y)
                    {
                        speed.X -= seconds * BRAKE_ACCELERATION;
                        if (speed.X < MIN_SPEED)
                            speed.X = MIN_SPEED;
                    }
                    else
                    {
                        speed.X += seconds * ACCELERATION;
                        if (speed.X > MAX_SPEED)
                            speed.X = MAX_SPEED;
                    }

                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                        translation += speed * seconds * direction;
                    }
                }            
            }
        }

        public Vector3 visibleWorld
        {
            get
            {
                Matrix invWorldToScreen = Matrix.Invert(game.worldToScreen);
                var sMin = Vector3.Transform(new Vector3(0, 0, 0), invWorldToScreen);
                var sMax = Vector3.Transform(new Vector3(screen.X, -screen.Y, 0), invWorldToScreen);
                return sMax - sMin;
            }
        }

        private void RestrictLocation()
        {
            Vector3 min = new Vector3(-World.WorldWidth + visibleWorld.X / 2, visibleWorld.Y / 2, 0);
            Vector3 max = new Vector3(+World.WorldWidth - visibleWorld.X / 2, 10000, 0);

            if (this._translation.X < min.X)
                this._translation.X = min.X;
            else if (this._translation.X > max.X)
                this._translation.X = max.X;
            if (this._translation.Y < min.Y)
                this._translation.Y = min.Y;
            else if (this._translation.Y > max.Y)
                this._translation.Y = max.Y;
        }
    }
}