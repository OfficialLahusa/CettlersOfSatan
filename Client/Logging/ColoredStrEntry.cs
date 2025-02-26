using ImGuiNET;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class ColoredStrEntry : ILogEntry
    {
        private string _text;
        private Color _color;

        public ColoredStrEntry(string text, Color color)
        {
            _text = text;
            _color = color;
        }

        public void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.ColorToVec4(_color));
            ImGui.TextWrapped(_text);
            ImGui.PopStyleColor();
        }

        public string GetText()
        {
            return _text;
        }
    }
}
