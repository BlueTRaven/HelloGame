using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Guis.Widgets;

namespace HelloGame.Entities
{
    public class EntitySpawner : ISelectable
    {
        public Rectangle bounds;
        public int type;

        public bool spawnRandomPosition;

        public float startRotation;
        public EnemyNoticeState startNoticeState;

        public int spawnDistance = 1024;

        public List<Entity> attachedEntities;
        public bool hasAttachedEntities;

        public bool spawned { get; private set; }

        public string info1, info2;

        public EntitySpawner(Rectangle bounds, int type, float startRotation, bool spawnRandomPosition = false, EnemyNoticeState noticeState = EnemyNoticeState.Alert, string info1 = "", string info2 = "")
        {
            this.bounds = bounds;
            this.type = type;
            this.startRotation = startRotation;
            this.startNoticeState = noticeState;
            this.spawnRandomPosition = spawnRandomPosition;

            this.info1 = info1;
            this.info2 = info2;

            attachedEntities = new List<Entity>();
        }

        public void SpawnEntity(World world)
        {
            GetEntitiesToSpawn(world);

            if (!spawned)
            {
                foreach (Entity e in attachedEntities)
                {
                    if (e is Player)
                    {
                        world.player = (Player)e;
                        if (world.entities.Contains(world.player) && !world.damageTakers.Contains(world.player))
                            world.damageTakers.Add(world.player);
                        world.player.SetPosition(bounds.Center.ToVector2());
                    }

                    if (e is EntityLiving)
                        world.AddEntityLiving((EntityLiving)e);
                    else
                        world.AddEntity(e);
                }

                spawned = true;
            }
        }

        public void GetEntitiesToSpawn(World world)
        {
            if (!hasAttachedEntities)
            {
                List<Entity> entities = new List<Entity>();
                if (type == 0)
                {   //this entityspawner merely serves as a placemarker. The actual movement of the player to the spawner is done inside the player's loading code.
                    //entities.Add(new Player(world.collisionWorld.Create(0, 0, 32, 32)));
                }
                else if (type == 1)
                {
                    int type = 0;
                    int.TryParse(info1, out type);

                    entities.Add(new Undead(world, startNoticeState, startRotation, type));
                }
                else if (type == 2)
                {
                    bool left = info1 != null && (info1.ToLower() == "true" || info1.ToLower() == "left");
                    entities.Add(new EnemyDoor(world, 128, 256, left));
                }
                else if (type == 3)
                {
                    entities.Add(new DemonMan(world));
                }

                if (entities.Count > 0)
                {
                    foreach (Entity e in entities)
                    {
                        if (spawnRandomPosition)
                        {
                            if (e is EntityLiving)
                                ((EntityLiving)e).SetPosition(Main.rand.NextPointInside(bounds));
                            else e.position = Main.rand.NextPointInside(bounds);
                        }
                        else
                        {
                            if (e is EntityLiving)
                                ((EntityLiving)e).SetPosition(bounds.Center.ToVector2());
                            else e.position = bounds.Center.ToVector2();
                        }
                    }
                }

                attachedEntities = entities;
            }
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.Red);
            batch.DrawHollowCircle(bounds.Center.ToVector2(), 8, spawned ? Color.Orange : Color.Red, 2, 32);
            Vector2 rotVec = VectorHelper.GetAngleNormVector(startRotation);
            batch.DrawLine(bounds.Center.ToVector2() + rotVec * 8, bounds.Center.ToVector2() + rotVec * 16, Color.Red, 2);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), type.ToString(), bounds.Location.ToVector2(), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info1, bounds.Location.ToVector2() + new Vector2(0, 16), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info2, bounds.Location.ToVector2() + new Vector2(0, 32), Color.White);
        }

        public void DrawSelect_DEBUG(SpriteBatch batch)
        {
            if (!trueSelected)
            {
                batch.DrawHollowRectangle(bounds, 4, Color.White);
                batch.DrawHollowCircle(bounds.Center.ToVector2(), 10, Color.White, 4, 32);
            }
            else
            {
                batch.DrawHollowRectangle(bounds, 4, Color.Black);
                batch.DrawHollowCircle(bounds.Center.ToVector2(), 10, Color.Black, 4, 32);
            }
        }

        public SerEntitySpawner Save()
        {
            return new SerEntitySpawner()
            {
                Position = bounds.Save(),
                Type = type,
                SpawnRandomPosition = spawnRandomPosition,

                Info1 = info1,
                Info2 = info2,
                StartRotation = startRotation,
                StartNoticeState = (int)startNoticeState
            };
        }

        //ISelectableStuff
        public int index { get; set; }

        public bool trueSelected { get; set; }

        public void Delete_DEBUG(World world)
        {
            world.spawners[index] = null;
        }

        public void UpdateWindowProperties(WidgetWindowEditProperties window)
        {
            window.mode = 2;
            window.selected = this;

            WidgetWindowRectangle rect = window.GetWindow<WidgetWindowRectangle>("entity_bounds");
            rect.Set(bounds);
            WidgetTextBox text = window.GetWidget<WidgetTextBox>("entity_type");
            text.SetString(type.ToString());

            text = window.GetWidget<WidgetTextBox>("entity_info1");
            text.SetString(info1);

            text = window.GetWidget<WidgetTextBox>("entity_info2");
            text.SetString(info2);

            WidgetCheckbox check = window.GetWidget<WidgetCheckbox>("entity_spawnrandom");
            check.isChecked = spawnRandomPosition;

            text = window.GetWidget<WidgetTextBox>("entity_spawnrotation");
            text.SetString(startRotation.ToString());

            window.GetWidget<WidgetDropdown>("entity_spawnstate").SetIndex((int)startNoticeState);
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            bounds = window.GetWindow<WidgetWindowRectangle>("entity_bounds").GetRectangle();
            type = int.Parse(window.GetWidget<WidgetTextBox>("entity_type").GetStringSafely());

            info1 = window.GetWidget<WidgetTextBox>("entity_info1").GetStringSafely();
            info2 = window.GetWidget<WidgetTextBox>("entity_info2").GetStringSafely();

            spawnRandomPosition = window.GetWidget<WidgetCheckbox>("entity_spawnrandom").isChecked;

            float temp = startRotation;
            startRotation = 0;
            float.TryParse(window.GetWidget<WidgetTextBox>("entity_spawnrotation").GetStringSafely(temp.ToString()), out startRotation);

            startNoticeState = (EnemyNoticeState)window.GetWidget<WidgetDropdown>("entity_spawnstate").GetIndex();
        }

        public void Move(Vector2 amt)
        {
            bounds = new Rectangle(bounds.Location + amt.ToPoint(), bounds.Size);
            WidgetWindowRectangle rect = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").GetWindow<WidgetWindowRectangle>("entity_bounds");
            rect.Set(bounds);
        }
    }
}
