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
using Humper;

namespace HelloGame.Entities
{
    public class DemonMan : Enemy
    {
        public DemonMan(World world) : base(world.collisionWorld.Create(0, 0, 32, 32), 250, -90, EnemyNoticeState.HighAlert, 256)
        {
            texInfo = new TextureInfo(new TextureContainer("entity"), new Vector2(2), Color.White);
            chaseRadius = 128;
            chaseSpeed = 1;
            chaseMaxSpeed = 2;
            circleSpeed = .5f;
            circleMaxSpeed = 1;

            maxSpeed = 8;

            boss = true;

            AddGhostWeapon(new GhostWeaponHolyBlade());
        }

        public override void Update(World world)
        {
            base.Update(world);

            if (!attacking)
            {
                gwRestPos[0] = Vector2.Normalize(target.position - position) * 32;
                gwRestRot[0] = VectorHelper.GetAngleBetweenPoints(position, target.position);
            }
        }

        protected override void AddMoveset(int type)
        {
            base.AddMoveset(type);

            //Sword Beam Slam
            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {   
                if (move.counter2 > 0)
                    move.counter2--;
                else
                {
                    if (move.counter1 == 0)
                    {
                        move.counter1 = 1;
                        move.counter2 = 60;
                    }
                    else if (move.counter1 == 1)
                    {
                        move.counter1 = 2;
                        move.counter2 = 15;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                        attacking = true;
                    }
                    else if (move.counter1 == 2)
                    {
                        move.counter1 = 3;
                        move.counter2 = 30;
                        AttackWithWeapon(world, 0, 6, move.counter3);
                    }
                    else if (move.counter1 == 3)
                    {
                        move.counter1 = 4;
                        for (int i = 0; i < 128; i++)
                        {
                            Vector2 angle = VectorHelper.GetAngleNormVector(move.counter3) * i * 8;
                            world.AddEntity(new ParticleDust(Main.rand.Next(30, 90), position + angle + new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8)), Main.rand.NextFloat(5, 10), 0, Main.rand.NextFloat(0, 360), Color.Gold)
                                .SetFades(bossPhase == 0 ? Color.Gold : Color.Black, new Color(Color.Red, 0), -1, -1, false)   //for some reason, lerping to the latter color does not set the alpha...?
                                .SetRotates(120, 0, (Main.rand.NextCoinFlip() ? Main.rand.NextFloat() : Main.rand.NextFloat() - 1) * 4)
                                .SetGravity(-.1f, .1f, 5, new Action<Entity>(particle => particle.Die(world))));
                        }

                        move.counter2 = 60;
                    }
                    else { attacking = false; return true; }
                }

                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                int val = 0;
                if (distanceFromPlayer > 256)
                    val = 5;
                if (distanceFromPlayer > 512)
                    val = 10;;
                return val;
            })));

            //Spin and Slam
            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {   
                if (move.counter1 == 2)
                    attacking = false;
                else attacking = true;
                if (move.counter2 > 0)
                    move.counter2--;
                else
                {
                    if (move.counter1 == 0)
                    {   //delay
                        move.counter1 = 1;
                        move.counter2 = 30;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                        AttackWithWeapon(world, 0, 4, move.counter3, 30);
                    }
                    else if (move.counter1 == 1)
                    {   //spin & delay
                        move.counter1 = 2;
                        move.counter2 = 20;
                        velocityDecaysTimer = 15;
                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * 8;
                    }
                    else if (move.counter1 == 2)
                    {   //post spin delay
                        move.counter1 = 3;
                        move.counter2 = 15;
                    }
                    else if (move.counter1 == 3)
                    {   //slam & delay
                        move.counter1 = 4;
                        move.counter2 = 120;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                        AttackWithWeapon(world, 0, 2, move.counter3, 30);
                    }
                    else if (move.counter1 == 4)
                    {   //return
                        attacking = false;
                        return true;
                    }
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return distanceFromPlayer > 128 && distanceFromPlayer <= 320 ? 20 : 0;
            })));

            //Poke
            moveset.Add(MoveSingleSlashPokeOrSlam(0, 3, 30, 90, 256, 2));

            //Slam Jumpback
            moveset.Add(MoveSlamJumpback(0, 2, 45, 8, 90, 20, 256, 1));

            //Back and Forth Slash
            moveset.Add(MoveDoubleSlash(0, 0, 1, 30, 2, 5, 12, 90, 256, 2));

            //Phase Transition
            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {   
                attacking = false;
                if (move.counter2 > 0)
                {
                    move.counter2--;

                    if (move.counter1 == 1 && move.counter2 <= 120 && move.counter2 > 90)
                    {   //"smoke" damage animation
                        for (int i = 0; i < Main.rand.Next(12, 32); i++)
                        {
                            Particle p = world.AddEntity(new ParticleDust(Main.rand.Next(30, 75), position, Main.rand.Next(5, 10), 16, Main.rand.NextFloat(0, 360), Color.Black)
                                .SetGravity(-.1f, .1f, 5, new Action<Entity>(particle => particle.Die(world))));
                            if (p != null)
                            p.velocity = Main.rand.NextAngle() * 30;
                        }
                    }
                }
                else
                {
                    if (move.counter1 == 0)
                    {   //pre attack delay
                        move.counter1 = 1;
                        move.counter2 = 240;
                        AttackWithWeapon(world, 0, 5, 0);
                    }
                    else if (move.counter1 == 1)
                    {   //attack and more delay
                        move.counter1 = 2;
                        move.counter2 = 0;

                        bossPhase = 1;
                    }
                    else if (move.counter1 == 2)
                    {
                        return true;
                    }
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {   //if below half hp, and the phase is still 1, this attack should almost always be used.
                return health <= maxHealth / 2 && bossPhase != 1 ? 20 : 0;
            })));
        }

        public override void Die(World world)
        {
            base.Die(world);

            for (int i = 0; i < 128; i++)
            {
                if (world.entities[i] is Particle)
                {
                    world.entities[i].Die(world);
                }
            }

            for (int i = 0; i < Main.rand.Next(64, 128); i++)
            {
                Particle p = world.AddEntity(new ParticleDust(1000, position, Main.rand.Next(5, 10), 16, Main.rand.NextFloat(0, 360), Color.Black)
                    .SetGravity(-.1f, .1f, 5, new Action<Entity>(particle => particle.Die(world)))
                    .SetHomes(world.player, 970, 0, 1.2f, true));
                if (p != null)
                    p.velocity = Main.rand.NextAngle() * Main.rand.NextFloat(0, 25);
            }
        }
    }
}
