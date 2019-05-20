using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;

public class MountMap
{

    // the standard height for the map
    // is necessary, because the water needs space under the normal map
    public static float STANDARD_HEIGHT = 0.05f;
    public static float WATER_HEIGHT = 0.0499f;

    public float[,] heightMap;
    public TerrainerContent tc;

    public MountMap() { }

    public MountMap(TerrainerContent tc)
    {
        this.tc = tc;
        // Get all of layer zero.
        this.set(tc.tData.GetHeights(0, 0, tc.xTerrainRes, tc.yTerrainRes));
    }

    public void set(float[,] map)
    {
        this.heightMap = map;
    }


    // calc the minimal bound for a mountain
    int getMinBound(int a, float range, int min)
    {
        var bound = a - range;
        if (bound < min)
        {
            return min;
        }
        return (int)bound;
    }

    // calc the maximal bound for a mountain
    int getMaxBound(int a, float range, int max)
    {
        var bound = a + range;
        if (bound > max)
        {
            return max;
        }
        return (int)bound;
    }


    // generate the mountains on the biomes
    // for this purpose:
    //      - the first loop goes through each biome
    //          --> generate random mountainPoints on the base of mountainDegree (property of Biome)
    //      - the second loop goes through each mountainPoint
    //          --> calc the maximal mountainWidth from this point, which is the minimal diff to the border
    //          ---> for this puropose calc the diff of mountainPoint to each borderPoint of this biome
    //          --> calc the height for each point in the defined mountain
    //      - the heightmap will save in the terrainData
    public void createMountainsByBorders()
    {
        int biomeSize;
        float mountPercentage;
        int mountCount;
        Point newPoint;
        List<float> diffValues;
        float diff;
        float minValue;
        int minIndex;
        int maxMountainWidth;
        float maxHeight;
        int allMountCount = 0;

        tc.resetPoints();
        this.set(tc.tData.GetHeights(0, 0, tc.xTerrainRes, tc.yTerrainRes));

        for (int i = 0; i < tc.biomes.Count; i++)
        {
            if (tc.biomes[i].borderPoints.Count > 0)
            {
                biomeSize = (int)(10000 * ((float)tc.biomes[i].allPoints.Count / (float)tc.allPoints.Count));
                mountPercentage = tc.biomes[i].mountainsDegree;
                //print(biomeSize);
                mountCount = (int)(biomeSize * mountPercentage);

                //print(this.biomes[i].name + ": " + this.biomes[i].allPoints.Count + " / " + this.allPoints.Count + " = " + biomeSize + " | " + mountCount);

                for (int j = 0; j < mountCount; j++)
                {
                    newPoint = tc.biomes[i].allPoints[Terrainer.rnd.Next(0, tc.biomes[i].allPoints.Count)];

                    diffValues = new List<float>();
                    for (int k = 0; k < tc.biomes[i].borderPoints.Count; k++)
                    {
                        diffValues.Add((float)Math.Sqrt(Math.Pow(newPoint.y - tc.biomes[i].borderPoints[k].y, 2) + Math.Pow(newPoint.x - tc.biomes[i].borderPoints[k].x, 2)));
                    }
                    minValue = diffValues.Min();
                    minIndex = diffValues.ToList().IndexOf(minValue);
                    maxMountainWidth = (int)minValue;

                    var leftBound = this.getMinBound(newPoint.x, maxMountainWidth, 0);
                    var rightBound = this.getMaxBound(newPoint.x, maxMountainWidth, tc.xTerrainRes);
                    var topBound = this.getMinBound(newPoint.y, maxMountainWidth, 0);
                    var bottomBound = this.getMaxBound(newPoint.y, maxMountainWidth, tc.yTerrainRes);
                    for (var y = topBound; y < bottomBound; y++)
                    {
                        for (var x = leftBound; x < rightBound; x++)
                        {
                            diff = new Point(x,y).getPyramideDiff(newPoint);

                            //diff = (float)Math.Sqrt(Math.Pow(newPoint.y - y, 2) + Math.Pow(newPoint.x - x, 2));

                            if (diff <= maxMountainWidth)
                            {
                                maxHeight = (float)(maxMountainWidth * mountPercentage / 10000);

                                if (tc.biomes[i].isWater)
                                {
                                    maxHeight = (-1) * maxHeight;
                                }
                                //this.heightMap[x, y] = this.heightMap[x, y] + calcHeight(diff, maxMountainWidth, maxHeight) - calcHeight(maxMountainWidth, maxMountainWidth, maxHeight);
                                this.heightMap[x, y] = this.heightMap[x, y] + ((float)maxHeight * (float)Math.Cos((Math.PI * (float)diff) / (2 * (float)maxMountainWidth))); // +maxHeight/2;
                            }

                        }
                    }
                    allMountCount++;
                }
            }
        }

        for (int i = 0; i < tc.terr.isleDiff; i++)
        {
            for (int j = 0; j < tc.yTerrainRes; j++)
            {

                // complete left side
                newPoint = new Point(i, j);
                diff = newPoint.getPyramideDiff(new Point(tc.terr.isleDiff, j));
                this.heightMap[i, j] += this.normalHeightCalc(diff, tc.terr.isleDiff);

                // complete right side
                newPoint = new Point(tc.xTerrainRes - tc.terr.isleDiff + i, j);
                diff = newPoint.getPyramideDiff(new Point(tc.xTerrainRes - tc.terr.isleDiff, j));
                this.heightMap[tc.xTerrainRes - tc.terr.isleDiff + i, j] += this.normalHeightCalc(diff, tc.terr.isleDiff);
            }

            for (int j = 0; j < tc.xTerrainRes; j++)
            {
                // complete top side
                newPoint = new Point(j, i);
                diff = newPoint.getPyramideDiff(new Point(j, tc.terr.isleDiff));
                this.heightMap[j, i] += this.normalHeightCalc(diff, tc.terr.isleDiff);

                // complete bottom side
                newPoint = new Point(j, i + tc.yTerrainRes - tc.terr.isleDiff);
                diff = newPoint.getPyramideDiff(new Point(j, tc.yTerrainRes - tc.terr.isleDiff));
                this.heightMap[j, i + tc.yTerrainRes - tc.terr.isleDiff] += this.normalHeightCalc(diff, tc.terr.isleDiff);
            }
        }

        //print("Mountains: " + allMountCount);
        this.setHeightMapWithStandardHeights();
        this.tc.tData.SetAlphamaps(0, 0, this.tc.alphaMap.aMap);

    }

    // calc the height for a mountain
    float calcHeight(float x, float range, float maxHeight)
    {
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1);
    }

    float normalHeightCalc(float x, float range)
    {
        float maxHeight = MountMap.STANDARD_HEIGHT / 2;
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1) - MountMap.STANDARD_HEIGHT;
    }


    // set a new heightmap and add the standardheight
    public void setHeightMapWithStandardHeights()
    {
        float[,] newHeightMap = new float[this.heightMap.GetLength(0), this.heightMap.GetLength(1)];

        for (int y = 0; y < this.heightMap.GetLength(0); y++)
        {
            for (int x = 0; x < this.heightMap.GetLength(1); x++)
            {
                newHeightMap[x, y] = MountMap.STANDARD_HEIGHT + this.heightMap[x, y];
            }
        }
        this.tc.tData.SetHeights(0, 0, this.heightMap);
        //this.heightMap = newHeightMap;
    }

    public void setHeightMap()
    {
        this.tc.tData.SetHeights(0, 0, this.heightMap);
    }

}
