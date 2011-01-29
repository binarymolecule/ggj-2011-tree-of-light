﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lightseeds.GameObjects
{
    /*
    class Branch
    {
        Matrix fatherTransform;

        Vector3 localPosition;

        Vector2 pivot;

        float length, orientation, maxRotation;

        float timer;

        Texture2D texture;

        Branch[] branches;

        public Branch(Texture2D texture, Vector2 position, Vector2 size, float orientation)
        {
            this.texture = texture;
            this.localPosition = position;
            this.length = 1.0f;
            this.orientation = orientation;
            this.maxRotation = MathHelper.ToRadians(30.0f);
            this.pivot = new Vector2(0.5f, 1.0f);
            this.timer = 0.0f;
            this.branches = new Branch[0];
        }

        public void AddBranches(Branch[] b)
        {
            branches = b;
        }

        public void Update(GameTime gameTime, Vector2 position)
        {
            foreach (Branch b in branches)
            {
                b.Update(, gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector3 pos = Vector3.Transform(localPosition, fatherTransform);
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), null, Color.White, orientation, pivot, SpriteEffects.None, 1.0f);
        }
    }
    */
    public class Tree
    {
        public const float MIN_DISTANCE = 4.0f;
        
        public Vector3 position;
        public Vector2 offset;
        public Vector2 screenSize;
        public Vector2 fruitOffset;
        public Vector2 fruitSize;

        public TreeCollection parentCollection;


        #region gameplay properties

        public float growthTime;
        public float growth;
        public float lifeSpan;
        public float resistance = 0.5f;
        public float fruitTime;
        public int price;
        public string name;
        public double currentFruitTime;
        
        #endregion

        public bool RemoveOnNextUpdate = false;
        public enum TreeStatus
        {
            PLANTED,
            MATURE,
            DIED,
            KILLED,
            BLUEPRINT
        }

        public TreeStatus status;

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
                Vector3 screenPos = Vector3.Transform(position, parentCollection.game.worldToScreen);
                return new Vector2(screenPos.X, screenPos.Y);
            }
        }

        public Tree(TreeCollection coll, Vector3 position, TreeType type)
        {
            this.parentCollection = coll;
            this.position = position;
            this.screenSize = new Vector2(parentCollection.texture.Width, parentCollection.texture.Height);
            this.offset = new Vector2(-0.5f * screenSize.X, -screenSize.Y + 4.0f);
            this.fruitSize = new Vector2(parentCollection.fruitTexture.Width, parentCollection.fruitTexture.Height);
            this.fruitOffset = new Vector2(-0.5f * fruitSize.X, -screenSize.Y + 32.0f);
            this.growth = 0.1f;
            this.status = TreeStatus.PLANTED;

            switch (type)
            {
                case TreeType.BASE:
                    this.status = TreeStatus.MATURE;
                    this.growth = 0.1f;
                    break;
                case TreeType.FIGHTER:
                    this.growthTime = 20.0f;
                    this.fruitTime = 40.0f;
                    break;
                case TreeType.MOTHER:
                    this.growthTime = 30.0f;
                    this.fruitTime = 10.0f;
                    break;
                case TreeType.PAWN:
                    this.growthTime = 10.0f;
                    this.fruitTime = 20.0f;
                    break;
                case TreeType.TANK:
                    this.growthTime = 60.0f;
                    this.fruitTime = 30.0f;
                    break;
                case TreeType.BLUEPRINT:
                    this.status = TreeStatus.BLUEPRINT;
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            switch (status)
            {
                case TreeStatus.PLANTED:
                    growth += (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / growthTime;
                    if (growth > 1.0f)
                    {
                        status = TreeStatus.MATURE;
                        growth = 1.0f;
                    }
                    break;
                case TreeStatus.KILLED:
                    growth -= (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / growthTime;
                    if (growth < 0.0f)
                    {
                        this.RemoveOnNextUpdate = true;
                        growth = 0.0f;
                    }
                    break;
                case TreeStatus.MATURE:
                    currentFruitTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (currentFruitTime > this.fruitTime)
                    {
                        this.parentCollection.game.seedCollection.SpawnSeed(worldPosition + new Vector3(0, 2, 0));
                        currentFruitTime = 0;
                    }
                    break;
                case TreeStatus.BLUEPRINT:
                    growth = 1.0f;
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (status)
            {
                case TreeStatus.MATURE:
                    {
                        float fruitScale = (float)currentFruitTime / (float)fruitTime;
                        Rectangle rectangle = new Rectangle((int)(screenPosition.X + offset.X), (int)(screenPosition.Y + offset.Y),
                                                            (int)screenSize.X, (int)screenSize.Y);
                        Rectangle fruitRect = new Rectangle((int)(screenPosition.X + fruitScale * fruitOffset.X),
                                                            (int)(screenPosition.Y + fruitOffset.Y - 0.5f * fruitScale * fruitSize.Y),
                                                            (int)(fruitScale * fruitSize.X), (int)(fruitScale * fruitSize.Y));
                        spriteBatch.Draw(parentCollection.texture, rectangle, Color.Green);
                        spriteBatch.Draw(parentCollection.fruitTexture, fruitRect, Color.White);
                    }
                    break;
                case TreeStatus.PLANTED:
                    {
                        Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * growth * screenSize.X),
                                                            (int)(screenPosition.Y - growth * screenSize.Y + 4.0f),
                                                            (int)(growth * screenSize.X), (int)(growth * screenSize.Y));
                        spriteBatch.Draw(parentCollection.texture, rectangle, Color.White);
                    }
                    break;
                case TreeStatus.KILLED:
                    {
                        Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * growth * screenSize.X), (int)(screenPosition.Y - growth * screenSize.Y + 4.0f),
                                                            (int)(growth * screenSize.X), (int)(growth * screenSize.Y));
                        spriteBatch.Draw(parentCollection.texture, rectangle, Color.White);
                    }
                    break;
                case TreeStatus.DIED:
                    {
                        Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * screenSize.X), (int)(screenPosition.Y - screenSize.Y + 4.0f),
                                                            (int)screenSize.X, (int)screenSize.Y);
                        spriteBatch.Draw(parentCollection.texture, rectangle, Color.Gray);
                    }
                    break;
                case TreeStatus.BLUEPRINT:
                    {
                        Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * screenSize.X), (int)(screenPosition.Y - screenSize.Y + 4.0f),
                                                  (int)screenSize.X, (int)screenSize.Y);
                        spriteBatch.Draw(parentCollection.texture, rectangle, Color.Black);
                        break;
                    }

            }
        }

        public bool OccupiesPosition(float posX)
        {
            return (Math.Abs(posX - worldPosition.X) < MIN_DISTANCE);
        }
    }
}
