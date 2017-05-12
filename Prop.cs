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

namespace HelloGame
{
    public class Prop : ISelectable
    {
        public Vector2 position;
        public TextureInfo texInfo;
        public float shadowScale;

        public Prop(Vector2 position, TextureInfo texInfo, float shadowScale)
        {
            this.position = position;
            this.texInfo = texInfo;
            this.shadowScale = shadowScale;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Main.assets.GetTexture("shadow"), position, null, new Color(0, 0, 0, 63), 0, new Vector2(32, 16), shadowScale, SpriteEffects.None, Main.GetDepth(new Vector2(position.X, position.Y - 1)));
            batch.Draw(texInfo.texture.texture, position, texInfo.sourceRect, texInfo.tint, 0, texInfo.sourceRect.HasValue ? new Vector2(texInfo.sourceRect.Value.Width / 2, texInfo.sourceRect.Value.Height) : new Vector2(texInfo.texture.texture.Width / 2, texInfo.texture.texture.Height), texInfo.scale, SpriteEffects.None, Main.GetDepth(position));
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowCircle(position, 8, Color.Red, 2, 32);
        }

        public void DrawSelect_DEBUG(SpriteBatch batch)
        {
            if (!trueSelected)
            batch.DrawHollowCircle(position, 8, Color.White, 4, 32);
            else batch.DrawHollowCircle(position, 8, Color.Black, 4, 32);
        }

        public SerProp Save()
        {
            return new SerProp()
            {
                Position = position.Save(),
                TexInfo = texInfo.Save(),
                ShadowScale = shadowScale
            };
        }

        #region selectable
        public int index { get; set; }

        public bool trueSelected { get; set; }

        public void Delete_DEBUG(World world)
        {
            world.props[index] = null;
        }

        public void UpdateWindowProperties(WidgetWindowEditProperties window)
        {
            window.mode = 3;
            window.selected = this;

            WidgetWindowVector2 vec = window.GetWindow<WidgetWindowVector2>("prop_position");
            vec.Set(position);
            WidgetWindowTextureSelector tex = window.GetWindow<WidgetWindowTextureSelector>("prop_texture");
            tex.Set(texInfo);
            WidgetTextBox text = window.GetWidget<WidgetTextBox>("prop_shadowscale");
            text.SetString(shadowScale.ToString());
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            position = window.GetWindow<WidgetWindowVector2>("prop_position").GetVector2();
            texInfo = window.GetWindow<WidgetWindowTextureSelector>("prop_texture").GetTextureInfo();
            shadowScale = float.Parse(window.GetWidget<WidgetTextBox>("prop_shadowscale").GetStringSafely());
        }

        public void Move(Vector2 amt)
        {
            position += amt;
            WidgetWindowVector2 rect = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").GetWindow<WidgetWindowVector2>("prop_position");
            rect.Set(position);
        }
#endregion
    }
}
