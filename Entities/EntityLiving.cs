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
using Humper.Responses;

namespace HelloGame.Entities
{
    public enum StaggerType
    {
        None,
        Short,
        Medium,
        Long,
        KnockbackGetup,
        KnockdownGetup
    }

    public abstract class EntityLiving : Entity, IDamageDealer, IDamageTaker
    {
        public IMovement lastMovement;
        public IBox collideBox;
        private Vector2 hitboxSize;

        public Rectangle hitbox { get; set; }
        public Vector2 initialPosition;

        public int health { get; protected set; }
        public int maxHealth { get; protected set; }

        public bool invulnerable;
        protected int invulnerableTime;

        public bool staggered;
        protected int staggerTime;

        private FixedSizedQueue<int> prevHealth;
        protected FixedSizedQueue<int> damageSustained;

        public EntityLiving(Vector2 hitboxSize) : base()
        {
            this.hitboxSize = hitboxSize;
            prevHealth = new FixedSizedQueue<int>(4);
            damageSustained = new FixedSizedQueue<int>(4);   //record the last 4 seconds of damage sustained
        }

        public override void OnSpawn(EntitySpawner spawner, World world, Vector2 position)
        {
            base.OnSpawn(spawner, world, position);

            this.collideBox = world.collisionWorld.Create(0, 0, hitboxSize.X, hitboxSize.Y);
            SetPosition(position);
        }

        public override void PreUpdate(World world)
        {
            base.PreUpdate(world);

            if (alive % 60 == 0)
            {
                prevHealth.Enqueue(health);
                damageSustained.Enqueue(prevHealth.Peek() - health);
            }

            if (invulnerableTime > 0)
            {
                invulnerable = true;
                invulnerableTime--;
            }
            else invulnerable = false;

            if (staggerTime > 0)
            {
                staggered = true;
                staggerTime--;
            }
            else staggered = false;
        }

        public override void Update(World world)
        {
            base.Update(world);
        }

        public override void Move()
        {
            velocity = Vector2.Clamp(velocity, new Vector2(-maxSpeed), new Vector2(maxSpeed));

            if (canMove)
            {
                lastMovement = collideBox.Move((position.X - (collideBox.Width / 2)) + velocity.X, (position.Y - (collideBox.Height / 2)) + velocity.Y, (collision) => noclip ? CollisionResponses.None : CollisionResponses.Slide);
                position = lastMovement.ToVector2() + new Vector2((collideBox.Width / 2), (collideBox.Height / 2));
                hitbox = collideBox.Bounds.ToRectangle();
            }
        }

        public void SetPosition(Vector2 position, bool soft = false)
        {
            this.position = position;

            if (!soft) this.initialPosition = position;

            collideBox.Move(position.X, position.Y, (collision) => CollisionResponses.None);
        }

        public virtual bool TakeDamage(World world, int amt, StaggerType type, Vector2 direction = new Vector2() { X = 0, Y = 0 })
        {
            if (!invulnerable)
            {
                velocity = direction / 2;
                this.staggerTime = GetStaggerTime(type);
                health -= amt;
                if (health <= 0)
                {
                    Die(world);
                    return true;
                }
            }
            return false;
        }

        protected int GetStaggerTime(StaggerType type)
        {
            switch (type)
            {
                case (StaggerType.Short):
                    return 30;
                case StaggerType.Medium:
                    return 60;
                case StaggerType.Long:
                    return 95;
                case StaggerType.KnockbackGetup:
                    invulnerableTime = 130;
                    return 150;
                case StaggerType.KnockdownGetup:
                    invulnerableTime = 130;
                    return 150;
            }

            return 0;
        }

        public override void Die(World world, bool force = false, bool dropItems = true)
        {
            base.Die(world, force, dropItems);

            world.collisionWorld.Remove(collideBox);
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            if (texInfos != null && draw)
            {
                if (shadowScale > 0)
                    batch.Draw(Main.assets.GetTexture("shadow"), position, null, new Color(0, 0, 0, 63), 0, new Vector2(32, 16), shadowScale, SpriteEffects.None, Main.GetDepth(new Vector2(position.X, position.Y - 1)));
                for (int i = 0; i < texInfos.Length; i++)
                {
                    if (texInfos[i] != null)
                    {
                        batch.Draw(texInfos[i].texture.texture, position + Main.camera.up * height, texInfos[i].sourceRect, texInfos[i].tint, MathHelper.ToRadians(visualRotation), 
                            texInfos[i].sourceRect.HasValue ? texInfos[i].offset 
                            : new Vector2(texInfos[i].texture.texture.Width / 2, texInfos[i].texture.texture.Height), 
                            texInfos[i].scale, texInfos[i].GetSpriteEffects(), Main.GetDepth(new Vector2(position.X, position.Y - i * .5f)));
                    }
                }
            }

            if (Main.DEBUG)
            {
                batch.DrawHollowRectangle(collideBox.Bounds.ToRectangle(), 2, Color.Red);
            }
        }
    }
}
