using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PlayerState
    {
        public CardSet CardSet;
        public BuildingStock BuildingStock;
        public PortPrivileges PortPrivileges;

        public PlayerState()
        {
            CardSet = new CardSet();
            BuildingStock = new BuildingStock();
            PortPrivileges = PortPrivileges.None;
        }
    }
}
