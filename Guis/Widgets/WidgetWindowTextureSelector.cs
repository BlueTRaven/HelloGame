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
    public class WidgetWindowTextureSelector : WidgetWindow
    {
        bool animated;
        public WidgetWindowTextureSelector(Vector2 topleft) : base(new Rectangle(topleft.ToPoint(), new Point(128, 256)), false, null)
        {
            backgroundColor = Color.Black;
            WidgetTextBox name = AddWidget("name", new WidgetTextBox(new Rectangle(8, 8, 56, 24), Main.assets.GetFont("bitfontMunro8"), "name", 32, Enums.Alignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox R = AddWidget("R", new WidgetTextBox(new Rectangle(8, 40, 56, 24), Main.assets.GetFont("bitfontMunro12"), "R", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox G = AddWidget("G", new WidgetTextBox(new Rectangle(8, 72, 56, 24), Main.assets.GetFont("bitfontMunro12"), "G", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox B = AddWidget("B", new WidgetTextBox(new Rectangle(8, 104, 56, 24), Main.assets.GetFont("bitfontMunro12"), "B", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox A = AddWidget("A", new WidgetTextBox(new Rectangle(8, 136, 56, 24), Main.assets.GetFont("bitfontMunro12"), "A", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("textureshow_nomodify", new WidgetButton(new Rectangle(72, 8, 48, 48))).SetHasTexture(Main.assets.GetTexture("whitePixel")).SetBackgroundColor(Color.White, Color.White, Color.White, Color.White, Color.White);

            WidgetTextBox scaleX = AddWidget("scale_x", new WidgetTextBox(new Rectangle(72, 72, 48, 24), Main.assets.GetFont("bitfontMunro12"), "X", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox scaleY = AddWidget("scale_y", new WidgetTextBox(new Rectangle(72, 104, 48, 24), Main.assets.GetFont("bitfontMunro12"), "Y", 3, Enums.Alignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);

            WidgetCheckbox mirrorH = AddWidget("mirror_h", new WidgetCheckbox(new Rectangle(72, 136, 24, 24), Color.White));
            WidgetCheckbox mirrorV = AddWidget("mirror_v", new WidgetCheckbox(new Rectangle(96, 136, 24, 24), Color.White));

            //WidgetDropdown mirror = AddWidget("mirror", new WidgetDropdown(new Rectangle(72, 136, 48, 24), Main.assets.GetFont("bitfontMunro12"), 
            //"mirror", Color.White, Enums.Alignment.Left, 3, "None", "Horizontally", "Vertically"));

            WidgetDropdown rotate = AddWidget("rotate", new WidgetDropdown(new Rectangle(72, 168, 48, 16), Main.assets.GetFont("bitfontMunro8"),
                "rotate", Color.White, Enums.Alignment.Left, 4, "0", "90", "180", "270"));

            name.SetTabProgressesTo(R); //wew
            R.SetTabProgressesTo(G);
            G.SetTabProgressesTo(B);
            B.SetTabProgressesTo(A);
            A.SetTabProgressesTo(scaleX);
            scaleX.SetTabProgressesTo(scaleY);
            scaleY.SetTabProgressesTo(name);
        }

        public override void Update()
        {
            base.Update();

            if (GetWidget<WidgetTextBox>("name").state == 4 ||
                GetWidget<WidgetTextBox>("R").state == 4 ||
                GetWidget<WidgetTextBox>("G").state == 4 ||
                GetWidget<WidgetTextBox>("B").state == 4 ||
                GetWidget<WidgetTextBox>("A").state == 4)
            {
                Texture2D texture = Main.assets.GetTexture(GetWidget<WidgetTextBox>("name").text.ToString());
                Color brushColor = GetColor();
                if (texture != null)
                {
                    GetWidget<WidgetButton>("textureshow_nomodify").SetHasTexture(texture).SetTextureColors(brushColor, brushColor, brushColor, brushColor, brushColor);
                }
                else GetWidget<WidgetButton>("textureshow_nomodify").SetHasTexture(Main.assets.GetTexture("whitePixel")).SetBackgroundColor(brushColor, brushColor, brushColor, brushColor, brushColor);
            }
        }

        public string GetTexture()
        {
            return GetWidget<WidgetTextBox>("name").text.ToString();
        }

        public Vector2 GetScale()
        {
            Vector2 scale = Vector2.One;
            WidgetTextBox widget = GetWidget<WidgetTextBox>("scale_x");
            scale.X = float.Parse(widget.GetStringSafely("1"));
            widget = GetWidget<WidgetTextBox>("scale_y");
            scale.Y = float.Parse(widget.GetStringSafely("1"));

            return scale;
        }

        public Color GetColor()
        {
            Color color = Color.White;
            WidgetTextBox widget = GetWidget<WidgetTextBox>("R");
            color.R = (byte)(int.Parse(widget.GetStringSafely(color.R.ToString())) % 256);
            widget = GetWidget<WidgetTextBox>("G");
            color.G = (byte)(int.Parse(widget.GetStringSafely(color.G.ToString())) % 256);
            widget = GetWidget<WidgetTextBox>("B");
            color.B = (byte)(int.Parse(widget.GetStringSafely(color.B.ToString())) % 256);
            widget = GetWidget<WidgetTextBox>("A");
            color.A = (byte)(int.Parse(widget.GetStringSafely(color.A.ToString())) % 256);

            return color;
        }

        public bool GetIsAnimated()
        {
            return GetWidget<WidgetCheckbox>("isanimated").isChecked;
        }

        public int GetAnimatedFrameHeight()
        {
            try
            {
                return Main.assets.GetTexture(GetTexture()).Height / int.Parse(GetWidget<WidgetTextBox>("frames").GetStringSafely());
            }
            catch { return 0; }
        }

        public TextureInfo GetTextureInfo()
        {
            return new TextureInfo(new TextureContainer(GetTexture()), GetScale(), GetColor(), GetRotation(), GetSpriteEffects());
        }

        public SpriteEffects GetSpriteEffects()
        {
            SpriteEffects done = SpriteEffects.None;
            if (GetWidget<WidgetCheckbox>("mirror_h").isChecked)
                done |= SpriteEffects.FlipHorizontally;
            if (GetWidget<WidgetCheckbox>("mirror_v").isChecked)
                done |= SpriteEffects.FlipVertically;
            return done;
            //return (SpriteEffects)GetWidget<WidgetDropdown>("mirror").GetIndex();
        }

        public float GetRotation()
        {
            return GetWidget<WidgetDropdown>("rotate").GetIndex() * 90;
        }

        public void Set(TextureInfo texInfo)
        {
            GetWidget<WidgetTextBox>("name").SetString(texInfo.texture.name);
            GetWidget<WidgetTextBox>("R").SetString(texInfo.tint.R.ToString());
            GetWidget<WidgetTextBox>("G").SetString(texInfo.tint.G.ToString());
            GetWidget<WidgetTextBox>("B").SetString(texInfo.tint.B.ToString());
            GetWidget<WidgetTextBox>("A").SetString(texInfo.tint.A.ToString());

            GetWidget<WidgetTextBox>("scale_x").SetString(texInfo.scale.X.ToString());
            GetWidget<WidgetTextBox>("scale_y").SetString(texInfo.scale.Y.ToString());

            GetWidget<WidgetButton>("textureshow_nomodify").SetHasTexture(texInfo.texture.texture).SetTextureColors(texInfo.tint, texInfo.tint, texInfo.tint, texInfo.tint, texInfo.tint);

            GetWidget<WidgetCheckbox>("mirror_h").isChecked = (texInfo.mirror & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;  //I really don't know if this will work
            GetWidget<WidgetCheckbox>("mirror_v").isChecked = (texInfo.mirror & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            //GetWidget<WidgetDropdown>("mirror").SetIndex((int)texInfo.mirror);

            GetWidget<WidgetDropdown>("rotate").SetIndex((int)(texInfo.rotation / 90)); //this may not work, but since this should always be a multiple of 90, it should
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            batch.DrawHollowRectangle(bounds, 2, Color.Red);
        }
    }
}
