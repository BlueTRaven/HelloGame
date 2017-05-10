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
using Humper.Responses;

namespace HelloGame.Entities
{
    public abstract class Enemy : EntityLiving
    {
        protected List<Move> moveset;

        public Queue<Move> moveQueue;
        protected Move currentMove;

        public float distanceFromPlayer { get; protected set; }
        public Player target { get; protected set; }

        public List<Hits.Hit> hits;

        protected float chaseSpeed, chaseMaxSpeed;
        protected float chaseDistance;
        protected float circleSpeed, circleMaxSpeed;

        private float healthMaxWidth;
        private float preHitHealth;
        private int healthDelay;

        protected bool boss;
        protected int bossPhase;

        protected bool drawHealthBar = true;

        protected List<GhostWeapon> weapons;
        protected Vector2[] gwRestPos;
        protected float[] gwRestRot;

        protected bool attacking;

        protected Vector2 directionFacing;
        public Enemy(IBox hitbox, int health) : base(hitbox)
        {
            moveset = new List<Move>();
            hits = new List<Hits.Hit>();
            moveQueue = new Queue<Move>();
            AddMoveset();

            this.health = health;
            maxHealth = health;

            healthMaxWidth = 128;
            preHitHealth = maxHealth;

            weapons = new List<GhostWeapon>();

            gravity = .25f;
            maxZVel = 4;
        }

        protected void SetMaxHealth(int health)
        {
            this.health = health;
            this.maxHealth = health;
            preHitHealth = health;
        }

        public override void PreUpdate(World world)
        {
            base.PreUpdate(world);

            target = world.player;
            distanceFromPlayer = (world.player.position - position).Length();
        }

        public override void Update(World world)
        {
            if (healthDelay > 0)
                healthDelay--;
            if (preHitHealth > health && healthDelay == 0)
                preHitHealth--;

            if (currentMove != null)
                currentMove.Update(world);

            if (currentMove == null || currentMove.done || currentMove.interruptable)
            {
                SelectMove(world);
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].Update(world, position, gwRestPos[i], gwRestRot[i]);
            }

            base.Update(world);

            hits.CheckAndDelete(new Func<Hits.Hit, bool>(hit => hit.done));

            foreach (Hits.Hit hit in hits)
            {
                hit.center = position;
            }
        }

        public virtual void AttackWithWeapon(World world, int weaponIndex, int force, float rotation)
        {
            weapons[weaponIndex].Attack(this, force);
            Hits.Hit h = weapons[weaponIndex].ModifyHitForEntity(rotation);
            hits.Add(h);
            world.AddHitbox(h);
        }

        public override bool TakeDamage(World world, int amt, StaggerType type, Vector2 direction = new Vector2() { X = 0, Y = 0 })
        {
            if (!invulnerable)
            {
                velocity = direction / 2;
                this.staggerTime = GetStaggerTime(type);

                preHitHealth = health;
                healthDelay = 15;
                health -= amt;
                if (health <= 0)
                {
                    Die(world);
                    return true;
                }
            }
            return false;
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            weapons.ForEach(x => x.Draw(batch));
        }

        protected void AddGhostWeapon(GhostWeapon weapon)
        {
            weapons.Add(weapon);

            Array.Resize(ref gwRestPos, weapons.Count);
            Array.Resize(ref gwRestRot, weapons.Count);
        }

