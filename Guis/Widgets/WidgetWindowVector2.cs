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
    public class WidgetWindowVector2 : WidgetWindow
    {
        public WidgetWindowVector2(Vector2 position) : base(new Rectangle(position.ToPoint(), new Point(192, 26)), false, null)
        {
            WidgetTextBox x = AddWidget("x", new WidgetTextBox(new Rectangle(8, 8, 40, 18), Main.assets.GetFont("bfMunro12"), "X", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox y = AddWidget("y", new WidgetTextBox(new Rectangle(56, 8, 40, 18), Main.assets.GetFont("bfMunro12"), "Y", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            x.SetTabProgressesTo(y);
            y.SetTabProgressesTo(x);
        }

        public Vector2 GetVector2()
        {
            Vector2 vector = Vector2.Zero;

            WidgetTextBox widget = GetWidget<WidgetTextBox>("x");
            vector.X = (int)float.Parse(widget.GetStringSafely());
            widget = GetWidget<WidgetTextBox>("y");
            vector.Y = (int)float.Parse(widget.GetStringSafely());

            return vector;
        }

        public void Set(Vector2 vector)
        {
            GetWidget<WidgetTextBox>("x").SetString(vector.X.ToString());
            GetWidget<WidgetTextBox>("y").SetString(vector.Y.ToString());
        }
    }
}
