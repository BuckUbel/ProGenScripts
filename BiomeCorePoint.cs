using System;

namespace Assets.Scripts
{
    public class BiomeCorePoint : Point
    {
        public int textureIndex;
        public int corePointId;
        public float coreDiff;
        public float coreAngle;


        public BiomeCorePoint(int x, int y, int textureIndex)
        {
            this.set(x, y, textureIndex);
        }

        public BiomeCorePoint()
        {
        }
        public void set(int x, int y, int textureIndex)
        {
            this.x = x;
            this.y = y;
            this.textureIndex = textureIndex;

        }
    }
}