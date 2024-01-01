using SFML.System;

namespace Client
{
    public interface Screen
    {
        public void Update(Time deltaTime);
        public void HandleInput(Time deltaTime);
        public void Draw(Time deltaTime);

    }
}
