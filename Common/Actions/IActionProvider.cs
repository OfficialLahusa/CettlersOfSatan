using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public interface IActionProvider
    {
        static abstract List<Action> GetActionsForState(GameState state);
    }
}
