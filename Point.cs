using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Point
    {
        public int x = 0;
        public int y = 0;

        public Point(int x, int y)
        {
            this.set(x, y);
        }

        public Point()
        { }

        public void set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void createRandom(int minX, int maxX, int minY, int maxY)
        {
            this.x = Random.Range(minX, maxX);
            this.y = Random.Range(minY, maxY);
        }

        public int getDiff(Point p)
        {
            int a = (int)Math.Sqrt(Math.Pow(p.y - this.y, 2) + Math.Pow(p.x - this.x, 2));

            int b = (Math.Abs(p.y - this.y) + Math.Abs(p.x - this.x));
            return (int)a + b / 2;

        }
    }
}
