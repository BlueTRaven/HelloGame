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

namespace HelloGame
{
    public class Trigger : ISelectable
    {
        private static Dictionary<string, Action<World, Trigger>> commands;

        private string info1, info2, commandName;
        private Action<World, Trigger> command;

        public bool constant;

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
            if (commands.Keys.Contains(command))
                this.command = commands[command];

        }

        public void Update(World world)
        {
            command?.Invoke(world, this);
        }

        public bool PlayerEntered(World world)
        {
            if (bounds.Contains(world.player.hitbox) || bounds.Intersects(world.player.hitbox) || world.player.hitbox.Contains(bounds) || (contTrigger && triggered))
            {
                triggered = true;
                if (!triggered && !constant) return true;
                else if (triggered && !constant) return false;
                else if (constant) { triggerTime++; return true; }
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

        public static void LoadTriggerActions()
        {
            if (commands == null)
                commands = new Dictionary<string, Action<World, Trigger>>();

            commands.Add("default", new Action<World, Trigger>((world, trigger) => { }));

            commands.Add("save", new Action<World, Trigger>((world, trigger) => 
            {
                int parse = 0;

                int.TryParse(trigger.info1, out parse);

                world.player.Save(parse, world.name, world.player.saveName);
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
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.IndianRed);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), "TRIGGER", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), commandName, bounds.Location.ToVector2(), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info1, bounds.Location.ToVector2() + new Vector2(0, 16), Color.White);
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
            if (!commands.ContainsKey(comname))
                comname = "default";
            command = commands[comname];

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
}
