using Assets.Scripts;
using System.Linq;
using System;
using System.Collections.Generic;

public class AlphaMap
{

    public float[,,] aMap;

    public AlphaMap() { }

    public AlphaMap(float[,,] map)
    {
        this.set(map);
    }

    public void set(float[,,] map)
    {
        this.aMap = map;
    }

    public int getSettedIndex(Point p)
    {
        int[] lastDimArray = new int[this.aMap.GetLength(2)];

        for (int i = 0; i < lastDimArray.Length; i++)
        {
            int a;
            try
            {
                a = (int)this.aMap[p.x, p.y, i];
            }
            catch (Exception e)
            {
                a = 0;
            }
            lastDimArray[i] = a;
        }

        int maxValue = lastDimArray.Max();
        return lastDimArray.ToList().IndexOf(maxValue);
    }




    // generate a distribution of biomes and their associated texture
    // for this purpose:
    //      - generate a new AlphaMap object
    //      - the first loop will goes through each biome and generate random corePoints of them on the whole map
    //      - the second loop will goes through each pixel and calc the diff to each corePoint
    //          --> the corePoint with the smallest diff, will be the corePoint for this Pixel (BiomePoint)
    //      - the AlphaMap will save in the terrainData

    // Border Recognition 
    //
    //   o o o
    //   o x o
    //   o o o 
    //
    // Ein Punkt gehört dann zu einer Grenze, wenn die umgebenden Punkte eine andere Textur haben als er.
    // Die Textur eines Punktes wird durch die kürzeste Differenz zu den Kernpunkten ermittelt.
    //    
    // Um nun einen zu erkennen ob die umliegenden Pixel eine andere Textur haben, können nur die schon davor sichtbaren 4 Punkte (a) betrachtet werden
    // a a a 
    // a x o
    // o o o 
    // damit auch diese Punkte zu den Kernpunkten hinzugefügt werden können, muss nur gesagt werden WENN(x eine Grenze) dann auch alle a's mit anderem Texturindex

