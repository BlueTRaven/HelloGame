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

        public bool canMove = true, velocityDecays = true;

        public Entity()
        {
        }

        public virtual void PreUpdate(World world) { }

        public virtual void Update(World world)
        {
            if (canMove)
            {
                velocity = Vector2.Clamp(velocity, new Vector2(-maxSpeed), new Vector2(maxSpeed));
                position += velocity;
            }
        }

        public virtual void PostUpdate(World world)
        {
            if (velocityDecays)
                velocity *= groundFriction;
        }

        protected virtual void Die(World world)
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
