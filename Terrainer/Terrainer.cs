using System;
using System.Collections.Generic;
using Assets.Scripts;
using System.Linq;
using Assets;
//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Terrainer : MonoBehaviour
{
    public static System.Random rnd;

    [Header("Groe-Aufteilung")]
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

    // TerrainData is prop of Terrain
    // the imported TerrainData
    [Header("Grundlegende TerrainData")]
    public TerrainData BaseTerrainData;


    [Header("Bilder")]
    public Texture2D keyImageSprite;
    public Texture2D plusImageSprite;

    [Header("Seed")]
    public int currentSeed = 0;

    public GameObject sceneLoaderObject;
    private ProgressSceneLoader sceneLoader;

    private TerrainerContent tc;

    private int startSequenceNumber = 0;

    public Boolean startScript = false;

    public GameObject WinTextGameObject;

    public void activate()
    {
        startScript = true;
    }
    void StartSequence()
    {
        if (startSequenceNumber < 18)
        {
            switch (startSequenceNumber)
            {
                case 0:
                    this.sceneLoader = sceneLoaderObject.GetComponent<ProgressSceneLoader>();
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.START_LOADING);
                    tc = new TerrainerContent(this);
                    break;
                case 2:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_RND);
                    this.currentSeed = tc.randomGenerator(currentSeed);
                    break;
                case 4:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_TERRAIN);
                    tc.createTerrainObject();
                    break;
                case 6:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_BIOMES);
                    tc.createBiomesAndTextures();
                    tc.resetMaps();
                    tc.alphaMap.createAlphaMaps(tc);
                    break;
                case 8:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_BORDERS);
                    tc.alphaMap.calcBorders(tc);
                    break;
                case 10:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_MOUNTAINS);
                    tc.MountMap.createMountainsByBorders();
                    break;
                case 12:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_GAMEOBJECTS);
                    tc.DetailsMap.calculateDetailByBiomes();
                    tc.TreeMap.calculateTreesByBiomes();
                    tc.createPrefabObjects();
                    tc.createGameObjects();
                    break;
                case 14:
                    this.sceneLoader.UpdateBar(ProgressSceneLoader.LOAD_PLAYER);
                    tc.createPlayer();
                    this.sceneLoader.camera.SetActive(false);
                    // the collider have to reinitialized
                    tc.BaseTerrainCollider.enabled = false;

                    // TODO day-night changing
                    //var lightParent = new GameObject();
                    //var light = new GameObject();
                    //lightParent.transform.position = new Vector3(0, 0, 0);
                    //light.transform.position = new Vector3(this.tc.xTerrainRes, 0, this.tc.yTerrainRes);
                    //light.transform.LookAt(lightParent.transform.position);

                    //var lightComponent = light.AddComponent<Light>();
                    //lightComponent.type = LightType.Spot;
                    //lightComponent.color = new Color(255,255,0);
                    //lightComponent.spotAngle = 130;
                    //lightComponent.range = 3000;
                    //lightComponent.intensity = 30;

                    //lightParent.transform.parent = this.transform;
                    //light.transform.parent = lightParent.transform;

                    break;
                case 16:
                    tc.BaseTerrainCollider.enabled = true;
                    // PrefabUtility.CreatePrefab("Assets/SaveData/ProGen/Terrains/" + currentSeed + ".prefab", tc.BaseTerrainObj);
                    // PrefabUtility.CreatePrefab("Assets/SaveData/ProGen/World/" + currentSeed + ".prefab", this.gameObject);
                    tc = null;
                    this.sceneLoader.UpdateBar("");
                    this.sceneLoader.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
            startSequenceNumber++;
        }
    }

    public GameObject CreateGameObject(GameObject original, Vector3 position)
    {
        Quaternion rotation = Quaternion.identity;
        GameObject buildedGameObject = Instantiate(original, position, rotation);
        buildedGameObject.transform.parent = this.transform;
        return buildedGameObject;
    }




    public static int KEY_COUNT_PER_CHEST = 3;

    public float playerRunSpeed = 10.0f;
    public float playerJumpHeight = 10.0f;
    public int playerKeyCount = 0;
    public int playerDiamondMaxCount = 0;
    public int playerDiamondCount = 0;


    void OnGUI()
    {
        if (startScript)
        {
            StartSequence();
            for (int i = 0; i < this.playerKeyCount; i++)
            {
                if (i < 4)
                {
                    GUI.DrawTexture(new Rect(10 + i * 50, 10, 50, 50), keyImageSprite);
                }
                else
                {
                    GUI.DrawTexture(new Rect(10 + i * 50, 10, 50, 50), plusImageSprite);
                    break;
                }
            }
            GUI.Label(new Rect(Screen.width - 200, 10, 200, 100),
                "Diamanten: " + this.playerDiamondCount + " / " + this.playerDiamondMaxCount);
            if (this.playerDiamondMaxCount > 0 && this.playerDiamondMaxCount <= this.playerDiamondCount)
            {
                if (!WinTextGameObject.activeSelf)
                {
                    WinTextGameObject.SetActive(true);
                }
            }
            else
            {
                if (WinTextGameObject.activeSelf)
                {
                    WinTextGameObject.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
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

            if (this.playerKeyCount >= KEY_COUNT_PER_CHEST && !cc.isUnlocked)
            {
                cc.Unlock();
                this.playerKeyCount -= KEY_COUNT_PER_CHEST;
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
}
