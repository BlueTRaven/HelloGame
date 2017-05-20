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
using HelloGame.GhostWeapons; 

using Humper;

namespace HelloGame.Entities
{
    public class EnemyDragon1 : Enemy
    {
        public EnemyDragon1(World world, EnemyNoticeState state, float facingRotation) : base(new Vector2(48, 48), 64, facingRotation, state, 128, 1024, 0)
        {
            texInfos[0] = new TextureInfo(new TextureContainer("dragon1"), new Vector2(4), Color.LightGreen);
            texInfos[1] = new TextureInfo(new TextureContainer("dragon1_white"), new Vector2(4), Color.White);
            texInfos[2] = new TextureInfo(new TextureContainer("dragon1_eyes"), new Vector2(4), Color.Purple);

            chaseRadius = 128;
            chaseSpeed = .75f;
            chaseMaxSpeed = 8;
            circleSpeed = 1f;
            circleMaxSpeed = 8f;
        }
    }
}
