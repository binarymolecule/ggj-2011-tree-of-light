using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using lightseeds.Helpers;

namespace lightseeds.GameObjects
{
    public class ParticleCollection
    {
        public class Particle
        {
            public Vector3 position;
            public float rotation;
            public float scale;
            public Vector3 velocity;

            public float velocityDamping;
            public float rotationDamping;

            public float lifeTime;
            public float age;
            public float spin = 0.2f;
            public Texture2D texture;

            public Action OnDestroy;
            public float alpha;
        }

        private Game1 game;
        private List<Texture2D> smokeTextures;
        private List<Particle> particles;
        private SpriteBatch spriteBatch;
        public Random random;

        public ParticleCollection(Game1 game)
        {
            this.game = game;
            this.spriteBatch = new SpriteBatch(game.GraphicsDevice);
            this.random = new Random();
            this.particles = new List<Particle>();

            smokeTextures = new List<Texture2D>();
            smokeTextures.Add(game.Content.Load<Texture2D>("Smoke/Smoke_1"));
            smokeTextures.Add(game.Content.Load<Texture2D>("Smoke/Smoke_2"));
            smokeTextures.Add(game.Content.Load<Texture2D>("Smoke/Smoke_3"));
            smokeTextures.Add(game.Content.Load<Texture2D>("Smoke/Smoke_4"));
            smokeTextures.Add(game.Content.Load<Texture2D>("Smoke/Smoke_5"));


        }

        public Particle SpawnParticle(Vector3 position)
        {
            var p = new Particle()
            {
                position = position,
                texture = smokeTextures[random.Next(smokeTextures.Count-1)],
                lifeTime = 1.5f + (float)random.NextDouble() * 2.5f,
                rotation = (float)random.NextDouble(),
                spin = (float)random.NextDouble() * 0.7f
            };

            particles.Add(p);

            return p;
        }

        public void Update(GameTime gameTime)
        {
            List<Particle> toRemove = new List<Particle>();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var p in particles)
            {
                p.position += p.velocity * dt;
                p.rotation += p.spin * dt;
                p.age += dt;

                float ramp = 0.9f;
                float slope = 1;
                //p.scale = (p.age < ramp ? (1f - slope) + slope * (p.age / ramp) : 1);
                p.scale = (1 - ramp) + ramp * (p.age / p.lifeTime);

                float alphaRamp = 0.6f;
                float alphaSlope = 1;
                p.alpha = (p.age < alphaRamp ? (1f - alphaSlope) + slope * (p.age / alphaRamp) : 1);

                float remainingLife = p.lifeTime - p.age;
                float alphaOut = 1f;
                p.alpha = (remainingLife < alphaOut ? remainingLife / alphaOut : p.alpha);

                if (p.age > p.lifeTime)
                {
                    toRemove.Add(p);
                }

                
            }

            toRemove.ForEach(delegate(Particle p) {
                if(p.OnDestroy != null)
                    p.OnDestroy();

                particles.Remove(p);
            });
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var p in particles)
            {
                var screenPos = Vector3.Transform(p.position, game.worldToScreen).ToVector2();
                var srcRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, p.texture.Width, p.texture.Height);
                var color = new Color(1f,1f,1f,p.alpha);
                spriteBatch.Draw(p.texture, screenPos, null, color, p.rotation, 0.5f * p.texture.ToVector(), p.scale * 1.5f, SpriteEffects.None, 0);
            }
        }
    }
}
