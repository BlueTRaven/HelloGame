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
        public Undead(World world) : base(world.collisionWorld.Create(0, 0, 32, 32), 15)
        {
            texInfo = new TextureInfo(new TextureContainer("entity"), new Vector2(2), Color.White);

            chaseDistance = 112;
            chaseSpeed = .5f;
            chaseMaxSpeed = 1;
            circleSpeed = .1f;
            circleMaxSpeed = .5f;

            AddGhostWeapon(new GhostWeaponIronDagger());
        }

        public override void Update(World world)
        {
            if (!attacking)
            {
                gwRestPos[0] = Vector2.Normalize(target.position - position) * 32;
                gwRestRot[0] = VectorHelper.GetAngleBetweenPoints(position, target.position);
            }

            base.Update(world);
        }

        protected override void AddMoveset()
        {
            base.AddMoveset();

            moveset.Add(MoveSingleSlashPokeOrSlam(0, 0, 100, 160, 3, 5, 1.2f));     //single slash; common
            moveset.Add(MoveSingleSlashPokeOrSlam(0, 2, 100, 160, 3, 5, 1.2f));     //poke; common
            moveset.Add(MoveDoubleSlash(0, 0, 1, 15, 1.2f, 5, 15, 65, 160, 2));     //double slash; uncommon

            moveset.Add(MoveSingleSlashPokeOrSlam(0, 3, 100, 160, 1, 35, 3.5f));    //charge; rare
        }

    }
}
