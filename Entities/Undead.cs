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
using Humper;
using HelloGame.GhostWeapons;

namespace HelloGame.Entities
{
    public class Undead : Enemy
    {
        int type;

        public Undead(World world, EnemyNoticeState state, float facingRotation, int type) : base(world.collisionWorld.Create(0, 0, 32, 32), 30, facingRotation, state, 256, -1, type)
        {
            this.type = type;
            texInfo = new TextureInfo(new TextureContainer("entity"), new Vector2(2), Color.White);

            chaseRadius = 112;
            chaseSpeed = .5f;
            chaseMaxSpeed = 1;
            circleSpeed = .1f;
            circleMaxSpeed = .5f;

            if (type == 0)
                AddGhostWeapon(new GhostWeaponIronDagger());
            else if (type == 1)
                AddGhostWeapon(new GhostWeaponIronSword());
            else if (type == 2)
                AddGhostWeapon(new GhostWeaponHalberd());
        }

        public override void Update(World world)
        { 
            base.Update(world);
        }

        protected override void AddMoveset(int type)
        {
            base.AddMoveset(type);

            if (type == 0 || type == 1)
            {
                moveset.Add(MoveSingleSlashPokeOrSlam(0, 0, 30, 100, 160, 3, 5, 1.2f));     //single slash; common
                moveset.Add(MoveSingleSlashPokeOrSlam(0, 2, 30, 100, 160, 3, 5, 1.2f));     //poke; common
                moveset.Add(MoveDoubleSlash(0, 0, 1, 45, 1.2f, 5, 15, 65, 160, 2));     //double slash; uncommon

                moveset.Add(MoveSingleSlashPokeOrSlam(0, 3, 45, 100, 160, 1, 35, 3.5f));    //charge; rare
            }
            else if (type == 2)
            {   //halberd undead is just a longer range sword/dagger undead, with less moves.
                moveset.Add(MoveSingleSlashPokeOrSlam(0, 0, 45, 100, 160, 4, 5, 1.2f));     //single slash; common
                moveset.Add(MoveDoubleSlash(0, 0, 1, 45, 1.2f, 5, 15, 65, 160, 2));     //double slash; uncommon
            }
        }

    }
}
