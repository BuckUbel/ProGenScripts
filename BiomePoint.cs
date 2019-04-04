namespace Assets.Scripts
{
    public class BiomePoint : Point
    {
        public int textureIndex;

        public BiomePoint(int x, int y, int textureIndex)
        {
            this.set(x, y, textureIndex);
        }

        public BiomePoint()
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