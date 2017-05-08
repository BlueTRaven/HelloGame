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

namespace HelloGame.GhostWeapons
{
    public class GhostWeaponIronDagger : GhostWeapon
    {
        public GhostWeaponIronDagger() : base(Main.assets.GetTexture("ghostDagger"))
        {
            comboMax = 2;
            height = 16;
            restingHeight = 16;
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
            else if (combo == 2)
            {   //non player accesable
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 72, -45, 45, 60, 8, 5, Entities.StaggerType.Short, parent).Delay(5), 75, 30, AnimationPoke);
            }

            return null;
        }
    }
}
