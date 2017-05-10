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

namespace HelloGame.Entities.Particles
{
    public abstract class Particle : Entity
    {
        protected float rotation, scale;
        protected Color color;

        protected bool fades;
        private bool fadeAlways;
        protected Color startColor, endColor;
        protected int startFadeTime, endFadeTime;
        private int fadeTime, fadeTimeMax;
        protected bool deadOnFade;

        protected bool rotates;
        private bool rotateAlways;
        protected int startRotateTime, endRotateTime;
        protected float rotateAmt;

        protected bool wanders, wanderAlways;
        protected int startWanderTime, endWanderTime, wanderInterval;
        private int wanderIntervalMax;
        protected float wanderScale;

        public int timeleft { get; protected set; }

        public Particle(int timeleft)
        {
            this.timeleft = timeleft;
            canMove = true;
            maxSpeed = 30;
        }

        public override void Update(World world)
        {
            base.Update(world);

            if (timeleft > 0)
                timeleft--;
            else Die(world);

            if (rotates)
            {
                if (timeleft <= startRotateTime && timeleft > endRotateTime || rotateAlways)
                {
                    rotation += rotateAmt;
                }
            }

            if (fades)
            {
                if (timeleft <= startFadeTime && timeleft > endFadeTime || fadeAlways)
                {
                    fadeTime--;
                    color = Color.Lerp(endColor, startColor, (float)fadeTime / (float)fadeTimeMax);
                }
            }

            if (wanders)
            {
                if (wanderAlways || (timeleft <= startWanderTime && timeleft >= endWanderTime))
                {
                    if (wanderInterval > 0)
                    {
                        wanderInterval--;
                    }
                    else
                    {
                        velocity = Main.rand.NextAngle() * wanderScale;
                        wanderInterval = wanderIntervalMax;
                    }
                }
            }
        }

        public Particle SetWanders(int startWanderTime, int endWanderTime, float wanderScale, int wanderInterval)
        {
            wanders = true;

            this.startWanderTime = startWanderTime;
            this.endWanderTime = endWanderTime;
            FixStartEndTimes(ref this.startWanderTime, ref this.endWanderTime, startWanderTime, endWanderTime, ref wanderAlways);

            this.wanderScale = wanderScale;
            this.wanderInterval = wanderInterval;
            this.wanderIntervalMax = wanderInterval;
            return this;
        }

        public Particle SetGravity(float gravity, float startZvel, float maxZVel, Action<Entity> groundCollideAction)
        {
            this.gravity = gravity;
            this.zVel = startZvel;
            this.maxZVel = maxZVel;
            this.groundCollideAction = groundCollideAction;
            return this;
        }

        public Particle SetRotates(int startRotateTime, int endRotateTime, float rotateAmt)
        {
            this.rotates = true;

            FixStartEndTimes(ref this.startRotateTime, ref this.endRotateTime, startRotateTime, endRotateTime, ref rotateAlways);

            this.rotateAmt = rotateAmt;
            return this;
        }

        public Particle SetFades(Color startColor, Color endColor, int startFadeTime, int endFadeTime, bool deadOnFade)
        {
            this.fades = true;
            this.startColor = startColor;
            this.endColor = endColor;
            FixStartEndTimes(ref this.startFadeTime, ref this.endFadeTime, startFadeTime, endFadeTime, ref fadeAlways);
            if (!(startFadeTime == -1 || endFadeTime == -1))
                this.fadeTime = Math.Abs(endFadeTime - startFadeTime);
            else
                this.fadeTime = timeleft;
            this.fadeTimeMax = fadeTime;
            return this;
        }

        private void FixStartEndTimes(ref int localStart, ref int localEnd, int start, int end, ref bool always)
        {
            int eend = 0;
            int estart = 0;

            if (end > start)
            {
                eend = start;
                estart = end;
            }
            else { eend = end; estart = start; }

            if (eend == -1 || estart == -1)
                always = true;

            localStart = estart;
            localEnd = eend;
        }
    }
}