        public virtual void DrawHealthbar(SpriteBatch batch)
        {
            if (drawHealthBar)
            {
                float percent = (float)health / (float)maxHealth;
                float percentpre = (float)preHitHealth / (float)maxHealth;

                if (!boss)
                {
                    int ypos = (int)(position.Y + (32 * shadowScale));

                    batch.DrawRectangle(new Rectangle((int)(position.X - healthMaxWidth / 2), ypos, (int)healthMaxWidth, 16), Color.Gray);
                    batch.DrawRectangle(new Rectangle((int)(position.X - healthMaxWidth / 2), ypos, (int)(percentpre * healthMaxWidth), 16), Color.Orange);
                    batch.DrawRectangle(new Rectangle((int)(position.X - healthMaxWidth / 2), ypos, (int)(percent * healthMaxWidth), 16), Color.Red);

                    batch.DrawRectangle(new Rectangle((int)(percent * healthMaxWidth) + (int)(position.X - healthMaxWidth / 2), ypos, 2, 16), Color.White, 1);
                    batch.DrawRectangle(new Rectangle((int)(percentpre * healthMaxWidth) + (int)(position.X - healthMaxWidth / 2), ypos, 2, 16), Color.White, 1);
                    batch.DrawHollowRectangle(new Rectangle((int)(position.X - healthMaxWidth / 2), ypos, (int)healthMaxWidth, 16), 2, Color.DarkGray, 1);
                }
                else
                {
                    int width = Main.WIDTH - 96;
                    var pos = Camera.ToWorldCoords(new Vector2(48, Main.HEIGHT - 64));

                    batch.DrawRectangle(new Rectangle((int)pos.X, (int)pos.Y, width, 16), Color.Gray);
                    batch.DrawRectangle(new Rectangle((int)pos.X, (int)pos.Y, (int)(percentpre * width), 16), Color.Orange);
                    batch.DrawRectangle(new Rectangle((int)pos.X, (int)pos.Y, (int)(percent * width), 16), Color.Red);

                    batch.DrawRectangle(new Rectangle((int)(percent * width + pos.X), (int)pos.Y, 2, 16), Color.White, 1);
                    batch.DrawRectangle(new Rectangle((int)(percentpre * width + pos.X), (int)pos.Y, 2, 16), Color.White, 1);
                    batch.DrawHollowRectangle(new Rectangle((int)(pos.X), (int)pos.Y, (int)width, 16), 2, Color.DarkGray, 1);
                }
            }
        }

        protected virtual void AddMoveset()
        {
            moveset.Add(new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                velocityDecays = true;
                if (move.counter1 == 0)
                {
                    move.counter1 = 1;
                    move.counter2 = 30 + Main.rand.Next(0, 120);
                }

                if (move.counter2 > 0)
                {
                    move.counter2--;

                    if (distanceFromPlayer > chaseDistance)
                    {
                        velocity += Vector2.Normalize(target.position - position) * chaseSpeed;
                        maxSpeed = chaseMaxSpeed;
                    }
                    else
                    {
                        Vector2 next = Vector2.Transform(position - target.position, Matrix.CreateRotationZ(MathHelper.ToRadians(1)));  //this doesn't actually work
                        velocity += Vector2.Normalize(next - position) * circleSpeed;
                        maxSpeed = circleMaxSpeed;
                    }
                    return false;
                }
                maxSpeed = 8;
                return true;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return enemy.distanceFromPlayer > 96 ? 1 : 0;
            }), false));
        }

        protected Move SelectMove(World world)
        {
            if (currentMove != null && currentMove.interruptable)
            {
                if (moveQueue.Count > 0)
                {
                    if (moveQueue.Peek().weight >= currentMove.weight)
                    {
                        currentMove.Reset();
                        currentMove = moveQueue.Dequeue();
                    }
                }
                else
                {
                    Move choosemove = moveset.RandomElementByWeight(new Func<Move, float>(move => move.weightScenario(world, this, move)));
                    if (choosemove.weight >= currentMove.weight)
                    {
                        currentMove.Reset();
                        currentMove = choosemove;
                    }
                }

                return currentMove;
            }

            if (currentMove != null)
                currentMove.Reset();

            if (moveQueue.Count > 0)
                currentMove = moveQueue.Dequeue();
            else
                currentMove = moveset.RandomElementByWeight(new Func<Move, float>(move => move.weightScenario(world, this, move)));

            return currentMove;
        }

        protected void SetMove(Move move, int counter1 = 0, int counter2 = 0, int counter3 = 0, int counter4 = 0)
        {
            move.Reset();
            move.counter1 = counter1;
            move.counter2 = counter2;
            move.counter3 = counter3;
            move.counter4 = counter4;
            moveQueue.Enqueue(move);
        }
    }
}
