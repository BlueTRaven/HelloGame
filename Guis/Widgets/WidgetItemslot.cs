using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Items;

namespace HelloGame.Guis.Widgets
{
    public class WidgetItemslot : WidgetButton
    {
        private TextureInfo texture;

        private Item item;

        public WidgetItemslot(Vector2 position, Item item) : base(new Rectangle((int)position.X, (int)position.Y, 384, 24))
        {
            SetBackgroundColor(Color.White, Color.LightGray, Color.Gray, Color.Gray, Color.White);

            this.item = item;
            texture = new TextureInfo(new TextureContainer(Item.GetTextureName(item)), Vector2.One, Color.White);
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            batch.DrawRectangle(bounds, Color.White);
            batch.Draw(texture.texture.texture, new Rectangle(bounds.Location, new Point(24, 24)), Color.White);

            Rectangle postBounds = new Rectangle(bounds.Location + new Point(24, 0), new Point(384 - 24, 24));

            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), item.name, postBounds.Location.ToVector2() +     //Item name stuff
                TextHelper.GetAlignmentOffset(Main.assets.GetFont("bitfontMunro12"), item.name, postBounds, Enums.Alignment.Left), Color.White);
            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), "x" + item.count, postBounds.Location.ToVector2() +     //Item name stuff
                TextHelper.GetAlignmentOffset(Main.assets.GetFont("bitfontMunro12"), "x" + item.count, postBounds, Enums.Alignment.Right), Color.White);
        }
    }
}
