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
        private Vector2 lerpStartPos;
        private float lerpStartRot;
        private float lerpStartHeight;

        protected int combo, comboMax;

        public GhostWeaponAttack attack { get; protected set; }

        private Texture2D texture;
        private TextureInfo texInfo;

        protected Vector2 origin, size;

        public int returnTime { get; protected set; }

        public float height;
        protected float restingHeight;

        public float scale = 1.5f;
        private float scaleReturn = 1.5f;

        private bool verticalDraw;
        private float vertDrawScale;

        public bool animationDone { get; private set; }

        protected bool flipsOnCC, flipped;

        protected bool[] usesAnimation;

        private int alive;

        private FixedSizedQueue<Ghost> ghosts;

        private class Ghost
        {
            public Vector2 pos;
            public float rot;
            public float height;
            public int time;

            public Ghost(Vector2 pos, float rot, float height)
            {
                this.pos = pos;
                this.rot = rot;
                this.height = height;
                time = 5;
            }

            public void Update()
            {
                time--;
            }
        }

        public GhostWeapon(TextureInfo texInfo)
        {
            this.texInfo = texInfo;
            usesAnimation = new bool[8];
            ghosts = new FixedSizedQueue<Ghost>(5);

            origin = texInfo.sourceRect.HasValue ? new Vector2(texInfo.sourceRect.Value.Width / 2, texInfo.sourceRect.Value.Height / 2) : 
                                                   new Vector2(texInfo.texture.texture.Width / 2, texInfo.texture.texture.Height / 2);

            size = texInfo.sourceRect.HasValue ? new Vector2(texInfo.sourceRect.Value.Width, texInfo.sourceRect.Value.Height) :
                                                 new Vector2(texInfo.texture.texture.Width, texInfo.texture.texture.Height);
        }

        public virtual void Attack(IDamageDealer parent, int force = -1)
        {
            ghosts.Clear();

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
            alive++;

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

            foreach(Ghost g in ghosts)
            {
                g.Update();
            }

            if (attack != null)
            {
                if (alive % 2 == 0)
                {
                    ghosts.Enqueue(new Ghost(currentPosition, currentRotation, height));
                }

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
                    flipped = false;
                    animationDone = false;
                    attack.hit.center = parentCenter;
                    if (attack.hit.delay <= 0)
                        world.AddEntity(SpawnAnimationParticles(parentCenter));
                    attack.animation?.Invoke(world, parentCenter);
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
            batch.Draw(Main.assets.GetTexture("shadow"), currentPosition, null, new Color(0, 0, 0, 127), 0, new Vector2(32, 16), vertDrawScale, SpriteEffects.None, Main.GetDepth(new Vector2(currentPosition.X, currentPosition.Y - 1)));
            //batch.Draw(texture, currentPosition, null, new Color(0, 0, 0, 127), MathHelper.ToRadians(currentRotation), new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, Main.GetDepth(currentPosition));
            //batch.Draw(texture, currentPosition + (Main.camera.up * height), null, Color.White, MathHelper.ToRadians(currentRotation), new Vector2(texture.Width / 2, texture.Height / 2), scaleReturn, SpriteEffects.None, Main.GetDepth(currentPosition));
            
            batch.Draw(texInfo.texture.texture, currentPosition, texInfo.sourceRect, new Color(0, 0, 0, 127), MathHelper.ToRadians(currentRotation),    //draw the weapon shadow
                origin, scale, 
                flipped ? SpriteEffects.FlipVertically : SpriteEffects.None, Main.GetDepth(new Vector2(currentPosition.X, currentPosition.Y - 1)));

            for (int i = 1; i < ghosts.Count; i++)  //start at index 1 because 0 is always the current position.
            {
                Ghost g = ghosts.ElementAt(i);

                if (g.time > 0)
                {   //draw ghost images
                    batch.Draw(texInfo.texture.texture, g.pos + (Main.camera.up * g.height), texInfo.sourceRect, new Color(texInfo.tint, (float)i / ghosts.Count),  //draw the weapon
                      MathHelper.ToRadians(g.rot), origin, scaleReturn,
                      flipped ? SpriteEffects.FlipVertically : SpriteEffects.None, Main.GetDepth(new Vector2(currentPosition.X, currentPosition.Y - 1)));
                    //draw lines
                    batch.DrawLine(ghosts.ElementAt(i - 1).pos + (Main.camera.up * g.height), g.pos + (Main.camera.up * g.height), new Color(texInfo.tint, ((float)i / 2f) / ((float)ghosts.Count / 2f)), (int)(16f * ((float)i / (float)ghosts.Count)));
                }
            }

            batch.Draw(texInfo.texture.texture, currentPosition + (Main.camera.up * height), texInfo.sourceRect, texInfo.tint,  //draw the weapon
                MathHelper.ToRadians(currentRotation), origin, scaleReturn, 
                flipped ? SpriteEffects.FlipVertically : SpriteEffects.None, Main.GetDepth(currentPosition));
        }   

        public virtual Hit ModifyHitForEntity(float rotation, int delay = -1)
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

                if (delay != -1)
                    attack.hit.delay = delay;
            }

            return attack.hit;
        }

        protected abstract Particle SpawnAnimationParticles(Vector2 parentCenter);

        #region animations
        public void AnimationSwing(World world, Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            currentRotation = hit.max;
            currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(hit.max) * (hit.radius - (origin.X * 2))), .8f);
            if (flipsOnCC && !hit.clockwise)
                flipped = true;
            else flipped = false;
        }

        public Color GetBrushColorBelow(World world)
        {
            foreach(Brush b in world.brushes)
            {
                if (b != null)
                {
                    if (b.bounds.Contains(currentPosition))
                    {
                        int total = b.texInfo.texture.texture.Width * b.texInfo.texture.texture.Height;

                        Color[] colors = new Color[total];
                        b.texInfo.texture.texture.GetData(colors, 0, total);

                        return colors[Main.rand.Next(0, total)];
                    }
                }
            }

            return Color.Transparent;   //if there's no brushes below, just return transparent so we don't have to show some awkward unfitting color
        }

        public void AnimationSlam(World world, Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            float angle = (Math.Abs(hit.max - hit.min) / 2) + hit.min;//halfway through the slice
            currentRotation = angle;

            if ((angle > 0 && angle < 90) || (angle < 360 && angle > 360 - 90))
                flipped = true;
            else flipped = false;
            if (hit.delay > 5)
            {
                verticalDraw = true;
                currentRotation = 90;
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * 16), .12f);
                height = MathHelper.Lerp(height, 64, height / 64);

                lerpStartPos = currentPosition;
                lerpStartHeight = height;
            }
            else if (hit.delay <= 5)
            {
                verticalDraw = false;
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * (hit.radius - (origin.X * 2))), 1f / 8f);
                float p = ((float)lerpStartHeight / (float)height);
                height = MathHelper.Lerp(height, 0, 1f / 5f);

                if (usesAnimation[0])
                    texInfo.AddAnimationToQueue(1, true);
            }

            if (hit.delay <= 0 && hit.duration == hit.durationMax)
            {
                for (int i = 0; i < Main.rand.Next(8, 12); i++)
                {
                    ParticleDust p = (ParticleDust)world.AddEntity(new ParticleDust(Main.rand.Next(7, 16), currentPosition, Main.rand.NextFloat(8, 16), 0, Main.rand.Next(0, 360), GetBrushColorBelow(world)))
                        .SetRotates(-1, -1, Main.rand.NextCoinFlip() ? Main.rand.NextFloat(-5, -1.2f) : Main.rand.NextFloat(1.2f, 5));
                    p.velocity = Main.rand.NextAngle() * 2;
                    p.velocityDecaysTimer = 16;
                    p.gravity = 0;
                    p.zVel = 2f;
                    p.maxZVel = 4;
                }
            }
        }

        /// <summary>
        /// Very simple animation. Merely raises the ghostweapon above the enemy's head.
        /// Supports: HitCircle
        /// </summary>
        public void AnimationRaise(World world, Vector2 parentCenter)
        {
            HitCircle hit = (HitCircle)attack.hit;
            verticalDraw = true;

            currentRotation = 90;

            height = MathHelper.Lerp(height, 64, height / 64);
            currentPosition = Vector2.Lerp(currentPosition, parentCenter, .12f);
        }

        public void AnimationPoke(World world, Vector2 parentCenter)
        {
            HitArc hit = (HitArc)attack.hit;
            float angle = (Math.Abs(hit.max - hit.min) / 2) + hit.min;
            currentRotation = angle;
            if (hit.delay > 0)
            {
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * -8), .2f);
            }
            else
            {
                currentPosition = Vector2.Lerp(currentPosition, parentCenter + (VectorHelper.GetAngleNormVector(angle) * (hit.radius - (origin.X * 2))), .3f);
            }
        }
        #endregion
    }
}
