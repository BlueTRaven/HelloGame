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
    public class WidgetWindowEditProperties : WidgetWindow
    {
        public int mode = -1, prevMode;

        public ISelectable selected;
        public WidgetWindowEditProperties(Rectangle bounds) : base(bounds, false, null)
        {
            //brush
            AddWindow("brush_bounds", new WidgetWindowRectangle(Vector2.Zero));
            AddWindow("brush_texture", new WidgetWindowTextureSelector(new Vector2(0, 26)));
            //AddWidget("brush_type", new WidgetTextBox(new Rectangle(128, 34, 56, 24), Main.assets.GetFont("bfMunro12"), "type", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("brush_type", new WidgetDropdown(new Rectangle(128, 40, 56, 24), Main.assets.GetFont("bfMunro12"), "type", Color.White, TextAlignment.Left, 4, Enum.GetNames(typeof(BrushDrawType))));
            AddWidget("brush_drawahead", new WidgetCheckbox(new Rectangle(128, 64, 24, 24), Color.White));

            //wall
            AddWindow("wall_bounds", new WidgetWindowRectangle(Vector2.Zero));
            //entity
            AddWindow("entity_bounds", new WidgetWindowRectangle(Vector2.Zero));
            AddWidget("entity_type", new WidgetTextBox(new Rectangle(8, 32, 56, 24), Main.assets.GetFont("bfMunro12"), "type", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_info1", new WidgetTextBox(new Rectangle(8, 64, 56, 24), Main.assets.GetFont("bfMunro12"), "info 1", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_info2", new WidgetTextBox(new Rectangle(72, 64, 56, 24), Main.assets.GetFont("bfMunro12"), "info 2", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_spawnrandom", new WidgetCheckbox(new Rectangle(104, 32, 24, 24), Color.White));
            AddWidget("entity_spawnrotation", new WidgetTextBox(new Rectangle(138, 64, 56, 24), Main.assets.GetFont("bfMunro12"), "rotation", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("entity_spawnstate", new WidgetDropdown(new Rectangle(8, 120, 56, 24), Main.assets.GetFont("bfMunro12"), "mode", Color.White, TextAlignment.Left, 5, Enum.GetNames(typeof(EnemyNoticeState))));

            //prop
            AddWindow("prop_position", new WidgetWindowVector2(Vector2.Zero));
            AddWindow("prop_texture", new WidgetWindowTextureSelector(new Vector2(0, 26)));
            AddWidget("prop_shadowscale", new WidgetTextBox(new Rectangle(80, 34, 40, 18), Main.assets.GetFont("bfMunro12"), "scale", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            //trigger
            AddWindow("trigger_bounds", new WidgetWindowRectangle(Vector2.Zero));
            AddWidget("trigger_command", new WidgetTextBox(new Rectangle(8, 32, 56, 24), Main.assets.GetFont("bfMunro12"), "command", 4, TextAlignment.Left, TextBoxFilter.Alphabetical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("trigger_info", new WidgetTextBox(new Rectangle(8, 64, 56, 24), Main.assets.GetFont("bfMunro12"), "info", 4, TextAlignment.Left, TextBoxFilter.Alphabetical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("trigger_perm", new WidgetCheckbox(new Rectangle(104, 32, 24, 24), Color.White));
        }

        public override void Update()
        {
            base.Update();

            if (selected != null)
            {
                selected.UpdateSelectableProperties(this);
            }

            string[] showKeysWidgets = GetWidgetShowKeys(mode);
            string[] showKeysWindows = GetWindowShowKeys(mode);

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

        public override void PostUpdate()
        {
            base.PostUpdate();

            prevMode = mode;
        }

        #region get
        private string[] GetWidgetShowKeys(int mode)
        {
            if (mode == World.mode_brushes)
            {
                return new string[] { "brush_type", "brush_drawahead" };
            }
            else if (mode == World.mode_spawners)
            {
                return new string[] { "entity_type", "entity_info1", "entity_info2", "entity_spawnrandom", "entity_spawnrotation", "entity_spawnstate" };
            }
            else if (mode == World.mode_props)
            {
                return new string[] { "prop_shadowscale" };
            }
            else if (mode == World.mode_trigger)
            {
                return new string[] { "trigger_command", "trigger_info", "trigger_perm" };
            }

            return new string[] { };
        }

        private string[] GetWindowShowKeys(int mode)
        {
            if (mode == World.mode_brushes)
            {
                return new string[] { "brush_bounds", "brush_texture" };
            }
            else if (mode == World.mode_walls)
            {
                return new string[] { "wall_bounds" };
            }
            else if (mode == World.mode_spawners)
            {
                return new string[] { "entity_bounds" };
            }
            else if (mode == World.mode_props)
            {
                return new string[] { "prop_position", "prop_texture" };
            }
            else if (mode == World.mode_trigger)
            {
                return new string[] { "trigger_bounds" };
            }

            return new string[] { };
        }
        #endregion
    }
}
