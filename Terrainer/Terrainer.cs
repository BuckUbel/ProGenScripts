using System;
using System.Collections.Generic;
using Assets.Scripts;
using System.Linq;
using Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Terrainer : MonoBehaviour
{
    public static System.Random rnd;

    //
    public int biomCount = 25;
    public int terrainScalingFactor = 1;
    // diff to the edge for the isle
    public int isleDiff = 50;

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

    [Header("First-Person")]
    public GameObject FirstPersonGameObject;


    private static int keyCountPerChest = 3;

    private float playerRunSpeed = 10.0f;
    private float playerJumpHeight = 10.0f;
    private int playerKeyCount = 0;
    private int playerDiamondCount = 0;


    [Header("Collect&Win")]
    public GameObject[] KeyGameObjects;
    public int chestCount = 1;
    public GameObject[] ChestGameObjects;
    public GameObject[] CollectableGameObjects;

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
    public Texture2D streetTerrainTextures;

    [Header("Bäume und Grässer")]
    public GameObject[] treeObjects;
    public GameObject grassObject;
    public Texture2D grassTexture;

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

    // this multi-dimensional array contains the biome index of each point in the terrain
    private int[,] biomeMap;

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

    private int currentSeed;

    void randomGenerator()
    {

        currentSeed = (new System.Random()).Next();
        print(currentSeed);
        rnd = new System.Random(currentSeed);
    }

    void createTerrainObject()
    {
        this.BaseTerrainObj = new GameObject("TerrainIsle");
        this.BaseTerrainObj.transform.parent = this.transform;
        this.BaseTerrainData.baseMapResolution = 1024;
        int baseResolution = (int)Math.Pow(2, 8 + terrainScalingFactor) + 1;
        this.BaseTerrainData.SetDetailResolution(baseResolution, 16);
        this.BaseTerrainData.heightmapResolution = baseResolution;
        this.BaseTerrainData.alphamapResolution = baseResolution;

        this.BaseTerrainData.size = new Vector3(baseResolution, 600, baseResolution);

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

        this.alphaMap = new AlphaMap(new float[this.xTextureRes, this.yTextureRes, 6]);
        this.biomeMap = new int[this.xTextureRes, this.yTextureRes];
        this.heightMap = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);

        //print("Width: " + xTerrainRes + " / " + xTextureRes + " / " + terrainWidthScale + " / " + tData.alphamapResolution);
        //print("Length: " + yTerrainRes + " / " + yTextureRes + " / " + terrainLengthScale + " / " + tData.detailResolution);

    }

    void createBiomesAndTextures()
    {
        // generate the different Biomes
        this.biomes.Add(new Biome("city", 0, cityBiom, 0.05f, false));
        this.biomes.Add(new Biome("forest", 1, forestBiom, 0.2f, false));
        this.biomes.Add(new Biome("water", 2, waterBiom, 0.5f, true));
        this.biomes.Add(new Biome("field", 3, fieldBiom, 0.25f, false));
        this.biomes.Add(new Biome("mountain", 4, mountBiom, 0.4f, false));

        SplatPrototype[] terrainTextures = new SplatPrototype[6];
        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTextures[i] = new SplatPrototype();
        }
        terrainTextures[this.biomes[0].textureIndex].texture = this.cityTerrainTextures;
        terrainTextures[this.biomes[1].textureIndex].texture = this.forestTerrainTextures;
        terrainTextures[this.biomes[2].textureIndex].texture = this.waterTerrainTextures;
        terrainTextures[this.biomes[3].textureIndex].texture = this.fieldTerrainTextures;
        terrainTextures[this.biomes[4].textureIndex].texture = this.mountTerrainTextures;
        terrainTextures[5].texture = this.streetTerrainTextures;
        tData.splatPrototypes = terrainTextures;

        DetailPrototype[] grassDetailPrototype = new DetailPrototype[1];
        grassDetailPrototype[0] = new DetailPrototype();
        grassDetailPrototype[0].renderMode = DetailRenderMode.GrassBillboard;
        grassDetailPrototype[0].prototypeTexture = this.grassTexture;

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
        for (int i = 0; i < this.treeObjects.Length; i++)
        {
            treePrototypes[i] = new TreePrototype();
            treePrototypes[i].prefab = this.treeObjects[i];
        }
        tData.treePrototypes = treePrototypes;
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

        PrefabUtility.CreatePrefab("Assets/SaveData/ProGen/Terrains/" + currentSeed + ".prefab", this.BaseTerrainObj);
        PrefabUtility.CreatePrefab("Assets/SaveData/ProGen/World/" + currentSeed + ".prefab", this.gameObject);
    }
    void createPrefabObjects()
    {
        // load all Gameobjects in the other objects
        this.CityGameObjects = new CityObjects(this.cityStages, this.cityGroundFloors, this.cityRooftops);

        // ... other Objects
    }
    void initializeMap()
    {
        resetPoints();
        resetAlphaMaps();
        this.createAlphaMaps();
        this.calcBorders();
        this.createMountainsByBorders();

        // Get all of layer zero.
        var map = tData.GetDetailLayer(0, 0, tData.detailWidth, tData.detailHeight, 0);

        // For each pixel in the detail map...
        for (var y = 0; y < tData.detailHeight; y++)
        {
            for (var x = 0; x < tData.detailWidth; x++)
            {
                map[x, y] = 0;
                if (this.biomeMap[x, y] == 1)
                {
                    map[x, y] = 1;
                }
                if (this.biomeMap[x, y] == 3)
                {
                    map[x, y] = 1;
                }
                if (this.biomeMap[x, y] == 4)
                {
                    map[x, y] = 1;
                }
            }
        }

        // Assign the modified map back.
        tData.SetDetailLayer(0, 0, 0, map);

        // empty the old treeInstaces from persistent data
        this.terrain.terrainData.treeInstances = new List<TreeInstance>().ToArray();
        int[,] treeMap = new int[xTerrainRes, yTerrainRes];

        int treeCount = 1000; // TODO: with global scaleFactor
        int treeCounter = 0;

        while (treeCounter < treeCount)
        {
            int xTreePos = rnd.Next(0, xTerrainRes);
            int yTreePos = rnd.Next(0, yTerrainRes);
            int treePrototypeNumber = -1;
            if (this.biomeMap[xTreePos, yTreePos] == 1)
            {
                treePrototypeNumber = rnd.Next(0, 2);
            }
            if (this.biomeMap[xTreePos, yTreePos] == 3)
            {
                treePrototypeNumber = 0;
            }
            if (this.biomeMap[xTreePos, yTreePos] == 4)
            {
                treePrototypeNumber = 1;
            }
            if (treePrototypeNumber >= 0)
            {
                //TODO: scalefactor depends on the height of terrain
                TreeInstance pTI = new TreeInstance();
                float yPos = (float)xTreePos / (float)xTerrainRes;
                float xPos = (float)yTreePos / (float)yTerrainRes;
                pTI.position = new Vector3(xPos, 1, yPos);
                float scaleFactor = (float)rnd.NextDouble();
                pTI.widthScale = scaleFactor;
                pTI.heightScale = scaleFactor;
                pTI.color = Color.yellow;
                pTI.lightmapColor = Color.yellow;
                pTI.prototypeIndex = treePrototypeNumber; //?
                this.terrain.AddTreeInstance(pTI);
                treeCounter++;
            }
        }
    }

    void createGameObjects()
    {

        // create Waterobject
        GameObject waterGameObject = Instantiate(waterObjectTransform, new Vector3(0, terrainHeightScale * waterHeight, 0), Quaternion.identity) as GameObject;
        waterGameObject.transform.localScale = new Vector3(terrainWidthScale, 1, terrainLengthScale);
        //PrefabUtility.ConnectGameObjectToPrefab(this.BaseTerrainObj, waterGameObject);
        waterGameObject.transform.parent = this.transform;

        this.createHouseGrid();

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
                //print(maxValue);
                // create houseTile
                HouseTile ht = CityGameObjects.CreateHouse(tempPoint, (int)rnd.Next(2, 5), maxValue);
                //for (int i = 0; i < ht.houseObjects.Length; i++)
                //{
                //    PrefabUtility.ConnectGameObjectToPrefab(this.BaseTerrainObj, ht.houseObjects[i]);
                //}
                allHouses.Add(ht);
                renderHouseTile(ht);
                // allHouses.Add(buildGameObjectFromTextureMap(this.CityGameObjects.GetHouseObjects(3), new Vector3(ht.renderPoint.x, maxValue, ht.renderPoint.y), new Vector3(houseScaleFactor, houseScaleFactor, houseScaleFactor)));

                gridPoints++;
            }
        }
        print("Houses: " + gridPoints);

        for (int i = 0; i < allHouses.Count; i++)
        {
            for (int j = 0; j < HouseTile.streetSize; j++)
            {
                for (int k = 0; k < HouseTile.houseWidth; k++)
                {

                    this.alphaMap.aMap[allHouses[i].startPoint.x + k, allHouses[i].startPoint.y + j, 5] = 1;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + k, allHouses[i].startPoint.y + j, 0] = 0;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + j, allHouses[i].startPoint.y + k, 5] = 1;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + j, allHouses[i].startPoint.y + k, 0] = 0;
                    //  this.alphaMap.aMap[allHouses[i].startPoint.x, allHouses[i].startPoint.y+j, 5] = 1;
                    // this.alphaMap.aMap[allHouses[i].startPoint.x+j, allHouses[i].startPoint.y, 5] = 1;
                }
                for (int k = 0; k < HouseTile.houseLength; k++)
                {
                    this.alphaMap.aMap[allHouses[i].startPoint.x + k, allHouses[i].startPoint.y + j + HouseTile.houseLength - HouseTile.streetSize, 5] = 1;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + k, allHouses[i].startPoint.y + j + HouseTile.houseLength - HouseTile.streetSize, 0] = 0;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + j + HouseTile.houseLength - HouseTile.streetSize, allHouses[i].startPoint.y + k, 5] = 1;
                    this.alphaMap.aMap[allHouses[i].startPoint.x + j + HouseTile.houseLength - HouseTile.streetSize, allHouses[i].startPoint.y + k, 0] = 0;
                }
            }
        }
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);



    }
    //TODO: Tag Nacht | Windzone
    // the player must be initialized after each height-changing, else the collider is not working
    void createPlayer()
    {
        Point playerPos = new Point((int)terrainWidthScale / 2, (int)terrainLengthScale / 2);
        this.Player = Instantiate(FirstPersonGameObject, new Vector3(0, this.heightMap[playerPos.x, playerPos.y] * terrainHeightScale, 0), Quaternion.identity);
        this.Player.transform.parent = this.transform;


        // rb.col
    }
    public void CollisionDetected(string tagName, GameObject collidedGameObject)
    {
        Debug.Log(tagName);
        if (tagName == "Key")
        {
            this.playerKeyCount++;
        }
        if (tagName == "Chest")
        {
            ChestController cc = collidedGameObject.GetComponent<ChestController>();

            if (this.playerKeyCount >= keyCountPerChest && !cc.isUnlocked)
            {
                cc.Unlock();
                this.playerKeyCount -= keyCountPerChest;
            }
        }
        if (tagName == "Diamond")
        {
            DiamondController dc = collidedGameObject.GetComponent<DiamondController>();
            this.playerDiamondCount += dc.value;
            if (this.playerDiamondCount > 999)
            {
                this.playerDiamondCount = 999;
            }
        }
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
                    ((ht.height - standardHeight) * terrainHeightScale) + (HouseTile.stageHeight * i),
                    ht.renderPoint.x - (terrainWidthScale / 2)
                ),
                Quaternion.identity) as GameObject;
            buildedGameObject.transform.localScale = new Vector3(HouseTile.houseScaleFactor, HouseTile.houseScaleFactor, HouseTile.houseScaleFactor);
            buildedGameObject.transform.parent = this.transform;
        }

        GameObject chestGameObject = Instantiate(
            this.ChestGameObjects[0],
            new Vector3(
            ht.startPoint.y - (terrainLengthScale / 2),
            ((ht.height - standardHeight) * terrainHeightScale),
            ht.startPoint.x - (terrainWidthScale / 2)),
            Quaternion.identity) as GameObject;
        chestGameObject.transform.parent = this.transform;

        int rndDiamontNumber = rnd.Next(0, this.CollectableGameObjects.Length);

        GameObject diamonGameObject = Instantiate(
            this.CollectableGameObjects[rndDiamontNumber],
            new Vector3(
                ht.startPoint.y - (terrainLengthScale / 2),
                ((ht.height - standardHeight) * terrainHeightScale) + 0.2f,
                ht.startPoint.x - (terrainWidthScale / 2)),
            Quaternion.identity) as GameObject;
        diamonGameObject.transform.localScale = new Vector3(25, 25, 25);
        diamonGameObject.transform.parent = this.transform;

        int rndChestNumber = rnd.Next(0, this.KeyGameObjects.Length);

        GameObject keyGameObject = Instantiate(
            this.KeyGameObjects[rndChestNumber],
            new Vector3(
                ht.startPoint.y - (terrainLengthScale / 2) + HouseTile.houseWidth,
                ((ht.height - standardHeight) * terrainHeightScale) + 1,
                ht.startPoint.x - (terrainWidthScale / 2)),
            Quaternion.identity) as GameObject;

        keyGameObject.transform.parent = this.transform;

    }

    private float barDisplay = 0;
    public Texture2D keyImageSprite;

    void Update()
    {
        // for this example, the bar display is linked to the current time,
        // however you would set this value based on your desired display
        // eg, the loading progress, the player's health, or whatever.
        barDisplay = Time.time * 0.05f;
    }
    void OnGUI()
    {
        //Vector2 pos = new Vector2(20, 40);
        //Vector2 size = new Vector2(60, 20);
        //Texture2D progressBarEmpty = this.waterTerrainTextures;
        //Texture2D progressBarFull = this.cityTerrainTextures;


        //// draw the background:
        //GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y));
        //GUI.Box(new Rect(0, 0, size.x, size.y), progressBarEmpty);

        //// draw the filled-in part:
        //GUI.BeginGroup(new Rect(0, 0, size.x * barDisplay, size.y));
        //GUI.Box(new Rect(0, 0, size.x, size.y), progressBarFull);
        //GUI.EndGroup();

        //GUI.EndGroup();

        for (int i = 0; i < playerKeyCount; i++)
        {
            GUI.DrawTexture(new Rect(10 + i * 50, 10, 50, 50), keyImageSprite);
            if (i > 3)
            {
                // TODO: Plus image
                GUI.DrawTexture(new Rect(10 + i * 50, 10, 50, 50), keyImageSprite);
                break;
            }
        }
        GUI.Label(new Rect(Screen.width - 100, 10, 100, 100), "Diamanten: "+ this.playerDiamondCount);

        //GUI.Box(new Rect(Screen.width / 2 - 100, 25, 100, 30), "Startmenu");

        //// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        //if (GUI.Button(new Rect(Screen.width / 2 - 40, Screen.height / 2 - 10, 80, 20), "Generate a new map"))
        //{

        //    playerRunSpeed = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 50, Screen.height - 50, 100, 25), playerRunSpeed, 10.0f, 50.0f);
        //    playerJumpHeight = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 50, Screen.height - 25, 100, 25), playerJumpHeight, 10.0f, 50.0f);
        //}


    }
    // generate the GUI
    //void OnGUI()
    //{

    //    // the functions, which will called, if the button is clicked
    //    if (GUI.Button(new Rect(10, 10, 100, 25), "Generate"))
    //    {
    //        this.initializeMap();
    //    }
    //    if (GUI.Button(new Rect(10, 40, 100, 25), "Create Objects"))
    //    {
    //        this.createGameObjects();
    //        this.createPlayer();
    //    }

    //    if (GUI.Button(new Rect(10, 70, 100, 25), "Reset"))
    //    {
    //        resetPoints();
    //        resetAlphaMaps();
    //    }

    //}

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
            this.biomes[i].startCoreIds = this.allCorePoints.Count;
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
        int alphaMapWidth = xTextureRes;
        int alphaMapLength = yTextureRes;
        for (int i = 0; i < this.biomes.Count; i++)
        {
            this.biomes[i].createRandomPoints(this.biomCount, alphaMapWidth, alphaMapLength);
        }

        this.biomes[2].addCorePoint(new BiomeCorePoint(0, 0, this.biomes[2].textureIndex));

        this.getUpdateAllCorePoints();

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

                if (x > isleDiff && x < alphaMapWidth - isleDiff && y > isleDiff && y < alphaMapLength - isleDiff)
                {

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
                }
                else
                {

                    secondMinValue = 2;
                    secondMinIndex = this.biomes[2].startCoreIds + this.biomes[2].corePoints.Count - 1;

                    minValue = 0;
                    minIndex = this.biomes[2].startCoreIds + this.biomes[2].corePoints.Count - 1;
                }


                // create new BiomePoint on the base of the corePoint
                newBiomePoint = new BiomePoint(x, y, allCorePoints[minIndex], minIndex);

                // added to the allPoint List of these run-class
                this.allPoints.Add(newBiomePoint);


                // added to the allPoint List of the biome
                this.alphaMap.aMap[x, y, allCorePoints[minIndex].textureIndex] = (float)1;
                int biomeId = this.getBiomeIndexByTextureIndex(allCorePoints[minIndex].textureIndex);
                this.biomeMap[x, y] = biomeId;
                this.biomes[biomeId].allPoints.Add(newBiomePoint);


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
        int allMountCount = 0;
        resetPoints();

        this.heightMap = tData.GetHeights(0, 0, xTerrainRes, yTerrainRes);

        for (int i = 0; i < this.biomes.Count; i++)
        {
            if (this.biomes[i].borderPoints.Count > 0)
            {
                biomeSize = (int)(10000 * ((float)this.biomes[i].allPoints.Count / (float)this.allPoints.Count));
                mountPercentage = this.biomes[i].mountainsDegree;
                //print(biomeSize);
                mountCount = (int)(biomeSize * mountPercentage);

                //print(this.biomes[i].name + ": " + this.biomes[i].allPoints.Count + " / " + this.allPoints.Count + " = " + biomeSize + " | " + mountCount);

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
                                //this.heightMap[x, y] = this.heightMap[x, y] + calcHeight(diff, maxMountainWidth, maxHeight) - calcHeight(maxMountainWidth, maxMountainWidth, maxHeight);
                                this.heightMap[x, y] =
                                    this.heightMap[x, y] +
                                    ((float)maxHeight *
                                     (float)Math.Cos((Math.PI * (float)diff) /
                                                      (2 * (float)maxMountainWidth))); // +maxHeight/2;
                            }

                        }
                    }
                    allMountCount++;
                }
            }
        }

        for (int i = 0; i < this.isleDiff; i++)
        {
            for (int j = 0; j < yTerrainRes; j++)
            {

                // complete left side
                newPoint = new Point(i, j);
                diff = newPoint.getPyramideDiff(new Point(isleDiff, j));
                this.heightMap[i, j] += this.normalHeightCalc(diff, isleDiff);

                // complete right side
                newPoint = new Point(xTerrainRes - isleDiff + i, j);
                diff = newPoint.getPyramideDiff(new Point(xTerrainRes - isleDiff, j));
                this.heightMap[xTerrainRes - isleDiff + i, j] += this.normalHeightCalc(diff, isleDiff);
            }

            for (int j = 0; j < xTerrainRes; j++)
            {
                // complete top side
                newPoint = new Point(j, i);
                diff = newPoint.getPyramideDiff(new Point(j, isleDiff));
                this.heightMap[j, i] += this.normalHeightCalc(diff, isleDiff);

                // complete bottom side
                newPoint = new Point(j, i + yTerrainRes - isleDiff);
                diff = newPoint.getPyramideDiff(new Point(j, yTerrainRes - isleDiff));
                this.heightMap[j, i + yTerrainRes - isleDiff] += this.normalHeightCalc(diff, isleDiff);
            }
        }

        print("Mountains: " + allMountCount);
        this.setHeightMapWithStandardHeights();
        tData.SetAlphamaps(0, 0, this.alphaMap.aMap);

    }

    float normalHeightCalc(float x, float range)
    {
        float maxHeight = standardHeight / 2;
        float zeroPointWidth = (1 / (range * 2));
        float cosValue = (float)Math.Cos(zeroPointWidth * Math.PI * x);
        return maxHeight * (cosValue + 1) - standardHeight;
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
        float[,,] map = new float[this.xTextureRes, this.yTextureRes, 6];
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
