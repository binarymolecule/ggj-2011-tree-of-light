using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace lightseeds
{
    public class GameCamera
    {
        int index;

        Vector2 speed;

        bool reachedSpeedX, reachedSpeedY;

        public const float APPROACH_DIST = 1.0f;

        public const float MIN_DIST = 0.01f;

        public const float MAX_SPEED = 5.0f;

        public const float ACCELERATION = 0.25f;

        Vector3 translation;

        Vector3 source, target;

        Game1 game;

        PlayerSprite player;

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
            this.speed = Vector2.Zero;
            reachedSpeedX = reachedSpeedY = false;
        }

        public void Center(Vector3 position)
        {
            this.player = null;
            this.translation = position;
            this.target = position;
            this.speed = Vector2.Zero;
            reachedSpeedX = reachedSpeedY = false;
        }

        public void Reset()
        {
            Center(Vector3.Zero);
        }

        public void MoveTo(Vector3 position)
        {
            this.player = null;
            this.target = position;
            this.speed = Vector2.Zero;
            reachedSpeedX = reachedSpeedY = false;
        }

        public void Update(GameTime gameTime)
        {
            if (player != null)
            {
                target = player.worldPosition;
            }
            Vector3 direction = target - translation;
            float seconds = 0.001f * gameTime.ElapsedGameTime.Milliseconds;

            if (Math.Abs(direction.X) > (reachedSpeedX ? APPROACH_DIST : MIN_DIST))
            {
                speed.X += Math.Sign(direction.X) * seconds * ACCELERATION;
                translation.X += speed.X;
                reachedSpeedX = (Math.Abs(direction.X) > APPROACH_DIST);
            }
            else if (Math.Abs(direction.X) > MIN_DIST)
            {
                speed.X -= Math.Sign(direction.X) * seconds * ACCELERATION;
                translation.X += speed.X;
                if ((speed.X > 0 && translation.X > target.X) ||
                    (speed.X < 0 && translation.X < target.X))
                {
                    translation.X = target.X;
                    speed.X = 0.0f;
                    reachedSpeedX = false;
                }

            }
            else
            {
                translation.X = target.X;
                speed.X = 0.0f;
                reachedSpeedX = false;
            }
            if (Math.Abs(direction.Y) > (reachedSpeedY ? APPROACH_DIST : MIN_DIST))
            {
                speed.Y += Math.Sign(direction.Y) * seconds * ACCELERATION;
                translation.Y += speed.Y;
                reachedSpeedY = (Math.Abs(direction.Y) > APPROACH_DIST);
            }
            else if (Math.Abs(direction.Y) > MIN_DIST)
            {
                speed.Y -= Math.Sign(direction.Y) * seconds * ACCELERATION;
                translation.Y += speed.Y;
                if ((speed.Y > 0 && translation.Y > target.Y) ||
                    (speed.Y < 0 && translation.Y < target.Y))
                {
                    translation.Y = target.Y;
                    speed.Y = 0.0f;
                    reachedSpeedY = false;
                }
            }
            else
            {
                translation.Y = target.Y;
                speed.Y = 0.0f;
                reachedSpeedY = false;
            }

        }
    }
}