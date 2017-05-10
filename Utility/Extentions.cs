using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HelloGame.Entities;

using Humper;

namespace HelloGame.Utility
{
    public static class Extentions
    {
        #region save/load
        public static Trigger Load(this SerTrigger trigger)
        {
            return new Trigger(trigger.Bounds.Load(), trigger.Command, trigger.Info, trigger.PermTrigger, trigger.PermTrigger);
        }

        public static Prop Load(this SerProp prop)
        {
            return new Prop(prop.Position.Load(), prop.TexInfo.Load(), prop.ShadowScale);
        }

        public static EntitySpawner Load(this SerEntitySpawner spawner)
        {
            return new EntitySpawner(spawner.Position.Load(), spawner.Type, spawner.SpawnRandomPosition, spawner.Info1, spawner.Info2);
        }

        public static Wall Load(this SerWall wall, World world)
        {
            return new Wall(world, wall.Bounds.Load());
        }

        public static Brush Load(this SerBrush brush)
        {
            return new Brush(brush.Bounds.Load(), brush.TextureInfo.Load(), (BrushDrawType)brush.DrawType, brush.DrawAhead);
        }

        public static TextureInfo Load(this SerTexInfo texInfo)
        {
            return new TextureInfo(new TextureContainer(texInfo.Name), texInfo.Scale.Load(), texInfo.Tint.Load());
        }

        public static SerRectangle Save(this Rectangle rectangle)
        {
            return new SerRectangle()
            {
                X = rectangle.X,
                Y = rectangle.Y,
                Width = rectangle.Width,
                Height = rectangle.Height
            };
        }

        public static Rectangle Load(this SerRectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static SerVector2 Save(this Vector2 vector)
        {
            return new SerVector2()
            {
                X = vector.X,
                Y = vector.Y
            };
        }

        public static Vector2 Load(this SerVector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static SerColor Save(this Color color)
        {
            return new SerColor()
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }

        public static Color Load(this SerColor color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
        #endregion
        
        public static void CheckAndDelete<T>(this List<T> sequence, Func<T, bool> deletableSelector)
        {
            foreach (T value in sequence.ToList())
            {
                if (deletableSelector(value))
                {
                    sequence.Remove(value);
                }
            }
        }

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            float totalWeight = sequence.Sum(weightSelector);
            // The weight we are after...
            float itemWeightIndex = Main.rand.NextFloat() * totalWeight;
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;
            }

            return default(T);
        }

        public static int RoundDown(this int integer, int size)
        {
            return (int)(Math.Floor((double)(integer / size)) * size);
        }

        public static int RoundUp(this int integer, int size)
        {
            return (int)(Math.Ceiling((double)(integer / size)) * size);
        }

        public static float RoundDown(this float integer, int size)
        {
            return (int)(Math.Floor((double)(integer / size)) * size);
        }

        public static float RoundUp(this float integer, int size)
        {
            return (int)(Math.Ceiling((double)(integer / size)) * size);
        }

        public static Vector2 ToVector2(this IMovement move)
        {
            return new Vector2(move.Destination.Left, move.Destination.Top);    //top left of the rectangle
        }

        public static Rectangle ToRectangle(this Humper.Base.RectangleF rectangle)
        {
            return new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        }

        #region draw
        public static void DrawRectangle(this SpriteBatch batch, Rectangle area, Color color, float depth = 0)
        {
            Texture2D whitePixel = Main.assets.whitePixel;

            batch.Draw(whitePixel, area, null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }

        public static void DrawHollowRectangle(this SpriteBatch batch, Rectangle area, int width, Color color, float depth = 0)
        {
            Texture2D whitePixel = Main.assets.whitePixel;

            batch.Draw(whitePixel, new Rectangle(area.X, area.Y, area.Width, width), null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
            batch.Draw(whitePixel, new Rectangle(area.X, area.Y, width, area.Height), null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
            batch.Draw(whitePixel, new Rectangle(area.X + area.Width - width, area.Y, width, area.Height), null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
            batch.Draw(whitePixel, new Rectangle(area.X, area.Y + area.Height - width, area.Width, width), null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }

        public static void DrawHollowCircle(this SpriteBatch batch, Vector2 center, float radius, Color color, int lineWidth = 2, int segments = 16)
        {
            Vector2[] vertex = new Vector2[segments];

            double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            for (int i = 0; i < segments; i++)
            {
                vertex[i] = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                theta += increment;
            }

            DrawHollowPolygon(batch, vertex, segments, color, lineWidth);
        }
        public static void DrawHollowPolygon(this SpriteBatch batch, Vector2[] vertex, int count, Color color, int lineWidth)
        {
            Texture2D whitePixel = Main.assets.whitePixel;

            if (count > 0)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    DrawLine(batch, vertex[i], vertex[i + 1], color, lineWidth);
                }
                DrawLine(batch, vertex[count - 1], vertex[0], color, lineWidth);
            }
        }

        public static void DrawLine(this SpriteBatch batch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Texture2D whitePixel = Main.assets.whitePixel;

            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            batch.Draw(whitePixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
        #endregion

        public static int MaxIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int maxIndex = -1;
            T maxValue = default(T); // Immediately overwritten anyway

            int index = 0;
            foreach (T value in sequence)
            {
                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        #region random
        public static Vector2 NextAngle(this Random rand)
        {
            return Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(rand.NextFloat(0, 360))));
        }

        public static Vector2 NextPointInside(this Random rand, Rectangle rectangle)
        {
            float x = rand.NextFloat(rectangle.X, rectangle.X + rectangle.Width);
            float y = rand.NextFloat(rectangle.Y, rectangle.Y + rectangle.Height);

            return new Vector2(x, y);
        }

        public static double NextDouble(this Random rand, double minimum, double maximum)
        {
            return rand.NextDouble() * (maximum - minimum) + minimum;
        }

        public static float NextFloat(this Random rand)
        {
            return (float)rand.NextDouble();
        }

        public static float NextFloat(this Random rand, float minimum, float maximum)
        {
            return (float)rand.NextDouble() * (maximum - minimum) + minimum;
        }

        public static bool NextCoinFlip(this Random rand)
        {   //non inclusive, so either 0 or 1
            return rand.Next(2) == 0;
        }
        #endregion
    }

    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }

        public T Peek()
        {
            T outobj;
            base.TryPeek(out outobj);
            return outobj;
        }
    }
}