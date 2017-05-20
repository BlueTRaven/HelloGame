using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Google.Protobuf;

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
    public class Player : EntityLiving
    {
        public float movespeed { get; private set; }

        private bool canInput = true;
        private int inputTimer;

        private bool willRoll;

        private bool rolling;

        public int stamina, staminaMax, staminaRegainDelay;
        private bool staminaLossPunish;

        private GhostWeapon weapon;

        public int entrancePoint;

        public string saveName { get; set; }

        public int healing;
        public List<int> kills;

        public List<Item> items;

        public Player() : base(new Vector2(32))
        {
            texInfos[0] = GetPlayerCharacterTexInfo("charBase", new Vector2(5));

            health = 25;
            maxHealth = 25;

            stamina = 100;
            staminaMax = 100;

            ((GuiHud)Main.guis["hud"]).SetHealth(health, maxHealth, maxHealth * 2, health);
            ((GuiHud)Main.guis["hud"]).SetStamina(stamina, staminaMax, staminaMax * 2, stamina);

            weapon = new GhostWeaponIronSword();

            kills = new List<int>();
            items = new List<Item>();
        }

        protected void ResetStats()
        {
            velocityDecays = true;

            if (inputTimer > 0)
            {
                inputTimer--;
                canInput = false;
            }
            else canInput = true;

            if (canInput)
            {
                movespeed = .5f;
                maxSpeed = 4;
            }

            noclip = false;
            if (Main.DEBUG)
            {
                invulnerableTime = 60;
                movespeed = 4;
                maxSpeed = 20;
                noclip = true;
            }
        }

        public override void PreUpdate(World world)
        {
            if (world.collisionWorld != null && collideBox == null)
            {
                collideBox = world.collisionWorld.Create(0, 0, 32, 32);
            }

            if (healing > 0)
            {
                healing--;
                if (health <= maxHealth)
                    health++;
                else
                {
                    health = maxHealth;
                    healing = 0;
                }

                ((GuiHud)Main.guis["hud"]).SetHealth(health, maxHealth, maxHealth * 2, health);
            }

            ResetStats();

            if (staggerTime > 0)
                movespeed = 0;

            if (stamina < staminaMax)
            {
                if (staminaRegainDelay > 0)
                    staminaRegainDelay--;
                else
                {
                    ((GuiHud)Main.guis["hud"]).SetStamina(stamina + 1, staminaMax, staminaMax * 2, stamina + 1, 0);
                    stamina++;
                }
            }
            else staminaLossPunish = false;

            base.PreUpdate(world);

            if (rolling)
            {
                velocityDecays = false;

                if (canInput)
                {
                    rolling = false;
                }
            }

            if (canInput)
            {
                bool moved = false;

                if (!staminaLossPunish)
                {
                    if (Main.keyboard.KeyHeld(Keys.Space))
                        willRoll = true;
                    if (Main.keyboard.KeyHeldAfterTime(Keys.Space, 30))
                    {
                        willRoll = false;
                        movespeed = 1.2f;
                        maxSpeed = 6;

                        ConsumeStanima(1);
                    }
                    if (!Main.keyboard.KeyHeld(Keys.Space) && willRoll)
                    {
                        willRoll = false;
                        inputTimer = 8;
                        invulnerableTime = 8;
                        velocityDecaysTimer = 8;
                        rolling = true;

                        maxSpeed = 12;
                        movespeed = 12;

                        ConsumeStanima(30);
                    }
                }

                int pressed = 0;
                if (Main.keyboard.KeyHeld(Main.options.leftKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.X -= movespeed;
                    texInfos[0].AddAnimationToQueue(2, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.upKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.Y -= movespeed;
                    texInfos[0].AddAnimationToQueue(4, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.rightKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.X += movespeed;
                    texInfos[0].AddAnimationToQueue(1, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.downKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.Y += movespeed;
                    texInfos[0].AddAnimationToQueue(3, true);
                }

                if (pressed > 1)
                    velocity *= .95f;

                if (!moved || velocity.Length() < .5f)
                {
                    texInfos[0].AddAnimationToQueue(0, true);

                    rolling = false;
                    inputTimer = 0;
                }

                if (Main.keyboard.KeyHeld(Keys.R))
                {
                    health++;
                    ((GuiHud)Main.guis["hud"]).SetHealth(health, maxHealth, maxHealth * 2, health);
                }

                if (Main.mouse.LeftButtonPressed())
                {
                    float ang = VectorHelper.GetAngleBetweenPoints(position, Main.mouse.GetWorldPosition());
                    /*int[] a = HitArc.GetMinMax(ang, 90);
                    world.AddHitbox(new HitArc(position, 128, a[0], a[1], 10, 50, 30, StaggerType.Short, this).SetAnimated(15, false));*/
                    weapon.Attack(this);
                    world.AddHitbox(weapon.ModifyHitForEntity(ang));

                    staggerTime = weapon.attack.resetTime;
                }
            }

            Main.camera.target = position;
        }

        public override void Update(World world)
        {
            base.Update(world);

            weapon.Update(world, position, velocity != Vector2.Zero ? Vector2.Normalize(velocity) * 16 : Vector2.Zero, VectorHelper.GetVectorAngle(velocity).RoundDown(90));
        }

        public void AddItem(Item item)
        {   //Adder for later
            items.Add(item);
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            weapon.Draw(batch);
        }

        public override bool TakeDamage(World world, int amt, StaggerType type, Vector2 direction = new Vector2() { X = 0, Y = 0 })
        {
            if (!invulnerable)
            {
                velocity = direction;
                velocityDecaysTimer = (int)Math.Abs(direction.Length());    //this is a dumb idea tbh
                this.staggerTime = GetStaggerTime(type);
                ((GuiHud)Main.guis["hud"]).SetHealth(health - amt, maxHealth, maxHealth * 2, health);
                health -= amt;
                if (health <= 0)
                {
                    Die(world);
                    return true;
                }
            }
            return false;
        }

        public void ConsumeStanima(int amt)
        {
            ((GuiHud)Main.guis["hud"]).SetStamina(stamina - amt, staminaMax, staminaMax * 2, stamina);

            stamina -= amt;

            staminaRegainDelay = 45;
            if (stamina < 0)
            {
                stamina = 0;
                staminaRegainDelay = 90;
                staminaLossPunish = true;
            }
        }

        public override void Die(World world, bool force = false, bool dropItems = true)
        {
            base.Die(world, force);

            Load(world, this, saveName, maxHealth);
        }

        public bool HasItem<T>(int type) where T : Item
        {
            return items.Where(x => x is T && x.type == type).Count() > 0;
        }

        public T GetItem<T>(int type) where T : Item
        {
            return (T)(items.Where(x => x is T && x.type == type).ToList()[0]); //always return at the 0 index. Duplicates will not make a difference.
        }

        #region Serialization
        public SerPlayer Save(int entranceIndex, string mapname, string saveName)
        {
            string fSavename;
            SerPlayer p = new SerPlayer()
            {
                EntrancePoint = entranceIndex,
                Health = health,
                MapName = mapname,
            };

            p.Kills.AddRange(kills);

            if (saveName.EndsWith(".hgsf"))
                fSavename = saveName.Split('.')[0];
            else fSavename = saveName;

            using (var output = File.Create("Saves/" + fSavename + ".hgsf"))
            {
                p.WriteTo(output);
            }

            this.saveName = saveName;
            return p;
        }

        /// <summary>
        /// Load function used when we want to load a preexisting save file.
        /// </summary>
        /// <param name="player">Player who's properties and fields will be modified.</param>
        /// <param name="saveName">The save name of the file to load, to modify the player's properties and fields.</param>
        public static void Load(World world, Player player, string saveName, int overrideHp = -1)
        {
            SerPlayer p = null;
            try
            {
                using (var input = File.OpenRead("Saves/" + saveName))
                {
                    p = SerPlayer.Parser.ParseFrom(input);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading the save file " + saveName + ".\n" + e);
                return;
            }

            player.entrancePoint = p.EntrancePoint;
            player.health = overrideHp == -1 ? p.Health : overrideHp;
            ((GuiHud)Main.guis["hud"]).SetHealth(player.health, player.maxHealth, player.maxHealth * 2, player.health);
            player.kills = p.Kills.ToList();
            player.velocity = Vector2.Zero;
            player.dead = false;
            world.Load(p.MapName);
            player.collideBox = world.collisionWorld.Create(0, 0, 32, 32); //we have to recreate the IBox because I'm dumb apparently

            for (int i = 0; i < world.spawners.Length; i++)
            {
                if (world.spawners[i] != null)
                {
                    if (world.spawners[i].type == 0)
                    {   //check for player spawners
                        int parse = 0;
                        int.TryParse(world.spawners[i].info1, out parse);
                        if (parse == player.entrancePoint)
                        {   //check to see if the entrance point of the player's save file is the same as the current spawner's
                            world.spawners[i].attachedEntities.Add(player); //manually add the already created player.
                            world.spawners[i].hasAttachedEntities = true;   //have to do this to prevent it from overwriting the attachedEntities List
                            world.spawners[i].SpawnEntity(world, player);
                            break;
                        }
                    }
                }
            }

            Main.camera.Position = player.position;

            player.saveName = saveName;
        }

        /// <summary>
        /// Load function used when a new savefile must be created.
        /// </summary>
        /// <param name="player">The serialized player.</param>
        /// <param name="forceLoadMapName">DEPRECATED</param>
        public static void Load(World world, SerPlayer player, string forceLoadMapName = "-1")
        {
            var temp = world.player.saveName;
            world.Load(forceLoadMapName == "-1" ? player.MapName : forceLoadMapName);
            world.player.collideBox = world.collisionWorld.Create(0, 0, 32, 32);    //we have to recreate the IBox because I'm dumb apparently
            world.player.kills = player.Kills.ToList();

            for (int i = 0; i < world.spawners.Length; i++)
            {
                if (world.spawners[i] != null)
                {
                    if (world.spawners[i].type == 0)
                    {   //check for player spawners
                        int parse = 0;
                        int.TryParse(world.spawners[i].info1, out parse);
                        if (parse == player.EntrancePoint)
                        {   //check to see if the entrance point of the player's save file is the same as the current spawner's
                            world.spawners[i].attachedEntities.Add(world.player); //manually add the already created player.
                            world.spawners[i].hasAttachedEntities = true;   //have to do this to prevent it from overwriting the attachedEntities List
                            world.spawners[i].SpawnEntity(world, world.player);
                            world.player.health = player.Health;
                            break;
                        }
                    }
                }
            }
            Main.camera.Position = world.player.position;

            world.player.saveName = temp;
        }
        #endregion
    }
}
