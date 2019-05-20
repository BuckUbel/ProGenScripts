using UnityEngine;
using System.Collections.Generic;

public class TreeMap
{
    public TerrainerContent tc;

    public TreeMap() { }

    public TreeMap(TerrainerContent tc)
    {
        this.tc = tc;
    }

    public void calculateTreesByBiomes()
    {
        // empty the old treeInstaces from persistent data
        tc.terrain.terrainData.treeInstances = new List<TreeInstance>().ToArray();

        int treeCount = 1000; // TODO: with global scaleFactor
        int treeCounter = 0;

        while (treeCounter < treeCount)
        {
            int xTreePos = Terrainer.rnd.Next(0, tc.xTerrainRes);
            int yTreePos = Terrainer.rnd.Next(0, tc.yTerrainRes);
            int treePrototypeNumber = -1;
            if (tc.biomeMap[xTreePos, yTreePos] == 1)
            {
                treePrototypeNumber = Terrainer.rnd.Next(0, 2);
            }
            if (tc.biomeMap[xTreePos, yTreePos] == 3)
            {
                treePrototypeNumber = 0;
            }
            if (tc.biomeMap[xTreePos, yTreePos] == 4)
            {
                treePrototypeNumber = 1;
            }
            if (treePrototypeNumber >= 0)
            {
                //TODO: scalefactor depends on the height of terrain
                TreeInstance pTI = new TreeInstance();
                float yPos = (float)xTreePos / (float)tc.xTerrainRes;
                float xPos = (float)yTreePos / (float)tc.yTerrainRes;
                pTI.position = new Vector3(xPos, 1, yPos);
                float scaleFactor = (float)Terrainer.rnd.NextDouble();
                pTI.widthScale = scaleFactor;
                pTI.heightScale = scaleFactor;
                pTI.color = Color.yellow;
                pTI.lightmapColor = Color.yellow;
                pTI.prototypeIndex = treePrototypeNumber; //?
                tc.terrain.AddTreeInstance(pTI);
                treeCounter++;
            }
        }
    }
}
