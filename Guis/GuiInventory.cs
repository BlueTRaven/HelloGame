using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Items;
using HelloGame.Guis.Widgets;

namespace HelloGame.Guis
{
    public class GuiInventory : Gui
    {
        private Main main;

        public GuiInventory(Main main) : base("inventory")
        {
            this.main = main;
            AddWidget("openhud_dummy", new WidgetButton(new Rectangle(0, 0, 0, 0)))
                .SetOpensGui("hud")
                .SetKeybind(Keys.Escape);

            backgroundColor = new Color(Color.Black, 127);

            AddWindow("items", new WidgetWindowScrollable(new Rectangle(128, 64, 384, 592), 0, new Dictionary<string, Widget>()));
        }

        public override void Update()
        {
            base.Update();

            WidgetWindowScrollable scroll = GetWidgetWindow<WidgetWindowScrollable>("items");
            
            scroll.widgets.Clear();

            for (int i = 0; i < main.world.player.items.Count; i++)
            {
                scroll.AddWidget("i", new WidgetItemslot(new Vector2(0, i * 32), main.world.player.items[i]));
            }
            scroll.height = (main.world.player.items.Count * 32);
        }
    }
}
