using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Hits;
using HelloGame.GhostWeapons;
using HelloGame.Entities.Particles;
using HelloGame.Items;
using HelloGame.Guis;

using Humper;

namespace HelloGame.Entities.NPCs
{
    public class NPC : EntityLiving
    {
        protected Queue<NPCMove> actionQueue;
        protected NPCMove currentAction;

        protected bool interacting;

        protected string endInteractionOutrangeText, endInteractionOutofActionsText;

        public string name { get; private set; }

        protected float[] counters;

        private const float interactDistance = 128;
        public NPC(Vector2 hitboxSize, string name) : base(hitboxSize)
        {
            counters = new float[4];
            this.name = name;

            actionQueue = new Queue<NPCMove>();

            maxSpeed = 8;
        }

        public override void Update(World world)
        {
            base.Update(world);

            if (currentAction != null)
                currentAction.Update(world, this);  

            if ((IsInteracting(world) || interacting) && !CheckEndInteraction(world))
            {
                OnInteracting(world);
            }
        }

        public virtual bool IsInteracting(World world)
        {
            if (Math.Abs((world.player.position - position).Length()) <= interactDistance)
            {
                if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    return true;
                else GuiHud.SetPromptText(2, "Press {0} to talk");
            }
            return false;
        }

        public virtual void OnInteracting(World world)
        {
            interacting = true;

            if (currentAction == null || currentAction.done)
            {
                currentAction = actionQueue.Dequeue();
            }
        }

        protected virtual bool CheckEndInteraction(World world)
        {
            if ((actionQueue.Count == 0 && currentAction.done) || Math.Abs((world.player.position - position).Length()) > interactDistance)
            {
                interacting = false;
                return true;
            }
            return false;
        }
    }

    public class NPCMove
    {
        public bool done { get; private set; }
        public Func<World, NPC, bool> action;

        public NPCMove(Func<World, NPC, bool> action)
        {
            this.action = action;
        }

        public void Update(World world, NPC npc)
        {
            if (!done)
                done = action.Invoke(world, npc);
        }
    }
}
