using UnityEngine;

namespace Assets.Scripts
{
    public class HouseTile
    {
        public static int streetSize = 4;
        // Länge : Breite == 2 : 1
        public static int preHouseLength = 7; // Türseite
        public static int postHouseLength = 9; //Gartenseite
        public static int houseWidth = 38; // 38 - 8 - 6 - (2 x 4) = 14 Breite

        public static int preHouseWidth = 1; // links der TÜr
        public static int postHouseWidth = 1; // rechts der Tür
        public static int houseLength = 38; // 38 - 1 - 1 - (2 x 4) = 28 Länge

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
            // street size is not immportant here, because, it is on each side equal
            this.renderPoint = new Point(
                startPoint.x + houseWidth / 2 + preHouseWidth - postHouseWidth,
                startPoint.y + houseLength / 2 + preHouseLength - postHouseLength
                );
        }

        private void Render()
        {

        }
    }
}