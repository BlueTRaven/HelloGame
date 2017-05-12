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
    public class Gui : IWidgetHolder
    {
        public Dictionary<string, WidgetWindow> windows;
        //public WidgetWindow[] windows;
        public Dictionary<string, Widget> widgets;
        //public Widget[] widgets;

        public Color backgroundColor;

        public bool stopsWorldDraw, stopsWorldUpdate, stopsWorldCreation;

        public Gui(string key)
        {
            //windows = new WidgetWindow[128];
            //widgets = new Widget[128];

            widgets = new Dictionary<string, Widget>();
            windows = new Dictionary<string, WidgetWindow>();

            Main.guis.Add(key, this);
        }

        public virtual void PreUpdate()
        {
            foreach (WidgetWindow window in windows.Values)
            {
                window.PreUpdate();
            }

            foreach (Widget widget in widgets.Values)
            {
                    widget.PreUpdate();
            }
        }

        public virtual void Update()
        {
            foreach (WidgetWindow window in windows.Values)
            {
                    window.Update();
            }

            foreach (Widget widget in widgets.Values)
            {
                    widget.Update();
            }
        }

        public virtual void PostUpdate()
        {
            foreach (WidgetWindow window in windows.Values)
            {
                    window.PostUpdate();
            }

            foreach (Widget widget in widgets.Values)
            {
                if (widget != null)
                widget.PostUpdate();
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.DrawRectangle(new Rectangle(0, 0, Main.WIDTH, Main.HEIGHT), backgroundColor);

            foreach (WidgetWindow window in windows.Values)
            {
                    window.Draw(batch);
            }

            foreach (Widget widget in widgets.Values)
            {
                if (widget != null)
                    widget.Draw(batch);
            }
        }

        public T AddWindow<T>(string key, T window) where T : WidgetWindow
        {
            windows.Add(key, window);

            return window;
        }

        public T AddWidget<T>(string key, T widget) where T : Widget
        {
            widgets.Add(key, widget);

            return widget;
        }

        public T GetWidget<T>(string index) where T : Widget
        {
            return widgets.Keys.Contains(index) ? (T)widgets[index] : null;
        }

        public T GetWidgetWindow<T>(string index) where T : WidgetWindow
        {
            return windows.Keys.Contains(index) ? (T)windows[index] : null;
        }

        public bool LastClickOnWidget()
        {
            foreach(Widget widget in widgets.Values)
            {
                if (widget.IsClicked() || widget.IsHeld())
                    return true;
            }

            foreach (WidgetWindow window in windows.Values)
            {
                if (window.LastClickOnWidget())
                    return true;
            }

            return false;
        }
    }
}
