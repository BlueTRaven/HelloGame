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
        private static Dictionary<string, Action<World, Trigger, string>> commands;

        private string info, commandName;
        private Action<World, Trigger, string> command;

        public bool constant;

        public bool permTrigger;
        private bool triggered;

        public Rectangle bounds;

        public Trigger(Rectangle bounds, string command, string info, bool permTrigger, bool triggered = false)
        {
            this.bounds = bounds;
            this.commandName = command;
            this.info = info;
            if (commands.Keys.Contains(command))
                this.command = commands[command];

            this.permTrigger = permTrigger;
            this.triggered = triggered;
        }

        public void Update(World world)
        {
            command?.Invoke(world, this, info);
        }

        public bool PlayerEntered(World world)
        {
            if (bounds.Contains(world.player.hitbox) || bounds.Intersects(world.player.hitbox) || world.player.hitbox.Contains(bounds))
            {
                if (!triggered && !constant)
                {
                    triggered = true;
                    return true;
                }
                else if (triggered && !constant) return false;
                else if (constant) return true;
            }
            return false;
        }

        public SerTrigger Save()
        {
            return new SerTrigger()
            {
                Bounds = bounds.Save(),
                Command = commandName,
                Info = info,
                PermTrigger = permTrigger,
                Triggered = permTrigger ? triggered : false //if it's a permanant trigger, save the triggered value. Otherwise, just save false.
            };
        }

        public static void LoadTriggerActions()
        {
            if (commands == null)
                commands = new Dictionary<string, Action<World, Trigger, string>>();

            commands.Add("loadmap", new Action<World, Trigger, string>((world, trigger, info) => 
            {
                trigger.constant = false;

                if (trigger.PlayerEntered(world))
                {
                    world.Load(info);
                }
            }));

            commands.Add("mapfunc", new Action<World, Trigger, string>((world, trigger, info) =>
            {
                trigger.constant = true;
                trigger.permTrigger = true;

                if (trigger.PlayerEntered(world))
                {
                    int parsed = 0;
                    int.TryParse(info, out parsed);

                    world.MapFunc(parsed);
                }
            }));
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            batch.DrawHollowRectangle(bounds, 2, Color.IndianRed);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), "TRIGGER", bounds.Location.ToVector2() - new Vector2(0, 16), Color.White);

            batch.DrawString(Main.assets.GetFont("bfMunro12"), commandName, bounds.Location.ToVector2(), Color.White);
            batch.DrawString(Main.assets.GetFont("bfMunro12"), info, bounds.Location.ToVector2() + new Vector2(0, 16), Color.White);
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
            text = window.GetWidget<WidgetTextBox>("trigger_info");
            text.SetString(info.ToString());
            window.GetWidget<WidgetCheckbox>("trigger_perm").isChecked = permTrigger;
        }

        public void UpdateSelectableProperties(WidgetWindowEditProperties window)
        {
            bounds = window.GetWindow<WidgetWindowRectangle>("trigger_bounds").GetRectangle();
            command = commands[window.GetWidget<WidgetTextBox>("trigger_command").GetStringSafely()];
            info = window.GetWidget<WidgetTextBox>("trigger_info").GetStringSafely();
            permTrigger = window.GetWidget<WidgetCheckbox>("trigger_perm").isChecked;
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
