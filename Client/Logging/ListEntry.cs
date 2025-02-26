using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class ListEntry : ILogEntry
    {
        public List<ILogEntry> Entries { get; set; }

        public ListEntry(List<ILogEntry> entries)
        {
            Entries = entries;
        }

        public ListEntry(params ILogEntry[] entries) : this(entries.ToList()) { }

        public string GetText()
        {
            StringBuilder textBuilder = new StringBuilder();

            for (int i = 0; i < Entries.Count; i++)
            {
                textBuilder.Append(Entries[i].GetText());
                if (i < Entries.Count - 1) textBuilder.Append(" ");
            }

            return textBuilder.ToString();
        }

        public void Draw()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (i > 0) ImGui.SameLine();
                Entries[i].Draw();

            }
        }
    }
}
