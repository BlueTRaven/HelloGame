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
    public abstract class Hit
    {
        protected List<EntityLiving> hittables;
        public IDamageDealer parent;

        public bool done;
        public int duration;
        public readonly int durationMax;

        public int damage;
        public float knockback;
        public StaggerType type;

        protected List<IDamageTaker> hit;

        public Vector2 center;

        public int delay;

        public Hit(int maxDuration, int damage, float knockback, StaggerType type, IDamageDealer parent, List<EntityLiving> hittables = null)
        {
            this.parent = parent;
            this.duration = maxDuration;
            this.durationMax = maxDuration;

            this.damage = damage;
            this.knockback = knockback;
            this.type = type;

            this.hittables = hittables;

            hit = new List<IDamageTaker>();
        }

        public Hit Delay(int delay)
        {
            this.delay = delay;
            return this;
        }

        public virtual void PreUpdate()
        {

        }

        public virtual void Update()
        {
            if (delay > 0)
                delay--;
            else
            {
                duration--;

                if (duration <= 0)
                    done = true;
            }
        }

        public virtual void PostUpdate()
        {

        }

        public virtual bool Collided(IDamageTaker check)
        {
            if (delay > 0)
                return false;

            if ((check is Player && parent is Enemy) || parent == null)
            {   //if we're hitting the player as an enemy or null, go on...
                if (!hit.Contains(check) && (parent == null || check != parent))
                {
                    if (Collision(check))
                    {
                        hit.Add(check);
                        return true;
                    }
                }
            }
            else if (parent is Player && check is Enemy)
            {   //else, we're the player hitting an enemy. Always hit.
                if (!hit.Contains(check) && (parent == null || check != parent))
                {
                    if (Collision(check))
                    {
                        hit.Add(check);
                        return true;
                    }
                }
            }
            else if (hittables != null && hittables.Contains(check))
            {   //finally, we're an enemy hitting another enemy. In which case, we only want to hit the enemy if it's in the hittables list.
                if (!hit.Contains(check) && (parent == null || check != parent))
                {
                    if (Collision(check))
                    {
                        hit.Add(check);
                        return true;
                    }
                }
            }
            return false;
        }

        public abstract void Draw_DEBUG(SpriteBatch batch);

        protected abstract bool Collision(IDamageTaker check);

        public abstract Vector2 GetHitDirection(IDamageTaker taker);
    }
}
