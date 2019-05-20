using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Linq;
using Assets;

public class TerrainerContent
{


    // the resolution of terrain
    public int xTerrainRes;
    public int yTerrainRes;
    public float terrainWidthScale;
    public float terrainLengthScale;
    public float terrainHeightScale;

    // the resolution of texture
    public int xTextureRes;
    public int yTextureRes;

    // the object, which contains the alpha maps
    // these will be used to calculate the opacity of the different textures
    public AlphaMap alphaMap;

    // this multi-dimensional array contains the height of each point in the terrain
    //public float[,] heightMap;

    // this multi-dimensional array contains the biome index of each point in the terrain
    public int[,] biomeMap;

    // this list contains all biomes
    public List<Biome> biomes = new List<Biome>();

    //this list contains all corePoints, from which the bioms are generated
    public List<BiomeCorePoint> allCorePoints = new List<BiomeCorePoint>();

    // this list contains all point from the whole map, with their properties
    public List<BiomePoint> allPoints = new List<BiomePoint>();

    private GameObjectContainer CityGameObjects;

    // terrain gameObject
    public GameObject BaseTerrainObj;
    // the terrain prop in gameObject
    public Terrain terrain;
    // the collider prop in gameObject
    public TerrainCollider BaseTerrainCollider;

    // generated
    public TerrainData tData;

    // the Player object
    public GameObject Player;

    // all cities with their houeses
    public List<City> allCities = new List<City>();

    // all corePointIds, in which are placed houses
    public List<int> createdCityIds = new List<int>();


    public Terrainer terr;
    public DetailsMap DetailsMap;
    public TreeMap TreeMap;
    public MountMap MountMap;

    public TerrainerContent(Terrainer terr)
    {
        this.terr = terr;
    }

    public int randomGenerator(int inputSeed)
    {
        int newSeed = inputSeed;
        if (newSeed == 0)
        {
            newSeed = (new System.Random()).Next();
            Debug.Log("New Seed: " + newSeed);
        }
        Terrainer.rnd = new System.Random(newSeed);
        return newSeed;

    }

    public void createTerrainObject()
    {
        this.BaseTerrainObj = new GameObject("TerrainIsle");
        this.BaseTerrainObj.transform.parent = terr.transform;
        terr.BaseTerrainData.baseMapResolution = 1024;
        int baseResolution = (int)Math.Pow(2, 8 + terr.terrainScalingFactor) + 1;
        terr.BaseTerrainData.SetDetailResolution(baseResolution, 16);
        terr.BaseTerrainData.heightmapResolution = baseResolution;
        terr.BaseTerrainData.alphamapResolution = baseResolution;

        terr.BaseTerrainData.size = new Vector3(baseResolution, 600, baseResolution);

        this.BaseTerrainCollider = this.BaseTerrainObj.AddComponent<TerrainCollider>();
        this.BaseTerrainCollider.terrainData = terr.BaseTerrainData;

        this.terrain = BaseTerrainObj.AddComponent<Terrain>();
        this.terrain.terrainData = terr.BaseTerrainData;

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

        this.alphaMap = new AlphaMap(new float[this.xTextureRes, this.yTextureRes, 6]);
        this.biomeMap = new int[this.xTextureRes, this.yTextureRes];
        this.MountMap = new MountMap(this);
        this.DetailsMap = new DetailsMap(this);
        this.TreeMap = new TreeMap(this);


        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
        tData.SetDetailLayer(0, 0, 0, this.DetailsMap.dMap);
        tData.SetHeights(0, 0, this.MountMap.heightMap);
        tData.treeInstances = new List<TreeInstance>().ToArray();

        //print("Width: " + xTerrainRes + " / " + xTextureRes + " / " + terrainWidthScale + " / " + tData.alphamapResolution);
        //print("Length: " + yTerrainRes + " / " + yTextureRes + " / " + terrainLengthScale + " / " + tData.detailResolution);

    }
    public void createBiomesAndTextures()
    {
        // generate the different Biomes
        this.biomes.Add(new Biome("city", 0, terr.cityBiom, 0.05f, false));
        this.biomes.Add(new Biome("forest", 1, terr.forestBiom, 0.2f, false));
        this.biomes.Add(new Biome("water", 2, terr.waterBiom, 0.5f, true));
        this.biomes.Add(new Biome("field", 3, terr.fieldBiom, 0.25f, false));
        this.biomes.Add(new Biome("mountain", 4, terr.mountBiom, 0.4f, false));

        SplatPrototype[] terrainTextures = new SplatPrototype[6];
        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTextures[i] = new SplatPrototype();
        }
        terrainTextures[this.biomes[0].textureIndex].texture = terr.cityTerrainTextures;
        terrainTextures[this.biomes[1].textureIndex].texture = terr.forestTerrainTextures;
        terrainTextures[this.biomes[2].textureIndex].texture = terr.waterTerrainTextures;
        terrainTextures[this.biomes[3].textureIndex].texture = terr.fieldTerrainTextures;
        terrainTextures[this.biomes[4].textureIndex].texture = terr.mountTerrainTextures;
        terrainTextures[5].texture = terr.streetTerrainTextures;
        tData.splatPrototypes = terrainTextures;

