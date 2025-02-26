using Common;
using ImGuiNET;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class DevelopmentCardEntry : ILogEntry
    {
        private DevelopmentCardType _cardType;

        public DevelopmentCardEntry(DevelopmentCardType cardType)
        {
            _cardType = cardType;
        }

        public void Draw()
        {
            Color col = ColorPalette.GetCardColor(_cardType);
            Vector4 col4 = ColorPalette.ColorToVec4(col);
            ImGui.PushStyleColor(ImGuiCol.Text, col4);
            ImGui.TextWrapped(GetText());
            ImGui.PopStyleColor();
        }

        public string GetText()
        {
            return _cardType.GetName();
        }
    }
}
