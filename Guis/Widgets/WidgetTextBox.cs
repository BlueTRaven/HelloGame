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
    public enum TextBoxFilter
    {
        Alphabetical,
        Numerical,
        AlphaNumeric
    }

    public class WidgetTextBox : Widget
    {
        private bool hasBackgroundColor;
        private Color currentBackgroundColor, colorUnhover, colorActive;

        private bool showCursor;
        private int cursorTimer;
        private int cursorShowTime = 20, cursorResetTime = 50;

        public bool hold;

        private SpriteFont font;

        public bool hasStartText { get; private set; }
        public string startText;
        public StringBuilder text;
        public int max;

        private WidgetTextBox tabProgressTo;

        public TextAlignment alignment;

        public TextBoxFilter filter;

        public bool canTabProgressThisTick;
        public WidgetTextBox(Rectangle bounds, SpriteFont font, string startText, int max, TextAlignment alignment = TextAlignment.TopLeft, TextBoxFilter filter = TextBoxFilter.AlphaNumeric) : base(bounds)
        {
            this.font = font;
            this.startText = startText;
            text = new StringBuilder(startText);

            if (text.ToString() != "")
                hasStartText = true;

            this.max = max;

            this.alignment = alignment;
            this.filter = filter;
        }

        public WidgetTextBox SetTabProgressesTo(WidgetTextBox otherbox)
        {
            tabProgressTo = otherbox;

            return this;
        }

        public WidgetTextBox SetBackgroundColor(Color colorUnhover, Color colorActive)
        {
            hasBackgroundColor = true;
            this.colorUnhover = colorUnhover;
            this.colorActive = colorActive;
            return this;
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            canTabProgressThisTick = true;
        }

        public override void Update()
        {
            base.Update();

            hasStartText = text.ToString() == startText;
            switch(state)
            {
                case state_held:
                    currentBackgroundColor = colorActive;
                    break;
                default:
                    currentBackgroundColor = colorUnhover;
                    break;
            }

            if (active && state == state_held)
            UpdateCursorTimer();
        }

        private void UpdateCursorTimer()
        {
            cursorTimer++;

            if (cursorTimer < cursorShowTime)
                showCursor = true;
            else showCursor = false;

            if (cursorTimer >= cursorResetTime)
                cursorTimer = 0;
        }

        public override void OnClick()
        {
            base.OnClick();
            hold = true;
        }

        public override bool IsHeld()
        {
            return hold;
        }

        public override void OnHeld()
        {
            base.OnHeld();
            Main.stopKeyboardInput = true;

            if (tabProgressTo != null && (Main.keyboard.KeyPressed(Keys.Tab, true) || Main.keyboard.KeyHeldAfterTime(Keys.Tab, 90, true)) && canTabProgressThisTick)
            {
                tabProgressTo.OnClick();    //wew
                tabProgressTo.canTabProgressThisTick = false;
                return;
            }
            char[] chars = Main.keyboard.GetKeyboardInput();

            if (chars != null)
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] != '\0')
                    {
                        if (text.Length != max || hasStartText)
                        {
                            if (filter == TextBoxFilter.Numerical)
                                if (!char.IsDigit(chars[i]) && !(chars[i] == 46 && !HasDecimal()))
                                    continue;
                            if (filter == TextBoxFilter.Alphabetical)
                                if (!char.IsLetter(chars[i]) && !char.IsSymbol(chars[i]) && !char.IsPunctuation(chars[i]) && chars[i] != ' ')
                                    continue;
                            if (filter == TextBoxFilter.AlphaNumeric)
                                if (!char.IsLetterOrDigit(chars[i]) && !char.IsSymbol(chars[i]) && !char.IsPunctuation(chars[i]) && chars[i] != ' ')
                                    continue;
                            if (hasStartText)
                            {
                                hasStartText = false;
                                text.Clear();
                            }

                            if (font.Characters.Contains(chars[i]))
                                text.Append(chars[i]);
                            else text.Append("{?}");
                        }
                        else break;
                    }
                }
            }

            if (Main.keyboard.KeyHeldAfterTime(Keys.Back, 60, true) || Main.keyboard.KeyPressed(Keys.Back, true))
            {
                if (hasStartText)
                {
                    hasStartText = false;
                    text.Clear();
                }
                else
                {
                    if (text.Length > 0)
                    {
                        text.Remove(text.Length - 1, 1);
                    }
                }
            }
        }

        public void SetString(string setTo)
        {
            text = new StringBuilder(setTo);
            hasStartText = false;
        }

        public string GetStringSafely(string defalt = "-1") //mispelled on purpose
        {
            if (hasStartText)
            {
                if (filter == TextBoxFilter.Numerical)
                {
                    try
                    {
                        return int.Parse(text.ToString()).ToString();   //yeah lazy
                    }
                    catch
                    {
                        return defalt == "-1" ? "0" : defalt;
                    }
                }
                else if (filter == TextBoxFilter.Alphabetical || filter == TextBoxFilter.AlphaNumeric)
                    return defalt == "-1" ? "0" : defalt;
            }

            if (filter == TextBoxFilter.Numerical && text.ToString() == "")
                return "0";
            return text.ToString();
        }

        public bool HasDecimal()
        {
            return text.ToString().Contains('.');
        }

        public override bool IsReleased()
        {
            return state == state_held && ((Main.mouse.LeftButtonPressed() && !bounds.Contains(Main.mouse.currentPosition)) || Main.keyboard.KeyPressed(Keys.Enter, true) || ((Main.keyboard.KeyPressed(Keys.Tab, true) && tabProgressTo != null) && canTabProgressThisTick));
        }

        public override void OnRelease()
        {
            base.OnRelease();

            hold = false;
            Main.stopKeyboardInput = false;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (active)
            {
                if (hasBackgroundColor)
                {
                    batch.DrawRectangle(bounds, currentBackgroundColor);
                }

                batch.DrawString(font, text.ToString(), bounds.Location.ToVector2() + TextHelper.GetAlignmentOffset(font, text.ToString(), bounds, alignment), Color.White);

                if (hold)
                {
                    if (showCursor)
                    {
                        if (text.Length > 0 && text.Length < max)
                        {
                            Vector2 size = font.MeasureString(text.ToString());
                            batch.DrawRectangle(new Rectangle((bounds.Location.ToVector2() + TextHelper.GetAlignmentOffset(font, text.ToString(), bounds, alignment) + new Vector2(size.X, 0)).ToPoint(), new Point(4, (int)size.Y)), Color.Black);
                        }
                    }
                }
            }
        }
    }
}
