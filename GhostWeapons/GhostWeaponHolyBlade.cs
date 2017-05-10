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
using HelloGame.Entities.Particles;

namespace HelloGame.GhostWeapons
{
    public class GhostWeaponHolyBlade : GhostWeapon
    {
        public GhostWeaponHolyBlade() : base(Main.assets.GetTexture("ghostHolyBlade"))
        {
            comboMax = 3;
            height = 16;
            restingHeight = 16;
        }

        protected override Particle SpawnAnimationParticles(Vector2 parentCenter)
        {
            return new ParticleDust(Main.rand.Next(5, 20), currentPosition + new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8)), Main.rand.Next(1, 5), Main.rand.NextFloat(height - 1, height + 1), Main.rand.NextFloat(0, 360), Color.Gold)
                .SetRotates(-1, -1, Main.rand.Next(-1, 1))
                .SetWanders(-1, -1, .5f, 10);
        }

        protected override GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent)
        {
            if (combo == 0)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -45, 45, 8, 15, 10, Entities.StaggerType.Short, parent).SetAnimated(15, true).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 1)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -45, 45, 8, 15, 10, Entities.StaggerType.Short, parent).SetAnimated(15, false).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 2)
            {   //slam
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 160, -15, 15, 10, 45, 0, Entities.StaggerType.KnockdownGetup, parent).Delay(25), 45, 30, AnimationSlam);
            }
            else if (combo == 3)    //player inaccessable 
            {   //poke
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -15, 15, 15, 35, 20, Entities.StaggerType.Short, parent).Delay(60), 45, 30, AnimationPoke);
            }
            else if (combo == 4)
            {   //360 swing
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, 0, 360, 20, 30, 10, Entities.StaggerType.Medium, parent).SetAnimated(15, false).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 5)
            {   //raise explosion
                return new GhostWeaponAttack(new HitCircle(Vector2.Zero, 128, 5, 45, 15, Entities.StaggerType.KnockdownGetup, parent).Delay(120), 120, 30, AnimationRaise);
            }
            else if (combo == 6)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 1024, -1, 1, 10, 45, 0, Entities.StaggerType.Long, parent).Delay(25), 45, 30, AnimationSlam);
            }

            return null;
        }
    }
}
