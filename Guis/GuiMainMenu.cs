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
            SpriteFont font = Main.assets.GetFont("bfMunro23_bold");
            AddWidget("start", new WidgetButton(new Rectangle(128, 128, 64, 64)))
                .SetHasText(font, MenuOptions.Start, Color.White, Enums.Alignment.Center)
                .SetOpensGui("saveselect")
                .SetKeybind(Keys.Enter)
                .SetAnchored(this, Enums.Alignment.Center);

            backgroundColor = Color.Black;

            stopsWorldDraw = true;
            stopsWorldUpdate = true;
            stopsWorldCreation = true;

            Dictionary<string, Widget> dict = new Dictionary<string, Widget>();

            for (int i = 0; i < 256; i++)
                dict.Add(i.ToString(), new WidgetButton(new Rectangle(0, i * 24, 128, 16))
                    .SetBackgroundColor(Color.White, Color.White, Color.White, Color.White, Color.White)
                    .SetHasText(Main.assets.GetFont("bfMunro8"), (i+ 1).ToString(), Color.White));

            AddWindow("test", new WidgetWindowScrollable(new Rectangle(512, 64, 128, 256), (dict.Count * 24), dict));
        }
    }
}
