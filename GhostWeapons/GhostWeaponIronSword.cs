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
    public class GhostWeaponIronSword : GhostWeapon
    {
        public GhostWeaponIronSword() : base(Main.assets.GetTexture("ghostSword"))
        {
            comboMax = 3;
            height = 16;
            restingHeight = 16;
        }

        protected override GhostWeaponAttack GetNextAttack(int combo, IDamageDealer parent)
        {
            if (combo == 0)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 96, -45, 45, 10, 10, 10, Entities.StaggerType.Short, parent).SetAnimated(15, true).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 1)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 96, -45, 45, 10, 10, 10, Entities.StaggerType.Short, parent).SetAnimated(15, false).Delay(5), 15, 30, AnimationSwing);
            }
            else if (combo == 2)
            {
                return new GhostWeaponAttack(new HitArc(Vector2.Zero, 128, -15, 15, 10, 15, 0, Entities.StaggerType.Medium, parent).Delay(25), 45, 30, AnimationSlam);
            }

            return null;
        }
    }
}
