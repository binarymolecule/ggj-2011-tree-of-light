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

        public const float BRAKE_DIST = 5.0f;

        public const float MIN_DIST = 0.1f;

        public const float MIN_SPEED = 5.0f, MAX_SPEED = 30.0f;

        public const float ACCELERATION = 100.0f, BRAKE_ACCELERATION = 60.0f;

        public Vector2 WORLD_OFFSET = new Vector2(0.0f, 3.0f);

        Vector2 translation;

        Vector2 source, target;

        Game1 game;

        PlayerSprite player;

        public static GameCamera CurrentCamera;

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
            bool targetIsMoving = false;
            if (player != null)
            {
                target = new Vector2(player.worldPosition.X, player.worldPosition.Y);
                targetIsMoving = (player.currentDirection != Direction.None);
            }

            Vector2 direction = target - translation;
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
                if (distance < MIN_DIST && !targetIsMoving)
                {
                    translation = target;
                    speed = 0.0f;
                    direction = Vector2.Zero;
                    isMoving = false;
                }
                else 
                {
                    if (distance < BRAKE_DIST)
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