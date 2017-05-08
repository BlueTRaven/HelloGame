﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;

namespace HelloGame.Guis.Widgets
{
    public abstract class WidgetWindow : IWidgetHolder
    {
        public Rectangle bounds;
        public bool draggable { get; private set; }
        public Dictionary<string, Widget> widgets;
        public Dictionary<string, WidgetWindow> windows;

        public Color backgroundColor;

        public bool active = true;

        public Vector2 createdPosition;

        public WidgetWindow(Rectangle bounds, bool draggable, Dictionary<string, Widget> widgets)
        {
            this.widgets = new Dictionary<string, Widget>();
            this.windows = new Dictionary<string, WidgetWindow>();

            this.bounds = bounds;
            this.draggable = draggable;

            if (widgets != null)
                this.widgets = widgets;

            createdPosition = bounds.Location.ToVector2();
        }

        public virtual void PreUpdate()
        {
            if (active)
            {
                foreach (Widget widget in widgets.Values)
                {
                    widget.bounds = new Rectangle((int)widget.createdPosition.X + bounds.Location.X, (int)widget.createdPosition.Y + bounds.Location.Y, widget.bounds.Width, widget.bounds.Height);
                    widget.PreUpdate();
                }

                foreach (WidgetWindow window in windows.Values)
                {
                    window.bounds = new Rectangle((int)window.createdPosition.X + bounds.Location.X, (int)window.createdPosition.Y + bounds.Location.Y, window.bounds.Width, window.bounds.Height);
                    window.PreUpdate();
                }
            }
        }

        public virtual void Update()
        {
            if (active)
            {
                foreach (Widget widget in widgets.Values)
                {
                    widget.Update();
                }

                foreach (WidgetWindow window in windows.Values)
                {
                    window.Update();
                }
            }
        }

        public virtual void PostUpdate()
        {
            if (active)
            {
                foreach (Widget widget in widgets.Values)
                {
                    widget.PostUpdate();
                }

                foreach (WidgetWindow window in windows.Values)
                {
                    window.PostUpdate();
                }
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            if (active)
            {
                batch.DrawRectangle(bounds, backgroundColor);

                foreach (Widget widget in widgets.Values)
                {
                    widget.Draw(batch);
                }

                foreach (WidgetWindow window in windows.Values)
                {
                    window.Draw(batch);
                }
            }
        }

        public T AddWidget<T>(string key, T widget) where T : Widget
        {
            widgets.Add(key, widget);

            return widget;
        }

        public T AddWindow<T>(string key, T window) where T : WidgetWindow
        {
            windows.Add(key, window);

            return window;
        }

        public T GetWindow<T>(string index) where T : WidgetWindow
        {
            return (T)windows[index];
        }

        public T GetWidget<T>(string index) where T : Widget
        {
            return (T)widgets[index];
        }

        public bool LastClickOnWidget()
        {
            foreach (Widget widget in widgets.Values)
            {
                if (widget.IsClicked() || widget.IsHeld())
                    return true;
            }

            foreach (WidgetWindow window in windows.Values)
            {
                if (window.LastClickOnWidget())
                    return true;
            }

            return Main.mouse.LeftButtonPressed() && bounds.Contains(Main.mouse.currentPosition);
        }
    }
}
