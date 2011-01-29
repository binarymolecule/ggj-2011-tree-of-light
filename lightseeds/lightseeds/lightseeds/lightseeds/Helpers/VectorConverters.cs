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
        public static TreeType Next(this TreeType type)
        {
            var a = Enum.GetValues(typeof(TreeType));
            var l = new List<TreeType>();
            foreach (var tt in a)
            {
                if ((TreeType)tt != TreeType.BASE)
                {
                    l.Add((TreeType)tt);
                }
            }
            var i = l.FindIndex(((x) => x == type)) + 1;
            return (i > l.Count-1 ? l.First() : l[i]);
        }
        public static TreeType Previous(this TreeType type)
        {
            var a = Enum.GetValues(typeof(TreeType));
            var l = new List<TreeType>();
            foreach (var tt in a)
            {
                if ((TreeType)tt != TreeType.BASE)
                {
                    l.Add((TreeType)tt);
                }
            }
            var i = l.FindIndex(((x) => x == type)) - 1;
            return (i < 0 ? l.Last() : l[i]);
        }
    }
    
}
