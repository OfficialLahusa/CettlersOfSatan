using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Logging
{
    public interface ILogEntry
    {
        public string GetText();
        public void Draw();
    }
}
