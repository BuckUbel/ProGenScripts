using System;
using System.Linq;
using Boo.Lang;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Biome
    {
        public string name;
        public int startCoreIds;
        public List<BiomeCorePoint> corePoints = new List<BiomeCorePoint>();
        public List<BiomePoint> borderPoints = new List<BiomePoint>();
        public List<BiomePoint> contentPoints = new List<BiomePoint>();
        public List<BiomePoint> allPoints = new List<BiomePoint>();
        public int textureIndex;
        public float globalPercentage;
        public float mountainsDegree;
        public bool isWater;

        public Biome(string name, int textureIndex, float globalPercentage, float mountainsDegree, bool isWater)
        {
            this.set(name, textureIndex, globalPercentage, mountainsDegree, isWater);
        }

        public void set(string name, int textureIndex, float globalPercentage, float mountainsDegree, bool isWater)
        {
            this.name = name;
            this.textureIndex = textureIndex;
            this.globalPercentage = globalPercentage;
            this.mountainsDegree = mountainsDegree;
            this.isWater = isWater;
        }
        public void addCompletlyNewBorderPoint(BiomePoint bp, int index)
        {
            bp.indexInGlobalAllPoints = index;
            this.borderPoints.Add(bp);
            this.allPoints.Add(bp);
        }

        public void addBorderPoint(BiomePoint bp, int index)
        {
            bp.isBorder = true;
            bp.indexInGlobalAllPoints = index;
            this.borderPoints.Add(bp);
        }
        public void addContentPoint(BiomePoint bp, int index)
        {
            bp.isBorder = false;
            bp.indexInGlobalAllPoints = index;
            this.contentPoints.Add(bp);
        }
        public void addCorePoint(Point p)
        {
            this.corePoints.Add(new BiomeCorePoint(p.x, p.y, this.textureIndex));
        }
        public void addCorePoint(int x, int y)
        {
            this.corePoints.Add(new BiomeCorePoint(x, y, this.textureIndex));
        }

        public void createRandomPoints(int globalCount, int xMax, int yMax)
        {
            this.corePoints = new List<BiomeCorePoint>();
            BiomeCorePoint tempPoint = new BiomeCorePoint();
            tempPoint.textureIndex = this.textureIndex;

            for (int i = 0; i < Math.Abs(globalCount * this.globalPercentage); i++)
            {
                tempPoint.createRandom(0, xMax, 0, yMax);
                this.addCorePoint(tempPoint);
            }
        }

        public BiomePoint getRandomPoint()
        {
            int randomNumber = (int)Random.Range(0, this.allPoints.Count - 1);
            return this.allPoints[randomNumber];
        }

        public BiomeCorePoint getNearestPoint(Point point)
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