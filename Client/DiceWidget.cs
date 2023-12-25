using Common;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class DiceWidget : Drawable
    {
        private RectangleShape _firstDie;
        private RectangleShape _secondDie;
        private Sound _diceRollSound;
        private static readonly int _size = 180;
        private static readonly Color _inactiveColor = new Color(255, 255, 255, 85);
        private int _first;
        private int _second;
        private bool _active;
        public int First
        {
            get { return _first; }
            set
            {
                if(value >= 1 && value <= 6)
                {
                    _first = value;
                }
            }
        }
        public int Second
        {
            get { return _second; }
            set
            {
                if (value >= 1 && value <= 6)
                {
                    _second = value;
                }
            }
        }

        public bool Active { 
            get { return _active; }
            set
            {
                _active = value;
                _firstDie.FillColor = value ? Color.White : _inactiveColor;
                _secondDie.FillColor = _firstDie.FillColor;
            }
        }

        public DiceWidget(RenderWindow window)
        {
            _firstDie = new RectangleShape(new SFML.System.Vector2f(_size, _size));
            _firstDie.Texture = TextureAtlas.Texture;

            _secondDie = new RectangleShape(new SFML.System.Vector2f(_size, _size));
            _secondDie.Texture = TextureAtlas.Texture;

            _diceRollSound = new Sound(Sounds.DiceRolling);

            Active = false;

            SetValues();
            UpdateSprites();
            UpdatePosition(window);

            window.Resized += Window_Resized;
        }

        private void Window_Resized(object? sender, SFML.Window.SizeEventArgs e)
        {
            if(sender != null)
            {
                UpdatePosition((RenderWindow)sender);
            }
        }

        public void Roll()
        {
            SetValues();
            UpdateSprites();
            _diceRollSound.Play();
        }

        private void SetValues(int? first = null, int? second = null)
        {
            if (first == null)  First  = Utils.Random.Next(6) + 1;
            if (second == null) Second = Utils.Random.Next(6) + 1;
        }

        private void UpdatePosition(RenderWindow window)
        {
            // Position at the bottom right corner of view coordinate system (center = 0, 0)
            _firstDie.Position = new Vector2f(window.Size.X / 2 - 2*_size - 25, window.Size.Y / 2 - _size - 25);
            _secondDie.Position = new Vector2f(window.Size.X / 2 - _size - 25, window.Size.Y / 2 - _size - 25);
        }

        private void UpdateSprites()
        {
            _firstDie.TextureRect = TextureAtlas.GetTextureRect(TextureAtlas.Sprite.DiceOne + (_first - 1));
            _secondDie.TextureRect = TextureAtlas.GetTextureRect(TextureAtlas.Sprite.DiceOne + (_second - 1));
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_firstDie, states);
            target.Draw(_secondDie, states);
        }

        public bool Contains(float x, float y)
        {
            return _firstDie.GetGlobalBounds().Contains(x, y) || _secondDie.GetGlobalBounds().Contains(x, y);
        }
    }
}
