using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Hits;
using HelloGame.GhostWeapons;
using HelloGame.Entities.Particles;
using HelloGame.Items;
using HelloGame.Guis;

using Humper;

namespace HelloGame.Guis.Widgets
{
    public class WidgetDialogueBox : WidgetButton
    {
        public string[] texts;
        public int currentIndex;

        private int timer;
        private const int timermax = 240;
        public WidgetDialogueBox(SpriteFont font, params string[] texts) : base(new Rectangle(0, Main.HEIGHT - 128, Main.WIDTH, 128))
        {
            this.texts = texts;
            this.font = font;   //we can reuse font since it's not gonna be used for anything else.

            for (int i = 0; i < this.texts.Length; i++)
            {   //wrap the texts
                this.texts[i] = TextHelper.WrapText(font, this.texts[i], Main.WIDTH - 8);
            }

            SetBackgroundColor(new Color(Color.DarkSlateBlue, 47), new Color(Color.DarkSlateBlue, 47), new Color(Color.DarkSlateBlue, 47), new Color(Color.DarkSlateBlue, 47), new Color(Color.DarkSlateBlue, 47));
        }

        public void Replace(SpriteFont font, params string[] texts)
        {
            this.texts = texts;
            this.font = font;   //we can reuse font since it's not gonna be used for anything else.

            for (int i = 0; i < this.texts.Length; i++)
            {   //wrap the texts
                this.texts[i] = TextHelper.WrapText(font, this.texts[i], Main.WIDTH - 8);
            }

            active = true;
            timer = timermax;
            currentIndex = 0;
        }

        public override void Update()
        {
            base.Update();

            timer--;
        }

        public override bool IsClicked()
        {
            return Main.keyboard.KeyPressed(Keys.Enter) || timer <= 0;
        }

        public override void OnClick()
        {
            base.OnClick();
            
            Increment();   
        }

        private void Increment()
        {
            currentIndex++;
            timer = timermax;

            if (currentIndex >= texts.Length)
            {
                currentIndex = 0;
                active = false;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);

            if (active)
            {
                if (currentIndex < texts.Length)
                    batch.DrawString(font, texts[currentIndex], bounds.Location.ToVector2() + new Vector2(16), Color.White);
            }
        }
    }
}
