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

        public Player(IBox hitbox) : base(hitbox)
        {
            texInfo = new TextureInfo(new TextureContainer("animationtest"), new Vector2(6), Color.White).SetAnimated(8, 8,
                new Animation(0, 8, 8, 2, true, new int[] { 45, 30 }),  //idle
                new Animation(8, 8, 8, 3, true, new int[] { 15, 15, 15 }),  //walk down
                new Animation(16, 8, 8, 3, true, new int[] { 15, 15, 15 }), //walk left
                new Animation(24, 8, 8, 3, true, new int[] { 15, 15, 15 }), //walk right
                new Animation(32, 8, 8, 3, true, new int[] { 15, 15, 15 }));//walk up

            health = 25;
            maxHealth = 25;

            stamina = 100;
            staminaMax = 100;

            ((GuiHud)Main.guis["hud"]).SetHealth(health, maxHealth, maxHealth * 2, health);
            ((GuiHud)Main.guis["hud"]).SetStamina(stamina, staminaMax, staminaMax * 2, stamina);

            weapon = new GhostWeaponIronSword();
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
                    texInfo.AddAnimationToQueue(2, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.upKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.Y -= movespeed;
                    texInfo.AddAnimationToQueue(4, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.rightKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.X += movespeed;
                    texInfo.AddAnimationToQueue(3, true);
                }
                if (Main.keyboard.KeyHeld(Main.options.downKeybind))
                {
                    pressed++;
                    moved = true;
                    velocity.Y += movespeed;
                    texInfo.AddAnimationToQueue(1, true);
                }

                if (pressed > 1)
                    velocity *= .95f;

                if (!moved && velocity.Length() < .5f)
                {
                    texInfo.AddAnimationToQueue(0, true);

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
    }
}
