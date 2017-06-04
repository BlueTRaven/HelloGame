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
            texInfos[1] = new TextureInfo(new TextureContainer("dragon1_White"), new Vector2(4), Color.White);
            texInfos[2] = new TextureInfo(new TextureContainer("dragon1_Eyes"), new Vector2(4), Color.Purple);

            chaseRadius = 128;
            chaseSpeed = .75f;
            chaseMaxSpeed = 8;
            circleSpeed = 1f;
            circleMaxSpeed = 8f;

            maxSpeed = 10;
        }

        protected override void AddMoveset(int type)
        {
            base.AddMoveset(type);

            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                if (move.counter2 > 0)
                {
                    move.counter2--;

                    if (move.counter1 == 1)
                    {
                        if (Math.Abs((target.position - position).Length()) < 256)
                        {
                            var a = VectorHelper.GetVectorAngle(target.position - position);

                            SetPosition(target.position + new Vector2(-256, 0).RotateBy(a), true);
                        }
                    }
                    if (move.counter1 == 3)
                    {
                        Vector2 newPos = new Vector2(move.counter3, move.counter4);
                        //magic number yay
                        SetPosition(Vector2.Lerp(position, newPos, 0.0222222222f), true);
                        velocity = Vector2.Zero;
                        if ((newPos - position).X < 0)
                        {
                            visualRotation -= .5f;
                            if (visualRotation < -15)
                                visualRotation = -15;
                        }
                        else if ((newPos - position).X > 0)
                        {
                            visualRotation += .5f;
                            if (visualRotation > 15)
                                visualRotation = 15;
                        }
                    }
                    else visualRotation = 0;
                }
                else
                {
                    if (move.counter1 == 0)
                    {
                        move.counter1 = 1;
                        move.counter2 = 15;
                    }
                    else if (move.counter1 == 1)
                    {   //pre delay
                        move.counter1 = 2;
                        move.counter2 = 15;
                    }
                    else if (move.counter1 == 2)
                    {   //lerp to pos
                        move.counter1 = 3;
                        move.counter2 = 60;
                        var newPos = Vector2.Normalize(VectorHelper.GetPerp(target.position - position)) * 512 + position;
                        move.counter3 = newPos.X;
                        move.counter4 = newPos.Y;
                    }
                    else if (move.counter1 == 3)
                    {
                        move.counter1 = 4;
                        move.counter2 = 60;
                        velocity = Vector2.Normalize(target.position - position) * 10;  //lunge at the player
                        velocityDecaysTimer = 60;
                    }
                    else if (move.counter1 == 4)
                        return true;
                    return false;
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) => 
            {
                return 20;
            })));
        }
    }
}
