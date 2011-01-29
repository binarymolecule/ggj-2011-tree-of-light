using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using lightseeds.Helpers;

namespace lightseeds.GameObjects
{
    public class TheVoid
    {
        public Vector3 direction;
        public float horizontalPosition;
        private Game1 game;
        private Texture2D tex;

        public float spawnTimer = 0.5f;
        private int particlePool;
        private const int PARTICLE_FILL = 75;

        public TheVoid(Game1 game)
        {
            this.game = game;

            tex = game.Content.Load<Texture2D>("textures/smoke");
        }

        public void Update(GameTime gt)
        {
            float accumulatedRepulsion = 1.0f;

            foreach (var player in game.players)
            {
                if (IsBehind(player.worldPosition.X))
                {
                    // player is dead

                    player.Respawn();
                }
            }

            foreach(var tree in game.treeCollection.trees) {
                double distance = Math.Abs(tree.position.X - horizontalPosition);

                if (distance < 2 && tree.status != Tree.TreeStatus.KILLED) {
                    tree.status = lightseeds.GameObjects.Tree.TreeStatus.KILLED;
                }
                else if (distance < 10)
                {
                    if (tree.status == Tree.TreeStatus.MATURE)
                    {
                        var realResistance = tree.resistance;
                        // todo: apply curve
                        accumulatedRepulsion = Math.Min(accumulatedRepulsion, realResistance);
                    }
                }
            }


            float regularSpeed = 0.5f * (float)gt.ElapsedGameTime.TotalSeconds;
            regularSpeed *= accumulatedRepulsion;

            this.horizontalPosition += regularSpeed * direction.X;

            // adding new particles

            spawnTimer -= (float)gt.ElapsedGameTime.TotalSeconds;
            if (spawnTimer < 0 || particlePool < PARTICLE_FILL)
            {
                particlePool++;
                float xpos = this.horizontalPosition + direction.X * ((float)game.particleCollection.random.NextDouble() * 6.5f - 6);
                var p = game.particleCollection.SpawnParticle(new Vector3(xpos, (float)game.particleCollection.random.NextDouble() * 10.0f, 0));
                p.OnDestroy = delegate()
                {
                    particlePool--;
                };
                spawnTimer = 0.5f;
            }
            
        }

        public bool IsBehind(float x)
        {
            return direction.X * (x - horizontalPosition) <= 0;
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex, Vector3.Transform(new Vector3(horizontalPosition, 5, 0), game.worldToScreen).ToVector2(), Color.White);
        }
    }
}
