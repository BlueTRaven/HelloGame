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
    public class NPCTest : NPC
    {

        public NPCTest() : base(new Vector2(32), "Test")
        {
            actionQueue.Enqueue(new NPCMove(new Func<World, NPC, bool>((world, npc) => 
            {
                if (counters[0] == 0)
                {
                    counters[0] = 1;
                    GuiHud.SetDialogueText(Main.assets.GetFont("bitfontMunro12"), "What? And who might you be?", 
                        "My name is " + name + ".", "Pleased to meet you.", "Here, take this.");
                }
                else
                {
                    if (!GuiHud.GetDialogueBox().active)
                    {   //if done
                        counters[0] = 0;
                        return true;
                    }
                }
                return false;
            })));
            actionQueue.Enqueue(new NPCMove(new Func<World, NPC, bool>((world, npc) =>
            {
                if (counters[0] == 0)
                {
                    world.player.AddItem(new ItemKey(0, 1));
                    counters[0] = 1;
                }

                if (GuiHud.ItemDisplayActive())
                    return false;
                return true;
            })));

            texInfos[0] = new TextureInfo(new TextureContainer("entity"), Vector2.One, Color.White);
        }
    }
}
