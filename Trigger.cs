using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Entities;
using HelloGame.Guis;
using HelloGame.Guis.Widgets;
using HelloGame.Utility;
using HelloGame.Items;
using static HelloGame.Entities.Player;

namespace HelloGame
{
    public class Trigger : ISelectable
    {
        private string info1, info2, commandName;
        public static Dictionary<string, Func<Command>> commands = new Dictionary<string, Func<Command>>();
        private Command command;

        private bool triggered;
        private bool contTrigger;

        public Rectangle bounds;

        public int triggerTime { get; private set; }

        public bool stopped;

        public bool noSave;

        public Trigger(Rectangle bounds, string command, string info1, string info2)
        {
            this.bounds = bounds;
            this.commandName = command;
            this.info1 = info1;
            this.info2 = info2;

            if (GetCommandNames().Contains(command))
                this.command = commands[command].Invoke();//GetCommand(command);
        }

        public void OnCreated(World world)
        {
            command.startAction?.Invoke(world, this);
        }

        public void Update(World world)
        {
            if (!stopped)
                command.Update(world, this);
        }

        public bool PlayerEntered(World world)
        {
            if (contTrigger && triggered)
                return true;

            if (bounds.Contains(world.player.hitbox) || bounds.Intersects(world.player.hitbox) || world.player.hitbox.Contains(bounds))
            {
                if (!triggered)   //if it has not triggered yet, do so.
                    triggered = true;
                return true;
            }
            return false;
        }

        public SerTrigger Save()
        {
            return new SerTrigger()
            {
                Bounds = bounds.Save(),
                Command = commandName,
                Info1 = info1,
                Info2 = info2
            };
        }

        public static List<string> GetCommandNames()
        {
            return commands.Keys.ToList();
        }

