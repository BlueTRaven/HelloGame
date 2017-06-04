using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HelloGame.Utility
{
    public static class TextHelper
    {
        public static Vector2 GetAlignmentOffset(SpriteFont font, string text, Rectangle bounds, Enums.Alignment alignment)
        {
            Vector2 size = Vector2.Zero;
            try { size = font.MeasureString(text); }
            catch { Console.WriteLine("Text was 0, 0."); return Vector2.Zero; }

            if (alignment ==Enums.Alignment.TopLeft)
                return Vector2.Zero;
            else if (alignment ==Enums.Alignment.Left)
                return new Vector2(0, (bounds.Height / 2) - (size.Y / 2));
            else if (alignment ==Enums.Alignment.BottomLeft)
                return new Vector2(0, bounds.Height - size.Y);
            else if (alignment ==Enums.Alignment.Bottom)
                return new Vector2((bounds.Width / 2) - (size.X / 2), bounds.Height - size.Y);
            else if (alignment ==Enums.Alignment.BottomRight)
                return new Vector2(bounds.Width - size.X, bounds.Height - size.Y);
            else if (alignment ==Enums.Alignment.Right)
                return new Vector2(bounds.Width - size.X, (bounds.Height / 2) - (size.Y / 2));
            else if (alignment ==Enums.Alignment.TopRight)
                return new Vector2(bounds.Width - size.X, 0);
            else if (alignment ==Enums.Alignment.Top)
                return new Vector2((bounds.Width / 2) - (size.X / 2), 0);
            else if (alignment ==Enums.Alignment.Center)
                return new Vector2((bounds.Width / 2) - (size.X / 2), (bounds.Height / 2) - (size.Y / 2));
            else return Vector2.Zero;
        }

        public static float StringWidth(SpriteFont font, string text)
        {
            return font.MeasureString(text).X;
        }

        public static string WrapText(SpriteFont font, string text, float lineWidth)
        {
            const string space = " ";
            string[] words = text.Split(new string[] { space }, StringSplitOptions.None);
            float spaceWidth = StringWidth(font, space),
                spaceLeft = lineWidth,
                wordWidth;
            StringBuilder result = new StringBuilder();

            foreach (string word in words)
            {
                wordWidth = StringWidth(font, word);
                if (word.Contains("\n"))
                    spaceLeft = lineWidth;
                else if (wordWidth + spaceWidth > spaceLeft)
                {
                    result.AppendLine();
                    spaceLeft = lineWidth - wordWidth;
                }
                else
                {
                    spaceLeft -= (wordWidth + spaceWidth);
                }
                result.Append(word + space);
            }

            return result.ToString();
        }

        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
