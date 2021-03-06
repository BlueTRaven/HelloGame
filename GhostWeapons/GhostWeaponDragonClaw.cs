﻿using System;
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
    public class GhostWeaponDragonClaw : GhostWeapon
    {
        public GhostWeaponDragonClaw(TextureInfo texInfo) : base(texInfo)
        {
        }

        protected override GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent)
        {
            if (combo == 0)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -45, 45, 10, 14, 10, StaggerType.Short, parent).SetAnimated(15, true).Delay(8), 30, 30, AnimationSwing);
            }
            else if (combo == 1)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -45, 45, 10, 14, 10, StaggerType.Short, parent).SetAnimated(15, false).Delay(8), 30, 30, AnimationSwing);
            }
            return null;
        }

        protected override Particle SpawnAnimationParticles(Vector2 parentCenter)
        {
            return new ParticleDust(Main.rand.Next(5, 20), currentPosition + new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8)), 
                Main.rand.Next(1, 5), Main.rand.NextFloat(height - 1, height + 1), Main.rand.NextFloat(0, 360), Color.Silver)
                .SetRotates(-1, -1, Main.rand.Next(-20, 20))
                .SetWanders(-1, -1, .5f, 10);
        }
    }
}
