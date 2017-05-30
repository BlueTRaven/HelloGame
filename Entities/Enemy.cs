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
using HelloGame.Entities.Particles;
using HelloGame.Items;

using Humper;
using Humper.Responses;

namespace HelloGame.Entities
{
    public enum EnemyNoticeState
    {
        Alert,      //player must be in line of sight.
        HighAlert,  //player can be anywhere in its aggro range.
        Sleeping,   //will not aggro unless hit, in which case it transfers into alert state.
        Aggrod,     //Aware of the player; able to perform moves.
        Returning   //returning to its initial position, after the player has moved out of its chase radius.
    }

    public abstract class Enemy : EntityLiving
    {
        public string name;

        protected List<Move> moveset;

        public Queue<Move> moveQueue;
        protected Move currentMove;

        public float distanceFromPlayer { get; protected set; }
        public Player target { get; protected set; }

        public List<Hits.Hit> hits;

        protected Enums.DirectionClock chaseDirection;
        protected float chaseSpeed, chaseMaxSpeed;
        protected float chaseRadius;
        protected float circleSpeed, circleMaxSpeed;
        protected Vector2 chasePos;

        private float healthMaxWidth;
        private float preHitHealth;
        private int healthDelay;

        private bool boss;
        private int bossIndex;
        private Vector2 bossAnchor;
        protected int bossPhase;
        protected string bossDescription;

        protected bool introducted;
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

        protected Queue<Item> drops;

        /// <summary>
        /// Creates A new Enemy.
        /// </summary>
        /// <param name="hitbox">The hitbox of the enemy. Can be 0, 0 relative; is moved manually by EntitySpawner.</param>
        /// <param name="health">The starting HP of the enemy.</param>
        /// <param name="state">The starting Notice State of the enemy. When in doubt, use Alert.</param>
        /// <param name="aggroRadius">How close the player has to be to the enemy to get noticed. Only applicable in HighAlert notice state.</param>
        /// <param name="leashRadius">How far away from its spawn point an enemy will chase the player. Set to -1 for infinte chase range.</param>
        public Enemy(Vector2 hitboxSize, int health, float facingRotation, EnemyNoticeState state, float aggroRadius, float leashRadius = -1, int type = 0) : base(hitboxSize)
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

            drops = new Queue<Item>();

            gravity = .25f;
            maxZVel = 4;
        }

        protected void SetMaxHealth(int health)
        {
            this.health = health;
            this.maxHealth = health;
            preHitHealth = health;
        }

        protected void SetChase(Enums.DirectionClock chaseDirection, float chaseRadius, float chaseSpeed, float chaseMaxSpeed, float circleSpeed, float circleMaxSpeed)
        {
            this.chaseDirection = chaseDirection;
            this.chaseRadius = chaseRadius;
            this.chaseSpeed = chaseSpeed;
            this.chaseMaxSpeed = chaseMaxSpeed;
            this.circleSpeed = circleSpeed;
            this.circleMaxSpeed = circleMaxSpeed;
        }

        protected void AddGhostWeapon(GhostWeapon weapon)
        {
            weapons.Add(weapon);

            Array.Resize(ref gwRestPos, weapons.Count);
            Array.Resize(ref gwRestRot, weapons.Count);
        }

