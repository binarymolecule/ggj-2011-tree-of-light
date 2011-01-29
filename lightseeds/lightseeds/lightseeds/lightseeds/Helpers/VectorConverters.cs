using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using lightseeds.GameObjects;

namespace lightseeds.Helpers
{
    public static class VectorConverters
    {
        public static Vector2 ToVector(this Viewport vp)
        {
            return new Vector2(vp.Width, vp.Height);
        }

        public static Vector2 ToVector(this Texture2D tx)
        {
            return new Vector2(tx.Width, tx.Height);
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
    }
}
