using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Common;
using ImGuiNET;
using SFML.Graphics;

namespace Client.Logging
{
    public class EventLog
    {
        private List<ILogEntry> _lines;
        private bool _scrollToBottom = false;

        public EventLog()
        {
            _lines = new List<ILogEntry>();
        }

        public void Draw()
        {
            ImGui.BeginChild("ScrollLog", new Vector2(250, 350), true, ImGuiWindowFlags.NavFlattened);

            foreach (ILogEntry line in _lines)
            {
                line.Draw();
            }

            if (_scrollToBottom)
            {
                ImGui.SetScrollHereY();

                _scrollToBottom = false;
            }

            ImGui.EndChild();
        }

        public void WriteLine(params ILogEntry[] entries)
        {
            if (entries.Length == 1)
            {
                _lines.Add(entries[0]);
            }
            else if (entries.Length > 1)
            {
                _lines.Add(new ListEntry(entries));
            }

            _scrollToBottom = true;
        }

        public void Clear()
        {
            _lines.Clear();
        }
    }
}
