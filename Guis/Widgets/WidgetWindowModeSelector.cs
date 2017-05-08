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
    public class WidgetWindowModeSelector : WidgetWindow
    {
        public int mode;
        private int prevMode;

        public WidgetWindowModeSelector(Rectangle bounds, bool draggable, Dictionary<string, Widget> widgets) : base(bounds, draggable, widgets)
        {
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            int i = 0;
            foreach (Widget widget in widgets.Values)
            {
                if (widget.state == 2)//pressed
                    mode = i;
                i++;
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            prevMode = mode;
        }

        public bool ModeChanged()
        {
            return prevMode != mode;
        }
    }
}
