using Common;
using Common.Actions;
using SFML.Graphics;
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
            Color playerColor = ColorPalette.GetPlayerColor(action.PlayerIndex);

            // Action entry
            switch (action)
            {
                case EndTurnAction endTurnAction:
                    _log.WriteLine(new ColoredStrEntry("Ended turn", playerColor));
                    break;

                case RollAction rollAction:
                    _log.WriteLine(new ColoredStrEntry($"Rolled {rollAction.RollResult.Total} ({rollAction.RollResult.First}+{rollAction.RollResult.Second})", playerColor));
                    // TODO: Yield summary
                    break;

                case DiscardAction discardAction:
                    StringBuilder sb = new StringBuilder();
                    foreach (ResourceCardType resourceType in CardSet<ResourceCardType>.Values)
                    {
                        for (int i = 0; i < discardAction.SelectedCards.Get(resourceType); i++)
                        {
                            sb.Append(resourceType.GetAbbreviation());
                        }
                    }
                    _log.WriteLine(new ColoredStrEntry($"Discarded {discardAction.SelectedCards.Count()} ({sb})", playerColor));
                    break;

                case RobberAction robberAction:
                    _log.WriteLine(new ColoredStrEntry("Moved robber", playerColor));
                    // TODO: Stolen card and victim
                    break;

                case FirstInitialSettlementAction firstInitialSettlementAction:
                    _log.WriteLine(new ColoredStrEntry("Placed 1st initial settlement", playerColor));
                    break;

                case FirstInitialRoadAction firstInitialRoadAction:
                    _log.WriteLine(new ColoredStrEntry("Placed 1st initial road", playerColor));
                    break;

                case SecondInitialSettlementAction secondInitialSettlementAction:
                    _log.WriteLine(new ColoredStrEntry("Placed 2nd initial settlement", playerColor));
                    // TODO: Initial cards
                    break;

                case SecondInitialRoadAction secondInitialRoadAction:
                    _log.WriteLine(new ColoredStrEntry("Placed 2nd initial road", playerColor));
                    break;

                // TODO: Remaining Actions in order used by LegalActionProvider
                // TODO: Changes in Longest Road/Largest Army Leader

                // Fallback: Basic string conversion of action in player color
                default:
                    _log.WriteLine(new ColoredStrEntry(action.ToString(), playerColor));
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
