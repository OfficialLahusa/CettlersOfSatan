using Common;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static Common.CardSet;

namespace Client
{
    public class Card : Transformable, Drawable
    {
        private CardType _type;

        private RectangleShape _cardBorder;
        private RectangleShape _cardFill;
        private RectangleShape _icon;

        public static readonly Vector2f Size = new Vector2f(80, 120f);

        private const float BORDER_WIDTH = 6f;

        public Card(CardType type = CardType.Unknown)
        {
            _cardBorder = new RectangleShape(Size);

            _cardFill = new RectangleShape(new Vector2f(Size.X - 2 * BORDER_WIDTH, Size.Y - 2 * BORDER_WIDTH));
            _cardFill.Origin = _cardFill.Size / 2;
            _cardFill.Position = _cardBorder.Size / 2;

            _icon = new RectangleShape(new Vector2f(60f, 60f));
            _icon.Origin = _icon.Size / 2;
            _icon.Position = _cardBorder.Size / 2;
            _icon.Texture = TextureAtlas.Texture;

            SetType(type);
        }

        public void SetType(CardType type)
        {
            _type = type;

            _cardBorder.FillColor = ColorPalette.GetCardIconColor(type);

            _cardFill.FillColor = ColorPalette.GetCardColor(type);

            _icon.TextureRect = TextureAtlas.GetSprite(type).GetTextureRect();
            _icon.FillColor = ColorPalette.GetCardIconColor(type);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform.Combine(Transform);

            target.Draw(_cardBorder, states);
            target.Draw(_cardFill, states);
            target.Draw(_icon, states);

            states.Transform.Combine(InverseTransform);
        }

        public bool Contains(Vector2f pos)
        {
            Vector2f transformedPos = InverseTransform.TransformPoint(pos);
            return _cardBorder.GetLocalBounds().Contains(transformedPos.X, transformedPos.Y);
        }
    }
}
