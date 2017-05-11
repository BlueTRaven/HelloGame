using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Guis;

namespace HelloGame
{
    /*
     * Todo:
     * Better World system, i.e. use separate classes for each map so I don't have to code a ton of stuff in the one class. 
     * 
     * Attack interruption system for ghostweapons, like stagger or parry.
     * 
     * Maybe critical hits?
     * 
     * Add player save file. Saves the current map name, the player's hp, the last world "entrance point" 
     * they used, as well as an array of bools(?) of all the enemies/bosses they've killed.
     * 
     * Water?
     * 
     * Different damage types, such as poison, fire, or just physical
     * 
     * Options Menu
     * More Options
     * 
     * Fix HitArc collision detection, it doesn't always work well
     * 
     * Add some form of distort effect behind weapon attacks?
     */
    public class Main : Game
    {
        public static bool DEBUG = true;

        public static int WIDTH = 1280, HEIGHT = 720;
        public static float ASPECT_RATIO = (float)WIDTH / (float)HEIGHT;

        GraphicsDeviceManager graphics;
        SpriteBatch batch;

        public static Assets assets;

        public static Random rand;

        public static Options options;

        public static GameKeyboard keyboard;
        public static GameMouse mouse;

        public static Camera camera;

        public static Dictionary<string, Gui> guis;
        public static Gui activeGui;

        public static Action exit;

        public static long alive;

        public static bool stopKeyboardInput;

        private World world;

        public static List<TextureInfo> animatedTextures = new List<TextureInfo>();

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
            
            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            camera.Zoom.X = (float)graphics.PreferredBackBufferWidth / (float)WIDTH;
            camera.Zoom.Y = (float)graphics.PreferredBackBufferHeight / (float)HEIGHT;
        }

        protected override void Initialize()
        {
            assets = new Assets();

            rand = new Random();

            options = new Options();

            keyboard = new GameKeyboard();
            mouse = new GameMouse();

            camera = new Camera(GraphicsDevice.Viewport);

            guis = new Dictionary<string, Gui>();

            exit = this.Exit;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);

            assets.Load(GraphicsDevice, Content);

            grayscale = Content.Load<Effect>("Shaders/test");
        }

        protected override void UnloadContent()
        {
        
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                alive++;

                if (alive == 1)
                {
                    activeGui = new GuiHud();
                    activeGui = new GuiEditor();
                    activeGui = new GuiMainMenu();
                    world = new World();
                }

                keyboard.PreUpdate();
                mouse.PreUpdate();
                activeGui.PreUpdate();
                world.PreUpdate();

                keyboard.Update();
                mouse.Update();
                activeGui.Update();
                world.Update();
                camera.Update();

                foreach (TextureInfo animinfo in animatedTextures)
                    animinfo.Update();

                if (keyboard.KeyModifierPressed(Keys.LeftAlt, Keys.T))
                    DEBUG = !DEBUG;

                keyboard.PostUpdate();
                mouse.PostUpdate();
                activeGui.PostUpdate();
                world.PostUpdate();
                camera.PostUpdate(this);

                base.Update(gameTime);

                grayscale.Parameters["DisplaceMap"].SetValue(assets.GetTexture("displace"));
                grayscale.Parameters["maximum"].SetValue(1f);
                grayscale.Parameters["time"].SetValue(alive);
            }
        }

        public static float GetDepth(Vector2 position)
        {
            Vector2 screenCoords = Camera.ToScreenCoords(position);

            return MathHelper.Clamp(screenCoords.Y / HEIGHT, 0, 1);
        }

        private RenderTarget2D render;
        private Effect grayscale;
        public static Effect currentEffect;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            currentEffect = grayscale;
            if (render == null)
                render = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            GraphicsDevice.SetRenderTarget(render);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            batch.Begin(DEBUG ? SpriteSortMode.Deferred : SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, camera.GetViewMatrix());
            world.Draw(batch);
            batch.End();

            GraphicsDevice.SetRenderTarget(null);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null);
            batch.Draw(render, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, 0, 0);
            batch.End();/**/

            /*batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, camera.GetViewMatrix());
            world.Draw(batch);
            batch.End();/**/


            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null);

            activeGui.Draw(batch);

            batch.End();

            base.Draw(gameTime);
        }
    }
}
