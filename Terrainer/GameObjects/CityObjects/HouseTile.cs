using Assets.Scripts;
using UnityEngine;

public class HouseTile : CityTile
{
    public static int streetSize = 4;
    // Länge : Breite == 2 : 1
    public static int preHouseLength = 7; // Türseite
    public static int postHouseLength = 9; //Gartenseite
    public static int houseWidth = 38; // 38 - 8 - 6 - (2 x 4) = 14 Breite

    public static int preHouseWidth = 1; // links der TÜr
    public static int postHouseWidth = 1; // rechts der Tür
    public static int houseLength = 38; // 38 - 1 - 1 - (2 x 4) = 28 Länge


    public static int houseScaleFactor = 50;

    public GameObject[] houseObjects;

    public HouseTile() { }

    public HouseTile(Point startPoint)
    {
        this.setPoints(startPoint);
    }

    public HouseTile(Point startPoint, GameObject[] houseObjects, int stageCount, float height)
    {
        this.setPoints(startPoint);
        this.houseObjects = houseObjects;
        this.height = height;
        this.stageCount = stageCount;

        // Breite zu Höhe == 3.3 : 1
        this.stageHeight = 3.75f;
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

    //public static bool testHouseTilePoint(BiomePoint tempPoint, Terrainer terrainer)
    //{
    //    bool isGridable = true;
    //    for (int i = 0; i < HouseTile.houseWidth; i++)
    //    {
    //        for (int j = 0; j < HouseTile.houseLength; j++)
    //        {
    //            if (tempPoint.x + i < terrainer.xTextureRes &&
    //                tempPoint.y + j < terrainer.yTextureRes)
    //            {
    //                if (terrainer.biomes[0].textureIndex !=
    //                    terrainer.alphaMap.getSettedIndex(new Point(tempPoint.x + i, tempPoint.y + j)) ||
    //                    terrainer.allPoints[tempPoint.indexInGlobalAllPoints + i + (j * terrainer.xTextureRes)].isBusy == true)
    //                {

    //                    isGridable = false;
    //                    i = HouseTile.houseWidth;
    //                    j = HouseTile.houseLength;
    //                }
    //            }
    //            if (tempPoint.x + i >= terrainer.xTextureRes ||
    //                tempPoint.y + j >= terrainer.yTextureRes)
    //            {
    //                isGridable = false;
    //                i = HouseTile.houseWidth;
    //                j = HouseTile.houseLength;
    //                tempPoint.isBusy = true;
    //            }

    //        }
    //    }
    //    return isGridable;
    //}
}
