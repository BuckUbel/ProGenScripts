
using Assets.Scripts;
using UnityEngine;

namespace Assets
{
    public class CityObjects
    {

        public GameObject[] stages;
        public GameObject[] groundFloors;
        public GameObject[] rooftops;

        public CityObjects(GameObject[] stages, GameObject[] groundFloors, GameObject[] rooftops)
        {
            this.stages = stages;
            this.groundFloors = groundFloors;
            this.rooftops = rooftops;
        }

        public HouseTile CreateHouse(Point startPoint, int stageCount, float height)
        {
            return new HouseTile(startPoint, this.GetHouseObjects(stageCount), height);
        }

        public GameObject[] GetHouseObjects(int stageCount)
        {
            if (stageCount < 2)
            {
                return null;
            }
            else
            {
                GameObject[] stageObjects = new GameObject[stageCount];
                int rndFloor = Terrainer.rnd.Next(groundFloors.Length);
                stageObjects[0] = this.groundFloors[rndFloor];
                int rndRoofTops = Terrainer.rnd.Next(rooftops.Length);
                stageObjects[stageCount-1] = this.rooftops[rndRoofTops];

                for (int i = 1; i < stageCount - 1; i++)
                {
                    int rndStages = Terrainer.rnd.Next(stages.Length);
                    stageObjects[i] = this.stages[rndStages];
                }
                
                return stageObjects;
            }
        }
    }
}