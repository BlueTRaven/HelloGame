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

        protected int itemType;

        public int type { get; private set; }

        public Item(int itemType, int type)
        {
            this.itemType = itemType;
            this.type = type;
            description = new string[8] { "", "", "", "", "", "", "", "" };
        }

        public SerItem Save()
        {
            return new SerItem()
            {
                ItemType = itemType,
                Type = type
            };
        }
    }
}
