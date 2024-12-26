using Common;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static SFML.Graphics.Text;

namespace Client
{
    public class CardWidget : Drawable
    {
        private RenderWindow _window;
        private CardSet _cardSet;

        private Card _cardPrimitive;

        private Text _tooltip;

        private FloatRect _cardStackHitbox;
        private float[] _cardStackHover;

        private const float CARD_SHIFT = 20f;

        public CardWidget(RenderWindow window, CardSet cardSet)
        {
            _window = window;
            _cardSet = cardSet;

            _cardPrimitive = new Card();

            _tooltip = new Text("Test", GameScreen.Font, 24);
            _tooltip.OutlineColor = new Color(64, 64, 64, 255);
            _tooltip.OutlineThickness = 1;

            _cardStackHitbox = new FloatRect(-window.Size.X / 2 + 25, window.Size.Y / 2 - 50 - Card.Size.Y, Card.Size.X, Card.Size.Y + 25);

            _cardStackHover = new float[Enum.GetNames(typeof(CardSet.CardType)).Length];
        }

        public void Update(float deltaTime, Vector2f mousePos)
        {
            float offsetX = 0f;

            foreach (CardSet.CardType cardType in Enum.GetValues(typeof(CardSet.CardType)))
            {
                uint countOfType = _cardSet.Get(cardType);

                if (countOfType == 0)
                {
                    continue;
                }
                else
                {
                    _cardStackHitbox.Left = -_window.Size.X / 2 + 25 + offsetX;
                    _cardStackHitbox.Width = Card.Size.X + (countOfType - 1) * CARD_SHIFT;

                    bool hovering = _cardStackHitbox.Contains(mousePos.X, mousePos.Y);

                    /*for (uint i = 0; i < countOfType; i++)
                    {
                        _cardPrimitive.Position = new Vector2f(-_window.Size.X / 2 + 25 + offsetX, _window.Size.Y / 2 - 25 - Card.Size.Y);

                        if (_cardPrimitive.Contains(mousePos))
                        {
                            hovering = true;
                            break;
                        }

                        offsetX += CARD_SHIFT;
                    }*/

                    // Smoothly hover/unhover
                    _cardStackHover[(int)cardType] += (hovering ? 5f : -5f) * deltaTime;
                    _cardStackHover[(int)cardType] = MathF.Max(MathF.Min(_cardStackHover[(int)cardType], 1f), 0f);

                    offsetX += Card.Size.X + countOfType * CARD_SHIFT;
                }
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            float offsetX = 0f;

            foreach(CardSet.CardType cardType in Enum.GetValues(typeof(CardSet.CardType)))
            {
                uint countOfType = _cardSet.Get(cardType);

                if (countOfType == 0)
                {
                    continue;
                }
                else
                {
                    float hoverAmount = Easing.ExpOut(_cardStackHover[(int)cardType]);
                    float hoverY = hoverAmount * 25f;

                    _cardPrimitive.SetType(cardType);

                    if(hoverAmount > 0.01f)
                    {
                        _tooltip.DisplayedString = CardSet.GetName(cardType);

                        float tooltipWidth = _tooltip.GetGlobalBounds().Width;
                        float cardStackWidth = Card.Size.X + CARD_SHIFT * (countOfType - 1);
                        float tooltipCenteringOffset = (cardStackWidth - tooltipWidth) / 2;

                        _tooltip.Position = new Vector2f(
                            -_window.Size.X / 2 + MathF.Max(25 + offsetX + tooltipCenteringOffset, 0),
                            _window.Size.Y / 2 - 25 - 1.325f * Card.Size.Y - hoverY
                        );

                        byte tooltipAlpha = (byte)(hoverAmount * 255);
                        _tooltip.FillColor = new Color(255, 255, 255, tooltipAlpha);

                        target.Draw(_tooltip, states);
                    }
                    

                    for (uint i = 0; i < countOfType; i++)
                    {
                        _cardPrimitive.Position = new Vector2f(
                            -_window.Size.X / 2 + 25 + offsetX,
                            _window.Size.Y / 2 - 25 - Card.Size.Y - hoverY
                        );

                        target.Draw(_cardPrimitive, states);

                        offsetX += CARD_SHIFT;
                    }

                    offsetX += Card.Size.X;
                }
            }
        }

        public void SetCardSet(CardSet cardSet)
        {
            _cardSet = cardSet;
            Array.Clear(_cardStackHover);
        }
    }
}
