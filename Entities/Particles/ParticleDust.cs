using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Utility;
using HelloGame.Guis;
using HelloGame.Guis.Widgets;
using HelloGame.Hits;

namespace HelloGame.Entities.Particles
{
    public class ParticleDust : Particle
    {
        public ParticleDust(int timeleft, Vector2 position, float scale, float height, float rotation, Color color) : base(timeleft)
        {
            this.position = position;
            this.scale = scale;
            this.height = height;
            this.rotation = rotation;
            this.color = color;
        }

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(Main.assets.GetTexture("whitePixel"), position, null, new Color(0, 0, 0, color.A < 127 ? color.A : 127), MathHelper.ToRadians(rotation), new Vector2(.5f, .5f), scale, SpriteEffects.None, Main.GetDepth(position));
            batch.Draw(Main.assets.GetTexture("whitePixel"), position + Main.camera.up * height, null, color, MathHelper.ToRadians(rotation), new Vector2(.5f, .5f), scale, SpriteEffects.None, Main.GetDepth(position));
        }
    }
}
