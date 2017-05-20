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
    public class GhostWeaponAxe : GhostWeapon
    {
        public GhostWeaponAxe() : base(new TextureInfo(new TextureContainer("ghostAxe"), Vector2.Zero, Color.White).SetAnimated(32, 24, 
            new Animation(0, 32, 24, 1, true, false, 5), 
            new Animation(32, 32, 24, 1, true, false, 5)))
        {
            comboMax = 3;
            height = 18;
            restingHeight = 18;

            flipsOnCC = true;

            usesAnimation[0] = true;
        }

        protected override GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent)
        {
            if (combo == 0)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 10, 12, 10, StaggerType.Short, parent).SetAnimated(15, true).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 1)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 10, 12, 10, StaggerType.Short, parent).SetAnimated(15, false).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 2)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 96, -15, 15, 10, 15, 0, StaggerType.Medium, parent).Delay(28), 45, 30, AnimationSlam);
            }
            return null;
        }

        protected override Particle SpawnAnimationParticles(Vector2 parentCenter)
        {
            return new ParticleDust(Main.rand.Next(5, 20), currentPosition + VectorHelper.GetAngleNormVector(currentRotation) * 48 + new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8)), Main.rand.Next(1, 5), Main.rand.NextFloat(height - 1, height + 1), Main.rand.NextFloat(0, 360), Color.Silver)
                .SetRotates(-1, -1, Main.rand.Next(-20, 20))
                .SetWanders(-1, -1, .5f, 10);
        }
    }
}
