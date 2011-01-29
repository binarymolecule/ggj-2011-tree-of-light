using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using lightseeds.Helpers;
using Microsoft.Xna.Framework.Content;

namespace lightseeds.GameObjects
{
    public class World
    {
        public class Chunk
        {
            public BoundingBox bb;
            public Vector2 position;

            public bool ShouldBeDrawn(int x, int w) {
                return position.X > x && position.X < x + w;
            }
        }

        public List<Vector2> heights;
        public List<Chunk> chunks;
        private ContentManager content;
        private Texture2D tile;
        private GraphicsDevice graphicsDevice;
        private VertexBuffer groundVbo;
        private VertexBuffer toppingVbo;
        private Effect spriteEffect;
        private Texture2D grassTopping;

        public void Load()
        {
            graphicsDevice = GameServices.GetService<GraphicsDevice>();
            content = GameServices.GetService<ContentManager>();
            tile = content.Load<Texture2D>("testtile");
            grassTopping = content.Load<Texture2D>("ground/Grass");
            spriteEffect = content.Load<Effect>("effects/sprite");

            heights = new List<Vector2>();

            Random randomizer = new Random();
            Vector2 lastHeight = new Vector2(-200, 5);
            while (lastHeight.X < 200)
            {
                float angle = (float)randomizer.NextDouble();
                float length = (float)randomizer.NextDouble() * 3.5f + 0.5f;

                Vector2 upVector = new Vector2(length, (float)Math.Min(6.5f-lastHeight.Y, 0.5f));
                Vector2 downVector = new Vector2(length, (float)Math.Max(0.75f-lastHeight.Y, -0.5f));

                Vector2 stepVector = Vector2.Lerp(upVector, downVector, angle);

                lastHeight += stepVector;
                heights.Add(lastHeight);
            }

            GenerateChunks();
            GenerateTerrain();
        }

        public void GenerateChunks()
        {
            chunks = new List<Chunk>();

            for (int i = 0; i < heights.Count-1; i++)
            {
                var v1 = heights[i];
                var v2 = heights[i+1];
                chunks.Add(new Chunk()
                {
                    position = v1
                });
            }
        }

        public void GenerateTerrain() {
            groundVbo = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), heights.Count * 2, BufferUsage.None);
            toppingVbo = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), heights.Count * 2, BufferUsage.None);

            var vertices = new VertexPositionColorTexture[heights.Count * 2];
            var topVertices = new VertexPositionColorTexture[heights.Count * 2];
            for (int i = 0; i < heights.Count; i++)
            {
                var x = heights[i].X;
                var y = heights[i].Y;

                vertices[i * 2] = new VertexPositionColorTexture(new Vector3(x, 0, 0), Color.White, Vector2.Zero);
                vertices[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(x, y, 0), Color.White, Vector2.Zero);

                topVertices[i * 2] = new VertexPositionColorTexture(new Vector3(x, y - 0.2f, 0), Color.Green, Vector2.Zero);
                topVertices[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(x, y + 0.2f, 0), Color.Green, Vector2.Zero);
            }

            groundVbo.SetData(vertices);
            toppingVbo.SetData(topVertices);
        }

        public float getHeigth(float xPos)
        {
            var a = 0;
            while (xPos > heights[a].X)
            {
                a++;
            }

            return Math.Max(heights[a - 1].Y, heights[a].Y);
        }

        public float getMinHeigth(float xPos)
        {
            var a = 0;
            while (xPos > heights[a].X)
            {
                a++;
            }

            return 0.5f * (heights[a - 1].Y + heights[a].Y);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(Game1 game, GameTime gameTime, int x, int w)
        {
            foreach (var chunk in chunks.Select((c) => c.ShouldBeDrawn(x, w)))
            {
                var texture = tile;
                var r = new Rectangle(0, 0, texture.Width, texture.Height);
                

                //sb.Draw(tile, Vector2.Zero, Color.White);
            }

            graphicsDevice.Textures[0] = tile;
            graphicsDevice.SamplerStates[0] = new SamplerState()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp
            };
            graphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                
            };

            spriteEffect.Parameters["ViewportSize"].SetValue(graphicsDevice.Viewport.ToVector());
            spriteEffect.Parameters["TextureSize"].SetValue(tile.ToVector());
            spriteEffect.Parameters["MatrixTransform"].SetValue(GameCamera.CurrentCamera.worldTransform * game.worldToScreen);
            //spriteEffect.Parameters["MatrixTransform"].SetValue(Matrix.CreateScale(0.05f));
            //spriteEffect.Parameters["TextureSampler"].SetValue(tile);
            spriteEffect.CurrentTechnique.Passes[0].Apply();

            graphicsDevice.SetVertexBuffer(groundVbo);
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, (heights.Count - 1) * 2);

            graphicsDevice.Textures[0] = grassTopping;
            spriteEffect.CurrentTechnique.Passes[0].Apply();

            graphicsDevice.SetVertexBuffer(toppingVbo);
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, (heights.Count - 1) * 2);
        }
    }
}
