using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Guis.Widgets;
using HelloGame.Entities;

namespace HelloGame.Guis
{
    public class GuiSaveSelect : Gui
    {
        bool[] isnew;
        WidgetButton[] slots;
        WidgetButton[] deleteSlots;

        private bool haschecked;
        private bool creating;

        public string saveFileName;

        public GuiSaveSelect() : base("saveselect")
        {
            isnew = new bool[8];
            slots = new WidgetButton[8];
            deleteSlots = new WidgetButton[8];

            for (int i = 0; i < 8; i++)
            {
                isnew[i] = true;
                slots[i] = AddWidget("saveslot_" + i, new WidgetButton(new Rectangle((int)((Main.WIDTH / 2) - 128), 64 * i + 72 + i * 8, 256, 64))
                    .SetBackgroundColor(Color.White, Color.LightGray, Color.DarkGray, Color.Gray, Color.White)
                    .SetHasText(Main.assets.GetFont("bfMunro12"), "New File " + (i + 1), Color.White, Utility.TextAlignment.Center));
                deleteSlots[i] = AddWidget("delsaveslot_" + i, new WidgetButton(new Rectangle((Main.WIDTH / 2) + 128, 64 * i + 72 + i * 8, 64, 64))
                    .SetBackgroundColor(Color.White, Color.LightGray, Color.DarkGray, Color.Gray, Color.White)
                    .SetHasText(Main.assets.GetFont("bfMunro12"), "Delete", Color.White, Utility.TextAlignment.Center));
                deleteSlots[i].active = false;
            }

            backgroundColor = Color.Black;

            stopsWorldDraw = true;
            stopsWorldUpdate = true;
        }

        public override void Update()
        {
            base.Update();

            if (!haschecked)
            {
                if (!Directory.Exists("Saves"))
                    Directory.CreateDirectory("Saves");
                DirectoryInfo di = new DirectoryInfo("Saves");
                FileInfo[] fi = di.GetFiles("*.hgsf");

                for (int i = 0; i < 8; i++)
                {
                    isnew[i] = true;
                }
                for (int i = 0; i < fi.Length; i++)
                {
                    isnew[i] = false;
                    slots[i].text = fi[i].Name.Split('.')[0];
                    deleteSlots[i].active = true;
                }
                haschecked = true;
            }

            if (GetWidget<WidgetTextBox>("savename") != null)
            {
                if (Main.keyboard.KeyPressed(Keys.Escape))
                {
                    creating = false;
                    widgets["savename"] = null;

                    for (int i = 0; i < 8; i++)
                        slots[i].active = true;
                }
                else if (Main.keyboard.KeyPressed(Keys.Enter))
                {
                    saveFileName = GetWidget<WidgetTextBox>("savename").GetStringSafely("Default");
                    Main.activeGui = Main.guis["hud"];
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (slots[i].state == Widget.state_clicked)
                    {
                        if (isnew[i])
                        {

                            slots[i].active = false;
                            creating = true;
                            AddWidget("savename", new WidgetTextBox(slots[i].bounds, Main.assets.GetFont("bfMunro12"), "Enter A Save Name", 32, Utility.TextAlignment.Center, TextBoxFilter.AlphaNumeric)
                                .SetHasBlackList('.')); //we don't want periods as it uses them to determine the file name.
                        }
                        else
                        {
                            saveFileName = slots[i].text;
                            Main.activeGui = Main.guis["hud"];
                        }
                    }
                    else if (deleteSlots[i].state == Widget.state_clicked)
                    {
                        deleteSlots[i].active = false;
                        haschecked = false;
                        File.Delete("Saves/" + slots[i].text + ".hgsf");

                        slots[i].text = "New File " + (i + 1);
                    }
                }
            }
        }
    }
}
