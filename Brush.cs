using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Guis.Widgets;

namespace HelloGame
{
    public enum BrushDepth
    {
        DrawDiffered,
        DrawWorldDepth,
        DrawTop
    }

    public enum BrushDrawType
    {
        Tile,
        Stretch
    }

    public class Brush : ISelectable
    {
        public Rectangle bounds, originBounds;

        public TextureInfo texInfo;

        public BrushDrawType drawType;
        public BrushDepth depth;

        public Brush (Rectangle bounds, TextureInfo info, BrushDrawType drawType = BrushDrawType.Tile, BrushDepth depth = BrushDepth.DrawDiffered)
        {
            this.bounds = bounds;
            this.originBounds = bounds;
            this.texInfo = info;

            this.drawType = drawType;
            this.depth = depth;
        }

        public void Draw(SpriteBatch batch)
        {
            if (drawType == BrushDrawType.Tile)
            {
                batch.Draw(texInfo.texture.texture, bounds.Location.ToVector2(), new Rectangle(Point.Zero, (bounds.Size.ToVector2() / texInfo.scale).ToPoint()), texInfo.tint, 0, Vector2.Zero, texInfo.scale, SpriteEffects.None, GetDrawDepth(depth));
            }
            else if (drawType == BrushDrawType.Stretch)
            {   //scale is pointless on stretch mode.
                batch.Draw(texInfo.texture.texture, bounds, null, texInfo.tint, 0, Vector2.Zero, SpriteEffects.None, GetDrawDepth(depth));
            }
            /*else if (drawType == BrushDrawType.WallTile)
            {
                batch.Draw(texInfo.texture.texture, bounds.Location.ToVector2(), new Rectangle(Point.Zero, (bounds.Size.ToVector2() / texInfo.scale).ToPoint()), texInfo.tint, 0, Vector2.Zero, texInfo.scale, SpriteEffects.None, Main.GetDepth(new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height)));
            }
            else if (drawType == BrushDrawType.WallStretch)
            {
                batch.Draw(texInfo.texture.texture, bounds, null, texInfo.tint, 0, Vector2.Zero, SpriteEffects.None, Main.GetDepth(new Vector2(bounds.Width / 2, bounds.Height)));
            }*/
        }

        public float GetDrawDepth(BrushDepth depth)
        {
            if (texInfo.texture.name == "shadowPixel" && depth != BrushDepth.DrawTop)
                Console.WriteLine("[Warning] Brush with shadowPixel not set to brush depth DrawTop. This may cause draw order issues.");

            if (depth == BrushDepth.DrawDiffered)
                return 0;
            else if (depth == BrushDepth.DrawWorldDepth)
                return Main.GetDepth(new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height));
            else if (depth == BrushDepth.DrawTop)
            {
                if (texInfo.texture.name != "shadowPixel")
                    return 1;
                else return .2f;
            }
            return 0;
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.Red);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), "BRUSH", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), "Depth: " + GetDrawDepth(depth), bounds.Location.ToVector2(), Color.White);
        }

        public void DrawSelect_DEBUG(SpriteBatch batch)
        {
            if (!trueSelected)
                batch.DrawHollowRectangle(bounds, 4, Color.White);
            else batch.DrawHollowRectangle(bounds, 4, Color.Black);
        }

        public SerBrush Save()
        {
            return new SerBrush()
            {
                Bounds = bounds.Save(),
                DrawDepth = (int)depth,
                DrawType = (int)drawType,
                TextureInfo = texInfo.Save(),
            };
        }

        #region selectable
        public int index { get; set; }

        public bool trueSelected { get; set; }

        public void Delete_DEBUG(World world)
        {
            world.brushes[index] = null;
        }

        public void UpdateWindowProperties(WidgetWindowEditProperties window)
        {
            window.mode = 0;
            window.selected = this;

            WidgetWindowRectangle rect = window.GetWindow<WidgetWindowRectangle>("brush_bounds");
            rect.Set(bounds);
            WidgetWindowTextureSelector tex = window.GetWindow<WidgetWindowTextureSelector>("brush_texture");
            tex.Set(texInfo);

            window.GetWidget<WidgetDropdown>("brush_type").SetIndex((int)drawType);
            window.GetWidget<WidgetDropdown>("brush_depth").SetIndex((int)drawType);
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            bounds = window.GetWindow<WidgetWindowRectangle>("brush_bounds").GetRectangle();
            texInfo = window.GetWindow<WidgetWindowTextureSelector>("brush_texture").GetTextureInfo();
            //string text = window.GetWidget<WidgetTextBox>("brush_type").GetStringSafely();
            //drawType = (BrushDrawType)int.Parse(text);
            drawType = (BrushDrawType)window.GetWidget<WidgetDropdown>("brush_type").GetIndex();
            depth = (BrushDepth)window.GetWidget<WidgetDropdown>("brush_depth").GetIndex();
            //drawAhead = window.GetWidget<WidgetCheckbox>("brush_drawahead").isChecked;
        }

        public void Move(Vector2 amt)
        {
            bounds = new Rectangle(bounds.X + (int)amt.X, bounds.Y + (int)amt.Y, bounds.Width, bounds.Height);
            WidgetWindowRectangle rect = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").GetWindow<WidgetWindowRectangle>("brush_bounds");
            rect.Set(bounds);
        }
        #endregion
    }
}
