using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Guis.Widgets;
using HelloGame.Entities;
using HelloGame.Utility;
using HelloGame.Content.Localization;

namespace HelloGame.Guis
{
    public class GuiMainMenu : Gui
    {
        public GuiMainMenu() : base("menu")
        {
            SpriteFont font = Main.assets.GetFont("bitfontMunro23BOLD");
            AddWidget("start", new WidgetButton(new Rectangle(128, 128, 64, 64)))
                .SetHasText(font, MenuOptions.Start, Color.White, Enums.Alignment.Center)
                .SetOpensGui("saveselect")
                .SetKeybind(Keys.Enter)
                .SetAnchored(this, Enums.Alignment.Center);

            backgroundColor = Color.Black;

            stopsWorldDraw = true;
            stopsWorldUpdate = true;
            stopsWorldCreation = true;
        }
    }
}
