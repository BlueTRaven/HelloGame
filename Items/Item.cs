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
    public abstract class Item
    {
        public string name;
        public string[] description;

        public int type { get; private set; }

        public Item(int type)
        {
            this.type = type;
            description = new string[8] { "", "", "", "", "", "", "", "" };
        }
    }
}
