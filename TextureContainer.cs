using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelloGame
{
    public struct TextureContainer
    {
        public string name;
        public Texture2D texture;

        public TextureContainer(string name)
        {
            this.name = name;

            texture = Main.assets.GetTexture(name);
        }
    }
}
