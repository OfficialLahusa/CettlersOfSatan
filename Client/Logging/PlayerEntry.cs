using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class PlayerEntry : ILogEntry
    {
        private int _playerIdx;

        public PlayerEntry(int playerIdx)
        {
            _playerIdx = playerIdx;
        }

        public void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.ColorToVec4(ColorPalette.GetPlayerColor(_playerIdx)));
            ImGui.TextWrapped(GetText());
            ImGui.PopStyleColor();
        }

        public string GetText()
        {
            return $"Player {_playerIdx}";
        }
    }
}
