using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;

namespace HelloGame.Guis.Widgets
{
    public class WidgetWindowScrollable : WidgetWindow
    {
        public float height;

        private Vector2 clickDistance;

        private bool holding;

        private WidgetButton scrollbar;

        private RenderTarget2D scrollRenderTarget;
        private Rectangle scrollSourceRect;
        /// <summary>
        /// Creates A new WidgetWindowScrollable
        /// </summary>
        /// <param name="bounds">The visible bounds of the window.</param>
        /// <param name="height">The full height of all the widgets in the window, or the point it should stop scrolling.</param>
        public WidgetWindowScrollable(Rectangle bounds, float height, Dictionary<string, Widget> widgets) : base(bounds, false, widgets)
        {
            this.height = height;
            float fheight = ((float)bounds.Height / (float)height);
            scrollbar = new WidgetButton(new Rectangle(bounds.X + bounds.Width, 0, 16, 
                (int)((float)bounds.Height * fheight))).SetBackgroundColor(Color.White, Color.LightGray, Color.White, Color.Gray, Color.White);  //Scrollbar
            scrollbar.anchor = new Vector2(0, bounds.Y); //honestly I have no idea why this works
            scrollSourceRect = new Rectangle(0, 0, bounds.Width, bounds.Height);

            shouldAnchor = false;
        }

        public override void PreUpdate()
        {
            base.PreUpdate();
            scrollbar.PreUpdate();
        }
        public override void Update()
        {
            base.Update();

            scrollbar.Update();

            if (scrollbar.state == Widget.state_clicked)
            {
                clickDistance = scrollbar.createdPosition - Main.mouse.currentPosition;
                holding = true;
            }

            scrollbar.createdPosition.Y -= Main.mouse.deltaScrollWheelValue;

            if (Main.mouse.LeftButtonHeld() && holding)
            {
                scrollbar.createdPosition.Y = (Main.mouse.currentPosition + clickDistance).Y;
            }
            else holding = false;

            scrollbar.createdPosition.Y = MathHelper.Clamp(scrollbar.createdPosition.Y, 0, bounds.Height - scrollbar.bounds.Height);
            scrollbar.OnHeld();

            float p = (float)scrollbar.createdPosition.Y / (float)(bounds.Height - scrollbar.bounds.Height);
            float y = MathHelper.Lerp(0, height - scrollSourceRect.Height, p);
            foreach (Widget w in widgets.Values)
            {   //I'm a genius
                w.anchor.Y = -y;
            }
        }
        public override void PostUpdate()
        {
            base.PostUpdate();
            scrollbar.PostUpdate();
        }

        public override void Draw(SpriteBatch batch)
        {
            if (scrollRenderTarget == null)
                scrollRenderTarget = new RenderTarget2D(batch.GraphicsDevice, Main.WIDTH, Main.HEIGHT);    //cutoff is 4096

            batch.End();

            batch.GraphicsDevice.SetRenderTarget(scrollRenderTarget);
            batch.GraphicsDevice.Clear(Color.DarkGray);

            batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null);

            base.Draw(batch);   //draw normal widgets

            batch.End();

            batch.GraphicsDevice.SetRenderTarget(null);

            batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null);

            batch.Draw(scrollRenderTarget, bounds, scrollSourceRect, Color.White);
            batch.DrawLine(new Vector2(bounds.X + bounds.Width, bounds.Y), new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height), Color.Black);
            batch.DrawRectangle(new Rectangle(bounds.X + bounds.Width, bounds.Y, 16, bounds.Height), Color.DarkGray);
            scrollbar.Draw(batch);  //draw the scrollbar on default rendertarget
        }
    }
}
