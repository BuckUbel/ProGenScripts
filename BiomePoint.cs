
using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class BiomePoint : Point
    {
        public int corePointId;
        public BiomeCorePoint corePoint;
        public float coreDiff;
        public float coreAngle;


        public BiomePoint(int x, int y, BiomeCorePoint corePoint, int corePointId)
        {
            this.set(x, y, corePoint, corePointId);
        }

        public BiomePoint()
        {
        }
        public void set(int x, int y, BiomeCorePoint corePoint, int corePointId)
        {
            this.x = x;
            this.y = y;
            this.corePoint = corePoint;
            this.corePointId = corePointId;
            //this.calcDiff();
            //this.calcAngle();

        }

        public void calcDiff()
        {
            this.coreDiff = (float)Math.Sqrt(Math.Pow(corePoint.y - this.y, 2) + Math.Pow(corePoint.x - this.x, 2));

        }


        public void calcAngle()
        {
            int yDiff = corePoint.y - this.y;
            int xDiff = corePoint.x - this.x;


            if (xDiff == 0 && yDiff > 0)
            {
                this.coreAngle =  90;
            }
            if (xDiff == 0 && yDiff < 0)
            {
                this.coreAngle = 270;
            }
            if (xDiff < 0 && yDiff == 0)
            {
                this.coreAngle = 180;
            }
            if (xDiff > 0 && yDiff == 0)
            {
                this.coreAngle = 0;
            }
            if (xDiff != 0 && yDiff != 0)
            {
                float angle = (float)(Math.Atan(xDiff / yDiff) * (180 / Math.PI));
                if (xDiff < 0)
                {
                    angle += 180;
                }
                this.coreAngle = (angle + 360) % 360;
            }

        }

        public List<Point> getArroundPoints()
        {
            List<Point> returnList = new List<Point>();
            returnList.Add(new Point(this.x + 1, this.y));
            returnList.Add(new Point(this.x + 1, this.y + 1));
            returnList.Add(new Point(this.x + 1, this.y - 1));
            returnList.Add(new Point(this.x - 1, this.y));
            returnList.Add(new Point(this.x - 1, this.y + 1));
            returnList.Add(new Point(this.x - 1, this.y - 1));
            returnList.Add(new Point(this.x, this.y + 1));
            returnList.Add(new Point(this.x, this.y - 1));
            return returnList;
        }
    }
}