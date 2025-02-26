using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Common.Actions.Action;

namespace Client.Logging
{
    public class ActionLogger
    {
        private EventLog _log;
        private int _prevRound;
        private int _prevPlayer;

        public ActionLogger(EventLog log)
        {
            _log = log;
        }

        public void Init()
        {
            _log.WriteLine(new RoundEntry(0));
            _prevRound = 0;
            _prevPlayer = 0;
        }

        public void Log(Action action, GameState state)
        {
            // Action entry
            switch(action)
            {
                default:
                    _log.WriteLine(new ColoredStrEntry(action.ToString(), ColorPalette.GetPlayerColor(action.PlayerIndex)));
                    break;
            }

            // Round divider & header
            if (state.Turn.RoundCounter > _prevRound)
            {
                _log.WriteLine(new RoundEntry(state.Turn.RoundCounter));
            }
            // Player turn divider
            else if (state.Turn.PlayerIndex != _prevPlayer)
            {
                _log.WriteLine(new SeparatorEntry());
            }

            // Game conclusion
            if (state.HasEnded)
            {
                _log.WriteLine(new SeparatorEntry());
                _log.WriteLine(new ColoredStrEntry($"Player {state.Turn.PlayerIndex} won", ColorPalette.GetPlayerColor(state.Turn.PlayerIndex)));
            }

            _prevRound = state.Turn.RoundCounter;
            _prevPlayer = state.Turn.PlayerIndex;
        } 
    }
}
