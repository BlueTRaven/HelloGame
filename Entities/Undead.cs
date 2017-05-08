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
        public Undead(World world) : base(world.collisionWorld.Create(0, 0, 32, 32), 20)
        {
            texInfo = new TextureInfo(new TextureContainer("entity"), new Vector2(2), Color.White);

            chaseDistance = 112;
            chaseSpeed = .5f;
            chaseMaxSpeed = 1;
            circleSpeed = .1f;
            circleMaxSpeed = .5f;

            SetMaxHealth(100);

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

            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                attacking = true;
                if (move.counter1 == 0)
                {
                    move.counter2 = 60;
                    move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                    move.counter1 = 1;

                    velocity = Vector2.Zero;
                }

                if (move.counter2 > 0)  //delay
                    move.counter2--;
                else
                {
                    if (move.counter1 == 1)
                    {
                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * 5;
                        velocityDecays = false;
                        AttackWithWeapon(world, 0, 2, move.counter3);
                        move.counter1 = 2;
                    }

                    if (weapons[0].animationDone)
                    {
                        attacking = false;
                        return true;
                    }
                }

                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) => 
            {
                return enemy.distanceFromPlayer < 128 ? 1 : 0;
            })));

            //1
            /*moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) => 
            {
                if (move.counter1 == 0)
                {
                    move.counter1 = 1;
                    move.counter2 = 30; //lunge time
                    move.counter3 = 60; //delay time;
                    velocity = Vector2.Zero;
                }
                if (move.counter3 > 0)
                    move.counter3--;
                else
                {
                    if (move.counter1 == 1)
                    {
                        move.counter1 = 2;

                        enemy.velocity = Vector2.Normalize(enemy.target.position - enemy.position) * 6;
                        enemy.hits.Add(world.AddHitbox(new HitCircle(enemy.position, 24, move.counter2, 30, 20, StaggerType.Short, enemy)));
                    }
                    if (move.counter2 > 0)
                    {
                        move.counter2--;
                        enemy.velocityDecays = false;
                        ((HitCircle)enemy.hits[0]).center = enemy.position;
                        return false;
                    }
                    enemy.velocityDecays = true;
                    SetMove(moveset[0]);
                    enemy.moveQueue.Enqueue(moveset[0]);
                    return true;
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) => 
            {
                return enemy.distanceFromPlayer < 128 ? 1 : 0;  //this move will not be used if the player's distance is > 128.
            }), false));

            //2
            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                if (move.counter1 == 0)
                {
                    move.counter1 = 1;
                    move.counter2 = 60; //delay time;
                    velocity = Vector2.Zero;
                }
                else if (move.counter1 == 3)
                {
                    move.counter1 = 1;
                    velocity = Vector2.Zero;
                }
                if (move.counter2 > 0)
                {
                    if (enemy.hits.Count > 0)
                        ((HitArc)enemy.hits[0]).center = position;

                    if (move.counter2 > 70)
                        velocityDecays = false;
                    else velocityDecays = true;
                    move.counter2--;
                }
                else
                {
                    if (move.counter1 == 1)
                    {
                        move.counter1 = 2;
                        move.counter2 = 90;

                        enemy.velocity = Vector2.Normalize(enemy.target.position - enemy.position) * 4;
                        int[] a = HitArc.GetMinMax(VectorHelper.GetVectorAngle(target.position - position), 90);
                        enemy.hits.Add(world.AddHitbox(new HitArc(position, 64, a[0], a[1], 10, 30, 20, StaggerType.Short, this).SetAnimated(22.5f, true)));
                        return false;
                    }
                    if (move.counter1 == 2)
                    {
                        if (Main.rand.NextCoinFlip())
                        {
                            if (move.counter3 < 3)
                            SetMove(moveset[2], counter1: 3, counter2: 0, counter3: move.counter3 + 1);
                        }
                        else
                            SetMove(moveset[0]);
                        return true;
                    }
                    return false;
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return enemy.distanceFromPlayer < 128 ? 1 : 0;  //this move will not be used if the player's distance is > 128.
            }), false));*/
        }

    }
}
