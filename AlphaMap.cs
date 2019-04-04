using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using System.Linq;
using UnityEngine;

public class AlphaMap
{

    public float[,,] aMap;

	public AlphaMap() { }

    public AlphaMap(float[,,] map)
    {
        this.aMap = map;
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
                int a = (int) this.aMap[p.x, p.y, i];
            lastDimArray[i] = a;
        }

        int maxValue = lastDimArray.Max();
        return lastDimArray.ToList().IndexOf(maxValue);
    }

}
