using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Globalization;
using HelloGame.Utility;

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
            whitePixel = new Texture2D(device, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });
            textures.Add("whitePixel", whitePixel);

            DirectoryInfo d = new DirectoryInfo("Content");
            List<FileInfo> files = d.GetFiles("*", SearchOption.AllDirectories).ToList();

            foreach (FileInfo file in files)
            {
                string name = file.Name.Split('.')[0];

                string keyname = TextHelper.FirstCharacterToLower(name);

                string[] split = file.DirectoryName.Split('\\');
                int index = split.ToList().IndexOf("Content");
                string dirName = split[index + 1];

                string directory = "";
                for (int i = index + 1; i < split.Length; i++)
                {   //super efficency
                    directory += split[i] + '/';
                }

                if (dirName == "Fonts")
                {
                    if (!fonts.ContainsKey(keyname))
                        fonts.Add(keyname, content.Load<SpriteFont>(directory + name));
                }
                else if (dirName == "Textures")
                {
                    if (!textures.ContainsKey(keyname)) 
                        textures.Add(keyname, content.Load<Texture2D>(directory + name));
                }
            }
            
            /*textures.Add("shadowPixel", content.Load<Texture2D>("Textures/ShadowPixel"));
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
            textures.Add("brickFloor", content.Load<Texture2D>("Textures/Brush/BrickFloor"));
            textures.Add("chip_overlay1", content.Load<Texture2D>("Textures/Brush/Chip_overlay1"));
            textures.Add("vine_overlay1", content.Load<Texture2D>("Textures/Brush/Vine_overlay1"));
            textures.Add("tileFloor1", content.Load<Texture2D>("Textures/Brush/TileFloor1"));
            textures.Add("tileFloor2", content.Load<Texture2D>("Textures/Brush/TileFloor2"));
            textures.Add("tileFloor3", content.Load<Texture2D>("Textures/Brush/TileFloor3"));

            //prop
            textures.Add("tree1", content.Load<Texture2D>("Textures/Prop/Tree1"));
            textures.Add("torch_animated", content.Load<Texture2D>("Textures/Prop/torch_animated"));

            //item
            textures.Add("keyItem", content.Load<Texture2D>("Textures/Item/KeyItem"));
            textures.Add("potionItem", content.Load<Texture2D>("Textures/Item/PotionItem"));

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
            this.fonts.Add("bfMunro72", content.Load<SpriteFont>("Fonts/bitfontMunro72"));*/
        }
    }
}
