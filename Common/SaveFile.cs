using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    using Action = Actions.Action;

    public class SaveFile
    {
        public GameState GameState { get; set; }
        public List<Action> PlayedActions { get; set; }
        public List<Action> UndoHistory {  get; set; }

        public SaveFile(GameState gameState, List<Action> playedActions, List<Action> undoHistory)
        {
            GameState = gameState;
            PlayedActions = playedActions;
            UndoHistory = undoHistory;
        }

        /// <summary>
        /// Parameterless constructor for deserialization
        /// </summary>
        private SaveFile()
        {
            GameState = null!;
            PlayedActions = null!;
            UndoHistory = null!;
        }
    }
}
