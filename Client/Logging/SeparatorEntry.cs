using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class SeparatorEntry : ILogEntry
    {
        public SeparatorEntry()
        {

        }

        public void Draw()
        {
            ImGui.Separator();
        }

        public string GetText()
        {
            return string.Empty;
        }
    }
}
