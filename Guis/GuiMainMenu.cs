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

namespace HelloGame.Guis
{
    public class GuiMainMenu : Gui
    {
        public GuiMainMenu() : base("menu")
        {
            SpriteFont font = Main.assets.GetFont("bfMunro23_bold");
            AddWidget("start", new WidgetButton(new Rectangle(128, 128, 64, 64)))
                .SetHasText(font, "Start", Color.White, Utility.TextAlignment.Center)
                .SetOpensGui("saveselect")
                .SetKeybind(Keys.Enter);

            AddWidget("test", new WidgetDropdown(new Rectangle(256, 128, 128, 32), Main.assets.GetFont("bfMunro12"), "test", Color.White, Utility.TextAlignment.Left, 5, Enum.GetNames(typeof(EnemyNoticeState))));

            backgroundColor = Color.Black;

            stopsWorldDraw = true;
            stopsWorldUpdate = true;
        }
    }
}
