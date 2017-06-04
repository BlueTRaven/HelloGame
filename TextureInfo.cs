using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using HelloGame.Utility;
using Microsoft.Xna.Framework.Graphics;

namespace HelloGame
{
    public class TextureInfo
    {
        public TextureContainer texture;
        public Vector2 scale;
        public Color tint;

        public float rotation;
        public SpriteEffects mirror;

        public bool isAnimated;
        public Rectangle? sourceRect;
        public Vector2 offset;

        private FixedSizedQueue<int> playIndexQueue;
        private int currentPlayIndex;
        private Animation[] animations;
        private Animation currentAnimation;

        public TextureInfo(TextureContainer container, Vector2 scale, Color tint, float rotation = 0, SpriteEffects mirror = SpriteEffects.None)
        {
            this.texture = container;
            this.scale = scale;
            this.tint = tint;

            this.rotation = rotation;
            this.mirror = mirror;

            playIndexQueue = new FixedSizedQueue<int>(8);
        }

        #region animation
        public void Update()
        {
            if (UpdateAnimation(currentPlayIndex))
            {
                if (playIndexQueue.Count > 0)
                    currentPlayIndex = playIndexQueue.Dequeue();
                else currentPlayIndex = 0;

                sourceRect = currentAnimation.GetSourceRect();
            }
            offset = currentAnimation.GetOffset();
        }

        public TextureInfo SetAnimated(int baseWidth, int baseHeight, params Animation[] animations)
        {
            isAnimated = true;
            this.animations = animations;
            sourceRect = new Rectangle(0, 0, baseWidth, baseHeight);

            Main.animatedTextures.Add(this);

            return this;
        }

        public void AddAnimationToQueue(int index, bool interrupt = false, bool interruptsame = false)
        {
            if (interrupt)
            {
                playIndexQueue.Clear();
                if (!interruptsame && currentPlayIndex == index || !currentAnimation.interruptable)
                    return;

                currentPlayIndex = index;

                if (currentAnimation != null)
                    currentAnimation.Reset();
                currentAnimation = animations[index];
                currentAnimation.Reset();   //just to be safe
            }
            else
            {
                if (!(playIndexQueue.Peek() == index || currentPlayIndex == index))
                    playIndexQueue.Enqueue(index);
            }
        }

        private bool UpdateAnimation(int indexPlay)
        {
            currentAnimation = animations[indexPlay];
            currentAnimation.Animate();
            sourceRect = animations[indexPlay].GetSourceRect();

            if (currentAnimation.done)
            {
                currentAnimation.Reset();
                return true;
            }

            return false;
        }
        #endregion
        
        public SpriteEffects GetSpriteEffects()
        {
            return currentAnimation != null && currentAnimation.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        public int GetFirstAnimationFrames()
        {
            if (animations == null)
                return -1;
            return animations[0].GetFrameCount();
        }

        public int GetFirstAnimationFrameDurations()
        {
            if (animations == null)
                return -1;
            return animations[0].GetFramesDurations();
        }

        public SerTexInfo Save()
        {
            return new SerTexInfo()
            {
                Name = texture.name,
                Scale = scale.Save(),
                Tint = tint.Save(),
                Mirror = (int)mirror,
                Rotation = (int)rotation
            };
        }
    }

    public class Animation
    {
        public readonly int frameHeight, frameWidth, numFrames, baseX;
        private readonly IReadOnlyCollection<int> eachFrameDurationMax;
        public int[] eachFrameDuration;
        public Vector2[] eachFrameOffset;
        private int currentFrameHeight;

        private int currentFrame;

        public bool interruptable;

        public bool done;

        public bool flipped;

        public Animation(int baseX, int frameWidth, int frameHeight, int numFrames, bool interruptable, bool flipped, int[] eachFrameDuration, Vector2[] eachFrameOffset = null)
        {
            this.baseX = baseX;

            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.numFrames = numFrames;
            this.flipped = flipped;
            this.interruptable = interruptable;

            this.eachFrameOffset = eachFrameOffset;

            eachFrameDurationMax = new int[eachFrameDuration.Length];
            eachFrameDurationMax = Array.AsReadOnly(eachFrameDuration).ToArray();
            this.eachFrameDuration = eachFrameDuration;
        }

        public void Animate()
        {
            if (!done)
            {
                eachFrameDuration[currentFrame]--;

                if (eachFrameDuration[currentFrame] <= 0)
                {
                    currentFrameHeight += frameHeight;
                    currentFrame++;

                    if (currentFrame > numFrames - 1)
                    {
                        currentFrame = numFrames;
                        done = true;
                    }
                }
            }
        }

        public void Reset()
        {
            done = false;
            currentFrameHeight = 0;
            currentFrame = 0;
            eachFrameDuration = eachFrameDurationMax.ToArray();
        }

        public Rectangle GetSourceRect()
        {
            return new Rectangle(baseX, currentFrameHeight, frameWidth, frameHeight);
        }

        public Vector2 GetOffset()
        {
            Rectangle sr = GetSourceRect();
            Vector2 vec = new Vector2(sr.Width / 2, sr.Height);
            if (eachFrameOffset != null)
            {
                vec += eachFrameOffset[currentFrame];
            }
            return vec;
        }

        public int GetFrameCount()
        {
            return eachFrameDurationMax.Count;
        }

        public int GetFramesDurations()
        {
            return eachFrameDurationMax.ToArray()[0];
        }
    }
}
