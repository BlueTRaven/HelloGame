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

        protected TextureInfo[] texInfos;
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

        public int index;

        protected Action<Entity, Vector2, TextureInfo>[] animations;
        protected Vector2 targetPos;
        public Entity()
        {
            texInfos = new TextureInfo[8];
            animations = new Action<Entity, Vector2, TextureInfo>[8];
        }

        public virtual void OnSpawn(World world, Vector2 position)
        {
            this.position = position;
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

            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i] != null)
                {
                    animations[i]?.Invoke(this, targetPos, texInfos[i]);
                }
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

        public virtual void Die(World world, bool force = false, bool dropItems = true)
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

        public TextureInfo GetPlayerCharacterTexInfo(string characterTextureFile, Vector2 scale)
        {
            return new TextureInfo(new TextureContainer(characterTextureFile), scale, Color.White).SetAnimated(8, 8,
                new Animation(8, 8, 8, 1, true, false, new int[] { 2 }),            //0. idle
                new Animation(0, 8, 8, 2, true, false, new int[] { 15, 15 }),       //1. walk right
                new Animation(0, 8, 8, 2, true, true, new int[] { 15, 15 }),        //2. walk left
                new Animation(8, 8, 8, 3, true, false, new int[] { 15, 15, 15 }),   //3. walk down
                new Animation(16, 8, 8, 3, true, false, new int[] { 15, 15, 15 }),  //4. walk up
                new Animation(24, 8, 8, 1, true, false, new int[] { 2 }),           //5. jump right
                new Animation(24, 8, 8, 1, true, true, new int[] { 2 }),            //6. jump left
                new Animation(32, 8, 8, 1, true, false, new int[] { 2 }),           //7. jump down
                new Animation(40, 8, 8, 1, true, false, new int[] { 2 }));          //8. jump up
        }

        public int GetFacingDirection(Vector2 focusTarget)
        {
            float angle = VectorHelper.GetAngleBetweenPoints(position, focusTarget);
            if (angle <= 45 || angle > 315)
                return 0;   //left
            else if (angle > 45 && angle <= 135)
                return 1;   //up
            else if (angle > 135 && angle <= 225)
                return 2;   //right
            else if (angle > 225 && angle <= 315)
                return 3;   //down
            return 0;
        }
    }
}
