using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace lightseeds.GameObjects
{
    class Tree
    {
        public Vector2 position;

        // gameplay properties
        public float growthTime;
        public float growthAmount = 0f;
        public int price;
    }
}
