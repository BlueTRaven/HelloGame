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
    public class WidgetButton : Widget
    {
        protected bool hasText;
        protected SpriteFont font;
        protected string text;
        protected TextAlignment alignment;
        public Color textColor;

        private bool hasBackgroundColor;
        private Color colorUnhover, colorHover, colorClick, colorHold, colorRelease;
        private Color currentBackgroundColor;

        private bool hasMultipleTexture, hasGlobalTexture;
        private Texture2D textureUnhover, textureHover, textureClick, textureHold, textureRelease;
        private Color textureColorUnhover, textureColorHover, textureColorClick, textureColorHold, textureColorRelease;

        private bool opensGui;
        private string openGuiKey;

        public WidgetButton(Rectangle bounds) : base(bounds)
        {   //dummy class so we can use the base widget functionality.

        }

        public WidgetButton SetHasText(SpriteFont font, string text, Color textColor, TextAlignment alignment = TextAlignment.Center)
        {
            hasText = true;
            this.font = font;
            this.text = text;
            this.textColor = textColor;
            this.alignment = alignment;
            return this;
        }

        public WidgetButton SetBackgroundColor(Color colorUnhover, Color colorHover, Color colorClick, Color colorHold, Color colorRelease)
        {
            hasBackgroundColor = true;

            this.colorUnhover = colorUnhover;
            this.colorHover = colorHover;
            this.colorClick = colorClick;
            this.colorHold = colorHold;
            this.colorRelease = colorRelease;

            return this;
        }

        public WidgetButton SetHasMultipleTexture(Texture2D textureUnhover, Texture2D textureHover, Texture2D textureClick, Texture2D textureHold, Texture2D textureRelease)
        {
            if (hasGlobalTexture)
                hasGlobalTexture = false;
            hasMultipleTexture = true;
            this.textureUnhover = textureUnhover;
            this.textureHover = textureHover;
            this.textureClick = textureClick;
            this.textureHold = textureHold;
            this.textureRelease = textureRelease;
            return this;
        }

        public WidgetButton SetHasTexture(Texture2D texture)
        {
            if (hasMultipleTexture)
                hasMultipleTexture = false;
            hasGlobalTexture = true;

            textureUnhover = texture;

            return this;
        }

        public WidgetButton SetTextureColors(Color textureColorUnhover, Color textureColorHover, Color textureColorClick, Color textureColorHold, Color textureColorRelease)
        {
            if (hasMultipleTexture || hasGlobalTexture)
            {
                this.textureColorUnhover = textureColorUnhover;
                this.textureColorHover = textureColorHover;
                this.textureColorClick = textureColorClick;
                this.textureColorHold = textureColorHold;
                this.textureColorRelease = textureColorRelease;
            }
            return this;
        }

        public WidgetButton SetOpensGui(string guiKey)
        {
            opensGui = true;
            openGuiKey = guiKey;

            return this;
        }

        public override void Update()
        {
            base.Update();

            if (hasBackgroundColor)
            {
                switch (state)
                {
                    case state_default:
                        currentBackgroundColor = colorUnhover;
                        break;
                    case state_hovered:
                        currentBackgroundColor = colorHover;
                        break;
                    case state_clicked:
                        currentBackgroundColor = colorClick;
                        break;
                    case state_held:
                        currentBackgroundColor = colorHold;
                        break;
                    case state_released:
                        currentBackgroundColor = colorRelease;
                        break;
                }
            }
        }

        public override void OnClick()
        {
            base.OnClick();
            
            if (opensGui)
                Main.activeGui = Main.guis[openGuiKey];
        }

        public override void Draw(SpriteBatch batch)
        {
            if (active)
            {
                if (hasBackgroundColor)
                {
                    batch.DrawRectangle(bounds, currentBackgroundColor);
                }

                if (hasGlobalTexture)
                {
                    batch.Draw(textureUnhover, bounds, Color.White);
                }

                if (hasText)
                {
                    batch.DrawString(font, text, bounds.Location.ToVector2() + TextHelper.GetAlignmentOffset(font, text, bounds, alignment), textColor);
                }
            }
        }
    }
}
