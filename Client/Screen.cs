using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public interface Screen
    {
        public void Update(Time deltaTime);
        public void HandleInput(Time deltaTime);
        public void Draw(Time deltaTime);

    }
}
