using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HelloGame.Guis.Widgets
{
    public abstract class Widget
    {
        public const int state_hovered = 1, state_clicked = 2, state_held = 3, state_released = 4, state_default = 0;

        public Vector2 createdPosition;
        public Vector2 anchor;
        public Rectangle bounds;

        public int state { get; protected set; }

        private bool hasKeybind;
        private Keys keybind;

        public bool active = true;
        public Widget(Rectangle bounds)
        {
            this.bounds = bounds;

            this.createdPosition = bounds.Location.ToVector2();
        }

        public Widget SetKeybind(Keys keybind)
        {
            hasKeybind = true;
            this.keybind = keybind;
            return this;
        }

        public virtual void PreUpdate()
        {
            if (anchor != null && anchor != Vector2.Zero)
            {
                bounds = new Rectangle((int)(createdPosition.X + anchor.X), (int)(createdPosition.Y + anchor.Y), bounds.Width, bounds.Height);
            }

            if (active)
                UpdateState();
        }

        public virtual void Update()
        {
        }

        public virtual void PostUpdate()
        {
            if (active)
            {
                //if (state == state_released || state == state_hovered)
                    state = state_default;
            }
            else state = state_default;
        }

        public virtual void UpdateState()
        {
            if (IsHovered())
            {
                OnHover();
            }

            if (IsClicked())
            {
                OnClick();
            }

            if (IsHeld())
            {
                OnHeld();
            }

            if (IsReleased())
            {
                OnRelease();
            }
        }

        public virtual bool IsHovered()
        {
            return bounds.Contains(Main.mouse.currentPosition);
        }

        public virtual bool IsClicked()
        {
            return (IsHovered() && Main.mouse.LeftButtonPressed()) || (hasKeybind && Main.keyboard.KeyPressed(keybind));
        }

        public virtual bool IsHeld()
        {
            return !IsClicked() && IsHovered() && Main.mouse.LeftButtonHeld();
        }

        public virtual bool IsReleased()
        {
            //return (state == state_held && !IsHeld());
            return false;
        }

        public virtual void OnHover()
        {
            state = state_hovered;
        }

        public virtual void OnClick()
        {
            state = state_clicked;
        }

        public virtual void OnHeld()
        {
            state = state_held;
        }

        public virtual void OnRelease()
        {
            state = state_released;
        }

        public abstract void Draw(SpriteBatch batch);
    }
}
