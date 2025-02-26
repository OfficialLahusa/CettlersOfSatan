using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Common;
using ImGuiNET;
using SFML.Graphics;

namespace Client
{
    public class EventLog
    {
        private List<LogEntry> _lines;
        private bool _scrollToBottom = false;

        public EventLog()
        {
            _lines = new List<LogEntry>();
        }

        public void Draw()
        {
            ImGui.BeginChild("ScrollLog", new System.Numerics.Vector2(250, 350), true, ImGuiWindowFlags.NavFlattened);

            foreach (LogEntry line in _lines)
            {
                line.Draw();
            }

            if(_scrollToBottom)
            {
                ImGui.SetScrollHereY();

                _scrollToBottom = false;
            }

            ImGui.EndChild();
        }

        public void WriteLine(params LogEntry[] entries)
        {
            if(entries.Length == 1)
            {
                _lines.Add(entries[0]);
            }
            else if(entries.Length > 1)
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

    public interface LogEntry
    {
        public string GetText();
        public void Draw();
    }

    public class ListEntry : LogEntry
    {
        public List<LogEntry> Entries { get; set; }

        public ListEntry(List<LogEntry> entries)
        {
            Entries = entries;
        }

        public ListEntry(params LogEntry[] entries) : this(entries.ToList()) { }

        public string GetText()
        {
            StringBuilder textBuilder = new StringBuilder();

            for(int i = 0; i < Entries.Count; i++)
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

    public class StrEntry : LogEntry
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

    public class ColoredStrEntry : LogEntry
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

    public class PlayerEntry : LogEntry
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

    public class ResourceCardEntry : LogEntry
    {
        private ResourceCardType _cardType;

        public ResourceCardEntry(ResourceCardType cardType)
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

    public class DevelopmentCardEntry : LogEntry
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

    public class SeparatorEntry : LogEntry
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

    public class RoundEntry : LogEntry
    {
        private SeparatorEntry _separator;
        private StrEntry _text;

        public RoundEntry(int roundIdx)
        {
            _separator = new SeparatorEntry();
            _text = new StrEntry($"Round {roundIdx}");
        }

        public void Draw()
        {
            _separator.Draw();
            _text.Draw();
            _separator.Draw();
        }

        public string GetText()
        {
            return _text.GetText();
        }
    }
}
