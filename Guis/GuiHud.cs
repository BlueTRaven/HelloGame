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

namespace HelloGame.Guis
{
    public class GuiHud : Gui
    {
        private float currentHealth;
        private float healthMax;
        private float healthMaxWidth;
        private float preHitHealth;
        private int healthDelay;

        private float currentStamina;
        private float staminaMax;
        private float staminaMaxWidth;
        private float preHitStamina;
        private int staminaDelay;

        private int promptTime;
        public bool showPrompt;
        public string promptText = "Press {0} to <Action>";

        private int bigtextTime, bigTextFadeDelay, bigTextFadeInTime;
        private const int bigtextTimeMax = 60;
        private Color currentColor, baseColor, toColor;
        private SpriteFont font, smallFont;
        private string text, littletext;

        public GuiHud() : base("hud")
        {
            AddWidget("openeditor_dummy", new WidgetButton(new Rectangle(0, 0, 0, 0)))
                .SetOpensGui("editor")
                .SetKeybind(Keys.O);
        }

        public override void Update()
        {
            if (promptTime <= 0)
            {
                showPrompt = false;
                promptText = "Press {0} to <Action>";
            }
            else
            {
                promptTime--;
            }

            base.Update();

            if (healthDelay > 0)
                healthDelay--;
            if (preHitHealth > currentHealth && healthDelay == 0)
                preHitHealth--;

            if (staminaDelay > 0)
                staminaDelay--;
            if (preHitStamina > currentStamina && staminaDelay == 0)
                preHitStamina--;

            if (bigTextFadeInTime > 0)
            {
                bigTextFadeInTime--;
                currentColor = Color.Lerp(baseColor, toColor, (float)bigTextFadeInTime / (float)(bigtextTimeMax / 2));
            }
            else
            {
                if (bigTextFadeDelay <= 0)
                {
                    if (bigtextTime > 0)
                    {
                        bigtextTime--;
                        currentColor = Color.Lerp(toColor, baseColor, (float)bigtextTime / (float)bigtextTimeMax);
                    }
                }
                else bigTextFadeDelay--;
            }
        }

        public void ShowPrompt(int time, string actionName = "Press {0} to <Action>", bool force = false)
        {
            showPrompt = true;

            if (promptTime > 0 && !force)
                return;

            promptTime = time;
            promptText = actionName;
        }