        DetailPrototype[] grassDetailPrototype = new DetailPrototype[1];
        grassDetailPrototype[0] = new DetailPrototype();
        grassDetailPrototype[0].renderMode = DetailRenderMode.GrassBillboard;
        grassDetailPrototype[0].prototypeTexture = terr.grassTexture;

        //grassDetailPrototype[0].renderMode = DetailRenderMode.Grass;
        //grassDetailPrototype[0].renderMode = DetailRenderMode.VertexLit;
        //grassDetailPrototype[0].prototype = this.grassObject;
        //grassDetailPrototype[0].usePrototypeMesh = true;

        grassDetailPrototype[0].bendFactor = 1.0f;
        grassDetailPrototype[0].dryColor = new Color(150, 150, 0);
        grassDetailPrototype[0].healthyColor = new Color(0, 255, 0);
        grassDetailPrototype[0].minWidth = 0;
        grassDetailPrototype[0].maxWidth = 2;
        grassDetailPrototype[0].minHeight = 0;
        grassDetailPrototype[0].maxHeight = 2;

        tData.detailPrototypes = grassDetailPrototype;

        tData.wavingGrassAmount = 0.5f;
        tData.wavingGrassSpeed = 0.5f;
        tData.wavingGrassStrength = 0.5f;

        TreePrototype[] treePrototypes = new TreePrototype[3];
        for (int i = 0; i < terr.treeObjects.Length; i++)
        {
            treePrototypes[i] = new TreePrototype();
            treePrototypes[i].prefab = terr.treeObjects[i];
        }
        tData.treePrototypes = treePrototypes;
    }

    public void createPrefabObjects()
    {
        // load all Gameobjects in the other objects
        this.CityGameObjects = new GameObjectContainer(this.terr.cityStages, this.terr.cityGroundFloors, this.terr.cityRooftops);

        // ... other Objects
    }


    public void createGameObjects()
    {
        this.createWaterObject();
        this.createHouseGrid();
        this.calculateGreenDiamonds();
    }

    public void createWaterObject()
    {
        // create Waterobject
        Vector3 waterPosition = new Vector3(0, terrainHeightScale * MountMap.WATER_HEIGHT, 0);
        GameObject waterGameObject = this.terr.CreateGameObject(this.terr.waterObjectTransform, waterPosition) as GameObject;
        waterGameObject.transform.localScale = new Vector3(terrainWidthScale, 1, terrainLengthScale);
    }
    void createHouseGrid()
    {
        BiomePoint tempPoint;
        BiomePoint allPointTempPoint;
        List<BiomeCorePoint> usedCorePoints = new List<BiomeCorePoint>();
        List<int> usedCorePointIds = new List<int>();
        List<float> diffValues;
        float maxValue;
        bool isGridable;
        bool isMultiplicationFromOldPoint;
        int indexOfCorePoint = -1;
        for (int k = 0; k < this.biomes[0].contentPoints.Count; k++)
        {
            isGridable = true;
            isMultiplicationFromOldPoint = true;
            tempPoint = this.biomes[0].contentPoints[k];

            indexOfCorePoint = usedCorePointIds.IndexOf(tempPoint.corePointId);
            if (indexOfCorePoint != -1)
            {
                isMultiplicationFromOldPoint = (tempPoint.x - usedCorePoints[indexOfCorePoint].x) % HouseTile.houseWidth == 0;
            }
            if (isMultiplicationFromOldPoint)
            {
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
                                    this.allPoints[tempPoint.indexInGlobalAllPoints + i + (j * xTextureRes)].isBusy ==
                                    true)
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
                                tempPoint.isBusy = true;
                            }

                        }
                    }
                }
                if (isGridable && !tempPoint.isBusy)
                {
                    // uncomment for grid points
                    //this.alphaMap.aMap[tempPoint.x, tempPoint.y, 0] = 0;

                    City currentCity;
                    int cityId = createdCityIds.IndexOf(tempPoint.corePointId);

                    if (cityId == -1)
                    {
                        currentCity = new City(this.allCorePoints[tempPoint.corePointId]);
                        allCities.Add(currentCity);
                        createdCityIds.Add(tempPoint.corePointId);
                    }
                    else
                    {
                        currentCity = allCities[cityId];
                    }

                    this.allPoints[tempPoint.indexInGlobalAllPoints].isBusy = true;
                    diffValues = new List<float>();

                    for (int i = 0; i < HouseTile.houseWidth; i++)
                    {
                        for (int j = 0; j < HouseTile.houseLength; j++)
                        {
                            allPointTempPoint =
                                this.allPoints[tempPoint.indexInGlobalAllPoints + i + (j * xTextureRes)];
                            allPointTempPoint.isBusy = true;
                            //this.alphaMap.aMap[allPointTempPoint.x, allPointTempPoint.y,0] = 0;
                            diffValues.Add(this.MountMap.heightMap[allPointTempPoint.x, allPointTempPoint.y]);
                        }
                    }
                    maxValue = diffValues.Min();

                    // TODO: make random which house is in it
                    // first step: add only numbers to City
                    // 0 --> null
                    // 1 --> house
                    // 2 --> empty
                    // 3 --> ?      (cityPlaceHolderId)
                    // second step: run function render City from city
                    //              -- in it the city runs through each object and render a CityTyle on base of the suroúnding datas
                    //                      --> for that calculate the left, right, top and bottom tile for the specific tile
                    // ==> for that create city map --> int[x,y] = cityPlaceHolderId ==> indicates, what for a thing is there

                    HouseTile ht = CityGameObjects.CreateHouse(tempPoint, (int)Terrainer.rnd.Next(2, 5), maxValue);

                    currentCity.AddTile(ht);
                    renderHouseTile(ht);

                    usedCorePoints.Add(this.allCorePoints[tempPoint.corePointId]);
                    usedCorePointIds.Add(tempPoint.corePointId);
                }
            }
        }
        renderPlayObjects();

        //color the street
        for (int i = 0; i < allCities.Count; i++)
        {
            for (int m = 0; m < allCities[i].cityObjects.Count; m++)
            {
                for (int j = 0; j < HouseTile.streetSize; j++)
                {
                    Point tempStreetPoint = allCities[i].cityObjects[m].objectToRender.startPoint;

                    for (int k = 0; k < HouseTile.houseWidth; k++)
                    {
                        this.alphaMap.aMap[tempStreetPoint.x + k, tempStreetPoint.y + j, 5] = 1;
                        this.alphaMap.aMap[tempStreetPoint.x + k, tempStreetPoint.y + j, 0] = 0;
                        this.alphaMap.aMap[tempStreetPoint.x + j, tempStreetPoint.y + k, 5] = 1;
                        this.alphaMap.aMap[tempStreetPoint.x + j, tempStreetPoint.y + k, 0] = 0;
                        //  this.alphaMap.aMap[allHouses[i].startPoint.x, allHouses[i].startPoint.y+j, 5] = 1;
                        // this.alphaMap.aMap[allHouses[i].startPoint.x+j, allHouses[i].startPoint.y, 5] = 1;
                    }
                    for (int k = 0; k < HouseTile.houseLength; k++)
                    {
                        this.alphaMap.aMap[tempStreetPoint.x + k, tempStreetPoint.y + j + HouseTile.houseLength - HouseTile.streetSize, 5] = 1;
                        this.alphaMap.aMap[tempStreetPoint.x + k, tempStreetPoint.y + j + HouseTile.houseLength - HouseTile.streetSize, 0] = 0;
                        this.alphaMap.aMap[tempStreetPoint.x + j + HouseTile.houseLength - HouseTile.streetSize, tempStreetPoint.y + k, 5] = 1;
                        this.alphaMap.aMap[tempStreetPoint.x + j + HouseTile.houseLength - HouseTile.streetSize, tempStreetPoint.y + k, 0] = 0;
                    }
                }
            }

        }



        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);
    }

    public void renderPlayObjects()
    {
        for (int i = 0; i < this.allCities.Count; i++)
        {
            int rnd = Terrainer.rnd.Next(0, this.allCities[i].cityObjects.Count);
            Point tmpPoint = this.allCities[i].cityObjects[rnd].objectToRender.startPoint;
            float height = this.allCities[i].cityObjects[rnd].objectToRender.height;

            GameObject chestGameObject = this.terr.CreateGameObject(
                this.terr.ChestGameObjects[0],
                new Vector3(
                    tmpPoint.y - (terrainLengthScale / 2),
                    (height * terrainHeightScale),
                    tmpPoint.x - (terrainWidthScale / 2))) as GameObject;


            int rndDiamontNumber = Terrainer.rnd.Next(1, this.terr.CollectableGameObjects.Length);
            this.terr.playerDiamondMaxCount += rndDiamontNumber * 10;
            GameObject diamondGameObject = this.renderDiamond(tmpPoint, height, rndDiamontNumber, 25);
        }

        for (int i = 0; i < this.allCities.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int rnd = Terrainer.rnd.Next(0, this.allCities[i].cityObjects.Count);

                CityTile tempTile = this.allCities[i].cityObjects[rnd].objectToRender;
                float height = tempTile.height + (tempTile.stageHeight * (tempTile.stageCount - 2)) / terrainHeightScale;

                Point renderPoint = new Point(tempTile.renderPoint.x, tempTile.renderPoint.y);

                int rndKeyNumber = Terrainer.rnd.Next(0, this.terr.KeyGameObjects.Length);
                GameObject keyGameObject = this.renderKey(renderPoint, height, rndKeyNumber, 20);

            }

        }
    }
    GameObject renderDiamond(Point tempPoint, float height, int diamondNumber, int scaling)
    {
        GameObject gameObject = this.terr.CreateGameObject(
            this.terr.CollectableGameObjects[diamondNumber],
            new Vector3(
                tempPoint.y - (terrainLengthScale / 2),
                (height * terrainHeightScale) + (0.02f * scaling),
                tempPoint.x - (terrainWidthScale / 2))) as GameObject;
        gameObject.transform.localScale = new Vector3(scaling, scaling, scaling);
        return gameObject;
    }

    GameObject renderKey(Point tempPoint, float height, int keyNumber, int scaling)
    {
        GameObject gameObject = this.terr.CreateGameObject(
            this.terr.KeyGameObjects[keyNumber],
            new Vector3(
                tempPoint.y - (terrainLengthScale / 2),
                ((height) * terrainHeightScale) + 1.5f,
                tempPoint.x - (terrainWidthScale / 2))) as GameObject;
        gameObject.transform.localScale = new Vector3(scaling, scaling, scaling);
        return gameObject;
    }



    void renderHouseTile(HouseTile ht)
    {
        for (int i = 0; i < ht.houseObjects.Length; i++)
        {
            //TODO ebnen der Fläche unter einem haus auf dessen entsprechenden Höhe
            GameObject buildedGameObject = this.terr.CreateGameObject(
                ht.houseObjects[i],
                new Vector3(
                    ht.renderPoint.y - (terrainLengthScale / 2),
                    ((ht.height) * terrainHeightScale) + (ht.stageHeight * i),
                    ht.renderPoint.x - (terrainWidthScale / 2)
                )) as GameObject;
            buildedGameObject.transform.localScale = new Vector3(HouseTile.houseScaleFactor, HouseTile.houseScaleFactor, HouseTile.houseScaleFactor);
        }
        for (int i = -1; i < HouseTile.houseWidth - HouseTile.postHouseWidth - HouseTile.preHouseWidth; i++)
        {
            for (int j = -1; j < HouseTile.houseLength - HouseTile.postHouseLength - HouseTile.preHouseLength; j++)
            {
                this.MountMap.heightMap[ht.startPoint.x + i, ht.startPoint.y + j] = (ht.height);
            }
        }
        this.MountMap.setHeightMapWithStandardHeights();
    }


    //TODO: Tag Nacht | Windzone
    // the player must be initialized after each height-changing, else the collider is not working
    public void createPlayer()
    {
        int rndCityNumber = Terrainer.rnd.Next(0, this.allCities.Count);
        int rndHouseNumber = Terrainer.rnd.Next(0, this.allCities[rndCityNumber].cityObjects.Count);
        CityTile currentCityTile = this.allCities[rndCityNumber].cityObjects[rndHouseNumber].objectToRender;
        Point playerPos = new Point(currentCityTile.startPoint.x, currentCityTile.renderPoint.y);
        float height = ((currentCityTile.height + (currentCityTile.stageHeight * (currentCityTile.stageCount - 2)) / terrainHeightScale) * terrainHeightScale) + 1.5f;


        this.Player = this.terr.CreateGameObject(
            this.terr.FirstPersonGameObject,
            new Vector3(playerPos.y - (int)terrainWidthScale / 2, height, playerPos.x - (int)terrainLengthScale / 2)) as GameObject;
    }


    public void calculateGreenDiamonds()
    {
        // empty the old treeInstaces from persistent data
        int greenDiamondCount = 50; // TODO: with global scaleFactor
        int greenDiamondCounter = 0;

        while (greenDiamondCounter < greenDiamondCount)
        {
            int xDiaPos = Terrainer.rnd.Next(0, xTerrainRes);
            int yDiaPos = Terrainer.rnd.Next(0, yTerrainRes);
            int treePrototypeNumber = -1;
            if (biomeMap[xDiaPos, yDiaPos] == 1 || biomeMap[xDiaPos, yDiaPos] == 3 || biomeMap[xDiaPos, yDiaPos] == 4)
            {
                //only green diamonds
                GameObject diamondGameObject = this.renderDiamond(new Point(xDiaPos, yDiaPos), this.MountMap.heightMap[xDiaPos, yDiaPos], 0, 50);
                greenDiamondCounter++;
            }
        }
    }


    public void resetMaps()
    {
        resetPoints();
        resetAlphaMaps();
    }



    public void getUpdateAllCorePoints()
    {
        this.allCorePoints = new List<BiomeCorePoint>();
        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.biomes[i].startCoreIds = this.allCorePoints.Count;
            this.allCorePoints.AddRange(this.biomes[i].corePoints);
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









    // reset all textures
    public void resetAlphaMaps()
    {
        float[,,] map = new float[this.xTextureRes, this.yTextureRes, 6];
        tData.SetAlphamaps(0, 0, map);
    }

    // reset all heights to the standardheight
    public void resetPoints()
    {
        var heights = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);

        for (int y = 0; y < yTerrainRes; y++)
        {
            for (int x = 0; x < xTerrainRes; x++)
            {
                heights[x, y] = MountMap.STANDARD_HEIGHT;
            }
        }
        this.terrain.terrainData.SetHeights(0, 0, heights);

    }
}
