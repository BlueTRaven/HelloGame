using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Entities;
using HelloGame.Guis;
using HelloGame.Guis.Widgets;
using HelloGame.Utility;

namespace HelloGame.Items
{
    public class ItemKey : Item
    {
        public string useText;

        public ItemKey(int type) : base(0, type)
        {
            if (type == 0)
            {
                name = "Citadel Courtyard Door Key";
                description[0] = "A key used to open the large stone door in the citadel courtyard.";

                useText = GetGenericUseText(name);
            }
        }

        private string GetGenericUseText(string add)
        {
            return "Used " + add;
        }
    }
}
