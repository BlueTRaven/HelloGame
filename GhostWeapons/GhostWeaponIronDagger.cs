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
    public class GhostWeaponIronDagger : GhostWeapon
    {
        public GhostWeaponIronDagger() : base(new TextureInfo(new TextureContainer("ghostDagger"), Vector2.Zero, Color.White))
        {
            comboMax = 2;
            height = 16;
            restingHeight = 16;
        }

        protected override Particle SpawnAnimationParticles(Vector2 parentCenter)
        {
            return new ParticleDust(Main.rand.Next(5, 20), currentPosition + new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8)), Main.rand.Next(1, 5), Main.rand.NextFloat(height - 1, height + 1), Main.rand.NextFloat(0, 360), Color.Silver)
                .SetRotates(-1, -1, Main.rand.Next(-1, 1))
                .SetWanders(-1, -1, .5f, 10);
        }

        protected override GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent)
        {
            if (combo == 0)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 6, 8, 5, Entities.StaggerType.Short, parent).SetAnimated(8, true).Delay(5), 6, 30, AnimationSwing);
            }
            else if (combo == 1)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 6, 8, 5, Entities.StaggerType.Short, parent).SetAnimated(8, false).Delay(5), 6, 30, AnimationSwing);
            }
            else if (combo == 2)    //poke
            {   //non player accessable
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -15, 15, 30, 8, 5, Entities.StaggerType.Short, parent).Delay(5), 45, 30, AnimationPoke);
            }
            else if (combo == 3)    //charge
            {   //non player accessable
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 30, 8, 5, Entities.StaggerType.Short, parent).Delay(5), 45, 30, AnimationPoke);
            }

            return null;
        }
    }
}