        public void ShowBigText(string text, string littletext, SpriteFont font, SpriteFont smallFont, Color color)
        {
            this.text = text;
            this.littletext = littletext;

            this.font = font;
            this.smallFont = smallFont;

            bigTextFadeDelay = 120;
            bigtextTime = bigtextTimeMax;
            bigTextFadeInTime = bigtextTimeMax / 2;
            currentColor = color;
            baseColor = color;
            toColor = Color.Transparent;
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            float percent = currentHealth / healthMax;
            float percentpre = preHitHealth / healthMax;
            batch.DrawRectangle(new Rectangle(8, 8, (int)healthMaxWidth, 32), Color.Gray);
            batch.DrawRectangle(new Rectangle(8, 8, (int)(percentpre * healthMaxWidth), 32), Color.Orange);
            batch.DrawRectangle(new Rectangle(8, 8, (int)(percent * healthMaxWidth), 32), Color.Red);

            batch.DrawRectangle(new Rectangle((int)(percent * healthMaxWidth) + 8, 8, 2, 32), Color.White);
            batch.DrawRectangle(new Rectangle((int)(percentpre * healthMaxWidth) + 8, 8, 2, 32), Color.White);
            batch.DrawHollowRectangle(new Rectangle(8, 8, (int)healthMaxWidth, 32), 2, Color.DarkGray);

            percent = currentStamina / staminaMax;
            percentpre = preHitStamina / staminaMax;
            batch.DrawRectangle(new Rectangle(8, 48, (int)staminaMaxWidth, 32), Color.Gray);
            batch.DrawRectangle(new Rectangle(8, 48, (int)(percentpre * staminaMaxWidth), 32), Color.LightGreen);
            batch.DrawRectangle(new Rectangle(8, 48, (int)(percent * staminaMaxWidth), 32), Color.Green);
            
            batch.DrawRectangle(new Rectangle((int)(percent * staminaMaxWidth) + 8, 48, 2, 32), Color.White);
            batch.DrawRectangle(new Rectangle((int)(percentpre * staminaMaxWidth) + 8, 48, 2, 32), Color.White);
            batch.DrawHollowRectangle(new Rectangle(8, 48, (int)staminaMaxWidth, 32), 2, Color.DarkGray);

            if (showPrompt)
            {
                batch.DrawRectangle(new Rectangle(Main.WIDTH / 2 - 128, Main.HEIGHT - 128, 256, 16), Color.Black);
                batch.DrawString(Main.assets.GetFont("bfMunro12"), string.Format(promptText, Main.options.interactKeybind), new Vector2(Main.WIDTH / 2 - 128, Main.HEIGHT - 128) + TextHelper.GetAlignmentOffset(Main.assets.GetFont("bfMunro12"), string.Format(promptText, Main.options.interactKeybind), new Rectangle(Main.WIDTH / 2 - 128, Main.HEIGHT - 128, 256, 16), Enums.Alignment.Center), Color.White);
            }

            if (bigtextTime > 0 && (bigTextFadeInTime != bigtextTimeMax / 2))
            {
                if (font != null && smallFont != null)
                {   //both big and small font
                    Vector2 s1 = font.MeasureString(text);
                    Rectangle rect = new Rectangle(Main.WIDTH / 2 - (int)s1.X / 2, Main.HEIGHT / 2 - (int)s1.Y / 2, (int)s1.X, (int)s1.Y);
                    batch.DrawString(font, text, rect.Location.ToVector2(), currentColor);

                    Vector2 s2 = smallFont.MeasureString(littletext);
                    rect = new Rectangle(Main.WIDTH / 2 - (int)s2.X / 2, Main.HEIGHT / 2 - (int)s2.Y / 2, (int)s2.X, (int)s2.Y);
                    batch.DrawString(smallFont, littletext, new Vector2(rect.X, rect.Y + s1.Y / 2), currentColor);
                }

                if (smallFont != null && font == null)
                {   //only small font
                    Vector2 s2 = smallFont.MeasureString(littletext);
                    Rectangle rect = new Rectangle(Main.WIDTH / 2 - (int)s2.X / 2, Main.HEIGHT / 2 - (int)s2.Y / 2, (int)s2.X, (int)s2.Y);
                    batch.DrawString(smallFont, littletext, new Vector2(rect.X, rect.Y), currentColor);
                }

                if (font != null && smallFont == null)
                {   //only big font
                    Vector2 s1 = font.MeasureString(text);
                    Rectangle rect = new Rectangle(Main.WIDTH / 2 - (int)s1.X / 2, Main.HEIGHT / 2 - (int)s1.Y / 2, (int)s1.X, (int)s1.Y);
                    batch.DrawString(font, text, rect.Location.ToVector2(), currentColor);
                }
            }
        }

        public void SetHealth(float current, float max, float maxWidth, float preHit)
        {
            currentHealth = current;
            healthMax = max;
            healthMaxWidth = maxWidth;
            if (preHitHealth < preHit)
            preHitHealth = preHit;
            healthDelay = 15;
        }

        public void SetStamina(float current, float max, float maxWidth, float preval, int delay = 10)
        {
            currentStamina = current;
            staminaMax = max;
            staminaMaxWidth = maxWidth;
            if (preHitStamina < preval)
                preHitStamina = preval;

            staminaDelay = delay;
        }

        public static void SetPromptText(int time, string actionName = "Press {0} to <Action>", bool force = false)
        {
            ((GuiHud)Main.guis["hud"]).ShowPrompt(time, actionName, force);
        }

        public static void SetBigText(string text, string littletext, SpriteFont font, SpriteFont smallFont, Color color)
        {
            ((GuiHud)Main.guis["hud"]).ShowBigText(text, littletext, font, smallFont, color);
        }
    }
}
