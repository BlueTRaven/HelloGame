using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;

using Humper;
using Humper.Responses;

namespace HelloGame.Entities
{
    public abstract class Entity : IDamageDealer
    {
        public Vector2 position, velocity;

        public float maxSpeed;
        public bool dead;

        public float groundFriction = .8f;

        protected TextureInfo texInfo;
        protected bool draw = true;
        protected float shadowScale = 1;

        protected bool noclip;

        public bool canMove = true;
        public bool velocityDecays = true;
        public int velocityDecaysTimer; //if time > 0, velocity DOES NOT DECAY

        public float gravity;
        public float height;
        public float zVel;
        public float maxZVel;
        protected Action<Entity> groundCollideAction;

        protected int alive;

        public Entity()
        {
        }

        public virtual void PreUpdate(World world) { alive++; }

        public virtual void Update(World world)
        {
            zVel -= gravity;

            if (zVel > maxZVel)
                zVel = maxZVel;
            else if (zVel < -maxZVel)
                zVel = -maxZVel;

            height += zVel;

            if (velocityDecaysTimer > 0)
            {
                velocityDecaysTimer--;
                velocityDecays = false;
            }
            else velocityDecays = true;

            if (height <= 0)
            {
                groundCollideAction?.Invoke(this);
                height = 0;
            }

            Move();
        }

        public virtual void Move()
        {
            velocity = Vector2.Clamp(velocity, new Vector2(-maxSpeed), new Vector2(maxSpeed));

            if (canMove)
            {
                position += velocity;
            }
        }

        public virtual void PostUpdate(World world)
        {
            if (velocityDecays)
                velocity *= groundFriction;
        }

        public virtual void Die(World world)
        {
            dead = true;
        }

        public virtual void Draw(SpriteBatch batch)
        {
            if (Main.DEBUG)
            {
                batch.DrawHollowCircle(position, 8, Color.Red);
            }
        }
    }
}
