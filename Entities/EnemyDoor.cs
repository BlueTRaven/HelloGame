using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Guis;
using HelloGame.Guis.Widgets;
using HelloGame.Hits;
using Humper;

namespace HelloGame.Entities
{
    public class EnemyDoor : Enemy
    {
        private float doorHeight, obscureHeight;
        private bool left;

        public bool opening;

        private float xmoved;

        public EnemyDoor(World world, float doorHeight, float obscureHeight, bool left = true) : base(new Vector2(64, 32), 1, 0, EnemyNoticeState.Sleeping, 0)
        {
            this.doorHeight = doorHeight;
            this.obscureHeight = obscureHeight;
            this.left = left;

            this.shadowScale = 0;   //don't need to draw a shadow
            this.noclip = true;
            this.drawHealthBar = false;
        }

        public override void PreUpdate(World world)
        {
            base.PreUpdate(world);

            invulnerableTime = 2;

            if (Main.keyboard.KeyPressed(Keys.X))
                opening = true;

            if (opening)
            {
                float wanty = initialPosition.Y - 32;
                float wantx = initialPosition.X - (left ? 128 : -128);

                if (position.Y != wanty)
                {
                    position.Y--;
                    obscureHeight--;
                }
                else
                {
                    if (xmoved < 128)
                    //if (position.X != wantx)
                    {
                        xmoved++;
                        position.X -= left ? 1 : -1;
                    }
                    else
                    {
                        Die(world);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            if (draw)
            {
                Rectangle rect = new Rectangle((int)(position.X - 32), (int)(position.Y - doorHeight) + 16, 64, (int)doorHeight);
                batch.DrawRectangle(new Rectangle(rect.X + (left ? (int)xmoved : 0), (int)position.Y + 16, (int)(64 - (xmoved > 64 ? 64 : xmoved)), 64), new Color(Color.Black, 63), 0);
                batch.DrawRectangle(rect, Color.White, Main.GetDepth(position));

                rect = new Rectangle((int)(position.X - 32), (int)(position.Y - (doorHeight + obscureHeight)) + 16, 64, (int)obscureHeight);
                batch.DrawRectangle(rect, Color.Black, 1);
            }

            if (Main.DEBUG)
            {
                batch.DrawHollowRectangle(collideBox.Bounds.ToRectangle(), 2, Color.Red);
            }
        }
    }
}
