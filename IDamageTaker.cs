using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using HelloGame.Entities;

namespace HelloGame
{
    public interface IDamageTaker
    {
        Rectangle hitbox { get; set; }

        bool TakeDamage(World world, int amt, StaggerType type, Vector2 direction = new Vector2() { X = 0, Y = 0 });
    }
}
