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

        public const float MAX_RANGE = 5.0f;

        public const float BRAKE_DIST = 1.0f;

        public const float MIN_DIST = 0.01f;

        public const float MIN_SPEED = 0.1f, MAX_SPEED = 5.0f;

        public const float ACCELERATION = 8.0f;

        Vector3 translation;

        Vector3 source, target;

        Game1 game;

        PlayerSprite player;

        public static GameCamera CurrentCamera;

        public Matrix screenTransform
        {
            get
            {
                return Matrix.CreateTranslation(-translation.X * game.worldToScreen.M11,
                                                -translation.Y * game.worldToScreen.M12, 0.0f);
            }
        }

        public Matrix worldTransform
        {
            get
            {
                return Matrix.CreateTranslation(-translation);
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
            this.translation = position;
            this.target = position;
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
            this.target = position;
            this.speed = 0.0f;
            isMoving = false;
        }

        public void Update(GameTime gameTime)
        {
            if (player != null)
            {
                target = player.worldPosition;
            }

            Vector3 direction = target - translation;
            float seconds = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
            float distance = direction.Length();

            if (!isMoving)
            {
                if (distance > MAX_RANGE)
                {
                    isMoving = true;
                }
            }

            if (isMoving)
            {
                if (distance < MIN_DIST)
                {
                    translation = target;
                    speed = 0.0f;
                    direction = Vector3.Zero;
                    isMoving = false;
                }
                else 
                {
                    if (distance < BRAKE_DIST)
                    {
                        speed -= seconds * ACCELERATION;
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