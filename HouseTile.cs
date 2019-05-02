using UnityEngine;

namespace Assets.Scripts
{
    public class HouseTile
    {
        // Länge : Breite == 2 : 1
        public static int preHouseLength = 6; // Türseite
        public static int postHouseLength = 8; //Gartenseite
        public static int houseWidth = 28; // 28 - 8 - 6 = 14 Breite

        public static int preHouseWidth = 0; // links der TÜr
        public static int postHouseWidth = 0; // rechts der Tür
        public static int houseLength = 28; // 28 - 0 - 0 = 28 Länge

        // Breite zu Höhe == 3.3 : 1
        public static float stageHeight = 3.3f;

        public static int houseScaleFactor = 50;

        // left top point
        public Point startPoint;

        // middle point
        public Point renderPoint;

        public GameObject[] houseObjects;
        public float height;

        public HouseTile() { }

        public HouseTile(Point startPoint)
        {
            this.setPoints(startPoint);
        }

        public HouseTile(Point startPoint, GameObject[] houseObjects, float height)
        {
            this.setPoints(startPoint);
            this.houseObjects = houseObjects;
            this.height = height;
        }

        private void setPoints(Point startPoint)
        {
            this.startPoint = startPoint;
            this.renderPoint = new Point(startPoint.x + houseWidth / 2 + preHouseWidth - postHouseWidth,
                startPoint.y + houseLength / 2 + preHouseLength - postHouseLength);

        }

        private void Render()
        {

        }
    }
}