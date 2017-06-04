using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;

namespace HelloGame.Guis.Widgets
{
    public class WidgetWindowRectangle : WidgetWindow
    {
        public WidgetWindowRectangle(Vector2 position) : base(new Rectangle(position.ToPoint(), new Point(192, 26)), false, null)
        {
            WidgetTextBox x = AddWidget("bounds_x", new WidgetTextBox(new Rectangle(8, 8, 40, 18), Main.assets.GetFont("bitfontMunro12"), "X", 4, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox y = AddWidget("bounds_y", new WidgetTextBox(new Rectangle(56, 8, 40, 18), Main.assets.GetFont("bitfontMunro12"), "Y", 4, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox width = AddWidget("bounds_width", new WidgetTextBox(new Rectangle(104, 8, 40, 18), Main.assets.GetFont("bitfontMunro12"), "Width", 4, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox height = AddWidget("bounds_height", new WidgetTextBox(new Rectangle(152, 8, 40, 18), Main.assets.GetFont("bitfontMunro12"), "Height", 4, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            x.SetTabProgressesTo(y);
            y.SetTabProgressesTo(width);
            width.SetTabProgressesTo(height);
            height.SetTabProgressesTo(x);
        }

        public Rectangle GetRectangle()
        {
            Rectangle rect = Rectangle.Empty;

            WidgetTextBox widget = GetWidget<WidgetTextBox>("bounds_x");
            rect.X = (int)float.Parse(widget.GetStringSafely());
            widget = GetWidget<WidgetTextBox>("bounds_y");
            rect.Y = (int)float.Parse(widget.GetStringSafely());
            widget = GetWidget<WidgetTextBox>("bounds_width");
            rect.Width = (int)float.Parse(widget.GetStringSafely());
            widget = GetWidget<WidgetTextBox>("bounds_height");
            rect.Height = (int)float.Parse(widget.GetStringSafely());

            return rect;
        }

        public void Set(Rectangle rectangle)
        {
            GetWidget<WidgetTextBox>("bounds_x").SetString(rectangle.X.ToString());
            GetWidget<WidgetTextBox>("bounds_y").SetString(rectangle.Y.ToString());
            GetWidget<WidgetTextBox>("bounds_width").SetString(rectangle.Width.ToString());
            GetWidget<WidgetTextBox>("bounds_height").SetString(rectangle.Height.ToString());
        }
    }
}
