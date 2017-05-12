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
    public enum EnemyNoticeState
    {
        Alert,      //player must be in line of sight.
        HighAlert,  //player can be anywhere in its aggro range.
        Sleeping,   //will not aggro unless hit, in which case it transfers into alert state.
        Aggrod,
        Returning   //returning to its initial position, after the player has moved out of its chase radius.
    }

    public abstract class Enemy : EntityLiving
    {
        protected List<Move> moveset;

        public Queue<Move> moveQueue;
        protected Move currentMove;

        public float distanceFromPlayer { get; protected set; }
        public Player target { get; protected set; }

        public List<Hits.Hit> hits;

        protected float chaseSpeed, chaseMaxSpeed;
        protected float chaseRadius;
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

        public EnemyNoticeState initialNoticeState;
        public EnemyNoticeState noticeState;
        public float aggroRadius;
        public float leashRadius;
        public float facingRotation;
        /// <summary>
        /// Creates A new Enemy.
        /// </summary>
        /// <param name="hitbox">The hitbox of the enemy. Can be 0, 0 relative; is moved manually by EntitySpawner.</param>
        /// <param name="health">The starting HP of the enemy.</param>
        /// <param name="state">The starting Notice State of the enemy. When in doubt, use Alert.</param>
        /// <param name="aggroRadius">How close the player has to be to the enemy to get noticed. Only applicable in HighAlert notice state.</param>
        /// <param name="leashRadius">How far away from its spawn point an enemy will chase the player. Set to -1 for infinte chase range.</param>
        public Enemy(IBox hitbox, int health, float facingRotation, EnemyNoticeState state, float aggroRadius, float leashRadius = -1, int type = 0) : base(hitbox)
        {
            moveset = new List<Move>();
            hits = new List<Hits.Hit>();
            moveQueue = new Queue<Move>();
            AddMoveset(type);

            this.health = health;
            maxHealth = health;

            healthMaxWidth = 128;
            preHitHealth = maxHealth;

            this.facingRotation = facingRotation;
            this.initialNoticeState = state;
            this.noticeState = state;
            this.aggroRadius = aggroRadius;
            this.leashRadius = leashRadius;

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
            UpdateGhostWeaponPositions();

            if (healthDelay > 0)
                healthDelay--;
            if (preHitHealth > health && healthDelay == 0)
                preHitHealth--;

            if (currentMove != null)
                currentMove.Update(world);

            if (noticeState == EnemyNoticeState.Aggrod)
            {
                if (currentMove == null || currentMove.done || currentMove.interruptable)
                {
                    SelectMove(world);
                }

                float distFromInit = Math.Abs((initialPosition - position).Length());
                if (leashRadius != -1 && distFromInit > leashRadius)
                {
                    noticeState = EnemyNoticeState.Returning;
                }

            }
            else if (noticeState == EnemyNoticeState.Returning)
            {
                velocity += Vector2.Normalize(initialPosition - position) * 1.2f;

                if (Math.Abs((initialPosition - position).Length()) < 8)
                    noticeState = initialNoticeState;
            }
            else if (noticeState == EnemyNoticeState.Alert)
            {   //spawn a hitarc with a radius of the aggroradius, dealing 0 damage with 0 knockback for only one frame.
                HitArc a = (HitArc)world.AddHitbox(new HitArc(position, aggroRadius, -45, 45, 1, 0, 0, StaggerType.None, this));
                a.min += facingRotation;
                a.max += facingRotation;

                if (a.Collided(target))
                {
                    noticeState = EnemyNoticeState.Aggrod;
                    a.done = true;  //so we don't even have to update it. Literally only used for collision.
                }
            }
            else if (noticeState == EnemyNoticeState.HighAlert)
            {
                if (Math.Abs((target.position - position).Length()) < aggroRadius)
                {
                    noticeState = EnemyNoticeState.Aggrod;
                }
            }
            else if (noticeState == EnemyNoticeState.Sleeping)
            {
                if (health < maxHealth) //if any damage is sustained, swap to alert mode.
                {
                    noticeState = EnemyNoticeState.Alert;
                }
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

        public virtual void AttackWithWeapon(World world, int weaponIndex, int force, float rotation, int delay = -1)
        {
            weapons[weaponIndex].Attack(this, force);
            Hits.Hit h = weapons[weaponIndex].ModifyHitForEntity(rotation, delay);
            hits.Add(h);
            world.AddHitbox(h);
        }

        public override bool TakeDamage(World world, int amt, StaggerType type, Vector2 direction = new Vector2() { X = 0, Y = 0 })
        {
            if (!invulnerable)
            {
                velocity = direction / 2;
                facingRotation = VectorHelper.GetVectorAngle(-direction);
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

        protected virtual void UpdateGhostWeaponPositions()
        {
            if (!attacking)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null)
                    {
                        gwRestPos[i] = noticeState == EnemyNoticeState.Aggrod ? Vector2.Normalize(target.position - position) * 32 : VectorHelper.GetAngleNormVector(facingRotation) * 32;

                        gwRestRot[i] = noticeState == EnemyNoticeState.Aggrod ? VectorHelper.GetAngleBetweenPoints(position, target.position) : facingRotation;
                    }
                }
            }
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
                    if (noticeState == EnemyNoticeState.Aggrod)
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
        }

        protected virtual void AddMoveset(int type)
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

                    if (distanceFromPlayer > chaseRadius)
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

        #region move presets
        protected Move MoveSingleSlashPokeOrSlam(int weaponIndex, int weaponMoveIndex, int preDelay, int postDelay, float weightDistance, int weight, 
            int velocityDecaysTime = 0, float lungeScale = 0)
        {
            return new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                if (move.counter2 > 0)
                {
                    move.counter2--;
                }
                else
                {
                    if (move.counter1 == 0)
                    {
                        move.counter1 = 1;
                        move.counter2 = preDelay;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                        AttackWithWeapon(world, weaponIndex, weaponMoveIndex, move.counter3, preDelay);
                    }
                    else if (move.counter1 == 1)
                    {
                        move.counter1 = 2;
                        move.counter2 = postDelay;

                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * lungeScale;
                        velocityDecaysTimer = velocityDecaysTime;
                    }
                    else if (move.counter1 == 2)
                    {
                        return true;
                    }
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return distanceFromPlayer <= weightDistance ? weight : 0;
            }));
        }

        protected Move MoveSlamJumpback(int weaponIndex, int weaponMoveIndex, int jumpbackDelay, float jumpbackScale, int postJumpDelay, 
            int velocityDecaysTime, float weightDistance, int weight)
        {
            return new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                if (move.counter2 > 0)
                {
                    move.counter2--;
                }
                else
                {
                    if (move.counter1 == 0)
                    {
                        move.counter1 = 1;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);
                        move.counter2 = jumpbackDelay;
                        AttackWithWeapon(world, 0, 2, move.counter3, jumpbackDelay);
                    }
                    else if (move.counter1 == 1)
                    {
                        move.counter1 = 2;
                        move.counter2 = postJumpDelay;
                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * -jumpbackScale;
                        velocityDecaysTimer = velocityDecaysTime;
                        zVel = 20;
                    }
                    else if (move.counter1 == 2)
                    {
                        return true;
                    }
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return distanceFromPlayer <= weightDistance ? weight : 0;
            }));
        }

        protected Move MoveDoubleSlash(int weaponIndex, int weaponMoveIndexBack, int weaponMoveIndexForth, int preDelay, 
            float lungeScale, int velocityDecaysTime, int forthDelay, int postDelay, float weightDistance, int weight)
        {
            return new Move(this, new Func<World, Enemy, Move, bool>((world, enemy, move) =>
            {
                attacking = true;
                if (move.counter2 > 0)
                    move.counter2--;
                else
                {
                    if (move.counter1 == 0)
                    {   //pre attack delay
                        move.counter1 = 1;
                        move.counter2 = preDelay;
                        move.counter3 = VectorHelper.GetAngleBetweenPoints(position, target.position);

                        AttackWithWeapon(world, weaponIndex, weaponMoveIndexBack, move.counter3, (int)move.counter2);
                    }
                    else if (move.counter1 == 1)
                    {   //attack one, no delay
                        move.counter1 = 2;
                        move.counter2 = forthDelay;

                        velocityDecaysTimer = velocityDecaysTime;
                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * lungeScale;

                        AttackWithWeapon(world, weaponIndex, weaponMoveIndexForth, move.counter3, (int)move.counter2);
                    }
                    else if (move.counter1 == 2)
                    {   //attack two, no delay
                        move.counter1 = 3;
                        move.counter2 = weapons[0].attack.runTime + postDelay;

                        velocityDecaysTimer = velocityDecaysTime;
                        velocity = VectorHelper.GetAngleNormVector(move.counter3) * lungeScale;
                    }
                    else if (move.counter1 == 3)
                    {   //return
                        attacking = false;
                        return true;
                    }
                }
                return false;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return distanceFromPlayer <= weightDistance ? weight : 0;
            }));
        }
        #endregion
    }
}
