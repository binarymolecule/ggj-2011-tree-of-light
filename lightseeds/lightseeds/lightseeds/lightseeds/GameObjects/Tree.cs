using System;
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

        public string[] names = new string[] {
"Birchbud",
"Bonehedge",
"Brownmane",
"Cedarblossom",
"Crownbush",
"Lockshoot",
"Lowpine",
"Marrowbriar",
"Mossalder",
"Slowoak",
"Strangecraft",
"Weedbirch",
"Wildrowan",
"Winterskin",
"Wisehazel",
"Beamleg",
"Beardbloom",
"Bramblebeard",
"Bramblefoot",
"Brightvine",
"Budbone",
"Calmoak",
"Copsemarrow",
"Fallmaple",
"Maplehedge",
"Shrubcrown",
"Skinwand",
"Weedpalm",
"Willowbeard",
"Winterelm",
"Ashherb",
"Beamsmile",
"Brighttimber",
"Calmmaple",
"Craftelm",
"Farelm",
"Junglemaple",
"Oakleaf",
"Palmtalon",
"Rootbone",
"Springweed",
"Summeryew",
"Talontwig",
"Timberoak",
"Willowbraid",
"Armstaff",
"Baldalder",
"Budbeard",
"Cedarbark",
"Copsecrown",
"Fallalder",
"Furylimb",
"Greenherb",
"Hollyshoot",
"Maplecrown",
"Oaktalon",
"Pinefury",
"Quickrowan",
"Toothmoss",
"Wisebriar",
"Ashbramble",
"Blossomskin",
"Bonejungle",
"Clawbloom",
"Elmtrunk",
"Herbfoot",
"Hollyseed",
"Honeybeard",
"Jungleskin",
"Palmsprig",
"Rowanlimb",
"Smallhazel",
"Strangemaple",
"Weedcedar",
"Wiseoak"
        };

        public Vector3 position;
        public Vector2 offset;
        public Vector2 screenSize;
        public Vector2 fruitOffset;
        public Vector2 fruitSize;
        public float groundHeight;

        public Texture2D texture, deadTexture, xrayTexture;

        private static Random random = new Random();

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
        public float glowStrength;
        public float glowRange;
        private float fallSpeed;

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

        public Tree(TreeCollection coll, Vector3 pos, TreeType type, bool isBlueprint, String preferredName)
        {
            position = pos;
            treeType = type;
            parentCollection = coll;
            fruitSize = new Vector2(parentCollection.fruitTexture.Width, parentCollection.fruitTexture.Height);
            texture = parentCollection.textures[(int)treeType];
            if (treeType != TreeType.BASE)
            {
                deadTexture = parentCollection.textures[(int)treeType + 4];
                xrayTexture = parentCollection.textures[(int)treeType + 8];
            }
            else
            {
                deadTexture = xrayTexture = texture;
            }

            screenSize = new Vector2(texture.Width, texture.Height);
            if (preferredName == "")
                name = names[random.Next(names.Length - 1)];
            else
                name = preferredName;

            if (treeType == TreeType.BASE)
            {
                offset = new Vector2(-0.5f * screenSize.X, -screenSize.Y + 210.0f);
                fruitOffset = new Vector2(0.0f, -96.0f);
            }
            else
            {
                offset = new Vector2(-0.5f * screenSize.X, -screenSize.Y + 64.0f);
                fruitOffset = new Vector2(0.0f, -96.0f);
            }
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
                    fruitTime = 15.0f;
                    resistance = 1.0f;
                    lifeSpan = 0.0f;
                    price = 0;
                    descriptionLines[0] = "Basic Tree";
                    descriptionLines[1] = "is Basic";
                    break;
                case TreeType.FIGHTER:
                    growthTime = 8.0f;
                    fruitTime = 20.0f;
                    resistance = 0.4f;
                    lifeSpan = 60.0f;
                    price = 3;
                    descriptionLines[0] = "Fighter Tree";
                    descriptionLines[1] = "fight for the light";
                    break;
                case TreeType.MOTHER:
                    growthTime = 15.0f;
                    fruitTime = 5.0f;
                    resistance = 0.6f;
                    lifeSpan = 80.0f;
                    price = 8;
                    descriptionLines[0] = "The Mother Tree";
                    descriptionLines[1] = "a lot of children";
                    break;
                case TreeType.PAWN:
                    growthTime = 5.0f;
                    fruitTime = 7.0f;
                    resistance = 0.6f;
                    lifeSpan = 25.0f;
                    price = 2;
                    descriptionLines[0] = "Pawn Tree";
                    descriptionLines[1] = "The Ace of Rage";
                    break;
                case TreeType.TANK:
                    growthTime = 25.0f;
                    fruitTime = 25.0f;
                    resistance = 0.3f;
                    lifeSpan = 130.0f;
                    price = 10;
                    descriptionLines[0] = "Tank Tree";
                    descriptionLines[1] = "you shall not pass";
                    break;
            }
            descriptionLines[2] = "Price: " + price.ToString() + " seeds";
        }

        public void Update(GameTime gameTime)
        {
            switch (status)
            {
                case TreeStatus.SEED:
                    fallSpeed += 25f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    position.Y -= (float)(gameTime.ElapsedGameTime.TotalSeconds) * fallSpeed;
                    this.glowRange = 0.8f + 0.2f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.35);
                    if (position.Y < groundHeight)
                    {
                        status = TreeStatus.PLANTED;
                        position.Y = groundHeight;
                    }
                    break;
                case TreeStatus.PLANTED:
                    this.glowStrength = 0.4f + 0.15f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5);
                    this.glowRange = 1f + 0.2f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.35);
                    growth += (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) / growthTime;
                    if (growth > 1.0f)
                    {
                        status = TreeStatus.MATURE;
                        growth = 1.0f;
                    }
                    break;
                case TreeStatus.MATURE:
                    this.glowStrength = 0.4f + 0.15f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5);
                    this.glowRange = 1f + 0.2f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.35);
                    // grow fruit
                    currentFruitTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (currentFruitTime > fruitTime)
                    {
                        parentCollection.game.seedCollection.SpawnSeed(worldPosition + new Vector3(0, 2, 0));
                        currentFruitTime = 0;
                    }
                    // decrease life span of tree
                    if (treeType != TreeType.BASE)
                    {
                        lifeSpan -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                        if (lifeSpan < 0.0f)
                            status = TreeStatus.DIED;
                    }
                    break;
                case TreeStatus.DIED:
                    
                    glowStrength -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    glowStrength = Math.Max(glowStrength, 0);
                    lifeSpan -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                    if (lifeSpan < -3.0f)
                        status = TreeStatus.KILLED;
                    break;
                case TreeStatus.KILLED:
                    glowStrength -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    glowStrength = Math.Max(glowStrength, 0);

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
                spriteBatch.Draw(texture, screenPosition + offset, Color.White);
                if (status == TreeStatus.MATURE)
                {
                    float fruitScale = (float)currentFruitTime / (float)fruitTime;
                    Rectangle fruitRect = new Rectangle((int)(screenPosition.X + fruitOffset.X - 0.5f * fruitScale * fruitSize.X),
                                                        (int)(screenPosition.Y + fruitOffset.Y - 0.5f * fruitScale * fruitSize.Y),
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
                                                                (int)(screenPosition.Y - growth * screenSize.Y + 64.0f),
                                                                (int)(growth * screenSize.X), (int)(growth * screenSize.Y));
                            spriteBatch.Draw(texture, rectangle, Color.White);
                        }
                        break;
                    case TreeStatus.MATURE:
                        {
                            float fruitScale = (float)currentFruitTime / (float)fruitTime;
                            Rectangle fruitRect = new Rectangle((int)(screenPosition.X + fruitOffset.X - 0.5f * fruitScale * fruitSize.X),
                                                                (int)(screenPosition.Y + fruitOffset.Y - 0.5f * fruitScale * fruitSize.Y),
                                                                (int)(fruitScale * fruitSize.X), (int)(fruitScale * fruitSize.Y));
                            spriteBatch.Draw(texture, screenPosition + offset, Color.White);
                            spriteBatch.Draw(parentCollection.fruitTexture, fruitRect, Color.White);
                        }
                        break;
                    case TreeStatus.KILLED:
                        {
                            Rectangle rectangle = new Rectangle((int)(screenPosition.X - 0.5f * growth * screenSize.X),
                                                                (int)(screenPosition.Y - growth * screenSize.Y + 64.0f),
                                                                (int)(growth * screenSize.X), (int)(growth * screenSize.Y));
                            var fade = Color.Lerp(Color.White, Color.Black, 3 - 3 * growth);
                            spriteBatch.Draw(deadTexture, rectangle, fade);
                        }
                        break;
                    case TreeStatus.DIED:
                        {
                            spriteBatch.Draw(deadTexture, screenPosition + offset, Color.White);
                        }
                        break;
                    case TreeStatus.BLUEPRINT:
                        {
                            bool buildable = (parentCollection.game.seedCollection.collectedSeedCount >= price);
                            spriteBatch.Draw(xrayTexture, screenPosition + offset, buildable ? Color.White : Color.Red);
                            break;
                        }
                }
            }
        }

        public bool OccupiesPosition(float posX)
        {
            return (Math.Abs(posX - worldPosition.X) < MIN_DISTANCE);
        }

        public String GetStatusInfo()
        {
            String info = "";
            switch (status)
            {
                case TreeStatus.PLANTED:
                    info += descriptionLines[0] + "\n";
                    info += "Growth: " + (int)(100.0f * (float)growth) + "%";
                    break;
                case TreeStatus.MATURE:
                    info += descriptionLines[0] + "\n";
                    info += "Fruit: " + (int)(100.0f * (float)currentFruitTime / (float)fruitTime) + "%";
                    break;
                case TreeStatus.BLUEPRINT:
                    info += String.Join("\n", descriptionLines[1], descriptionLines[2], descriptionLines[3]);
                    break;
            }
            if (status == TreeStatus.MATURE && treeType != TreeType.BASE)
            {
                info += "\nLife: " + (int)lifeSpan + " sec";
            }
            return info;
        }
    }
}
