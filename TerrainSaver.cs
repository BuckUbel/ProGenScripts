using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainSaver : MonoBehaviour
{

    private Terrain terrain;
    private TerrainData tData;

    public float standardHeight = 0.1f;

    public int biomCount = 25;
    public float cityBiom = 0.25f;
    public float waterBiom = 0.25f;
    public float forestBiom = 0.25f;
    public float fieldBiom = 0.25f;
    public float mountBiom = 0.25f;


    private int xTerrainRes;
    private int yTerrainRes;

    private int xTextureRes;
    private int yTextureRes;

    private AlphaMap alphaMap;

    private float[,] heightMap;

    private List<Biome> biomes = new List<Biome>();
    private List<BiomeCorePoint> allCorePoints = new List<BiomeCorePoint>();
    private List<BiomePoint> allPoints = new List<BiomePoint>();
    // Use this for initialization
    void Start()
    {
        this.terrain = transform.GetComponent<Terrain>();
        tData = terrain.terrainData;

        xTerrainRes = tData.heightmapWidth;
        yTerrainRes = tData.heightmapHeight;
        this.xTextureRes = tData.alphamapWidth;
        this.yTextureRes = tData.alphamapHeight;

        this.biomes.Add(new Biome("city", 0, cityBiom, 0.05f, false));
        this.biomes.Add(new Biome("forest", 1, forestBiom, 0.1f, false));
        this.biomes.Add(new Biome("water", 2, waterBiom, 0.5f, true));
        this.biomes.Add(new Biome("field", 3, fieldBiom, 0.25f, false));
        this.biomes.Add(new Biome("mountain", 4, mountBiom, 0.8f, false));

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnGUI()
    {

        if (GUI.Button(new Rect(10, 10, 100, 25), "Wrinkle"))
        {
            this.createAlphaMaps();
            print("Create AlphaMaps");
            this.calcBorders();
            print("Calc Borders");
            this.createMountainsByBorders();
            print("Create Mountains by borders");
        }
        if (GUI.Button(new Rect(10, 40, 100, 25), "Reset"))
        {
            resetPoints();
            resetAlphaMaps();
        }

    }

    public int getBiomeIndexByTextureIndex(int textureIndex)
    {
        for (int i = 0; i < this.biomes.Count; i++)
        {
            if (this.biomes[i].textureIndex == textureIndex)
            {
                return i;
            }
        }
        return -1;
    }
    public void calcBorders()
    {
        Point tempPoint = new Point();
        List<Point> aroundPixels;
        int textureIndex = 0;
        for (int j = 0; j < this.allPoints.Count; j++)
        {
            textureIndex = this.allPoints[j].corePoint.textureIndex;
            aroundPixels = this.allPoints[j].getArroundPoints();
            for (int i = 0; i < aroundPixels.Count; i++)
            {
                if (textureIndex != this.alphaMap.getSettedIndex(aroundPixels[i]))
                {
                    int biomeId = this.getBiomeIndexByTextureIndex(textureIndex);
                    this.biomes[biomeId].addBorderPoint(this.allPoints[j]);
                    i = aroundPixels.Count;
                }
            }
        }
        //for (int a = 0; a < this.biomes.Count; a++)
        //{
        //    print(this.biomes[a].name + " : " + this.biomes[a].borderPoints.Count);

        //    for (int j = 0; j < this.biomes[a].borderPoints.Count; j++)
        //    {
        //        this.alphaMap.aMap[this.biomes[a].borderPoints[j].x, this.biomes[a].borderPoints[j].y, this.biomes[a].textureIndex] = 0;

        //    }
        //}
        //tData.SetAlphamaps(0, 0, this.alphaMap.aMap);

    }

    float calcHeight(float x, float range, float maxHeight)
    {
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1);
    }

    float calcHeight(float x, float range)
    {
        float maxHeight = 0.1f / 20;
        return this.calcHeight(x, range, maxHeight);
    }
    void resetPoints()
    {
        var heights = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);


        for (int y = 0; y < yTerrainRes; y++)
        {
            for (int x = 0; x < xTerrainRes; x++)
            {
                heights[x, y] = standardHeight;
            }
        }
        tData.SetHeights(0, 0, heights);

    }

    int getMinBound(int a, float range, int min)
    {
        var bound = a - range;
        if (bound < min)
        {
            return min;
        }
        return (int)bound;
    }
    int getMaxBound(int a, float range, int max)
    {
        var bound = a + range;
        if (bound > max)
        {
            return max;
        }
        return (int)bound;
    }

    void getUpdateAllCorePoints()
    {
        this.allCorePoints = new List<BiomeCorePoint>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.allCorePoints.AddRange(this.biomes[i].corePoints);
        }
    }
    List<int> getNoMountainsIndexes()
    {
        List<int> returnList = new List<int>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            if (this.biomes[i].mountainsDegree == 0)
            {
                returnList.Add(i);
            }
        }
        return returnList;
    }

    List<int> waterBiomesIndexes()
    {
        List<int> returnList = new List<int>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            if (this.biomes[i].isWater == true)
            {
                returnList.Add(i);
            }
        }
        return returnList;
    }

    void createAlphaMaps()
    {
        resetAlphaMaps();
        this.alphaMap = new AlphaMap(new float[this.xTextureRes, this.yTextureRes, 5]);


        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.biomes[i].createRandomPoints(this.biomCount, this.xTextureRes, this.yTextureRes);
        }

        this.getUpdateAllCorePoints();

        Point tempPoint = new Point();
        BiomePoint newBiomePoint = new BiomePoint();
        List<int> diffValues;
        int minValue;
        int minIndex;

        // For each point on the alphamap...
        for (int y = 0; y < this.yTextureRes; y++)
        {
            for (int x = 0; x < this.xTextureRes; x++)
            {
                diffValues = new List<int>();
                tempPoint.set(x, y);
                for (int i = 0; i < this.allCorePoints.Count; i++)
                {
                    diffValues.Add(this.allCorePoints[i].getDiff(tempPoint));
                }

                minValue = diffValues.Min();
                minIndex = diffValues.ToList().IndexOf(minValue);
                newBiomePoint = new BiomePoint(x, y, allCorePoints[minIndex], minIndex);

                this.allPoints.Add(newBiomePoint);
                // uncomment this if-branch to activate an maximal biomwidth
                //if (minValue <= this.biomWidth)
                //{
                this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)1;
                this.biomes[this.getBiomeIndexByTextureIndex(allCorePoints[minIndex].textureIndex)].allPoints.Add(newBiomePoint);
                //}
                //else
                //{
                //    this.alphaMap.aMap[x, y, this.biomes[this.biomes.Count - 1].textureIndex] = (float)1;
                //    this.biomes[this.getBiomeIndexByTextureIndex(this.biomes[this.biomes.Count - 1].textureIndex)].allPoints.Add(newBiomePoint);

                //}


                // uncomment to mark the biome cores
                //if (minValue == 0 || minValue == 1)
                //{
                //    this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)0;
                //    this.biomes[this.getBiomeIndexByTextureIndex(allCorePoints[minIndex].textureIndex)].allPoints.Add(newBiomePoint);

                //}
            }
        }
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
    }
    void createMountainsByBorders()
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
        resetPoints();

        this.heightMap = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);


        for (int i = 0; i < this.biomes.Count; i++)
        {
            if (this.biomes[i].borderPoints.Count > 0)
            {
                biomeSize = this.biomes[i].allPoints.Count;
                mountPercentage = this.biomes[i].mountainsDegree;
                mountCount = (int)(biomeSize * mountPercentage / 100);
                print(this.biomes[i].name + ": " + biomeSize + " --> " + mountCount);
                //mountCount = (int)20;
                for (int j = 0; j < mountCount; j++)
                {
                    newPoint = this.biomes[i].allPoints[Random.Range(0, biomeSize)];

                    diffValues = new List<float>();
                    for (int k = 0; k < this.biomes[i].borderPoints.Count; k++)
                    {
                        diffValues.Add((float)Math.Sqrt(Math.Pow(newPoint.y - this.biomes[i].borderPoints[k].y, 2) + Math.Pow(newPoint.x - this.biomes[i].borderPoints[k].x, 2)));
                    }
                    minValue = diffValues.Min();
                    minIndex = diffValues.ToList().IndexOf(minValue);
                    maxMountainWidth = (int)minValue;

                    var leftBound = this.getMinBound(newPoint.x, maxMountainWidth, 0);
                    var rightBound = this.getMaxBound(newPoint.x, maxMountainWidth, xTerrainRes);
                    var topBound = this.getMinBound(newPoint.y, maxMountainWidth, 0);
                    var bottomBound = this.getMaxBound(newPoint.y, maxMountainWidth, yTerrainRes);
                    for (var y = topBound; y < bottomBound; y++)
                    {
                        for (var x = leftBound; x < rightBound; x++)
                        {
                            diff = (float)Math.Sqrt(Math.Pow(newPoint.y - y, 2) + Math.Pow(newPoint.x - x, 2));

                            if (diff <= maxMountainWidth)
                            {
                                //float maxHeight = 0.1f / 20;
                                maxHeight = (float)(maxMountainWidth * mountPercentage / 10000);

                                if (this.biomes[i].isWater)
                                {
                                    maxHeight = (-1) * maxHeight;
                                }
                                this.heightMap[x, y] = this.heightMap[x, y] + calcHeight(diff, maxMountainWidth, maxHeight) - calcHeight(maxMountainWidth, maxMountainWidth, maxHeight);
                            }

                        }
                    }

                }
            }
        }
        //tData.SetHeights(0, 0, this.heightMap);

        this.setHeightMapWithStandardHeights();

    }

    void setHeightMapWithStandardHeights()
    {
        float[,] newHeightMap = new float[this.heightMap.GetLength(0), this.heightMap.GetLength(1)];
        for (int y = 0; y < this.heightMap.GetLength(0); y++)
        {
            for (int x = 0; x < this.heightMap.GetLength(1); x++)
            {
                newHeightMap[x, y] = standardHeight + this.heightMap[x, y];
            }
        }
        tData.SetHeights(0, 0, this.heightMap);
        this.heightMap = newHeightMap;
    }
   
    void resetAlphaMaps()
    {
        float[,,] map = new float[this.xTextureRes, this.yTextureRes, 5];
        tData.SetAlphamaps(0, 0, map);
    }

}






