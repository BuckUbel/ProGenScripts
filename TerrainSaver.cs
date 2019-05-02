using System;
using System.Collections.Generic;
using Assets.Scripts;
using System.Linq;
using Assets;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TerrainSaver : MonoBehaviour
{
    public static System.Random rnd;

    //
    public int biomCount = 25;

    [Header("First-Person")]
    public GameObject FirstPersonGameObject;

    [Header("Biom-Verteilung")]

    [Range(0, 1)]
    public float cityBiom = 0.2f;
    [Range(0, 1)]
    public float forestBiom = 0.25f;
    [Range(0, 1)]
    public float waterBiom = 0.25f;
    [Range(0, 1)]
    public float fieldBiom = 0.2f;
    [Range(0, 1)]
    public float mountBiom = 0.1f;

    // waterObject
    [Header("Wasser Objekt")]
    public GameObject waterObjectTransform;

    [Header("Stadt Objekte")]

    public GameObject[] cityGroundFloors;
    public GameObject[] cityStages;
    public GameObject[] cityRooftops;
    private CityObjects CityGameObjects;

    [Header("Terrains")]
    public Texture2D cityTerrainTextures;
    public Texture2D forestTerrainTextures;
    public Texture2D waterTerrainTextures;
    public Texture2D fieldTerrainTextures;
    public Texture2D mountTerrainTextures;


    // the standard height for the map
    // is necessary, because the water needs space under the normal map
    private float standardHeight = 0.05f;
    private float waterHeight = 0.0499f;
    private float playerStartHeight = 0.1f;

    // the resolution of terrain
    private int xTerrainRes;
    private int yTerrainRes;
    private float terrainWidthScale;
    private float terrainLengthScale;
    private float terrainHeightScale;

    // the resolution of texture
    private int xTextureRes;
    private int yTextureRes;

    // the object, which contains the alpha maps
    // these will be used to calculate the opacity of the different textures
    private AlphaMap alphaMap;

    // this multi-dimensional array contains the height of each point in the terrain
    private float[,] heightMap;

    // this list contains all biomes
    private List<Biome> biomes = new List<Biome>();

    //this list contains all corePoints, from which the bioms are generated
    private List<BiomeCorePoint> allCorePoints = new List<BiomeCorePoint>();

    // this list contains all point from the whole map, with their properties
    private List<BiomePoint> allPoints = new List<BiomePoint>();


    // terrain gameObject
    private GameObject BaseTerrainObj;
    // the terrain prop in gameObject
    private Terrain terrain;
    // the collider prop in gameObject
    private TerrainCollider BaseTerrainCollider;

    // TerrainData is prop of Terrain
    // the imported TerrainData
    [Header("Grundlegende TerrainData")]
    public TerrainData BaseTerrainData;
    // generated
    private TerrainData tData;

    // the Player object
    private GameObject Player;

    // all house GameObjects
    private List<HouseTile> allHouses = new List<HouseTile>();

    void randomGenerator()
    {

        var seed = (new System.Random()).Next();
        print(seed);
        rnd = new System.Random(seed);
    }

    void createTerrainObject()
    {
        this.BaseTerrainObj = new GameObject("TerrainO");

        this.BaseTerrainData.size = new Vector3(512, 600, 512);
        this.BaseTerrainData.heightmapResolution = 512;
        this.BaseTerrainData.baseMapResolution = 1024;
        this.BaseTerrainData.SetDetailResolution(1024, 16);

        this.BaseTerrainCollider = this.BaseTerrainObj.AddComponent<TerrainCollider>();
        this.BaseTerrainCollider.terrainData = this.BaseTerrainData;

        this.terrain = BaseTerrainObj.AddComponent<Terrain>();
        this.terrain.terrainData = this.BaseTerrainData;

        // get the terrain component
        //this.terrain = BaseTerrain;

        // get the generated standard data
        this.tData = terrain.terrainData;

        // get the terrain resolution
        this.xTerrainRes = tData.heightmapWidth;
        this.yTerrainRes = tData.heightmapHeight;
        this.terrainWidthScale = tData.size.x;
        this.terrainLengthScale = tData.size.z;
        this.terrainHeightScale = tData.size.y;

        // transform terrain to middle point of global coordinates system
        this.terrain.transform.position = new Vector3(-terrainWidthScale / 2, 0, -terrainLengthScale / 2);

        // get the texture resolution
        this.xTextureRes = tData.alphamapWidth;
        this.yTextureRes = tData.alphamapHeight;

    }

    void createBiomesAndTextures()
    {
        // generate the different Biomes
        this.biomes.Add(new Biome("city", 0, cityBiom, 0.05f, false));
        this.biomes.Add(new Biome("forest", 1, forestBiom, 0.15f, false));
        this.biomes.Add(new Biome("water", 2, waterBiom, 0.5f, true));
        this.biomes.Add(new Biome("field", 3, fieldBiom, 0.2f, false));
        this.biomes.Add(new Biome("mountain", 4, mountBiom, 0.6f, false));

        SplatPrototype[] terrainTextures = new SplatPrototype[5];
        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTextures[i] = new SplatPrototype();
        }
        terrainTextures[this.biomes[0].textureIndex].texture = this.cityTerrainTextures;
        terrainTextures[this.biomes[1].textureIndex].texture = this.forestTerrainTextures;
        terrainTextures[this.biomes[2].textureIndex].texture = this.waterTerrainTextures;
        terrainTextures[this.biomes[3].textureIndex].texture = this.fieldTerrainTextures;
        terrainTextures[this.biomes[4].textureIndex].texture = this.mountTerrainTextures;
        tData.splatPrototypes = terrainTextures;
    }

    void createPrefabObjects()
    {
        // load all Gameobjects in the other objects
        this.CityGameObjects = new CityObjects(this.cityStages, this.cityGroundFloors, this.cityRooftops);

        // ... other Objects
    }

    // the player must be initialized after each height-changing, else the collider is not working
    void createPlayer()
    {
        this.Player = Instantiate(FirstPersonGameObject, new Vector3(0, this.playerStartHeight * terrainHeightScale, 0), Quaternion.identity);

    }
    void Start()
    {
        this.randomGenerator();

        this.createTerrainObject();
        this.createBiomesAndTextures();
        this.createPrefabObjects();

        this.initializeMap();
        this.createGameObjects();
        this.createPlayer();
    }

    GameObject buildGameObjectFromTextureMap(GameObject newObject, Vector3 location, Vector3 scaling)
    {
        GameObject buildedGameObject = Instantiate(newObject, new Vector3(location.z - (terrainLengthScale / 2), (location.y - standardHeight) * terrainHeightScale, location.x - (terrainWidthScale / 2)), Quaternion.identity) as GameObject;
        buildedGameObject.transform.localScale = scaling;
        return buildedGameObject;
    }

    void renderHouseTile(HouseTile ht)
    {
        for (int i = 0; i < ht.houseObjects.Length; i++)
        {

            GameObject buildedGameObject = Instantiate(
                ht.houseObjects[i],
                new Vector3(
                    ht.renderPoint.y - (terrainLengthScale / 2),
                    ((ht.height - standardHeight) * terrainHeightScale) + (HouseTile.stageHeight*i),
                    ht.renderPoint.x - (terrainWidthScale / 2)
                ),
                Quaternion.identity) as GameObject;
            buildedGameObject.transform.localScale = new Vector3(HouseTile.houseScaleFactor, HouseTile.houseScaleFactor, HouseTile.houseScaleFactor);
        }
    }

    void createHouseGrid()
    {
        //create city objects

        int houseScaleFactor = 50;

        int rndFloor = rnd.Next(cityGroundFloors.Length);
        //GameObject cityGroundFloor = buildGameObjectFromTextureMap(cityGroundFloors[rndFloor], new Vector3(this.allPoints[40000].x, 1, this.allPoints[40000].y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor));
        // GameObject cityGroundFloor1 = buildGameObjectFromTextureMap(cityGroundFloors[rndFloor], new Vector3(this.allPoints[90000].x, 1, this.allPoints[90000].y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor));
        //  GameObject cityGroundFloor2 = buildGameObjectFromTextureMap(cityGroundFloors[rndFloor], new Vector3(this.allPoints[120000].x, 1, this.allPoints[120000].y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor));
        //  GameObject cityGroundFloor3 = buildGameObjectFromTextureMap(cityGroundFloors[rndFloor], new Vector3(this.allPoints[140000].x, 1, this.allPoints[140000].y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor));

        // cityGroundFloor.AddComponent<MeshFilter>();


        //int rndStage = rnd.Next(cityStages.Length);
        //(cityStages[rndStage]

        //int rndRoof = rnd.Next(cityRooftops.Length);
        //(cityRooftops[rndRoof]
        BiomePoint tempPoint;
        BiomePoint allPointTempPoint;
        List<float> diffValues;
        float maxValue;
        bool isGridable;
        int gridPoints = 0;
        for (int k = 0; k < this.biomes[0].contentPoints.Count; k++)
        {
            isGridable = true;
            tempPoint = this.biomes[0].contentPoints[k];

            if (!tempPoint.isBusy)
            {
                for (int i = 0; i < HouseTile.houseWidth; i++)
                {
                    for (int j = 0; j < HouseTile.houseLength; j++)
                    {
                        if (tempPoint.x + i < this.xTextureRes &&
                            tempPoint.y + j < this.yTextureRes)
                        {


                            if (this.biomes[0].textureIndex !=
                                this.alphaMap.getSettedIndex(new Point(tempPoint.x + i, tempPoint.y + j)) ||
                                this.allPoints[tempPoint.indexInGlobalAllPoints + i + (j * xTextureRes)].isBusy == true)
                            {

                                isGridable = false;
                                i = HouseTile.houseWidth;
                                j = HouseTile.houseLength;
                            }
                        }
                        if (tempPoint.x + i >= this.xTextureRes ||
                            tempPoint.y + j >= this.yTextureRes)
                        {

                            isGridable = false;
                            i = HouseTile.houseWidth;
                            j = HouseTile.houseLength;
                        }

                    }
                }
            }
            if (isGridable && !tempPoint.isBusy)
            {
                // uncomment for grid points
                //this.alphaMap.aMap[tempPoint.x, tempPoint.y, 0] = 0;

                this.allPoints[tempPoint.indexInGlobalAllPoints].isBusy = true;
                diffValues = new List<float>();

                for (int i = 0; i < HouseTile.houseWidth; i++)
                {
                    for (int j = 0; j < HouseTile.houseLength; j++)
                    {
                        allPointTempPoint = this.allPoints[tempPoint.indexInGlobalAllPoints + i + (j * xTextureRes)];
                        allPointTempPoint.isBusy = true;
                        //this.alphaMap.aMap[allPointTempPoint.x, allPointTempPoint.y,0] = 0;
                        diffValues.Add(this.heightMap[allPointTempPoint.x, allPointTempPoint.y]);
                    }
                }
                maxValue = diffValues.Max();
                print(maxValue);
                // create houseTile
                HouseTile ht = CityGameObjects.CreateHouse(tempPoint, 3, maxValue);
                allHouses.Add(ht);
                renderHouseTile(ht);
                // allHouses.Add(buildGameObjectFromTextureMap(this.CityGameObjects.GetHouseObjects(3), new Vector3(ht.renderPoint.x, maxValue, ht.renderPoint.y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor)));

                gridPoints++;
            }
        }
        print("Houses: " + gridPoints);

        //tData.SetAlphamaps(0, 0, this.alphaMap.aMap);



    }

    void initializeMap()
    {
        resetPoints();
        resetAlphaMaps();
        this.createAlphaMaps();
        this.calcBorders();
        this.createMountainsByBorders();

    }

    void createGameObjects()
    {
        // create Waterobject
        GameObject waterGameObject = Instantiate(waterObjectTransform, new Vector3(0, terrainHeightScale * waterHeight, 0), Quaternion.identity) as GameObject;
        waterGameObject.transform.localScale = new Vector3(terrainWidthScale, 1, terrainLengthScale);

        this.createHouseGrid();


    }

    // generate the GUI
    void OnGUI()
    {

        // the functions, which will called, if the button is clicked
        if (GUI.Button(new Rect(10, 10, 100, 25), "Generate"))
        {
            this.initializeMap();
        }
        if (GUI.Button(new Rect(10, 40, 100, 25), "Create Objects"))
        {
            this.createGameObjects();
            this.createPlayer();
        }

        if (GUI.Button(new Rect(10, 70, 100, 25), "Reset"))
        {
            resetPoints();
            resetAlphaMaps();
        }

    }

    // get the id of a specific biome from the biome-list, with a specific textureIndex
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

    // calc the border pixels and add to the biome-objects
    // for this purpose, the loop goes through each pixel and look at the arounded pixels and their textureIndex
    // if the textureIndex is different this pixel is a border pixel and will save in their associated biome
    public void calcBorders()
    {
        Point tempPoint = new Point();
        List<Point> aroundPixels;
        int textureIndex = 0;

        for (int j = 0; j < this.allPoints.Count; j++)
        {
            textureIndex = this.allPoints[j].corePoint.textureIndex;
            int biomeId = this.getBiomeIndexByTextureIndex(textureIndex);
            aroundPixels = this.allPoints[j].getArroundPoints();
            bool isBorder = false;
            for (int i = 0; i < aroundPixels.Count; i++)
            {
                if (textureIndex != this.alphaMap.getSettedIndex(aroundPixels[i]))
                {
                    this.biomes[biomeId].addBorderPoint(this.allPoints[j], j);
                    isBorder = true;
                    // if one pixel has different t-Id, the loop should be break
                    i = aroundPixels.Count;
                }
            }
            if (!isBorder)
            {
                this.biomes[biomeId].addContentPoint(this.allPoints[j], j);
                //this.alphaMap.aMap[this.allPoints[j].x, this.allPoints[j].y, this.biomes[biomeId].textureIndex] = 0;
            }
        }
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);


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

    // calc the height for a mountain
    float calcHeight(float x, float range, float maxHeight)
    {
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1);
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

    // save the corePoints from the biomes into the local variable
    void getUpdateAllCorePoints()
    {
        this.allCorePoints = new List<BiomeCorePoint>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.allCorePoints.AddRange(this.biomes[i].corePoints);
        }
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

    void createAlphaMaps()
    {
        this.alphaMap = new AlphaMap(new float[this.xTextureRes, this.yTextureRes, 5]);

        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.biomes[i].createRandomPoints(this.biomCount, this.xTextureRes, this.yTextureRes);
        }

        this.getUpdateAllCorePoints();

        Point tempPoint = new Point();
        BiomePoint newBiomePoint = new BiomePoint();
        List<int> diffValues;
        int minValue = int.MaxValue;
        int secondMinValue = int.MaxValue;
        int minIndex = -1;
        int secondMinIndex = -1;

        // For each point on the alphamap...
        for (int y = 0; y < this.yTextureRes; y++)

        {
            for (int x = 0; x < this.xTextureRes; x++)
            {
                // reset values for diff calculation
                diffValues = new List<int>();
                minValue = int.MaxValue;
                secondMinValue = int.MaxValue;

                tempPoint.set(x, y);

                // get the smallest diff to a core
                for (int corePointIndex = 0; corePointIndex < this.allCorePoints.Count; corePointIndex++)
                {
                    int corePointDiff = this.allCorePoints[corePointIndex].getDiff(tempPoint);
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


                // create new BiomePoint on the base of the corePoint
                newBiomePoint = new BiomePoint(x, y, allCorePoints[minIndex], minIndex);

                // added to the allPoint List of these run-class
                this.allPoints.Add(newBiomePoint);


                // added to the allPoint List of the biome
                this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)1;
                this.biomes[this.getBiomeIndexByTextureIndex(allCorePoints[minIndex].textureIndex)].allPoints.Add(newBiomePoint);


                // border point --> points with almost equal distances to two cores
                if ((secondMinValue == minValue || secondMinValue == minValue + 1 || secondMinValue == minValue - 1) && allCorePoints[secondMinIndex].textureIndex != allCorePoints[minIndex].textureIndex)
                {
                    // biomes shares this area of border --> on texture
                    this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)0.5;
                    this.alphaMap.aMap[x, y, allCorePoints[secondMinIndex].textureIndex] = (float)0.5;

                    // --> also on allPoints List in the biomes
                    this.biomes[this.getBiomeIndexByTextureIndex(allCorePoints[secondMinIndex].textureIndex)].allPoints.Add(newBiomePoint);

                }
                // uncomment to mark the biome cores
                /*if (minValue == 0 || minValue == 1)
                 {
                  this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)0;
                  this.biomes[this.getBiomeIndexByTextureIndex(allCorePoints[minIndex].textureIndex)].allPoints.Add(newBiomePoint);

                 }*/

            }
        }

        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
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
                biomeSize = 10000 * this.biomes[i].allPoints.Count / this.allPoints.Count;
                mountPercentage = this.biomes[i].mountainsDegree;
                //print(biomeSize);
                mountCount = (int)(biomeSize * mountPercentage);
                for (int j = 0; j < mountCount; j++)
                {
                    newPoint = this.biomes[i].allPoints[Random.Range(0, this.biomes[i].allPoints.Count)];

                    diffValues = new List<float>();
                    for (int k = 0; k < this.biomes[i].borderPoints.Count; k++)
                    {
                        // this.alphaMap.aMap[this.biomes[i].borderPoints[k].x, this.biomes[i].borderPoints[k].y, this.biomes[i].textureIndex] = 0;

                        // diffValues.Add(this.biomes[i].borderPoints[k].getPythagorasDiff(newPoint));
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

        this.setHeightMapWithStandardHeights();
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);

    }

    // set a new heightmap and add the standardheight
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

    // reset all textures
    void resetAlphaMaps()
    {
        float[,,] map = new float[this.xTextureRes, this.yTextureRes, 5];
        tData.SetAlphamaps(0, 0, map);
    }

    // reset all heights to the standardheight
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
        this.terrain.terrainData.SetHeights(0, 0, heights);

    }

}
