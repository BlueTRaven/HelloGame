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
    public class CutsceneGenericCameraPan : Cutscene
    {
        public List<Vector2> panToNodes;

        public int lingerTime;

        public CutsceneGenericCameraPan(bool stopsWorldUpdate, Player player, int lingerTime, params Vector2[] panToNodes) : base(stopsWorldUpdate, player)
        {
            this.panToNodes = panToNodes.ToList();
            this.lingerTime = lingerTime;

            for (int i = 0; i < panToNodes.Length; i++)
            {
                CutsceneNode n = new CutsceneNode(new Func<World, CutsceneNode, bool>((world, node) =>
                {
                    player.staggered = true;
                    player.invulnerable = true;
                    if (node.counters[1] > 0)
                    {
                        node.counters[1]--;

                        if (node.counters[0] == 1)
                        {
                            Main.camera.target = panToNodes[(int)node.counters[2]];
                        }
                    }
                    else
                    {
                        if (node.counters[0] == 0)
                        {
                            node.counters[0] = 1;
                            node.counters[1] = lingerTime;
                        }
                        else return true;
                    }
                    return false;   //we don't need to add anything else as the camera's target should automatically be set back to the player.
                }));
                n.counters[2] = i;
                nodes.Enqueue(n);
            }
        }
    }
}
