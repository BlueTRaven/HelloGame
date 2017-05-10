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
    public class HitCircle : Hit
    {
        public float radius;

        public HitCircle(Vector2 center, float radius, int maxDuration, int damage, float knockback, StaggerType type, IDamageDealer parent, List<EntityLiving> hittables = null) : base(maxDuration, damage, knockback, type, parent, hittables)
        {
            this.center = center;
            this.radius = radius;
        }

        protected override bool Collision(IDamageTaker check)
        {
            if (delay <= 0)
            {
                Vector2 v = new Vector2(MathHelper.Clamp(center.X, check.hitbox.Left, check.hitbox.Right),
                                MathHelper.Clamp(center.Y, check.hitbox.Top, check.hitbox.Bottom));

                Vector2 direction = center - v;
                float distanceSquared = direction.LengthSquared();

                return ((distanceSquared > 0) && (distanceSquared < radius * radius));
            }
            return false;
        }

        public override Vector2 GetHitDirection(IDamageTaker taker)
        {
            return Vector2.Normalize(taker.hitbox.Center.ToVector2() - center) * knockback;
        }

        public override void Draw_DEBUG(SpriteBatch batch)
        {
            if (delay <= 0)
                batch.DrawHollowCircle(center, radius, Color.Red, 2, 32);
        }
    }
}
