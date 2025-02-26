using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public class RoundEntry : ILogEntry
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