        public static void GetCommands()
        {
            Dictionary<string, Func<Command>> commands = new Dictionary<string, Func<Command>>();

            commands.Add("default", delegate () { return new Command("default", new Func<World, Trigger, bool>((world, trigger) => { return true; })); });
            commands.Add("door_fsouth_double_citadelkey", delegate ()
            {
                return new Command("door_fsouth_double_citadelkey", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.PlayerEntered(world))
                    {
                        ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to open");

                        if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                        {
                            if (world.player.HasItem<ItemKey>(0))
                            {
                                int parse1 = 0;
                                int.TryParse(trigger.info1, out parse1);

                                int parse2 = 0;
                                int.TryParse(trigger.info2, out parse2);

                                try
                                {
                                    ((EnemyDoor)world.entities[parse1]).opening = true;
                                    ((EnemyDoor)world.entities[parse2]).opening = true;


                                    GuiHud.SetPromptText(120, world.player.GetItem<ItemKey>(0).useText, true);

                                    world.player.openables.Add(new Openable(world.name, trigger.index));
                                }
                                catch
                                {
                                    Console.WriteLine("[Error] Door Trigger index mismatch at " + trigger.index + ". Try saving and reloading the map.");
                                }
                                return true;
                            }
                            else
                            {
                                GuiHud.SetPromptText(120, "The door is locked.", true);
                            }
                        }
                    }
                    return false;
                }), StartCommandSpawnDoubleDoors, "door1index", "door2index");
            });
            commands.Add("door_fsouth_double_nokey", delegate () {
                return new Command("door_fsouth_double_nokey", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.PlayerEntered(world))
                    {
                        ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to open");

                        if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                        {
                            int parse1 = 0;
                            int.TryParse(trigger.info1, out parse1);

                            int parse2 = 0;
                            int.TryParse(trigger.info2, out parse2);

                            try
                            {
                                ((EnemyDoor)world.entities[parse1]).opening = true;
                                ((EnemyDoor)world.entities[parse2]).opening = true;

                                world.player.openables.Add(new Openable(world.name, trigger.index));
                            }
                            catch
                            {
                                Console.WriteLine("[Error] Door Trigger index mismatch at " + trigger.index + ". Try saving and reloading the map.");
                            }
                            return true;
                        }
                    }
                    return false;
                }), StartCommandSpawnDoubleDoors, "door1index", "door2index");
            } );
            commands.Add("door_noplayer", delegate ()
            {
                return new Command("door_noplayer", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.triggered)
                    {
                        int parse1 = 0;
                        int.TryParse(trigger.info1, out parse1);

                        int parse2 = 0;
                        int.TryParse(trigger.info2, out parse2);

                        try
                        {
                            ((EnemyDoor)world.entities[parse1]).opening = true;
                            ((EnemyDoor)world.entities[parse2]).opening = true;

                            world.player.openables.Add(new Openable(world.name, trigger.index));
                        }
                        catch
                        {
                            Console.WriteLine("[Error] Door Trigger index mismatch at " + trigger.index + ". Try saving and reloading the map.");
                        }
                        return true;
                    }
                    return false;
                }), StartCommandSpawnDoubleDoors, "door1index", "door2index");
            });
            commands.Add("save", delegate () {
                return new Command("save", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.PlayerEntered(world))
                    {
                        ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to save");

                        if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                        {
                            int parse = 0;

                            int.TryParse(trigger.info1, out parse);

                            world.player.Save(parse, world.name, world.player.saveName);
                            world.player.healing = world.player.maxHealth;
                            ((GuiHud)Main.guis["hud"]).ShowPrompt(120, "Game has been saved.", true);
                        }
                    }
                    return false;   //constant so long as the player is inside
                }), null, "savename");
            });
            commands.Add("loadmap", delegate () {
                return new Command("loadmap", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.PlayerEntered(world))
                    {
                        int parse = 0;
                        int.TryParse(trigger.info1, out parse);
                        SerPlayer p = world.player.Save(parse, trigger.info2, world.player.saveName);
                        Player.Load(world, p);
                        world.player.healing = world.player.maxHealth;
                        return true;
                    }
                    return false;
                }), null, "entrance index", "mapname");
            });
            commands.Add("trigger_entitykilled", delegate ()
            {
                return new Command("trigger_entitykilled", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    int parse1 = 0;
                    int.TryParse(trigger.info1, out parse1);

                    bool alldead = true;

                    if (world.spawners[parse1] != null)
                    world.spawners[parse1].attachedEntities.ForEach(x => { if (!x.dead) alldead = false; });
                    else
                    {
                        Console.WriteLine("[Error] Trigger index mismatch. " + trigger.index + " tried checking spawner index " + parse1 +
                               ", which does not exist in the current context. Try saving and reloading the map.");
                        return true;    //return true because we want this error only happening once. returning false would merely cause the same error to repeat every frame.
                    }

                    if (alldead)
                    {
                        int parse2 = 0;
                        int.TryParse(trigger.info2, out parse2);

                        if (world.triggers[parse2] != null)
                            world.triggers[parse2].triggered = true;    //manually trip the trigger
                        else
                        {
                            Console.WriteLine("[Error] Trigger index mismatch. " + trigger.index + " tried triggering index " + parse2 +
                               ", which does not exist in the current context. Try saving and reloading the map.");
                        }
                        return true;
                    }
                    return false;
                }));
            });
            commands.Add("trigger_entitieskilled", delegate ()
            {
                return new Command("trigger_entitieskilled", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    string[] splitindexes = trigger.info1.Split(',');   //split by comma

                    bool alldead = true;

                    for (int i = 0; i < splitindexes.Length; i++)
                    {
                        int parse1 = 0;
                        int.TryParse(splitindexes[i], out parse1);

                        if (world.spawners[parse1] != null)
                            alldead = world.spawners[parse1].AllAttachedDead();
                        else
                        {
                            Console.WriteLine("[Error] Trigger index mismatch. " + trigger.index + " tried checking spawner index " + parse1 +
                                   ", which does not exist in the current context. Try saving and reloading the map.");
                            return true;    //return true because we want this error only happening once. returning false would merely cause the same error to repeat every frame.
                        }
                    }

                    if (alldead)
                    {
                        int parse2 = 0;
                        int.TryParse(trigger.info2, out parse2);

                        if (world.triggers[parse2] != null)
                            world.triggers[parse2].triggered = true;    //manually trip the trigger
                        else
                        {
                            Console.WriteLine("[Error] Trigger index mismatch. " + trigger.index + " tried triggering index " + parse2 +
                               ", which does not exist in the current context. Try saving and reloading the map.");
                        }
                        return true;
                    }
                    return false;
                }));
            });
            commands.Add("introduce_name", delegate ()
            {
                return new Command("introduce_name", new Func<World, Trigger, bool>((world, trigger) => 
                {
                    if (trigger.PlayerEntered(world))
                    {
                        GuiHud.SetBigText(world.displayName, null, Main.assets.GetFont("bitfontMunro72"), null, Color.White);
                        return true;
                    }
                    return false;
                }));
            });
            commands.Add("teleport", delegate ()
            {
                return new Command("teleport", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    if (trigger.PlayerEntered(world))
                    {   //note that we never return true; we want this to always run
                        float parse1 = 0;
                        float.TryParse(trigger.info1, out parse1);

                        float parse2 = 0;
                        float.TryParse(trigger.info2, out parse2);

                        world.player.SetPosition(new Vector2(parse1, parse2));
                    }
                    return false;
                }), null, "X Pos", "Y Pos");
            });
            commands.Add("mapfunc", delegate () {
                return new Command("mapfunc", new Func<World, Trigger, bool>((world, trigger) =>
                {
                    trigger.contTrigger = true;

                    if (trigger.PlayerEntered(world))
                    {
                        int parsed = 0;
                        int.TryParse(trigger.info1, out parsed);

                        return world.MapFunc(parsed, trigger);
                    }
                    return false;
                }));
            });
            Trigger.commands = commands;
        }

        #region getcommand
        /*public Command GetCommand(string name)
        {
            if (name == "default") return new Command(new Func<World, Trigger, bool>((world, trigger) => { return true; }));

            if (name == "door_fsouth_double_citadelkey") return new Command(new Func<World, Trigger, bool>((world, trigger) =>
            {
                if (trigger.PlayerEntered(world))
                {
                    ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to open");

                    if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    {
                        if (world.player.HasItem<ItemKey>(0))
                        {
                            int parse1 = 0;
                            int.TryParse(trigger.info1, out parse1);

                            int parse2 = 0;
                            int.TryParse(trigger.info2, out parse2);

                            try
                            {
                                ((EnemyDoor)world.entities[parse1]).opening = true;
                                ((EnemyDoor)world.entities[parse2]).opening = true;

                                
                                GuiHud.SetPromptText(120, world.player.GetItem<ItemKey>(0).useText, true);
                            }
                            catch
                            {
                                Console.WriteLine("[Error] Door Trigger index mismatch at " + index + ". Try saving and reloading the map.");
                            }
                            return true;
                        }
                        else
                        {
                            GuiHud.SetPromptText(120, "The door is locked.", true);
                        }
                    }
                }
                return false;
            }), new Action<World, Trigger>((world, trigger) =>
            {   //since we create these manually, they aught to show up correctly
                EntityLiving door1 = world.AddEntity(new EnemyDoor(world, 128, 128, true));
                EntityLiving door2 = world.AddEntity(new EnemyDoor(world, 128, 128, false));
                door1.OnSpawn(world, trigger.bounds.Center.ToVector2() + new Vector2(-32, -(bounds.Height)));
                door2.OnSpawn(world, trigger.bounds.Center.ToVector2() + new Vector2(32, -(bounds.Height)));

                trigger.info1 = door1.index.ToString();
                trigger.info2 = door2.index.ToString();
            }));

            if (name == "door_fsouth_double_nokey") return new Command(new Func<World, Trigger, bool>((world, trigger) =>
            {
                if (trigger.PlayerEntered(world))
                {
                    ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to open");

                    if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    {
                        int parse1 = 0;
                        int.TryParse(trigger.info1, out parse1);

                        int parse2 = 0;
                        int.TryParse(trigger.info2, out parse2);

                        try
                        {
                            ((EnemyDoor)world.entities[parse1]).opening = true;
                            ((EnemyDoor)world.entities[parse2]).opening = true;
                        }
                        catch
                        {
                            Console.WriteLine("[Error] Door Trigger index mismatch at " + index + ". Try saving and reloading the map.");
                        }
                        return true;
                    }
                }
                return false;
            }), new Action<World, Trigger>((world, trigger) => 
            {   //since we create these manually, they aught to show up correctly
                EntityLiving door1 = world.AddEntity(new EnemyDoor(world, 128, 128, true));
                EntityLiving door2 = world.AddEntity(new EnemyDoor(world, 128, 128, false));
                door1.OnSpawn(world, trigger.bounds.Center.ToVector2() + new Vector2(-32, -(bounds.Height)));
                door2.OnSpawn(world, trigger.bounds.Center.ToVector2() + new Vector2(32, -(bounds.Height)));

                trigger.info1 = door1.index.ToString();
                trigger.info2 = door2.index.ToString();
            }));

            if (name == "save") return new Command(new Func<World, Trigger, bool>((world, trigger) =>
            {
                if (trigger.PlayerEntered(world))
                {
                    ((GuiHud)Main.guis["hud"]).ShowPrompt(2, "Press {0} to save");

                    if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    {
                        int parse = 0;

                        int.TryParse(trigger.info1, out parse);

                        world.player.Save(parse, world.name, world.player.saveName);
                        world.player.healing = world.player.maxHealth;
                        ((GuiHud)Main.guis["hud"]).ShowPrompt(120, "Game has been saved.", true);
                    }
                }
                return false;   //constant so long as the player is inside
            }));

            if (name == "loadmap") return new Command(new Func<World, Trigger, bool>((world, trigger) =>
            {
                if (trigger.PlayerEntered(world))
                {
                    int parse = 0;
                    int.TryParse(trigger.info1, out parse);
                    SerPlayer p = world.player.Save(parse, trigger.info2, world.player.saveName);
                    Player.Load(world, p);
                    world.player.healing = world.player.maxHealth;
                    return true;
                }
                return false;
            }));

            if (name == "mapfunc") return new Command(new Func<World, Trigger, bool>((world, trigger) =>
            {
                trigger.contTrigger = true;

                if (trigger.PlayerEntered(world))
                {
                    int parsed = 0;
                    int.TryParse(trigger.info1, out parsed);

                    return world.MapFunc(parsed, trigger);
                }
                return false;
            }));

            Console.WriteLine("Could not find key " + name + ". Are you sure it is spelled correctly? WARNING MAY CAUSE CRASHES");
            return null;
        }*/
        #endregion

        public static void StartCommandSpawnDoubleDoors(World world, Trigger trigger)
        {
            if (world.player.openables.FindAll((player) => { return player.index == trigger.index && player.mapname == world.name; }).Count > 0)
            {   //if this openable is saved to the player, then we don't even spawn the doors. just return.
                trigger.stopped = true;
                return;
            }
            EntityLiving door1 = world.AddEntity(new EnemyDoor(world, 128, 128, true));
            EntityLiving door2 = world.AddEntity(new EnemyDoor(world, 128, 128, false));
            door1.OnSpawn(null, world, trigger.bounds.Center.ToVector2() + new Vector2(-32, -8));
            door2.OnSpawn(null, world, trigger.bounds.Center.ToVector2() + new Vector2(32, -8));

            trigger.info1 = door1.index.ToString();
            trigger.info2 = door2.index.ToString();
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.IndianRed);

            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), "TRIGGER", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);

            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), commandName, bounds.Location.ToVector2(), Color.White);
            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), info1, bounds.Location.ToVector2() + new Vector2(0, 16), Color.White);
            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), info2, bounds.Location.ToVector2() + new Vector2(0, 32), Color.White);
            batch.DrawString(Main.assets.GetFont("bitfontMunro12"), triggered ? "triggered" : "", bounds.Location.ToVector2() + new Vector2(0, 48), Color.White);
        }

        #region Selectable
        public int index { get; set; }

        public bool trueSelected { get; set; }

        public void DrawSelect_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, trueSelected ? Color.Black : Color.White);
        }

        public void Delete_DEBUG(World world)
        {
            world.triggers[index] = null;
        }

        public void UpdateWindowProperties(WidgetWindowEditProperties window)
        {
            window.mode = 4;
            window.selected = this;

            WidgetWindowRectangle rect = window.GetWindow<WidgetWindowRectangle>("trigger_bounds");
            rect.Set(bounds);

            WidgetTextBox text = window.GetWidget<WidgetTextBox>("trigger_command");
            text.SetString(commandName.ToString());
            text = window.GetWidget<WidgetTextBox>("trigger_info1");
            text.SetString(info1.ToString());
            text = window.GetWidget<WidgetTextBox>("trigger_info2");
            text.SetString(info2.ToString());
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            bounds = window.GetWindow<WidgetWindowRectangle>("trigger_bounds").GetRectangle();
            string comname = window.GetWidget<WidgetTextBox>("trigger_command").GetStringSafely("default");
            if (commands.ContainsKey(comname))
                command = commands[comname]?.Invoke();
            else command = commands["default"]?.Invoke();

            info1 = window.GetWidget<WidgetTextBox>("trigger_info1").GetStringSafely();
            info2 = window.GetWidget<WidgetTextBox>("trigger_info2").GetStringSafely();
        }

        public void Move(Vector2 amt)
        {
            bounds = new Rectangle(bounds.Location + amt.ToPoint(), bounds.Size);
            WidgetWindowRectangle rect = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").GetWindow<WidgetWindowRectangle>("trigger_bounds");
            rect.Set(bounds);
        }
        #endregion
    }

    public class Command
    {
        private Func<World, Trigger, bool> runAction;
        public Action<World, Trigger> startAction;

        bool done;

        public string name { get; private set; }
        public string info1label { get; private set; }
        public string info2label { get; private set; }
        
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="runAction">Runs until returning true.</param>
        /// <param name="startAction">Runs on creation of the trigger. Use for things like spawning related entities (See door triggers)</param>
        public Command(string name, Func<World, Trigger, bool> runAction, Action<World, Trigger> startAction = null, string info1label = "", string info2label = "")
        {
            this.name = name;
            this.runAction = runAction;
            this.startAction = startAction;
            this.info1label = info1label;
            this.info2label = info2label;
        }

        public void Update(World world, Trigger trigger)
        {
            if (!done)  //run until return true
                done = runAction.Invoke(world, trigger);
        }
    }
}
