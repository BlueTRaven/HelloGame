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

using Humper;
using Humper.Responses;

namespace HelloGame
{
    public class Wall : ISelectable
    {
        public IBox box;

        public Rectangle bounds;

        public bool noSave;

        public Wall(World world, Rectangle bounds)
        {
            box = world.collisionWorld.Create(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            this.bounds = bounds;
        }

        public void Delete(World world)
        {
            world.collisionWorld.Remove(box);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.DrawRectangle(new Rectangle(bounds.X, bounds.Y, bounds.Width + 64, bounds.Height + 64), new Color(Color.Black, 63), 0);
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.Red);
            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), "WALL", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);
        }

        public SerWall Save()
        {
            return new SerWall()
            {
                Bounds = bounds.Save()
            };
        }

        #region selectable
        public int index { get; set; }

        public bool trueSelected { get; set; }

        public void DrawSelect_DEBUG(SpriteBatch batch)
        {
            if (!trueSelected)
                batch.DrawHollowRectangle(bounds, 4, Color.White);
            else batch.DrawHollowRectangle(bounds, 4, Color.Black);
        }

        public void Delete_DEBUG(World world)
        {
            Delete(world);
            world.walls[index] = null;
        }

        public void UpdateWindowProperties(WidgetWindowEditProperties window)
        {
            window.mode = 1;
            window.selected = this;

            WidgetWindowRectangle rect = window.GetWindow<WidgetWindowRectangle>("wall_bounds");
            rect.Set(bounds);
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            bounds = window.GetWindow<WidgetWindowRectangle>("wall_bounds").GetRectangle();
        }

        public void Move(Vector2 amt)
        {
            box.Move(bounds.Location.X + amt.X, bounds.Location.Y + amt.Y, (collision) => CollisionResponses.None);
            bounds = new Rectangle(bounds.Location + amt.ToPoint(), bounds.Size);
            WidgetWindowRectangle rect = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").GetWindow<WidgetWindowRectangle>("wall_bounds");
            rect.Set(bounds);
        }
#endregion
    }
}
