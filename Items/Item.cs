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

        public int itemType { get; private set; }

        public int type { get; private set; }

        public int count;
        public Item(int itemType, int type, int count)
        {
            this.itemType = itemType;
            this.type = type;
            this.count = count;

            description = new string[8] { "", "", "", "", "", "", "", "" };
        }

        public SerItem Save()
        {
            return new SerItem()
            {
                ItemType = itemType,
                Type = type,
                Count = count
            };
        }

        public static string GetTextureName(Item item)
        {
            switch (item.itemType)
            {
                case 0:     //key
                    return "keyItem";
                case 1:
                    return "potionItem";
                default:
                    return "unknownItem";
            }
        }
    }
}
