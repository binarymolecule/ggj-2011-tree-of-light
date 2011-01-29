using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace lightseeds
{
    public class GameCamera
    {
        int index;

        int timer;

        float speed;

        public const float MIN_SPEED = 0.04f, MAX_SPEED = 0.08f;

        public const float ACCELERATION = 1.1f;

        Vector3 translation;

        Vector3 source, target;

        Game1 game;

        PlayerSprite player;

        public Matrix screenTransform {
            get
            {
                return Matrix.CreateTranslation(-translation.X * game.worldToScreen.M11,
                                                -translation.Y * game.worldToScreen.M12, 0.0f);
            }
        }

        public Matrix worldTransform {
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
            this.timer = 0;
        }

        public void Center(Vector3 position)
        {
            timer = 0;
            speed = 0.0f;
            translation = position;
        }

        public void Reset()
        {
            translation = Vector3.Zero;
        }
        
        public void MoveTo(Vector3 position, int milliseconds)
        {
            player = null;
            timer = milliseconds;
            source = translation;
            target = position;
            speed = 1.0f / (float)milliseconds;
        }

        public void Update(GameTime gameTime)
        {
            if (player != null)
            {
                Vector3 direction = player.worldPosition - translation;
                if (direction.LengthSquared() < 0.01f)
                {

                }
                else
                {
                    translation += speed * direction;
                }
            }
            else
            {
                if (timer > 0)
                {
                    timer -= gameTime.ElapsedGameTime.Milliseconds;
                    if (timer <= 0)
                    {
                        Center(target);
                    }
                    else
                    {
                        float a = speed * timer;
                        float b = 1.0f - a;
                        translation.X = (int)Math.Round(a * source.X + b * target.X);
                        translation.Y = (int)Math.Round(a * source.Y + b * target.Y);
                    }
                }
            }
        }
    }
}