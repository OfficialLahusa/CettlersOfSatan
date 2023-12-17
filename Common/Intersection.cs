using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Intersection
    {
        public SortedList<Direction.Edge, Tile> neighbors;

        public Intersection()
        {
            neighbors = new SortedList<Direction.Edge, Tile>();
        }

        /*
         * TODO:
         * - Intersections iterativ für alle Knoten erzeugen
         * - Jeweils vorher prüfen, ob in der jeweiligen Richtung schon eine Intersection existiert
         * - Tiles registrieren sich bei Intersections, niemals andersherum
         * - SortedList für benachbarte Edges und Intersections
         * - Klasse Edge
         * - Tiles, Intersections und Edges Felder für Referenzen auf Häfen geben
         * - Alle Anlieger teilen sich eine globale Instanz jedes Hafens, damit die Privilegienauswertung nachher zentral funktioniert
         */
    }
}
