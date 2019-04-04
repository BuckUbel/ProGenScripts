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

    public Terrain terrain;
    private TerrainData tData;


    public float montainWidth;
    public int countMounts;

    public bool moreMountains = false;

    public int biomWidth;
    public int biomCount = 25;
    public float cityBiom = 0.25f;
    public float waterBiom = 0.25f;
    public float forestBiom = 0.25f;
    public float fieldBiom = 0.25f;


    private int xTerrainRes;
    private int yTerrainRes;

    private int xTextureRes;
    private int yTextureRes;

    private AlphaMap alphaMap;

    private float[,] heightMap;

    private List<Biome> biomes = new List<Biome>();
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

        this.biomes.Add(new Biome("city", 0, cityBiom, 0));
        this.biomes.Add(new Biome("forest", 1, forestBiom, 1));
        this.biomes.Add(new Biome("water", 2, waterBiom, 0));
        this.biomes.Add(new Biome("field", 3, fieldBiom, 1));

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
            this.createMountains();
            if (moreMountains)
            {
                this.createAlphaMapsOnMontains();
            }
            else
            {
                this.destroyMountainsOnNonMountainAlphaMaps();
            }
        }
        if (GUI.Button(new Rect(10, 40, 100, 25), "Reset"))
        {
            resetPoints();
            resetAlphaMaps();
        }

    }



    float calcHeight(float x, float range)
    {
        float maxHeight = 0.1f / 8;
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1);
    }
    void resetPoints()
    {
        var heights = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);


        for (int y = 0; y < yTerrainRes; y++)
        {
            for (int x = 0; x < xTerrainRes; x++)
            {
                heights[x, y] = 0;
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

    void getUpdateALLPoint()
    {
        this.allPoints = new List<BiomePoint>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.allPoints.AddRange(this.biomes[i].corePoints);
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
    void createAlphaMaps()
    {
        resetAlphaMaps();
        this.alphaMap = new AlphaMap(new float[this.xTextureRes, this.yTextureRes, 4]);


        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.biomes[i].createRandomPoints(this.biomCount, this.xTextureRes, this.yTextureRes);
        }

        this.getUpdateALLPoint();

        Point tempPoint = new Point();
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
                for (int i = 0; i < this.allPoints.Count; i++)
                {
                    diffValues.Add(this.allPoints[i].getDiff(tempPoint));
                }

                minValue = diffValues.Min();
                minIndex = diffValues.ToList().IndexOf(minValue);
                if (minValue > this.biomWidth)
                {
                    this.alphaMap.aMap[x, y, this.biomes.Count - 1] = (float)1;
                }
                else
                {
                    this.alphaMap.aMap[x, y, allPoints[minIndex].textureIndex] = (float)1;
                }
                //if (minValue == 0 || minValue == 1)
                //{
                //    this.alphaMap.aMap[x, y, allPoints[minIndex].textureIndex] = (float)0;
                //}
            }
        }
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
    }
    void createMountains()
    {
        //this.alphaMap = new AlphaMap(tData.GetAlphamaps(0, 0, tData.alphamapWidth, tData.alphamapHeight));
        resetPoints();
        this.heightMap = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);

        //float range = (xRes + yRes) / strength;
        for (var i = 0; i < countMounts; i++)
        {

            Point newPoint = new Point();
            newPoint.set(Random.Range(0, xTerrainRes), Random.Range(0, yTerrainRes));

            int textureIndex = 0;
            Point ltPoint = new Point();
            Point lbPoint = new Point();
            Point rtPoint = new Point();
            Point rbPoint = new Point();
            do
            {
                try
                {
                    newPoint.set(Random.Range(0, xTerrainRes), Random.Range(0, yTerrainRes));
                    textureIndex = this.alphaMap.getSettedIndex(newPoint);

                }
                catch (Exception e)
                {
                    print(e);
                    textureIndex = 0;
                }
            } while (this.getNoMountainsIndexes().Contains(textureIndex));



            var leftBound = this.getMinBound(newPoint.x, montainWidth, 0);
            var rightBound = this.getMaxBound(newPoint.x, montainWidth, xTerrainRes);
            var topBound = this.getMinBound(newPoint.y, montainWidth, 0);
            var bottomBound = this.getMaxBound(newPoint.y, montainWidth, yTerrainRes);

            Point tempPoint = new Point();
            for (var y = topBound; y < bottomBound; y++)
            {
                for (var x = leftBound; x < rightBound; x++)
                {
                    //tempPoint.set(x,y);
                    //float diff = (Math.Abs(newPoint.y - y) + Math.Abs(newPoint.x - x));
                    float diff = (float)Math.Sqrt(Math.Pow(newPoint.y - y, 2) + Math.Pow(newPoint.x - x, 2));
                    //float diff = newPoint.getDiff(tempPoint);

                    if (diff <= montainWidth)
                    {
                        this.heightMap[x, y] = this.heightMap[x, y] + calcHeight(diff, montainWidth) - calcHeight(montainWidth, montainWidth);
                    }
                }
            }
        }

        tData.SetHeights(0, 0, this.heightMap);

    }
    void createAlphaMapsOnMontains()
    {
        Point tempPoint = new Point();
        int currentIndex;
        for (int y = 0; y < this.yTextureRes; y++)
        {
            for (int x = 0; x < this.xTextureRes; x++)
            {
                tempPoint.set(x, y);
                if (this.heightMap[x, y] > 0)
                {
                    currentIndex = this.alphaMap.getSettedIndex(tempPoint);
                    if (this.getNoMountainsIndexes().Contains(currentIndex))
                    {
                        this.alphaMap.aMap[x, y, currentIndex] = (float)0;
                        this.alphaMap.aMap[x, y, this.biomes.Count - 1] = (float)1;
                    }

                }
            }
        }
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
    }

    void destroyMountainsOnNonMountainAlphaMaps()
    {
        Point tempPoint = new Point();
        int currentIndex;
        for (int y = 0; y < this.yTextureRes; y++)
        {
            for (int x = 0; x < this.xTextureRes; x++)
            {
                tempPoint.set(x, y);

                currentIndex = this.alphaMap.getSettedIndex(tempPoint);
                if (this.getNoMountainsIndexes().Contains(currentIndex))
                {

                    if (this.heightMap[x, y] > 0)
                    {
                        this.heightMap[x, y] = (float)0;
                    }

                }
            }
        }
        tData.SetHeights(0, 0, this.heightMap);
    }

    void resetAlphaMaps()
    {
        float[,,] map = new float[this.xTextureRes, this.yTextureRes, 4];
        tData.SetAlphamaps(0, 0, map);
    }

}






