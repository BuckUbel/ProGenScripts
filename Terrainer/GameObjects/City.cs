



//  beinhaltet:
//  - List of <OWO>
//  - biomeCorePointId
//  - OWO's können verschiedene Tiles beinhalten: --> zufall und einstellbar durch koeffizienten umliegende Gebäude, etc.
//              - HouseTile
//              - Empty
//              - spezifischere HouseTiles

using System.Collections.Generic;
using Assets.Scripts;

public class City
{
    public List<CityPlaceholderObject> cityObjects = new List<CityPlaceholderObject>();
    public BiomeCorePoint biomeCorePoint;

    public City(BiomeCorePoint bcp)
    {
        this.biomeCorePoint = bcp;
    }

    public void AddTile(CityTile ct)
    {
        CityPlaceholderObject cpo = new CityPlaceholderObject(ct);
        cityObjects.Add(cpo);
    }
}
