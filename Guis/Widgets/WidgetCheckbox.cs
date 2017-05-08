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
    public class WidgetCheckbox : Widget
    {
        Color backgroundColor;

        Texture2D checkTexture;

        public bool isChecked = false;
        public WidgetCheckbox(Rectangle bounds, Color backgroundColor) : base(bounds)
        {
            this.backgroundColor = backgroundColor;

            checkTexture = Main.assets.GetTexture("checkbox");
        }

        public override void OnClick()
        {
            base.OnClick();

            isChecked = !isChecked;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (active)
            {
                batch.DrawRectangle(bounds, backgroundColor);

                if (isChecked)
                    batch.Draw(checkTexture, bounds, Color.White);
            }
        }
    }
}
