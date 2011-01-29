using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace XnaAdventure
{
    /// <summary>
    /// Implements a 2D camera which can scroll horizontally and vertically.
    /// </summary>
    public class GameCamera
    {
        protected int maxX_, maxY_;
        protected int offsetX_, offsetY_;
        protected Vector2 offsetXY_;
        protected int timer_;
        protected int sourceX_, sourceY_;
        protected int targetX_, targetY_;
        protected float motionStep_;

        public GameCamera(int sceneWidth, int sceneHeight)
        {
            maxX_ = sceneWidth - MainGame.SCREEN_WIDTH;
            maxY_ = sceneHeight - MainGame.SCREEN_HEIGHT;
            offsetX_ = 0;
            offsetY_ = 0;
            offsetXY_ = new Vector2(0, 0);
            timer_ = 0;
            sourceX_ = 0;
            sourceY_ = 0;
            targetX_ = 0;
            targetY_ = 0;
            motionStep_ = 0;
        }

        /// <summary>
        /// Center position of camera to given position.
        /// </summary>
        public void Center(int cx, int cy)
        {
            offsetX = cx - MainGame.SCREEN_WIDTH / 2 - 16;
            offsetY = cy - MainGame.SCREEN_HEIGHT / 2 - 24;
        }

        /// <summary>
        /// Reset position of camera.
        /// </summary>
        public void Reset()
        {
            offsetX_ = 0;
            offsetY_ = 0;
            offsetXY_ = new Vector2(0, 0);
        }

        /// <summary>
        /// Get/set horizontal offset in pixels.
        /// </summary>
        public int offsetX
        {
            get
            {
                return offsetX_;
            }
            set
            {
                offsetX_ = (value < 0 ? 0 : (value > maxX_ ? maxX_ : value));
                offsetXY_.X = offsetX_;
            }
        }

        /// <summary>
        /// Get/set vertical offset in pixels.
        /// </summary>
        public int offsetY
        {
            get
            {
                return offsetY_;
            }
            set
            {
                offsetY_ = (value < 0 ? 0 : (value > maxY_ ? maxY_ : value));
                offsetXY_.Y = offsetY_;
            }
        }

        /// <summary>
        /// Get/set width of the scene in pixels.
        /// </summary>
        public int sceneWidth
        {
            get
            {
                return maxX_ + MainGame.SCREEN_WIDTH;
            }
            set
            {
                maxX_ = value - MainGame.SCREEN_WIDTH; ;
            }
        }

        /// <summary>
        /// Get/set height of the scene in pixels.
        /// </summary>
        public int sceneHeight
        {
            get
            {
                return maxY_ + MainGame.SCREEN_HEIGHT;
            }
            set
            {
                maxY_ = value - MainGame.SCREEN_HEIGHT;
            }
        }

        /// <summary>
        /// Return horizontal/vertical offset in pixels.
        /// </summary>
        public Vector2 offsetXY
        {
            get
            {
                return offsetXY_;
            }
        }

        /// <summary>
        /// Set target for camera motion.
        /// </summary>
        /// <param name="tx">X-coordinate of target</param>
        /// <param name="ty">Y-coordinate of target</param>
        /// <param name="time">Time for moving to target in ms</param>
        public void MoveTo(int tx, int ty, int time)
        {
            timer_ = time;
            sourceX_ = offsetX_;
            sourceY_ = offsetY_;
            targetX_ = tx - MainGame.SCREEN_WIDTH / 2 - 16;
            targetY_ = ty - MainGame.SCREEN_HEIGHT / 2 - 24;
            //targetX_ = (targetX_ < 0 ? 0 : (targetX_ > maxX_ ? maxX_ : targetX_));
            //targetY_ = (targetY_ < 0 ? 0 : (targetY_ > maxY_ ? maxY_ : targetY_));
            motionStep_ = 1.0f / (float)time;
        }

        /// <summary>
        /// Update game camera motion.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (timer_ > 0)
            {
                timer_ -= gameTime.ElapsedGameTime.Milliseconds;
                if (timer_ <= 0)
                {
                    timer_ = 0;
                    offsetX = targetX_;
                    offsetY = targetY_;
                } else
                {
                    float a = motionStep_ * timer_;
                    float b = 1.0f - a;
                    offsetX = (int)Math.Round(a * sourceX_ + b * targetX_);
                    offsetY = (int)Math.Round(a * sourceY_ + b * targetY_);                    
                }
            }
        }
    }
}