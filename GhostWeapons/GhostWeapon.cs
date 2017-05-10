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
using HelloGame.Entities;
using HelloGame.Entities.Particles;

namespace HelloGame.GhostWeapons
{
    public abstract class GhostWeapon
    {
        protected Vector2 currentPosition;
        protected float currentRotation;

        protected int combo, comboMax;

        public GhostWeaponAttack attack { get; protected set; }

        private Texture2D texture;

        public int returnTime { get; protected set; }

        public float height;
        protected float restingHeight;

        public float scale = 2;
        private float scaleReturn = 2;

        private bool verticalDraw;
        private float vertDrawScale;

        public bool animationDone { get; private set; }
        public GhostWeapon(Texture2D texture)
        {
            this.texture = texture;
        }

        public virtual void Attack(IDamageDealer parent, int force = -1)
        {
            if (force != -1)
            {
                attack = GetNextAttack(force, parent);
                return;
            }

            if (attack != null)
            {
                if (attack.runTime > 0)
                    return;

                if (attack.resetTime > 0 && combo < comboMax - 1)
                {
                    combo++;
                }
                else combo = 0;
            }

            attack = GetNextAttack(combo, parent);
        }

        protected abstract GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent);

        public virtual void Update(World world, Vector2 parentCenter, Vector2 restingPosition, float restingRotation)
        {
            if (verticalDraw)
            {
                if (scale > 0)
                    scale -= .1f;
                
                if (vertDrawScale < .25f)
                    vertDrawScale += .01f;
            }
            else
            {
                if (vertDrawScale > 0)
                    vertDrawScale -= .01f;

                if (scale < scaleReturn)
                    scale += .1f;
            }

            if (attack != null)
            {
                attack.Update();

                if (attack.resetTime <= 0)
                { 
                    combo = 0;

                    attack = null;

                    returnTime = 60;

                    animationDone = true;
                }
                else
                {
                    animationDone = false;
                    attack.hit.center = parentCenter;
                    world.AddEntity(SpawnAnimationParticles(parentCenter));
                    attack.animation?.Invoke(parentCenter);
                }
            }
            else
            {
                animationDone = false;

                if (returnTime > 0)
                {
                    currentPosition = Vector2.Lerp(currentPosition, parentCenter + restingPosition, .1f);
                    currentRotation = MathHelper.Lerp(currentRotation, restingRotation, .1f);
                    height = MathHelper.Lerp(height, restingHeight, .1f);
                    returnTime--;
                }
                else
                {
                    currentPosition = parentCenter + restingPosition;
                    currentRotation = restingRotation;
                }
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Draw(Main.assets.GetTexture("shadow"), currentPosition, null, new Color(0, 0, 0, 127), 0, new Vector2(texture.Width / 2, texture.Height / 2), vertDrawScale, SpriteEffects.None, Main.GetDepth(currentPosition));
            batch.Draw(texture, currentPosition, null, new Color(0, 0, 0, 127), MathHelper.ToRadians(currentRotation), new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, Main.GetDepth(currentPosition));
            batch.Draw(texture, currentPosition + (Main.camera.up * height), null, Color.White, MathHelper.ToRadians(currentRotation), new Vector2(texture.Width / 2, texture.Height / 2), scaleReturn, SpriteEffects.None, Main.GetDepth(currentPosition));
        }   

        public virtual Hit ModifyHitForEntity(float rotation)
        {
            if (!attack.hasHit)
            {
                if (attack.hit is HitArc)
                {
                    HitArc hit = (HitArc)attack.hit;
                    hit.max += rotation;
                    hit.min += rotation;
                }
                    attack.hasHit = true;
            }

            return attack.hit;
        }

        protected abstract Particle SpawnAnimationParticles(Vector2 parentCenter);

        #region animations
        public void AnimationSwing(Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            currentRotation = hit.max;
            currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(hit.max) * (hit.radius - texture.Width)), .3f);
        }

        public void AnimationSlam(Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            float angle = (Math.Abs(hit.max - hit.min) / 2) + hit.min;//halfway through the slice
            currentRotation = angle;
            
            if (hit.delay > 0)
            {
                verticalDraw = true;
                currentRotation = 90;
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * 16), .12f);
                height = MathHelper.Lerp(height, 64, height / 64);
            }
            else
            {
                verticalDraw = false;
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * (hit.radius - texture.Width)), .3f);
                height = MathHelper.Lerp(height, 0, .12f);
            }
        }

        /// <summary>
        /// Very simple animation. Merely raises the ghostweapon above the enemy's head.
        /// Supports: HitCircle
        /// </summary>
        public void AnimationRaise(Vector2 parentCenter)
        {   
            HitCircle hit = (HitCircle)attack.hit;
            verticalDraw = true;

            currentRotation = 90;

            height = MathHelper.Lerp(height, 64, height / 64);
        }

        public void AnimationPoke(Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            float angle = (Math.Abs(hit.max - hit.min) / 2) + hit.min;
            currentRotation = angle;
            if (hit.delay > 0)
            {
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * 16), .12f);
            }
            else
            {
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * (hit.radius - texture.Width)), .3f);
            }
        }
        #endregion
    }
}
