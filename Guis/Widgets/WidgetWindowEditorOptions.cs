using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Entities;

namespace HelloGame.Guis.Widgets
{
    public class WidgetWindowEditorOptions : WidgetWindow
    {
        private int mode;

        private WidgetWindowModeSelector modeSelector;

        private bool flag;
        public WidgetWindowEditorOptions(Rectangle bounds, bool draggable, WidgetWindowModeSelector modeSelector) : base(bounds, draggable, null)
        {
            this.modeSelector = modeSelector;
            this.backgroundColor = Color.Black;
            //brush mode widgets
            AddWindow("brush_textureselector", new WidgetWindowTextureSelector(new Vector2(0, 0)));
            GetWindow<WidgetWindowTextureSelector>("brush_textureselector").backgroundColor = Color.Black;

            AddWidget("brush_type", new WidgetDropdown(new Rectangle(112, 40, 64, 24), Main.assets.GetFont("bfMunro12"), "type", Color.White, TextAlignment.Left, 4, Enum.GetNames(typeof(BrushDrawType))));
            //AddWidget("brush_mode", new WidgetTextBox(new Rectangle(112, 40, 64, 24), Main.assets.GetFont("bfMunro12"), "mode", 1, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("brush_drawahead", new WidgetCheckbox(new Rectangle(112, 8, 24, 24), Color.White));

            //entity mode widgets
            AddWidget("entity_type", new WidgetTextBox(new Rectangle(8, 8, 56, 24), Main.assets.GetFont("bfMunro12"), "type", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_info1", new WidgetTextBox(new Rectangle(8, 40, 56, 24), Main.assets.GetFont("bfMunro12"), "info 1", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_info2", new WidgetTextBox(new Rectangle(72, 40, 56, 24), Main.assets.GetFont("bfMunro12"), "info 2", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_spawnrandom", new WidgetCheckbox(new Rectangle(104, 8, 24, 24), Color.White));
            AddWidget("entity_spawnrotation", new WidgetTextBox(new Rectangle(136, 40, 56, 24), Main.assets.GetFont("bfMunro12"), "rotation", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_spawnstate", new WidgetDropdown(new Rectangle(8, 72, 56, 24), Main.assets.GetFont("bfMunro12"), "mode", Color.White, TextAlignment.Left, 5, Enum.GetNames(typeof(EnemyNoticeState))));

            //prop mode widgets
            AddWindow("prop_textureselector", new WidgetWindowTextureSelector(new Vector2(0, -64)));
            GetWindow<WidgetWindowTextureSelector>("prop_textureselector").backgroundColor = Color.Black;
            AddWidget("prop_shadowscale", new WidgetTextBox(new Rectangle(112, 40, 32, 24), Main.assets.GetFont("bfMunro12"), "scale", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);

            //trigger mode widgets
            AddWidget("trigger_command", new WidgetTextBox(new Rectangle(8, 8, 56, 24), Main.assets.GetFont("bfMunro12"), "command", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("trigger_info", new WidgetTextBox(new Rectangle(8, 32, 56, 24), Main.assets.GetFont("bfMunro12"), "info", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("trigger_perm", new WidgetCheckbox(new Rectangle(96, 8, 16, 16), Color.White));
        }

        public override void Update()
        {
            base.Update();

            if (modeSelector.ModeChanged() || !flag)
            {
                string[] showKeysWidgets = GetWidgetShowKeys(modeSelector.mode);
                string[] showKeysWindows = GetWindowShowKeys(modeSelector.mode);

                foreach (string name in widgets.Keys)
                {
                    bool found = false;
                    foreach (string str in showKeysWidgets)
                    {
                        if (name == str)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        widgets[name].active = true;
                    else widgets[name].active = false;
                }

                foreach (string name in windows.Keys)
                {
                    bool found = false;
                    foreach (string str in showKeysWindows)
                    {
                        if (name == str)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        windows[name].active = true;
                    else windows[name].active = false;
                }
            }

            if (!flag)
                flag = true;
        }

        public string[] GetWidgetShowKeys(int mode)
        {
            if (mode == 0)
            {   //brush mode
                return new string[] { "brush_type", "brush_drawahead" };
            }
            if (mode == 2)
            {   //entity mode
                return new string[] { "entity_type", "entity_info1", "entity_info2", "entity_spawnrandom", "entity_spawnrotation", "entity_spawnstate" };
            }
            if (mode == 3)
            {   //prop mode
                return new string[] { "prop_shadowscale" };
            }
            if (mode == 4)
            {   //trigger mode
                return new string[] { "trigger_command", "trigger_info", "trigger_perm" };
            }

            return new string[] { };
        }

        public string[] GetWindowShowKeys(int mode)
        {
            if (mode == 0)
            {   //brush mode
                return new string[] { "brush_textureselector" };
            }
            if (mode == 3)
            {   //prop mode
                return new string[] { "prop_textureselector" };
            }

            return new string[] { };
        }

        public string GetBrushTexture()
        {
            return GetWidget<WidgetTextBox>("brush_textureshow_nomodify").text.ToString();
        }

        public string GetPropTexture()
        {
            return GetWidget<WidgetTextBox>("prop_textureshow_nomodify").text.ToString();
        }

        public Color GetBrushColor()
        {
            Color color = Color.White;
            WidgetTextBox widget = GetWidget<WidgetTextBox>("brush_R");
            if (!widget.hasStartText)
                color.R = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("brush_G");
            if (!widget.hasStartText)
                color.G = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("brush_B");
            if (!widget.hasStartText)
                color.B = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("brush_A");
            if (!widget.hasStartText)
                color.A = (byte)(int.Parse(widget.text.ToString()) % 256);

            return color;
        }

        public Color GetPropColor()
        {
            Color color = Color.White;
            WidgetTextBox widget = GetWidget<WidgetTextBox>("prop_R");
            if (!widget.hasStartText)
                color.R = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("prop_G");
            if (!widget.hasStartText)
                color.G = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("prop_B");
            if (!widget.hasStartText)
                color.B = (byte)(int.Parse(widget.text.ToString()) % 256);
            widget = GetWidget<WidgetTextBox>("prop_A");
            if (!widget.hasStartText)
                color.A = (byte)(int.Parse(widget.text.ToString()) % 256);

            return color;
        }

        public BrushDrawType GetBrushDrawType()
        {
            int val = GetWidget<WidgetDropdown>("brush_type").GetIndex();
            return (BrushDrawType)val;
        }
    }
}
