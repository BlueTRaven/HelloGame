using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Guis.Widgets;

namespace HelloGame.Guis
{
    public class GuiMainMenu : Gui
    {
        public GuiMainMenu() : base("menu")
        {
            SpriteFont font = Main.assets.GetFont("bfMunro23_bold");
            AddWidget("start", new WidgetButton(new Rectangle(128, 128, 64, 64)))
                .SetHasText(font, "Start", Color.White, Utility.TextAlignment.Center)
                .SetOpensGui("hud")
                .SetKeybind(Keys.Enter);

            backgroundColor = Color.Black;

            stopsWorldDraw = true;
            stopsWorldUpdate = true;
        }
    }
}
