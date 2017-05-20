using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace HelloGame
{
    public class Assets
    {
        public Dictionary<string, Texture2D> textures;
        public Dictionary<string, SpriteFont> fonts;

        public Texture2D whitePixel;

        public Assets()
        {
            textures = new Dictionary<string, Texture2D>();
            fonts = new Dictionary<string, SpriteFont>();
        }

        public Texture2D GetTexture(string name)
        {
            try
            {
                return textures[name];
            }
            catch
            {
                Console.WriteLine("Couldn't find and/or load texture file '" + name + "'!\n", true);
                return textures["whitePixel"];
            }
        }

        public SpriteFont GetFont(string name)
        {
            try
            {
                return this.fonts[name];
            }
            catch
            {
                Console.WriteLine("Couldn't find and/or load font file '" + name + "'!\n", true);
                return null;
            }
        }

        public void Load(GraphicsDevice device, ContentManager content)
        {
            //textures
            whitePixel = new Texture2D(device, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });
            textures.Add("whitePixel", whitePixel);
            textures.Add("shadow", content.Load<Texture2D>("Textures/Shadow"));
            textures.Add("displace", content.Load<Texture2D>("Textures/displace"));

            //entity
            textures.Add("animationtest", content.Load<Texture2D>("Textures/Entity/animationtest"));
            textures.Add("ghostSword", content.Load<Texture2D>("Textures/Entity/GhostSword"));
            textures.Add("ghostDagger", content.Load<Texture2D>("Textures/Entity/GhostDagger"));
            textures.Add("ghostAxe", content.Load<Texture2D>("Textures/Entity/GhostAxe"));
            textures.Add("ghostHalberd", content.Load<Texture2D>("Textures/Entity/GhostHalberd"));
            textures.Add("ghostHolyBlade", content.Load<Texture2D>("Textures/Entity/GhostHolyBlade"));
            textures.Add("charBase", content.Load<Texture2D>("Textures/Entity/Character/CharBase"));

            textures.Add("dragon1", content.Load<Texture2D>("Textures/Entity/Dragon1"));
            textures.Add("dragon1_eyes", content.Load<Texture2D>("Textures/Entity/Dragon1_Eyes"));
            textures.Add("dragon1_white", content.Load<Texture2D>("Textures/Entity/Dragon1_White"));

            //brush
            textures.Add("grass", content.Load<Texture2D>("Textures/Brush/Grass"));
            textures.Add("brick1", content.Load<Texture2D>("Textures/Brush/Brick1"));
            textures.Add("brickSide", content.Load<Texture2D>("Textures/Brush/BrickSide"));
            textures.Add("brickTop", content.Load<Texture2D>("Textures/Brush/BrickTop"));
            textures.Add("chip_overlay1", content.Load<Texture2D>("Textures/Brush/Chip_overlay1"));
            textures.Add("vine_overlay1", content.Load<Texture2D>("Textures/Brush/Vine_overlay1"));

            //prop
            textures.Add("tree1", content.Load<Texture2D>("Textures/Prop/Tree1"));
            textures.Add("torch_animated", content.Load<Texture2D>("Textures/Prop/torch_animated"));

            //gui
            textures.Add("brush", content.Load<Texture2D>("Textures/Gui/Brush"));
            textures.Add("wall", content.Load<Texture2D>("Textures/Gui/Wall"));
            textures.Add("entity", content.Load<Texture2D>("Textures/Gui/Entity"));
            textures.Add("prop", content.Load<Texture2D>("Textures/Gui/Prop"));
            textures.Add("trigger", content.Load<Texture2D>("Textures/Gui/Trigger"));
            textures.Add("arrowUp", content.Load<Texture2D>("Textures/Gui/ArrowUp"));
            textures.Add("arrowDown", content.Load<Texture2D>("Textures/Gui/ArrowDown"));
            textures.Add("checkbox", content.Load<Texture2D>("Textures/Gui/CheckBox"));
            textures.Add("select", content.Load<Texture2D>("Textures/Gui/Select"));

            //fonts
            this.fonts.Add("munro12", content.Load<SpriteFont>("Fonts/munro12"));
            this.fonts.Add("munro8", content.Load<SpriteFont>("Fonts/munro8"));
            this.fonts.Add("bfMunro12", content.Load<SpriteFont>("Fonts/bitfontMunro12"));
            this.fonts.Add("bfMunro8", content.Load<SpriteFont>("Fonts/bitfontMunro8"));
            this.fonts.Add("bfMunro23_bold", content.Load<SpriteFont>("Fonts/bitfontMunro23BOLD"));
        }
    }
}
