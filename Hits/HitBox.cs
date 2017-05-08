using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HelloGame.Utility;
using HelloGame.Entities;

namespace HelloGame.Hits
{
    public class HitBox : Hit
    {
        public Rectangle bounds;

        public HitBox(Rectangle bounds, int maxDuration, int damage, float knockback, StaggerType type, IDamageDealer parent, List<EntityLiving> hittables = null) : base(maxDuration, damage, knockback, type, parent, hittables)
        {
            this.bounds = bounds;
        }

        protected override bool Collision(IDamageTaker check)
        {
            if (check.hitbox.Contains(bounds) || bounds.Contains(check.hitbox) || bounds.Intersects(check.hitbox))
            {
                return true;
            }
            return false;
        }

        public override Vector2 GetHitDirection(IDamageTaker taker)
        {
            return Vector2.Normalize((taker.hitbox.Center - bounds.Center).ToVector2()) * knockback;
        }

        public override void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowCircle(bounds.Center.ToVector2(), 8, Color.Red);

            batch.DrawHollowRectangle(bounds, 2, Color.Red);
        }
    }
}
