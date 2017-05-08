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
    public class HitArc : Hit
    {
        public float radius;

        private bool animated;
        public bool clockwise { get; private set; }

        public float min { get; set; }
        public float max { get; set; }
        private float realMin, realMax, increasePerFrame, distanceFollow;

        public HitArc(Vector2 center, float radius, float min, float max, int maxDuration, int damage, float knockback, StaggerType type, IDamageDealer parent, List<EntityLiving> hittables = null) : base(maxDuration, damage, knockback, type, parent, hittables)
        {
            this.center = center;
            this.radius = radius;
            this.min = min;
            this.realMin = min;
            this.max = max;
            this.realMax = max;

            clockwise = true;
        }

        public override void Update()
        {
            base.Update();

            if (delay <= 0)
            {
                if (animated)
                {
                    if (clockwise ? max - min > distanceFollow : min - max > distanceFollow)
                        min += increasePerFrame;
                    max += increasePerFrame;
                }
            }
        }

        protected override bool Collision(IDamageTaker check)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(center.X, check.hitbox.Left, check.hitbox.Right),
                            MathHelper.Clamp(center.Y, check.hitbox.Top, check.hitbox.Bottom));

            float angle = VectorHelper.GetAngleBetweenPoints(center, v);

            if (clockwise)
            {
                if (!(angle > min && angle < max))
                {
                    return false;
                }
            }
            else
            {   //.....yeah. ok.
                if (!(angle > max && angle < min))
                {
                    return false;
                }
            }

            Vector2 direction = center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < radius * radius));
        }

        public HitArc SetAnimated(float distance, bool clockwise)
        {
            animated = true;
            this.clockwise = clockwise;
            if (!clockwise)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            distanceFollow = distance;
            increasePerFrame = (max - min) / (float)durationMax;

            max = min;
            return this;
        }

        public HitArc SetDistanced(float rotation, float distance)
        {
            int[] minmax = GetMinMax(rotation, distance);
            min = minmax[0];
            max = minmax[1];
            return this;
        }

        public override Vector2 GetHitDirection(IDamageTaker taker)
        {
            return Vector2.Normalize(taker.hitbox.Center.ToVector2() - center) * knockback;
        }

        public override void Draw_DEBUG(SpriteBatch batch)
        {
            if (delay > 0)
                return;
            Vector2 min = Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(this.min)));
            Vector2 max = Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(this.max)));
            batch.DrawLine(center, center + (min * radius), Color.Red, 2);
            batch.DrawLine(center, center + (max * radius), Color.Red, 2);
            batch.DrawLine(center + (min * radius), center + (max * radius), Color.Red, 2);
            batch.DrawHollowCircle(center, radius, Color.Red, 2, 32);
        }

        /// <summary>
        /// Get minimum and maximum values.
        /// </summary>
        /// <param name="rotation">the middle rotation.</param>
        /// <param name="distance">the total angle distance between min and max.</param>
        /// <returns></returns>
        public static int[] GetMinMax(float rotation, float distance)
        {
            int[] minmax = new int[2];

            minmax[0] = (int)(rotation - (distance / 2));
            minmax[1] = (int)(rotation + (distance / 2));

            return minmax;
        }
    }
}
