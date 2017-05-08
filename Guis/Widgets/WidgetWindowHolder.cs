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
    public class WidgetWindowHolder : WidgetWindow
    {
        public WidgetWindowHolder(Rectangle bounds, bool draggable, Dictionary<string, Widget> widgets) : base(bounds, draggable, widgets)
        {
        }
    }
}
