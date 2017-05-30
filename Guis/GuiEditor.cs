using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Guis.Widgets;
using HelloGame.Utility;

namespace HelloGame.Guis
{
    public class GuiEditor : Gui
    {
        Vector2 worldTarget;
        private const float movespeed = 20;
        private float cMoveSpeed;

        public List<ISelectable> selected;
        public ISelectable trueSelected;

        public GuiEditor() : base("editor")
        {
            stopsWorldUpdate = true;    //should stop the world from updating but not from drawing

            selected = new List<ISelectable>();

            AddWidget("openhud_dummy", new WidgetButton(new Rectangle(0, 0, 0, 0)))
                .SetOpensGui("hud") //dummy widget to return to hud
                .SetKeybind(Keys.O);

            AddWidget("savename", new WidgetTextBox(new Rectangle(8, 8, 128, 16), Main.assets.GetFont("bfMunro12"), "save name", 60, Enums.Alignment.Left)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("displayname", new WidgetTextBox(new Rectangle(8, 32, 128, 16), Main.assets.GetFont("bfMunro12"), "display name", 60, Enums.Alignment.Left)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("bg_r", new WidgetTextBox(new Rectangle(8, 54, 128, 16), Main.assets.GetFont("bfMunro12"), "bgr", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("bg_g", new WidgetTextBox(new Rectangle(8, 78, 128, 16), Main.assets.GetFont("bfMunro12"), "bgg", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("bg_b", new WidgetTextBox(new Rectangle(8, 102, 128, 16), Main.assets.GetFont("bfMunro12"), "bgb", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);

            Dictionary<string, Widget> windowWidgets = new Dictionary<string, Widget>();
            windowWidgets.Add("brush", new WidgetButton(new Rectangle(8, 8, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("brush")).SetKeybind(Keys.D1));
            windowWidgets.Add("wall", new WidgetButton(new Rectangle(48, 8, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("wall")).SetKeybind(Keys.D2));
            windowWidgets.Add("entity", new WidgetButton(new Rectangle(88, 8, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("entity")).SetKeybind(Keys.D3));
            windowWidgets.Add("prop", new WidgetButton(new Rectangle(128, 8, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("prop")).SetKeybind(Keys.D4));
            windowWidgets.Add("select", new WidgetButton(new Rectangle(168, 8, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("select")).SetKeybind(Keys.D5));
            windowWidgets.Add("trigger", new WidgetButton(new Rectangle(8, 48, 32, 32))
                .SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray)
                .SetHasTexture(Main.assets.GetTexture("trigger")).SetKeybind(Keys.D6));
            WidgetWindow window = AddWindow("modeselector", new WidgetWindowModeSelector(new Rectangle(0, Main.HEIGHT - 128, 208, 112), false, windowWidgets));
            window.backgroundColor = Color.Black;

            windowWidgets = new Dictionary<string, Widget>();

            windowWidgets.Add("up", new WidgetButton(new Rectangle(80, 2, 16, 6)).SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray).SetHasTexture(Main.assets.GetTexture("arrowUp")).SetKeybind(Keys.OemOpenBrackets));
            windowWidgets.Add("down", new WidgetButton(new Rectangle(80, 8, 16, 6)).SetBackgroundColor(Color.White, Color.LightGray, Color.LightGray, Color.Gray, Color.DarkGray).SetHasTexture(Main.assets.GetTexture("arrowDown")).SetKeybind(Keys.OemCloseBrackets));
            windowWidgets.Add("gridsize", new WidgetTextBox(new Rectangle(8, 2, 72, 12), Main.assets.GetFont("bfMunro12"), "16", 4, Enums.Alignment.Center, TextBoxFilter.Numerical).SetBackgroundColor(Color.White, Color.Gray));
            windowWidgets.Add("showgrid", new WidgetCheckbox(new Rectangle(102, 2, 12, 12), Color.White).SetKeybind(Keys.F));
            window = AddWindow("grid", new WidgetWindowHolder(new Rectangle(0, Main.HEIGHT - 16, 128, 16), false, windowWidgets));
            window.backgroundColor = Color.Black;

            AddWindow("modeoptions", new WidgetWindowEditorOptions(new Rectangle(208, Main.HEIGHT - 192, 256, 192), false, GetWidgetWindow<WidgetWindowModeSelector>("modeselector")));

            window = AddWindow("modifyproperties", new WidgetWindowEditProperties(new Rectangle(Main.WIDTH - 256, 0, 256, 256)));
            window.backgroundColor = Color.Black;
        }

        public void SetWorldOptions(string name, string displayname, Color color)
        {
            GetWidget<WidgetTextBox>("savename").SetString(name);
            GetWidget<WidgetTextBox>("displayname").SetString(displayname);
            GetWidget<WidgetTextBox>("bg_r").SetString(color.R.ToString());
            GetWidget<WidgetTextBox>("bg_g").SetString(color.G.ToString());
            GetWidget<WidgetTextBox>("bg_b").SetString(color.B.ToString());
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            Main.camera.target = worldTarget;

            cMoveSpeed = movespeed;

            if (Main.keyboard.KeyHeld(Keys.LeftShift))
                cMoveSpeed *= 2;

            if (Main.keyboard.KeyHeld(Main.options.leftKeybind))
            {
                worldTarget.X -= cMoveSpeed;
            }
            if (Main.keyboard.KeyHeld(Main.options.upKeybind))
            {
                worldTarget.Y -= cMoveSpeed;
            }
            if (Main.keyboard.KeyHeld(Main.options.rightKeybind))
            {
                worldTarget.X += cMoveSpeed;
            }
            if (Main.keyboard.KeyHeld(Main.options.downKeybind))
            {
                worldTarget.Y += cMoveSpeed;
            }
        }

        public override void Update()
        {
            base.Update();

            WidgetWindowModeSelector wwms = GetWidgetWindow<WidgetWindowModeSelector>("modeselector");

            #region gridsize stuff
            WidgetWindowHolder wwh = GetWidgetWindow<WidgetWindowHolder>("grid");
            
            if (wwh.GetWidget<WidgetButton>("up").IsClicked())
            {
                int val = 0;
                StringBuilder text = wwh.GetWidget<WidgetTextBox>("gridsize").text;
                int.TryParse(text.ToString(), out val);

                text = new StringBuilder((val + 1).ToString());
                wwh.GetWidget<WidgetTextBox>("gridsize").text = text;
            }

            if (wwh.GetWidget<WidgetButton>("down").IsClicked())
            {
                int val = 0;
                StringBuilder text = wwh.GetWidget<WidgetTextBox>("gridsize").text;
                int.TryParse(text.ToString(), out val);

                if (val - 1 > 0)
                {
                    text = new StringBuilder((val - 1).ToString());
                    wwh.GetWidget<WidgetTextBox>("gridsize").text = text;
                }
            }
            #endregion
        }

        public Color GetBackgroundColor()
        {
            Color color = Color.White;
            WidgetTextBox widget = GetWidget<WidgetTextBox>("bg_r");
            color.R = (byte)(int.Parse(widget.GetStringSafely(color.R.ToString())) % 256);
            widget = GetWidget<WidgetTextBox>("bg_g");
            color.G = (byte)(int.Parse(widget.GetStringSafely(color.G.ToString())) % 256);
            widget = GetWidget<WidgetTextBox>("bg_b");
            color.B = (byte)(int.Parse(widget.GetStringSafely(color.B.ToString())) % 256);

            color.A = 255;

            return color;
        }

        public int GetGridSize()
        {
            return int.Parse(GetWidgetWindow<WidgetWindowHolder>("grid").GetWidget<WidgetTextBox>("gridsize").GetStringSafely());
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
        }
    }
}
