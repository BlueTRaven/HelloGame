using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Entities;
using HelloGame.Guis.Widgets;

namespace HelloGame.Cutscenes
{
    public abstract class Cutscene
    {
        public bool stopsWorldUpdate;
        public Entity[] actors;

        protected Queue<CutsceneNode> nodes;
        protected CutsceneNode currentNode;

        public bool done;

        public Cutscene(bool stopsWorldUpdate, params Entity[] actors)
        {
            nodes = new Queue<CutsceneNode>();
            this.stopsWorldUpdate = stopsWorldUpdate;
            this.actors = actors;
        }

        public virtual void Update(World world)
        {
            if (currentNode == null || currentNode.done)
            {
                if (nodes.Count > 0)
                    currentNode = nodes.Dequeue();
                else done = true;
            }

            if (currentNode != null)
                currentNode.Update(world);
        }
    }

    public class CutsceneNode
    {
        public Func<World, CutsceneNode, bool> runFunction;

        public bool done { get; private set; }

        public float[] counters;
        public CutsceneNode(Func<World, CutsceneNode, bool> runFunction)
        {
            this.runFunction = runFunction;
            counters = new float[4];
        }

        public void Update(World world)
        {
            done = runFunction.Invoke(world, this);
        }
    }
}
