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
        private PlayerState _playerState;

        private Card _cardPrimitive;

        private Text _tooltip;

        private FloatRect _cardStackHitbox;
        private float[] _cardStackHover;

        private const float CARD_SHIFT = 20f;

        public CardWidget(RenderWindow window, PlayerState playerState)
        {
            _window = window;
            _playerState = playerState;

            _cardPrimitive = new Card();

            _tooltip = new Text("Test", GameScreen.Font, 24);
            _tooltip.OutlineColor = new Color(64, 64, 64, 255);
            _tooltip.OutlineThickness = 1;

            _cardStackHitbox = new FloatRect(-window.Size.X / 2 + 25, window.Size.Y / 2 - 50 - Card.Size.Y, Card.Size.X, Card.Size.Y + 25);

            _cardStackHover = new float[CardSet<ResourceCardType>.Values.Count + CardSet<DevelopmentCardType>.Values.Count];
        }

        public void Update(float deltaTime, Vector2f mousePos)
        {
            float offsetX = 0f;

            foreach (ResourceCardType cardType in CardSet<ResourceCardType>.Values)
            {
                uint countOfType = _playerState.ResourceCards.Get(cardType);

                if (countOfType == 0)
                {
                    continue;
                }
                else
                {
                    _cardStackHitbox.Left = -_window.Size.X / 2 + 25 + offsetX;
                    _cardStackHitbox.Width = Card.Size.X + (countOfType - 1) * CARD_SHIFT;

                    bool hovering = _cardStackHitbox.Contains(mousePos.X, mousePos.Y);

                    // Smoothly hover/unhover
                    _cardStackHover[(int)cardType] += (hovering ? 5f : -5f) * deltaTime;
                    _cardStackHover[(int)cardType] = MathF.Max(MathF.Min(_cardStackHover[(int)cardType], 1f), 0f);

                    offsetX += Card.Size.X + countOfType * CARD_SHIFT;
                }
            }

            foreach (DevelopmentCardType cardType in CardSet<DevelopmentCardType>.Values)
            {
                uint countOfType = _playerState.DevelopmentCards.Get(cardType);

                if (countOfType == 0)
                {
                    continue;
                }
                else
                {
                    _cardStackHitbox.Left = -_window.Size.X / 2 + 25 + offsetX;
                    _cardStackHitbox.Width = Card.Size.X + (countOfType - 1) * CARD_SHIFT;

                    bool hovering = _cardStackHitbox.Contains(mousePos.X, mousePos.Y);

                    // Smoothly hover/unhover
                    _cardStackHover[(int)cardType + CardSet<ResourceCardType>.Values.Count] += (hovering ? 5f : -5f) * deltaTime;
                    _cardStackHover[(int)cardType + CardSet<ResourceCardType>.Values.Count] = MathF.Max(MathF.Min(_cardStackHover[(int)cardType + CardSet<ResourceCardType>.Values.Count], 1f), 0f);

                    offsetX += Card.Size.X + countOfType * CARD_SHIFT;
                }
            }

        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            float offsetX = 0f;

            foreach(ResourceCardType cardType in CardSet<ResourceCardType>.Values)
            {
                uint countOfType = _playerState.ResourceCards.Get(cardType);

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
                        _tooltip.DisplayedString = cardType.GetName();

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

            foreach (DevelopmentCardType cardType in CardSet<DevelopmentCardType>.Values)
            {
                uint countOfType = _playerState.DevelopmentCards.Get(cardType);

                if (countOfType == 0)
                {
                    continue;
                }
                else
                {
                    float hoverAmount = Easing.ExpOut(_cardStackHover[(int)cardType + CardSet<ResourceCardType>.Values.Count]);
                    float hoverY = hoverAmount * 25f;

                    _cardPrimitive.SetType(cardType);

                    if (hoverAmount > 0.01f)
                    {
                        _tooltip.DisplayedString = cardType.GetName();

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

        public void SetPlayerState(PlayerState playerState)
        {
            _playerState = playerState;
            Array.Clear(_cardStackHover);
        }
    }
}
