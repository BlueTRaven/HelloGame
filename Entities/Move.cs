using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace HelloGame.Entities
{
    public class Move
    {
        public Func<World, Enemy, Move, bool> action;
        public Func<World, Enemy, Move, int> weightScenario;

        public Enemy parent;
        public float counter1, counter2, counter3, counter4;

        public bool done { get; private set; }

        public int weight { get; private set; }

        public bool interruptable;

        public bool reset;
        public Move(Enemy parent, Func<World, Enemy, Move, bool> action, Func<World, Enemy, Move, int> weightScenario, bool interruptable = false)
        {
            this.parent = parent;
            this.action = action;
            this.weightScenario = weightScenario;
            this.interruptable = interruptable;
        }

        public void Update(World world)
        {
            reset = false;

            if (action != null)
            done = action.Invoke(world, parent, this);
        }

        public void Reset()
        {
            if (!reset)
            {
                counter1 = 0;
                counter2 = 0;
                counter3 = 0;
                counter4 = 0;

                reset = true;
            }
        }
    }
}
