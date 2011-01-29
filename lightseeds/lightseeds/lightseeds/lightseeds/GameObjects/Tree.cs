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

        public const float SEED_FALL_SPEED = 4.0f;

        public Vector3 position;
        public Vector2 offset;
        public Vector2 screenSize;
        public Vector2 fruitOffset;
        public Vector2 fruitSize;
        public float groundHeight;

        public TreeCollection parentCollection;

        public TreeType treeType;

        #region gameplay properties

        public float growthTime;
        public float growth;
        public float lifeSpan;
        public float resistance;
        public float fruitTime;
        public int price;
        public string name;
        public double currentFruitTime;
        
        #endregion

        public bool RemoveOnNextUpdate = false;
        public enum TreeStatus
        {
            SEED,
            PLANTED,
            MATURE,
            DIED,
            KILLED,
            BLUEPRINT
        }

        public TreeStatus status;
        public string[] descriptionLines = new string[4];

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

        public Tree(TreeCollection coll, Vector3 pos, TreeType type, bool isBlueprint)
        {
            position = pos;
            treeType = type;
            parentCollection = coll;
            screenSize = new Vector2(parentCollection.texture.Width, parentCollection.texture.Height);
            offset = new Vector2(-0.5f * screenSize.X, -screenSize.Y + 4.0f);
            fruitSize = new Vector2(parentCollection.fruitTexture.Width, parentCollection.fruitTexture.Height);
            fruitOffset = new Vector2(-0.5f * fruitSize.X, -screenSize.Y + 32.0f);
            growth = 0.1f;
            status = TreeStatus.SEED;
            groundHeight = parentCollection.game.world.getHeigth(position.X);

            if (isBlueprint)
            {
                status = TreeStatus.BLUEPRINT;
                position.Y = groundHeight;
            }

            switch (treeType)
            {
                case TreeType.BASE:
                    position.Y = groundHeight;
                    status = TreeStatus.MATURE;
                    growth = 1.0f;
                    fruitTime = 25.0f;
                    resistance = 1.0f;
                    lifeSpan = -1.0f;
                    descriptionLines[0] = "Basic Tree";
                    descriptionLines[1] = "is Basic";
                    break;
                case TreeType.FIGHTER:
                    growthTime = 14.0f;
                    fruitTime = 30.0f;
                    resistance = 0.4f;
                    lifeSpan = 90.0f;
                    descriptionLines[0] = "Fighter Tree";
                    descriptionLines[1] = "is very angry";
                    break;
                case TreeType.MOTHER:
                    growthTime = 40.0f;
                    fruitTime = 10.0f;
                    resistance = 0.6f;
                    lifeSpan = 150.0f;
                    descriptionLines[0] = "The Mana Tree";
                    descriptionLines[1] = "ya know?!";
                    break;
                case TreeType.PAWN:
                    growthTime = 7.0f;
                    fruitTime = 25.0f;
                    resistance = 0.6f;
                    lifeSpan = 30.0f;
                    descriptionLines[0] = "Pawn Tree";
                    descriptionLines[1] = "The Ace of Rage";
                    break;
                case TreeType.TANK:
                    growthTime = 60.0f;
                    fruitTime = 30.0f;
                    resistance = 0.2f;
                    lifeSpan = 200.0f;
                    descriptionLines[0] = "Tank Tree";
                    descriptionLines[1] = "guess";
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            switch (status)
            {
                case TreeStatus.SEED:
                    position.Y -= (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) * SEED_FALL_SPEED;
                    if (position.Y < groundHeight)
                    {
                        status = TreeStatus.PLANTED;
                        position.Y = groundHeight;
                    }
                    break;
                case TreeStatus.PLANTED:
                    growth += (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / growthTime;
                    if (growth > 1.0f)
                    {
                        status = TreeStatus.MATURE;
                        growth = 1.0f;
                    }
                    break;
                case TreeStatus.MATURE:
                    currentFruitTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (currentFruitTime > fruitTime)
                    {
                        parentCollection.game.seedCollection.SpawnSeed(worldPosition + new Vector3(0, 2, 0));
                        currentFruitTime = 0;
                    }
                    break;
                case TreeStatus.KILLED:
                    growth -= (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / growthTime;
                    if (growth < 0.0f)
                    {
                        RemoveOnNextUpdate = true;
                        growth = 0.0f;
                    }
                    break;
                case TreeStatus.BLUEPRINT:
                    growth = 1.0f;
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (treeType == TreeType.BASE)
            {
                spriteBatch.Draw(parentCollection.baseTexture, new Vector2(screenPosition.X - 0.5f * parentCollection.baseTexture.Width, 
                                                                           screenPosition.Y - parentCollection.baseTexture.Height + 32.0f), Color.White);
                if (status == TreeStatus.MATURE)
                {
                    float fruitScale = (float)currentFruitTime / (float)fruitTime;
                    Rectangle fruitRect = new Rectangle((int)(screenPosition.X + fruitScale * fruitOffset.X),
                                                        (int)(screenPosition.Y + 0.5f * screenSize.Y - 0.5f * fruitScale * fruitSize.Y),
                                                        (int)(fruitScale * fruitSize.X), (int)(fruitScale * fruitSize.Y));
                    spriteBatch.Draw(parentCollection.fruitTexture, fruitRect, Color.White);
                }
            }
            else
            {
                switch (status)
                {
                    case TreeStatus.SEED:
                        {
                            float fruitScale = 0.25f;
                            Rectangle fruitRect = new Rectangle((int)(screenPosition.X - 0.5f * fruitScale * fruitSize.X),
                                                                (int)(screenPosition.Y - 0.5f * fruitScale * fruitSize.Y),
                                                                (int)(fruitScale * fruitSize.X), (int)(fruitScale * fruitSize.Y));
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
                    case TreeStatus.KILLED:
                        {
                            Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * growth * screenSize.X), (int)(screenPosition.Y - growth * screenSize.Y + 4.0f),
                                                                (int)(growth * screenSize.X), (int)(growth * screenSize.Y));
                            spriteBatch.Draw(parentCollection.texture, rectangle, Color.White);
                        }
                        break;
                    case TreeStatus.DIED:
                        {
                            Rectangle rectangle = new Rectangle((int)(screenPosition.X + offset.X), (int)(screenPosition.Y + offset.Y),
                                                                (int)screenSize.X, (int)screenSize.Y);
                            spriteBatch.Draw(parentCollection.texture, rectangle, Color.Gray);
                        }
                        break;
                    case TreeStatus.BLUEPRINT:
                        {
                            Rectangle rectangle = new Rectangle((int)(screenPosition.X + offset.X), (int)(screenPosition.Y + offset.Y),
                                                                (int)screenSize.X, (int)screenSize.Y);
                            spriteBatch.Draw(parentCollection.texture, rectangle, Color.Black);
                            break;
                        }
                }
            }
        }

        public bool OccupiesPosition(float posX)
        {
            return (Math.Abs(posX - worldPosition.X) < MIN_DISTANCE);
        }
    }
}
