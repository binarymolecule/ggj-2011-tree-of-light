using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace lightseeds
{
    public class GameCamera
    {
        int index;

        float speed;

        bool isMoving;

        public const float MAX_RANGE_X = 5.0f, MAX_RANGE_Y = 0.5f;

        public const float BRAKE_DIST_X = 5.0f, BRAKE_DIST_Y = 2.0f;

        public const float MIN_DIST = 0.1f;

        public const float MIN_SPEED = 5.0f, MAX_SPEED = 30.0f;

        public const float ACCELERATION = 100.0f, BRAKE_ACCELERATION = 60.0f;

        static public Vector2 WORLD_OFFSET = new Vector2(0.0f, 2.0f);

        static public Vector2 MIN_COORDS = new Vector2(-180.0f + 0.25f * Game1.WORLD_SCREEN_WIDTH, 0.5f * Game1.WORLD_SCREEN_HEIGHT) + WORLD_OFFSET;
        static public Vector2 MAX_COORDS = new Vector2(180.0f - 0.25f * Game1.WORLD_SCREEN_WIDTH, 100.0f) + WORLD_OFFSET;

        Vector2 translation;

        Vector2 target;

        Game1 game;

        PlayerSprite player;

        public static GameCamera CurrentCamera;

        private bool returnMode = false;

        private float returnFactor;

        private Vector2 returnSource, returnTarget;

        public void StartReturnMode(Vector2 retTarget)
        {
            returnMode = true;
            returnFactor = 0.0f;
            returnSource = translation;
            returnTarget = retTarget;
        }

        public Matrix screenTransform
        {
            get
            {
                Vector3 v = new Vector3(-(translation.X - WORLD_OFFSET.X) * game.worldToScreen.M11, 
                                        -(translation.Y - WORLD_OFFSET.Y) * game.worldToScreen.M22, 0.0f);
                return Matrix.CreateTranslation(v);
            }
        }

        public Matrix worldTransform
        {
            get
            {
                return Matrix.CreateTranslation(-(new Vector3(translation.X - WORLD_OFFSET.X, translation.Y - WORLD_OFFSET.Y, 0.0f)));
            }
        }

        public GameCamera(Game1 game, int index, Vector3 position)
        {
            this.game = game;
            this.index = index;
            this.player = null;

            Center(position);
        }

        public void FollowPlayer(PlayerSprite player)
        {
            this.player = player;
            this.speed = 0.0f;
            isMoving = false;
        }

        public void Center(Vector3 position)
        {
            this.player = null;
            this.translation = new Vector2(position.X, position.Y);
            this.target = new Vector2(position.X, position.Y);
            this.speed = 0.0f;
            isMoving = false;
        }

        public void Reset()
        {
            Center(Vector3.Zero);
        }

        public void MoveTo(Vector3 position)
        {
            this.player = null;
            this.target = new Vector2(position.X, position.Y);
            this.speed = 0.0f;
            isMoving = false;
        }

        public void Update(GameTime gameTime)
        {
            if (returnMode)
            {
                returnFactor += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
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
                if (target.X < MIN_COORDS.X)
                    target.X = MIN_COORDS.X;
                else if (target.X > MAX_COORDS.X)
                    target.X = MAX_COORDS.X;
                if (target.Y < MIN_COORDS.Y)
                    target.Y = MIN_COORDS.Y;
                else if (target.Y > MAX_COORDS.Y)
                    target.Y = MAX_COORDS.Y;
                targetIsMoving = !player.isStunned && (player.currentXAcceleration != 0 || player.currentYAcceleration !=0 );
            }

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
                    speed = 0.0f;
                    direction = Vector2.Zero;
                    isMoving = false;
                }
                else 
                {
                    if (distanceX < BRAKE_DIST_X && distanceY < BRAKE_DIST_Y)
                    {
                        speed -= seconds * BRAKE_ACCELERATION;
                        if (speed < MIN_SPEED)
                            speed = MIN_SPEED;
                    }
                    else
                    {
                        speed += seconds * ACCELERATION;
                        if (speed > MAX_SPEED)
                            speed = MAX_SPEED;
                    }
                    direction.Normalize();
                    translation += speed * seconds * direction;
                }            
            }
        }
    }
}