using System;
using System.Linq;
using Boo.Lang;

namespace Assets.Scripts
{
    public class Biome
    {
        public string name;
        public List<BiomePoint> corePoints = new List<BiomePoint>();
        public int textureIndex;
        public float globalPercentage;
        public float mountainsDegree;

        public Biome(string name, int textureIndex, float globalPercentage, float mountainsDegree)
        {
            this.set(name, textureIndex, globalPercentage, mountainsDegree);
        }

        public void set(string name, int textureIndex, float globalPercentage, float mountainsDegree)
        {
            this.name = name;
            this.textureIndex = textureIndex;
            this.globalPercentage = globalPercentage;
            this.mountainsDegree = mountainsDegree;
        }

        public void addPoint(Point p)
        {
            this.corePoints.Add(new BiomePoint(p.x, p.y, this.textureIndex));
        }
        public void addPoint(int x, int y)
        {
            this.corePoints.Add(new BiomePoint(x, y, this.textureIndex));
        }

        public void createRandomPoints(int globalCount, int xMax, int yMax)
        {
            this.corePoints = new List<BiomePoint>();
            BiomePoint tempPoint = new BiomePoint();
            tempPoint.textureIndex = this.textureIndex;

            for (int i = 0; i < Math.Abs(globalCount * this.globalPercentage); i++)
            {
                tempPoint.createRandom(0, xMax, 0, yMax);
                this.addPoint(tempPoint);
            }
        }

        public BiomePoint getNearestPoint(Point point)
        {
            List<int> diffValues = new List<int>();
            for (int i = 0; i < this.corePoints.Count; i++)
            {
                diffValues.Add(point.getDiff(this.corePoints[i]));
            }

            var minValue = diffValues.Min();
            var minIndex = diffValues.IndexOf(minValue);
            return this.corePoints[minIndex];
        }
    }
}