        public override void PreUpdate(World world)
        {
            base.PreUpdate(world);

            if (boss && world.player.kills.Contains(bossIndex))
                Die(world, true, false);

            target = world.player;
            targetPos = world.player.position;
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

                if (!introducted)
                {
                    OnAggroed(world);
                    introducted = true;
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

        public virtual void OnAggroed(World world)
        {
            if (boss)
            {
                GuiHud.SetBigText(name, bossDescription, Main.assets.GetFont("bfMunro72"), Main.assets.GetFont("bfMunro23_bold"), Color.White);
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

        public override void Die(World world, bool force = false, bool dropItems = true)
        {
            base.Die(world, force, dropItems);

            if (boss && !world.player.kills.Contains(bossIndex))
            {
                world.player.kills.Add(bossIndex);
            }
            if (boss)
            {   //NOTE: don't add spawner. spawner should already be created, but inaccessable.
                Prop pr = new Prop(bossAnchor, new TextureInfo(new TextureContainer("tree1"), Vector2.One, Color.White), .5f);
                pr.noSave = true;
                world.AddProp(pr);
                Trigger t = new Trigger(new Rectangle((bossAnchor - new Vector2(32)).ToPoint(), new Point(64)), "save", "2", "");
                t.noSave = true;
                world.AddTrigger(t);

                if (!force)
                {   //we use this so we don't spawn the particles every time the player loads from the boss's save point.
                    Main.camera.SetFade(new Color(255, 255, 255, 127), true, 45);
                    GuiHud.SetBigText("Victory Achieved", null, Main.assets.GetFont("bfMunro72"), null, Color.White);
                    for (int i = 0; i < 128; i++)
                    {   //kill all other particles
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

            if (dropItems)
            {
                while (drops.Count > 0)
                    world.player.items.Add(drops.Dequeue());
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            weapons.ForEach(x => x.Draw(batch));

            if (boss)
            {
                if (Main.DEBUG)
                    batch.DrawLine(position, bossAnchor, Color.Red, 2);
            }
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
                        float width = Main.WIDTH - 96;

                        Vector2 pos = Camera.ToWorldCoords(new Vector2(48, Main.HEIGHT - 64));

                        batch.DrawString(Main.assets.GetFont("bfMunro12"), name, pos - new Vector2(0, 16), Color.White);
                        batch.DrawRectangle(new Rectangle((int)pos.X, (int)pos.Y, (int)width, 16), Color.Gray);
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
                if (move.counter1 == 0)
                {
                    move.counter1 = 1;
                    move.counter2 = 120 + Main.rand.Next(0, 120);
                    move.counter3 = Main.rand.Next(2);  //next 0-1
                    chasePos = Vector2.Normalize(new Vector2(-1, 0).RotateBy(Main.rand.NextFloat(0, 360))) * (chaseRadius - 16);
                }

                if (move.counter2 > 0)
                {
                    move.counter2--;

                    if (distanceFromPlayer > chaseRadius)   //move towards the target position
                    {
                        velocity += Vector2.Normalize(target.position - position) * chaseSpeed;
                        maxSpeed = chaseMaxSpeed;
                    }
                    else
                    {
                        chasePos = chasePos.RotateBy(circleSpeed * move.counter3 == 0 ? -1 : 1);
                        velocity += Vector2.Normalize(target.position + (chasePos - position)) * circleSpeed;
                        maxSpeed = circleMaxSpeed;
                    }
                    
                    return false;
                }
                maxSpeed = 8;
                return true;
            }), new Func<World, Enemy, Move, int>((world, enemy, move) =>
            {
                return enemy.distanceFromPlayer > chaseRadius + 16 ? 1 : 0;
            })));
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
            {
                Move next = moveset.RandomElementByWeight(new Func<Move, float>(move => move.weightScenario(world, this, move)));
                if (next == currentMove)    //less duplicates probably
                    next = moveset.RandomElementByWeight(new Func<Move, float>(move => move.weightScenario(world, this, move)));
                currentMove = next;
            }

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

        /// <summary>
        /// Sets the current enemy to be a boss.
        /// </summary>
        /// <param name="bossIndex">The "boss index" used by the player to save what bosses they've killed.</param>
        /// <param name="bossAnchor">The place where the save location will appear. NOT relative to the enemy's position.</param>
        protected void SetBoss(int bossIndex, Vector2 bossAnchor)
        {
            this.boss = true;
            this.bossIndex = bossIndex;
            this.bossAnchor = bossAnchor;
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

        public Action<Entity, Vector2, TextureInfo> GetPlayerAnimation()
        {
            return new Action<Entity, Vector2, TextureInfo>((entity, focusTarget, texInfo) =>
            {   //this fancy thing converts the angle to its nearest angle to 90, then divides by 90 so it's 0-3. 0 = left, 1 = up, 2 = right, 3 = down
                int direction = entity.GetFacingDirection(focusTarget);

                if (zVel > 0)
                {
                    if (direction == 0)
                        texInfo.AddAnimationToQueue(6);
                    else if (direction == 1)
                        texInfo.AddAnimationToQueue(8);
                    else if (direction == 2)
                        texInfo.AddAnimationToQueue(5);
                    else if (direction == 3)
                        texInfo.AddAnimationToQueue(7);
                    return;
                }

                if (direction == 0)
                    texInfo.AddAnimationToQueue(2);
                else if (direction == 1)
                    texInfo.AddAnimationToQueue(4);
                else if (direction == 2)
                    texInfo.AddAnimationToQueue(1);
                else if (direction == 3)
                    texInfo.AddAnimationToQueue(3);

            });
        }
    }
}
