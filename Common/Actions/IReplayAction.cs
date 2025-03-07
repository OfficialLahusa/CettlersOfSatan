using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    /// <summary>
    /// Action that stores transaction history when applied, which is utilized for transactional undo/redo
    /// </summary>
    public interface IReplayAction
    {
        public bool HasHistory();
        public void ClearHistory();
    }
}
