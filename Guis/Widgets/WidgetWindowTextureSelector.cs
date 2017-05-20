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
        public WidgetWindowTextureSelector(Vector2 topleft) : base(new Rectangle(topleft.ToPoint(), new Point(104,128)), false, null)
        {
            backgroundColor = Color.Black;
            WidgetTextBox name = AddWidget("name", new WidgetTextBox(new Rectangle(8, 8, 56, 24), Main.assets.GetFont("bfMunro12"), "name", 32, TextAlignment.Left, TextBoxFilter.AlphaNumeric)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox R = AddWidget("R", new WidgetTextBox(new Rectangle(8, 40, 24, 16), Main.assets.GetFont("bfMunro8"), "R", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox G = AddWidget("G", new WidgetTextBox(new Rectangle(8, 62, 24, 16), Main.assets.GetFont("bfMunro8"), "G", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox B = AddWidget("B", new WidgetTextBox(new Rectangle(8, 84, 24, 16), Main.assets.GetFont("bfMunro8"), "B", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox A = AddWidget("A", new WidgetTextBox(new Rectangle(8, 104, 24, 16), Main.assets.GetFont("bfMunro8"), "A", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            AddWidget("textureshow_nomodify", new WidgetButton(new Rectangle(40, 40, 32, 32))).SetHasTexture(Main.assets.GetTexture("whitePixel")).SetBackgroundColor(Color.White, Color.White, Color.White, Color.White, Color.White);

            WidgetTextBox scaleX = AddWidget("scale_x", new WidgetTextBox(new Rectangle(40, 84, 24, 16), Main.assets.GetFont("bfMunro8"), "X", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox scaleY = AddWidget("scale_y", new WidgetTextBox(new Rectangle(40, 104, 24, 16), Main.assets.GetFont("bfMunro8"), "Y", 3, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);

            name.SetTabProgressesTo(R); //wew
            R.SetTabProgressesTo(G);
            G.SetTabProgressesTo(B);
            B.SetTabProgressesTo(A);
            A.SetTabProgressesTo(scaleX);
            scaleX.SetTabProgressesTo(scaleY);
            scaleY.SetTabProgressesTo(name);

            AddWidget("isanimated", new WidgetCheckbox(new Rectangle(80, 84 - 24, 16, 16), Color.White));
            WidgetTextBox frames = AddWidget("frames", new WidgetTextBox(new Rectangle(72, 84, 24, 16), Main.assets.GetFont("bfMunro8"), "frames", 2, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);
            WidgetTextBox frameTime = AddWidget("frametime", new WidgetTextBox(new Rectangle(72, 104, 24, 16), Main.assets.GetFont("bfMunro8"), "time", 4, TextAlignment.Left, TextBoxFilter.Numerical)).SetBackgroundColor(Color.White, Color.Gray);

            frames.SetTabProgressesTo(frameTime);
            frameTime.SetTabProgressesTo(frames);
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
            if (GetIsAnimated())
            {
                int[] frames = new int[int.Parse(GetWidget<WidgetTextBox>("frames").GetStringSafely())];
                for (int i = 0; i < frames.Length; i++)
                    frames[i] = int.Parse(GetWidget<WidgetTextBox>("frametime").GetStringSafely());
                return new TextureInfo(new TextureContainer(GetTexture()), GetScale(), GetColor())
                      .SetAnimated(Main.assets.GetTexture(GetTexture()).Width, GetAnimatedFrameHeight(),
                      new Animation(0, Main.assets.GetTexture(GetTexture()).Width, GetAnimatedFrameHeight(),
                      int.Parse(GetWidget<WidgetTextBox>("frames").GetStringSafely()), false,
                      false, frames));
            }
            return new TextureInfo(new TextureContainer(GetTexture()), GetScale(), GetColor());
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

            GetWidget<WidgetTextBox>("frames").SetString(texInfo.GetFirstAnimationFrames().ToString());
            GetWidget<WidgetTextBox>("frametime").SetString(texInfo.GetFirstAnimationFrameDurations().ToString());
        }
    }
}
