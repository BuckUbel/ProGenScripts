using System;
using System.Linq;
using Assets.Scripts;

public class DetailsMap
{
    public int[,] dMap;
    public TerrainerContent tc;

    public DetailsMap() { }

    public DetailsMap(TerrainerContent tc)
    {
        this.tc = tc;
        int[,] map = new int[tc.tData.detailWidth, tc.tData.detailHeight];
        // Get all of layer zero.
        this.set(map);
    }

    public void set(int[,] map)
    {
        this.dMap = map;
    }

    public void calculateDetailByBiomes()
    {
        // For each pixel in the detail map...
        for (var y = 0; y < tc.tData.detailHeight; y++)
        {
            for (var x = 0; x < tc.tData.detailWidth; x++)
            {
                this.dMap[x, y] = 0;
                if (tc.biomeMap[x, y] == 1)
                {
                    this.dMap[x, y] = 1;
                }
                if (tc.biomeMap[x, y] == 3)
                {
                    this.dMap[x, y] = 1;
                }
                if (tc.biomeMap[x, y] == 4)
                {
                    this.dMap[x, y] = 1;
                }
            }
        }

        // Assign the modified map back.
        tc.tData.SetDetailLayer(0, 0, 0, this.dMap);
    }
}
