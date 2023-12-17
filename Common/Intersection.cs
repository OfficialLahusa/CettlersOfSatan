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

        public Intersection(bool facesDownwards)
        {
            AdjacentTiles = new SortedList<Direction.Corner, Tile>();
            FacesDownwards = facesDownwards;
        }

        /*
         * TODO:
         * - Intersections iterativ für alle Knoten erzeugen
         * - Jeweils vorher prüfen, ob in der jeweiligen Richtung schon eine Intersection existiert
         * - Tiles registrieren sich bei Intersections, niemals andersherum
         * - SortedList für benachbarte Edges und Intersections
         * - Klasse Edge
         * - Zusätzlicher Pass für Intersections und Edges der Wasser-Tiles
         * - Tiles, Intersections und Edges Felder für Referenzen auf Häfen geben
         * - Alle Anlieger teilen sich eine globale Instanz jedes Hafens, damit die Privilegienauswertung nachher zentral funktioniert
         */
    }
}
