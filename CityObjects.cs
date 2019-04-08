
using UnityEngine;

namespace Assets
{
    public class CityObjects
    {
        public GameObject[] stages;
        public GameObject[] groundFloors;
        public GameObject[] rooftops;

        public CityObjects(GameObject[] stages, GameObject[] grounFloors, GameObject[] rooftops)
        {
            this.stages = stages;
            this.groundFloors = groundFloors;
            this.rooftops = rooftops;
        }
    }
}