    public void createAlphaMaps(TerrainerContent tc)
    {
        int alphaMapWidth = tc.xTextureRes;
        int alphaMapLength = tc.yTextureRes;
        for (int i = 0; i < tc.biomes.Count; i++)
        {
            tc.biomes[i].createRandomPoints(tc.terr.biomCount, alphaMapWidth, alphaMapLength);
        }

        tc.biomes[2].addCorePoint(new BiomeCorePoint(0, 0, tc.biomes[2].textureIndex));

        tc.getUpdateAllCorePoints();

        Point tempPoint = new Point();
        BiomePoint newBiomePoint = new BiomePoint();
        List<int> diffValues;
        int minValue = int.MaxValue;
        int secondMinValue = int.MaxValue;
        int minIndex = -1;
        int secondMinIndex = -1;

        // For each point on the alphamap...
        for (int y = 0; y < alphaMapLength; y++)

        {
            for (int x = 0; x < alphaMapWidth; x++)
            {
                // reset values for diff calculation
                diffValues = new List<int>();
                minValue = int.MaxValue;
                secondMinValue = int.MaxValue;

                tempPoint.set(x, y);

                if (x > tc.terr.isleDiff && x < alphaMapWidth - tc.terr.isleDiff && y > tc.terr.isleDiff && y < alphaMapLength - tc.terr.isleDiff)
                {

                    // get the smallest diff to a core
                    for (int corePointIndex = 0; corePointIndex < tc.allCorePoints.Count; corePointIndex++)
                    {
                        int corePointDiff = tc.allCorePoints[corePointIndex].getDiff(tempPoint);
                        diffValues.Add(corePointDiff);
                        if (corePointDiff < minValue)
                        {
                            secondMinValue = minValue;
                            secondMinIndex = minIndex;

                            minValue = corePointDiff;
                            minIndex = corePointIndex;

                        }
                        else if (corePointDiff < secondMinValue)
                        {
                            secondMinValue = corePointDiff;
                            secondMinIndex = corePointIndex;
                        }
                    }
                }
                else
                {

                    secondMinValue = 2;
                    secondMinIndex = tc.biomes[2].startCoreIds + tc.biomes[2].corePoints.Count - 1;

                    minValue = 0;
                    minIndex = tc.biomes[2].startCoreIds + tc.biomes[2].corePoints.Count - 1;
                }


                // create new BiomePoint on the base of the corePoint
                newBiomePoint = new BiomePoint(x, y, tc.allCorePoints[minIndex], minIndex);

                // added to the allPoint List of these run-class
                tc.allPoints.Add(newBiomePoint);


                // added to the allPoint List of the biome
                tc.alphaMap.aMap[x, y, tc.allCorePoints[minIndex].textureIndex] = (float)1;
                int biomeId = tc.getBiomeIndexByTextureIndex(tc.allCorePoints[minIndex].textureIndex);
                tc.biomeMap[x, y] = biomeId;
                tc.biomes[biomeId].allPoints.Add(newBiomePoint);


                // border point --> points with almost equal distances to two cores
                if ((secondMinValue == minValue || secondMinValue == minValue + 1 || secondMinValue == minValue - 1) && tc.allCorePoints[secondMinIndex].textureIndex != tc.allCorePoints[minIndex].textureIndex)
                {
                    // biomes shares this area of border --> on texture
                    tc.alphaMap.aMap[x, y, tc.allCorePoints[minIndex].textureIndex] = (float)0.5;
                    tc.alphaMap.aMap[x, y, tc.allCorePoints[secondMinIndex].textureIndex] = (float)0.5;

                    // --> also on allPoints List in the biomes
                    tc.biomes[tc.getBiomeIndexByTextureIndex(tc.allCorePoints[secondMinIndex].textureIndex)].allPoints.Add(newBiomePoint);

                }
                // uncomment to mark the biome cores
                /*if (minValue == 0 || minValue == 1)
                 {
                  tc.alphaMap.aMap[x, y, tc.allCorePoints[minIndex].textureIndex] = (float)0;
                  tc.biomes[tc.getBiomeIndexByTextureIndex(tc.allCorePoints[minIndex].textureIndex)].allPoints.Add(newBiomePoint);

                 }*/

            }
        }

        tc.tData.SetAlphamaps(0, 0, tc.alphaMap.aMap);
    }

    // calc the border pixels and add to the biome-objects
    // for this purpose, the loop goes through each pixel and look at the arounded pixels and their textureIndex
    // if the textureIndex is different this pixel is a border pixel and will save in their associated biome
    public void calcBorders(TerrainerContent tc)
    {
        Point tempPoint = new Point();
        List<Point> aroundPixels;
        int textureIndex = 0;

        for (int j = 0; j < tc.allPoints.Count; j++)
        {
            textureIndex = tc.allPoints[j].corePoint.textureIndex;
            int biomeId = tc.getBiomeIndexByTextureIndex(textureIndex);
            aroundPixels = tc.allPoints[j].getArroundPoints();
            bool isBorder = false;
            for (int i = 0; i < aroundPixels.Count; i++)
            {
                if (textureIndex != tc.alphaMap.getSettedIndex(aroundPixels[i]))
                {
                    tc.biomes[biomeId].addBorderPoint(tc.allPoints[j], j);
                    isBorder = true;
                    // if one pixel has different t-Id, the loop should be break
                    i = aroundPixels.Count;
                }
            }
            if (!isBorder)
            {
                tc.biomes[biomeId].addContentPoint(tc.allPoints[j], j);
                //this.alphaMap.aMap[this.allPoints[j].x, this.allPoints[j].y, this.biomes[biomeId].textureIndex] = 0;
            }
        }
        tc.tData.SetAlphamaps(0, 0, tc.alphaMap.aMap);


        // uncomment this section to see the borders of biomes
        /*
         for (int a = 0; a < this.biomes.Count; a++)
          {
              print(this.biomes[a].name + " : " + this.biomes[a].borderPoints.Count);

                    for (int j = 0; j < this.biomes[a].borderPoints.Count; j++)
                   {
                      this.alphaMap.aMap[this.biomes[a].borderPoints[j].x, this.biomes[a].borderPoints[j].y, this.biomes[a].textureIndex] = 0;

                 }
            }
           tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
         */
    }
}
