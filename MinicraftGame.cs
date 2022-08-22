﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minicraft.Scenes;
using Minicraft.Texture;
using Minicraft.Utils;

namespace Minicraft
{
    public class MinicraftGame : Microsoft.Xna.Framework.Game
    {
        public const string TITLE = "Minicraft";

        private static MinicraftGame _instance;
        private static Scene _scene = new MainMenuScene();

        public MinicraftGame()
        {
            _instance = this;
            Display.Constructor(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / Display.FRAMES_PER_SECOND);
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>((sender, eventArgs) => Display.UpdateSize(Window.ClientBounds.Width, Window.ClientBounds.Height));
        }

        protected override void LoadContent()
        {
            Textures.Initialize(GraphicsDevice);
            Display.LoadContent(GraphicsDevice, Content);
            // base call
            base.LoadContent();
        }

        protected override void Initialize()
        {
            // set window title
            Window.Title = TITLE;
            // set window properties
            Window.AllowAltF4 = false;
            // initialize display
            Display.Initialize();
            // base call
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            // update input
            Input.Update();
            // togglable fullscreen
            if (Input.KeyFirstDown(Keys.F11))
                Display.ToggleFullscreen();
            // update scene
            _scene.Update(gameTime);
            // base call
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // fill background
            GraphicsDevice.Clear(_scene.BackgroundColor);
            // begin drawing
            Display.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            // draw scene
            _scene.Draw(gameTime);
            // end drawing
            Display.SpriteBatch.End();
            // base call
            base.Draw(gameTime);
        }

        public static void SetScene(Scene scene) => _scene = scene;

        public static void EndProgram() => _instance.Exit();
    }
}
