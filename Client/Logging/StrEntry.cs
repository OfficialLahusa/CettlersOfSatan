using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class StrEntry : ILogEntry
    {
        private string _text;

        public StrEntry(string text)
        {
            _text = text;
        }

        public void Draw()
        {
            ImGui.TextWrapped(_text);
        }

        public string GetText()
        {
            return _text;
        }
    }
}
