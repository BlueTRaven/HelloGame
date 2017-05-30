using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelloGame.Hits;
using Microsoft.Xna.Framework;

namespace HelloGame.GhostWeapons
{
    public class GhostWeaponAttack
    {
        public Hit hit { get; private set; }
        public int resetTime { get; private set; }
        public int runTime { get; private set; }
        public readonly int runTimeMax;
        public bool hasHit;

        public Action<Vector2> animation;

        public GhostWeaponAttack(Hit hit, int runTime, int resetTime, Action<Vector2> animation)
        {
            this.hit = hit;
            this.resetTime = resetTime;
            this.runTime = runTime;
            this.runTimeMax = runTime;
            this.animation = animation;
        }

        public void Update()
        {
            if (runTime <= 0)
                resetTime--;
            else runTime--;
        }
    }
}
