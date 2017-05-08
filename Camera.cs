using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;

namespace HelloGame
{
    /// <summary>
    /// The way the camera moves to the target position.
    /// Smooth: Smoothly ramps up speed, moving towards the target position until it's near enough, at which point it snaps to the target location.
    /// Fast: Same as smooth, but faster speed and no ramp up.  //NOTE BUGGY
    /// Snap: Simply snaps the camera to the target position.
    /// </summary>
    public enum CameraMoveMode
    {
        Smooth,
        Fast,
        Snap
    }

    public class Camera
    {
        private readonly Viewport _viewport;

        public CameraMoveMode defaultMoveMode;
        public Camera(Viewport viewport)
        {
            _viewport = viewport;

            Rotation = 0;
            Zoom = Vector2.One;
            Origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            Position = Vector2.Zero;

            defaultMoveMode = CameraMoveMode.Smooth;
        }

        public CameraMoveMode moveMode;
        public CameraMoveMode tempMoveMode;
        public int moveModeDuration;

        public Vector2 target;
        private Vector2 prevTarget;
        public Vector2 velocity;

        public float oldCameraRotation;

        public Vector2 Position { get; set; }
        public Vector2 prevPosition { get; set; }
        public float Rotation { get; set; }
        public Vector2 Zoom;
        public Vector2 Origin { get; set; }

        public Rectangle clamp;

        public bool moving { get { return Position != prevPosition; } set { } }

        public Vector2 center { get { return new Vector2(_viewport.Width / 2, _viewport.Height / 2); } set { } }
        public Vector2 worldCenter { get { return Origin + Position; } set { Position = value - Origin; } }

        #region directions
        public Vector2 down
        {
            get
            {
                Vector2 finalvec = new Vector2(0, 1);
                Vector2 f = Vector2.Transform(finalvec, Matrix.CreateRotationZ(-Rotation));
                f.Normalize();
                return f;
            }
        }
        public Vector2 right
        {
            get
            {
                Vector2 finalvec = new Vector2(1, 0);
                Vector2 f = Vector2.Transform(finalvec, Matrix.CreateRotationZ(-Rotation));
                f.Normalize();
                return f;
            }
        }
        public Vector2 up
        {
            get
            {
                Vector2 finalvec = new Vector2(0, -1);
                Vector2 f = Vector2.Transform(finalvec, Matrix.CreateRotationZ(-Rotation));
                f.Normalize();
                return f;
            }
        }
        public Vector2 left
        {
            get
            {
                Vector2 finalvec = new Vector2(-1, 0);
                Vector2 f = Vector2.Transform(finalvec, Matrix.CreateRotationZ(-Rotation));
                f.Normalize();
                return f;
            }
        }
        #endregion

        public void AddRot(float amt)
        {
            Rotation = MathHelper.ToRadians(BindTo360((MathHelper.ToDegrees(Rotation) + amt)));
        }

        private float BindTo360(float val)
        {
            if (val > 360)
            {
                val -= 360;

                return val;
            }
            else if (val <= 0)
            {
                val += 360;

                return val;
            }
            return val;
        }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom.X, Zoom.Y, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public Matrix GetInverseViewMatrix()
        {
            return Matrix.Invert(GetViewMatrix());
        }

        #region setters
        private bool fading, fadeTo;
        private float fadeTimer, fadeTimerMax;
        private Color currentColor, fadeColor;
        /// <summary>
        /// Fades from transparent to a color or vice versa.
        /// </summary>
        /// <param name="color">The color to fade to or from.</param>
        /// <param name="fadeTo">fade to color (true) or from color (false).</param>
        public void SetFade(Color color, bool fadeTo, int time)
        {
            fadeColor = color;
            this.fadeTo = fadeTo;
            fadeTimer = time;
            fadeTimerMax = time;

            fading = true;
        }

        private bool tinting;
        private int tintTimer;
        private Color tintColor;
        public void SetTint(Color color, int time)
        {
            this.tintColor = color;
            this.tintTimer = time;

            this.tinting = true;
        }

        private bool quaking;
        private float quakeRange, quakeDuration;
        public void SetQuake(float range, int duration)
        {
            quakeRange = range;
            quakeDuration = duration;

            quaking = true;
        }

        public void SetMoveMode(CameraMoveMode mode, int duration)
        {
            tempMoveMode = mode;
            moveModeDuration = duration;
        }
        #endregion

        public void Update()
        {
            if (moveModeDuration > 0)
                moveMode = tempMoveMode;

            if (fading)
            {
                if (fadeTimer > 0)
                {
                    fadeTimer--;

                    float step = fadeTimer / fadeTimerMax;
                    if (fadeTo)
                        currentColor = Color.Lerp(Color.Transparent, fadeColor, step);
                    else
                        currentColor = Color.Lerp(fadeColor, Color.Transparent, step);
                }

                if (fadeTimer <= 0)
                    fading = false;
            }

            if (tinting)
            {
                if (tintTimer > 0)
                    tintTimer--;

                if (tintTimer <= 0)
                    tinting = false;
            }

            if (moveMode == CameraMoveMode.Snap)
                worldCenter = target;
            else if (moveMode == CameraMoveMode.Fast)
            {
                Vector2 dist = target - worldCenter;

                if (dist != Vector2.Zero)
                    dist = Vector2.Normalize(dist);
                else dist = Vector2.Zero;

                //velocity += dist;   //if distance is not zero, return normalized dist, otherwise return vector2.zero
                worldCenter += dist * 4;
            }
            else if (moveMode == CameraMoveMode.Smooth)
                worldCenter = Vector2.Lerp(worldCenter, target, .30f);
        }

        public void PostUpdate(Main main)
        {
            if (quaking)
            {
                if (quakeDuration > 0)
                {
                    quakeDuration--;

                    Position += new Vector2((float)Main.rand.NextDouble(quakeRange, -quakeRange), (float)Main.rand.NextDouble(quakeRange, -quakeRange));
                }
                else quaking = false;
            }

            if (clamp != Rectangle.Empty)
            {
                Position = Vector2.Clamp(Position, Vector2.Zero, new Vector2(clamp.Width - _viewport.Width, clamp.Height - _viewport.Height));
            }

            prevPosition = Position;
            prevTarget = target;

            moveMode = defaultMoveMode;
        }

        public static Vector2 ToScreenCoords(Vector2 worldCoords)
        {
            return worldCoords - Main.camera.Position;
        }

        public static Vector2 ToWorldCoords(Vector2 screenCoords)
        {
            return Vector2.Transform(screenCoords, Main.camera.GetInverseViewMatrix());
        }

        public void Draw(SpriteBatch batch)
        {
            if (fading)
                batch.DrawRectangle(new Rectangle(0, 0, Main.WIDTH, Main.HEIGHT), currentColor);

            if (tinting)
                batch.DrawRectangle(new Rectangle(0, 0, Main.WIDTH, Main.HEIGHT), tintColor);
        }
    }
}
