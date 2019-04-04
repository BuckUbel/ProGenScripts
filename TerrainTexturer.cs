using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.Scripts;

// used for Sum of array

public class TerrainTexturer : MonoBehaviour
{
    public int width;

    public Terrain terrain;
    private TerrainData terrainData;

    private int xRes;
    private int yRes;

    void Start()
    {
        this.terrain = transform.GetComponent<Terrain>();
        this.terrainData = terrain.terrainData;
        this.xRes = terrainData.alphamapWidth;
        this.yRes = terrainData.alphamapHeight;
    }

    void OnGUI()
    {

        if (GUI.Button(new Rect(125, 10, 100, 25), "Color"))
        {
            createAlphaMaps();
        }
        if (GUI.Button(new Rect(125, 40, 100, 25), "Reset"))
        {
            resetAlphaMaps();
        }

    }

    void createAlphaMaps()
    {
        float[,,] map = new float[this.xRes, this.yRes, 4];

        int completePixelCount = this.xRes * this.yRes;

        Point cityPoint = new Point();
        cityPoint.createRandom(0, this.xRes, 0, this.yRes);

        Point forestPoint = new Point();
        forestPoint.createRandom(0, this.xRes, 0, this.yRes);

        Point waterPoint = new Point();
        waterPoint.createRandom(0, this.xRes, 0, this.yRes);

        Point tempPoint = new Point();
        int[] diffValues = new int[3];
        int minValue;
        int minIndex;

        // For each point on the alphamap...
        for (int y = 0; y < this.yRes; y++)
        {
            for (int x = 0; x < this.xRes; x++)
            {
                tempPoint.set(x, y);
                diffValues[0] = cityPoint.getDiff(tempPoint);
                diffValues[1] = forestPoint.getDiff(tempPoint);
                diffValues[2] = waterPoint.getDiff(tempPoint);

                minValue = diffValues.Min();
                minIndex = diffValues.ToList().IndexOf(minValue);

                if (minValue > this.width)
                {
                    map[x, y, diffValues.Length] = (float)1;
                }
                else
                {
                    map[x, y, minIndex] = (float)1;
                }
                if (minValue == 0)
                {
                    map[x, y, minIndex] = (float)0;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, map);
    }

    void resetAlphaMaps()
    {
        float[,,] map = new float[this.xRes, this.yRes, 4];
        terrainData.SetAlphamaps(0, 0, map);
    }


}