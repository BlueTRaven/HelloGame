using System;
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
        public Vector2 anchor;

        public WidgetWindow(Rectangle bounds, bool draggable, Dictionary<string, Widget> widgets)
        {
            this.widgets = new Dictionary<string, Widget>();
            foreach (Widget widget in this.widgets.Values)
                widget.anchor = bounds.Location.ToVector2();
            this.windows = new Dictionary<string, WidgetWindow>();
            foreach (WidgetWindow window in this.windows.Values)
                window.anchor = bounds.Location.ToVector2();

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
                if (anchor != null && anchor != Vector2.Zero)
                {
                    bounds = new Rectangle((int)(createdPosition.X + anchor.X), (int)(createdPosition.Y + anchor.Y), bounds.Width, bounds.Height);
                }

                foreach (Widget widget in widgets.Values)
                {
                    if (anchor != null && anchor != Vector2.Zero)
                        widget.anchor = bounds.Location.ToVector2();
                    else
                        widget.anchor = bounds.Location.ToVector2();

                    widget.PreUpdate();
                }

                foreach (WidgetWindow window in windows.Values)
                {
                    if (anchor != null && anchor != Vector2.Zero)
                        window.anchor = anchor;
                    else
                        window.anchor = bounds.Location.ToVector2();

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
            window.anchor = bounds.Location.ToVector2();
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
