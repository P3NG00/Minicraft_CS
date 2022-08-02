using System;
using Microsoft.Xna.Framework;

namespace Game.Data
{
    public sealed class Button
    {
        // (0f, 0f) = top-left of window. (0.5f, 0.5f) = center of window. (1f, 1f) = bottom-right of window.
        private readonly Vector2 _relativeCenter;
        private readonly Point _size;
        private readonly string _text;
        private readonly Action _action;

        public Color ColorBox;
        public Color ColorText;
        public Color? ColorBoxHighlight = null;
        public Color? ColorTextHighlight = null;

        private Rectangle _lastRect;
        private bool _highlighted = false;

        public Button(Vector2 relativeCenter, Point size, string text, Color colorBox, Color colorText, Action actionOnClick)
        {
            _relativeCenter = relativeCenter;
            _size = size;
            _text = text;
            ColorBox = colorBox;
            ColorText = colorText;
            _action = actionOnClick;
        }

        private Rectangle Rectangle
        {
            get
            {
                var pos = ((Display.WindowSize.ToVector2() * _relativeCenter) - (_size.ToVector2() / 2f)).ToPoint();
                return new Rectangle(pos, _size);
            }
        }

        public void Update()
        {
            // get rectangle
            _lastRect = Rectangle;
            // test bounds
            _highlighted = _lastRect.Contains(Input.MousePosition);
            if (Input.MouseLeftFirstUp() && _highlighted)
                _action();
        }

        public void Draw()
        {
            // draw box
            Display.Draw(_lastRect, _highlighted && ColorBoxHighlight.HasValue ? ColorBoxHighlight.Value : ColorBox);
            // draw text centered in box
            var drawPos = _lastRect.Center.ToVector2() - (Display.FontUI.MeasureString(_text) / 2f);
            Display.DrawString(Display.FontUI, drawPos, _text, _highlighted && ColorTextHighlight.HasValue ? ColorTextHighlight.Value : ColorText);
        }
    }
}
