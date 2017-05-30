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
    public class WidgetDropdown : WidgetButton
    {
        string initialOption;
        string currentOption;
        int numOptions;

        string[] names;
        bool[] hovered, held, clicked;
        Rectangle[] allbounds;

        bool selecting;

        int currentIndex;

        public WidgetDropdown(Rectangle bounds, SpriteFont font, string baseText, Color textColor, Enums.Alignment alignment, 
            int numOptions, params string[] names) : base(bounds)
        {
            initialOption = baseText;
            currentOption = initialOption;
            SetHasText(font, baseText, textColor, alignment);
            SetBackgroundColor(Color.White, Color.LightGray, Color.DarkGray, Color.Gray, Color.White);

            this.numOptions = numOptions;
            this.names = names;
            hovered = new bool[names.Length];
            held = new bool[names.Length];
            clicked = new bool[names.Length];

            allbounds = new Rectangle[names.Length];

            for (int i = 1; i <= numOptions; i++)
            {
                allbounds[i - 1] = new Rectangle(bounds.X, bounds.Y + bounds.Height * i, bounds.Width, bounds.Height);
            }
        }

        public override bool IsHovered()
        {
            if (!selecting)
                return base.IsHovered();
            else
            {   //don't actually want to return true, as it will cause hovering problems.
                for (int i = 0; i < numOptions; i++)
                {   //reset all hovereds
                    hovered[i] = false;

                    if (allbounds[i].Contains(Main.mouse.currentPosition))
                        hovered[i] = true;
                }
            }

            return false;
        }

        public override bool IsClicked()
        {
            if (!selecting)
                return base.IsClicked();
            else
            {
                for (int i = 0; i < numOptions; i++)
                {
                    clicked[i] = false;

                    if (allbounds[i].Contains(Main.mouse.currentPosition) && Main.mouse.LeftButtonPressed())
                    {
                        clicked[i] = true;

                        currentOption = names[i];
                        text = currentOption;
                        selecting = false;

                        currentIndex = i;
                    }
                }
            }
            return false;
        }

        public override void OnClick()
        {
            base.OnClick();

            selecting = true;
        }

        public override void Update()
        {
            base.Update();

            for (int i = 1; i <= numOptions; i++)
            {
                allbounds[i - 1] = new Rectangle((int)(createdPosition.X + anchor.X), (int)(createdPosition.Y + anchor.Y) + bounds.Height * i, bounds.Width, bounds.Height);
            }

            if (selecting && Main.mouse.LeftButtonPressed() && allbounds.All(x => !x.Contains(Main.mouse.currentPosition)) && !bounds.Contains(Main.mouse.currentPosition))
            {   //if none of the rectangles contain the mouse, or the initial bounds
                selecting = false;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            if (selecting)
            {
                for (int i = 0; i < numOptions; i++)
                {
                    Color color = Color.White;
                    if (hovered[i])
                        color = Color.LightGray;
                    if (clicked[i])
                        color = Color.DarkGray;

                    batch.DrawRectangle(allbounds[i], color, 1);

                    batch.DrawString(font, names[i], allbounds[i].Location.ToVector2() + TextHelper.GetAlignmentOffset(font, names[i], bounds, alignment), textColor);
                }
            }
            else
            { 
                batch.Draw(Main.assets.GetTexture("arrowDown"), new Rectangle(bounds.X + bounds.Width - 32, bounds.Y, 32, 32), Color.White);
            }
        }

        public int GetIndex()
        {
            return currentIndex;
        }

        public void SetIndex(int index)
        {
            currentIndex = index;
            if (names.Length < index) currentOption = names[index];
            text = currentOption;
        }
    }
}
