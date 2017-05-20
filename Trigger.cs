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

namespace HelloGame
{
    public class Trigger : ISelectable
    {
        private string info1, info2, commandName;
        private Command command;

        private bool triggered;
        private bool contTrigger;

        public Rectangle bounds;

        public int triggerTime { get; private set; }

        public Trigger(Rectangle bounds, string command, string info1, string info2)
        {
            this.bounds = bounds;
            this.commandName = command;
            this.info1 = info1;
            this.info2 = info2;

            this.command = GetCommand(command);
        }

        public void OnCreated(World world)
        {
            command.startAction?.Invoke(world, this);
        }

        public void Update(World world)
        {
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

        public Command GetCommand(string name)
        {
            Dictionary<string, Command> commands = new Dictionary<string, Command>();

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
        }

        /*public static void LoadTriggerActions()
        {
            if (commands == null)
                commands = new Dictionary<string, Action<World, Trigger>>();

            commands.Add("default", new Action<World, Trigger>((world, trigger) => { }));

            commands.Add("door_double_nokey", new Action<World, Trigger>((world, trigger) => 
            {
                trigger.constant = true;
                if (trigger.PlayerEntered(world))
                {
                    if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    {

                    }
                }
            }));

            commands.Add("save", new Action<World, Trigger>((world, trigger) => 
            {
                trigger.constant = true;
                if (trigger.PlayerEntered(world))
                {
                    if (Main.keyboard.KeyPressed(Main.options.interactKeybind))
                    {
                        int parse = 0;

                        int.TryParse(trigger.info1, out parse);

                        world.player.Save(parse, world.name, world.player.saveName);
                        world.player.healing = world.player.maxHealth;
                    }

                    ((GuiHud)Main.guis["hud"]).showButtonPrompt = true;
                    ((GuiHud)Main.guis["hud"]).buttonPromptAction = "Save";
                }
            }));

            commands.Add("loadmap", new Action<World, Trigger>((world, trigger) => 
            {
                trigger.constant = false;

                if (trigger.PlayerEntered(world))
                {
                    int parse = 0;
                    int.TryParse(trigger.info1, out parse);
                    SerPlayer p = world.player.Save(parse, trigger.info2, world.player.saveName);
                    Player.Load(world, p);
                    world.player.healing = world.player.maxHealth;
                }
            }));

            commands.Add("mapfunc", new Action<World, Trigger>((world, trigger) =>
            {
                trigger.contTrigger = true;
                trigger.constant = true;

                if (trigger.PlayerEntered(world))
                {
                    int parsed = 0;
                    int.TryParse(trigger.info1, out parsed);

                    world.MapFunc(parsed, trigger);
                }
            }));
        }*/

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.IndianRed);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), "TRIGGER", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), commandName, bounds.Location.ToVector2(), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info1, bounds.Location.ToVector2() + new Vector2(0, 16), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info2, bounds.Location.ToVector2() + new Vector2(0, 32), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), triggered ? "triggered" : "", bounds.Location.ToVector2() + new Vector2(0, 48), Color.White);
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
            if (GetCommand(comname) == null)
                comname = "default";
            command = GetCommand(comname);

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
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="runAction">Runs until returning true.</param>
        public Command(Func<World, Trigger, bool> runAction, Action<World, Trigger> startAction = null)
        {
            this.runAction = runAction;
            this.startAction = startAction;
        }

        public void Update(World world, Trigger trigger)
        {
            if (!done)  //run until return true
                done = runAction.Invoke(world, trigger);
        }
    }
}
