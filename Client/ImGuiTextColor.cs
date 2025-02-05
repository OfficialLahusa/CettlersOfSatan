using ImGuiNET;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ImGuiTextColor : IDisposable
    {
        private bool _active;

        public ImGuiTextColor(Color textColor, bool active = true)
        {
            _active = active;

            if(active)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.ColorToVec4(textColor));
            }
        }

        public void Dispose()
        {
            if(_active)
            {
                ImGui.PopStyleColor();
            }
        }
    }
}
