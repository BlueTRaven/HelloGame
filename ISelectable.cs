using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using HelloGame.Guis.Widgets;
using Microsoft.Xna.Framework;

namespace HelloGame
{
    public interface ISelectable
    {
        int index { get; set; }

        bool trueSelected { get; set; }

        void DrawSelect_DEBUG(SpriteBatch batch);

        void Delete_DEBUG(World world);

        void UpdateWindowProperties(WidgetWindowEditProperties window);
        void UpdateSelectableProperties(WidgetWindowEditProperties window);

        void Move(Vector2 amt);
    }
}
