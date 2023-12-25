using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Intersection
    {
        // Key: Corner direction this intersection is on at the given tile
        public SortedList<Direction.Corner, Tile> AdjacentTiles;
        public readonly bool FacesDownwards;
        public enum BuildingType
        {
            None = 0,
            Settlement = 1,
            City = 2
        }
        public BuildingType Building { get; set; }
        // -1 => Nobody, 0/1/2 => Player 1/2/3
        public int Owner { get; set; }

        public Intersection(bool facesDownwards)
        {
            AdjacentTiles = new SortedList<Direction.Corner, Tile>();
            FacesDownwards = facesDownwards;
            Building = BuildingType.None;
            Owner = -1;
        }

        /*
         * TODO:
         * - SortedList für benachbarte Edges und Intersections
         * - Klasse Edge
         * - Zusätzlicher Pass für Intersections und Edges der Wasser-Tiles
         * - Tiles, Intersections und Edges Felder für Referenzen auf Häfen geben
         * - Alle Anlieger teilen sich eine globale Instanz jedes Hafens, damit die Privilegienauswertung nachher zentral funktioniert
         */
    }
}
