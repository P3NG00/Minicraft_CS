﻿using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Game.Data;

namespace Game
{
    public class Minicraft : Microsoft.Xna.Framework.Game
    {
        // constants
        private const string TITLE = "Minicraft";
        private const int FPS = 60;
        private const int TPS = 32;
        private const int BLOCK_SCALE = 20;
        private const int BLOCK_SCALE_MIN = 5;
        private const int BLOCK_SCALE_MAX = 25;
        private const float WORLD_UPDATED_PER_SECOND = 1f / 32f;
        private const float WORLD_GRAVITY = 10f;
        private const float PLAYER_SPEED = 5f;
        private const float PLAYER_JUMP = 3.5f;
        private const int UI_SPACER = 5;

        private readonly Point WindowSize = new Point(1280, 720);
        private readonly Point WorldSize = new Point(1024, 512);

        private readonly Vector2 PlayerSize = new Vector2(1.8f, 2.8f);

        // monogame
        private GraphicsDeviceManager _graphics;

        // variables
        private Display _display;
        private Entity _player;
        private World _world;
        private Input _input = new Input();
        private float _tickDelta = 0f;
        private Vector2 _mouseBlock;
        private Point _mouseBlockInt;
        private Block _currentBlock = Blocks.Dirt;
        private int[] _ticks = new[] {0, 0};
        private float[] _lastFps = new float[10];

        public Minicraft()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / FPS);
            IsMouseVisible = true;
            // TODO handle resizing and detecting
            // Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
            // create square for drawing
            Display.TextureSquare = new Texture2D(GraphicsDevice, 1, 1);
            Display.TextureSquare.SetData(new[] {Color.White});
            // load font
            Display.Font = Content.Load<SpriteFont>("type_writer");
            // create display handler
            _display = new Display(new SpriteBatch(GraphicsDevice), WindowSize, BLOCK_SCALE, FPS, TPS);
            // create world
            _world = WorldGen.GenerateWorld(WorldSize, WORLD_GRAVITY, (int)(((WorldSize.X * WorldSize.Y) * WORLD_UPDATED_PER_SECOND) / _display.TicksPerSecond));
            // create player
            _player = new Entity(Colors.Player, PlayerSize, PLAYER_SPEED, PLAYER_JUMP);
            var playerX = (int)(_world.Width / 2f);
            _player.Position = new Vector2(playerX, Math.Max(_world.GetTopBlock(playerX - 1).y, _world.GetTopBlock(playerX).y) + 1);
            // base call
            base.LoadContent();
        }

        protected override void Initialize()
        {
            // set window title
            Window.Title = TITLE;
            // set window properties
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Window.AllowAltF4 = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.PreferredBackBufferWidth = WindowSize.X;
            _graphics.PreferredBackBufferHeight = WindowSize.Y;
            _graphics.ApplyChanges();
            // base call
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            // add delta time
            _tickDelta += (float)gameTime.ElapsedGameTime.TotalSeconds;
            // update last tick count
            _ticks[1] = _ticks[0];
            // update for every tick step
            while (_tickDelta >= _display.TickStep)
            {
                // decrement delta time by tick step
                _tickDelta -= _display.TickStep;
                // increment tick counter
                _ticks[0]++;
                // clear previously updated positions
                Debug.UpdatedPoints.Clear();
                // update input
                _input.Update();
                // handle input
                if (_input.KeyFirstDown(Keys.End))
                    Exit();
                if (_input.KeyFirstDown(Keys.Tab))
                    _display.ShowGrid = !_display.ShowGrid;
                if (_input.KeyFirstDown(Keys.F1))
                    Window.IsBorderless = !Window.IsBorderless;
                if (Debug.Enabled && _input.KeyFirstDown(Keys.F11))
                    Debug.TrackUpdated = !Debug.TrackUpdated;
                if (_input.KeyFirstDown(Keys.F12))
                    Debug.Enabled = !Debug.Enabled;
                if (_input.KeyFirstDown(Keys.D1))
                    _currentBlock = Blocks.Dirt;
                if (_input.KeyFirstDown(Keys.D2))
                    _currentBlock = Blocks.Grass;
                if (_input.KeyFirstDown(Keys.D3))
                    _currentBlock = Blocks.Stone;
                if (_input.KeyFirstDown(Keys.D4))
                    _currentBlock = Blocks.Wood;
                if (_input.KeyFirstDown(Keys.D5))
                    _currentBlock = Blocks.Leaves;
                _display.BlockScale = Math.Clamp(_display.BlockScale + _input.ScrollWheel, BLOCK_SCALE_MIN, BLOCK_SCALE_MAX);
                // get block position from mouse
                var mousePos = _input.MousePosition.ToVector2();
                mousePos.Y = _display.WindowSize.Y - mousePos.Y - 1;
                _mouseBlock = ((mousePos - (_display.WindowSize.ToVector2() / 2f)) / _display.BlockScale) + (_player.Position + new Vector2(0, _player.Dimensions.Y / 2f));
                _mouseBlockInt = _mouseBlock.ToPoint();
                // catch out of bounds
                if (_mouseBlockInt.X >= 0 && _mouseBlockInt.X < _world.Width &&
                    _mouseBlockInt.Y >= 0 && _mouseBlockInt.Y < _world.Height)
                {
                    if (_input.ButtonLeftFirstDown())
                        _world.Block(_mouseBlockInt) = Blocks.Air;
                    else if (_input.ButtonRightFirstDown())
                        _world.Block(_mouseBlockInt) = _currentBlock;
                    else if (_input.ButtonMiddleFirstDown())
                        _world.Block(_mouseBlockInt).Update(_mouseBlockInt, _world);
                }
                // update world
                _world.Update();
                // update player
                _player.Update(_input, _display, _world);
                // update display handler
                _display.Update(_player);
            }
            // base call
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // store fps value
            for (int i = _lastFps.Length - 2; i >= 0; i--)
                _lastFps[i + 1] = _lastFps[i];
            _lastFps[0] = 1000f / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            // fill background
            GraphicsDevice.Clear(Colors.Background);
            // begin drawing
            _display.SpriteBatch.Begin();
            // draw world
            _world.Draw(_display, _player);
            // draw player
            _player.Draw(_display);
            // draw ui
            var drawPos = new Vector2(UI_SPACER, _display.WindowSize.Y - Display.Font.LineSpacing - UI_SPACER);
            _display.DrawString(drawPos, $"current block: {_currentBlock.Name}", Colors.FontUI);
            // draw debug
            if (Debug.Enabled)
            {
                var debugInfo = new[] {$"window_size: {_display.WindowSize.X}x{_display.WindowSize.Y}",
                                       $"world_size: {_world.Width}x{_world.Height}",
                                       $"time: {(_ticks[0] / (float)_display.TicksPerSecond):0.000}",
                                       $"ticks: {_ticks[0]} ({_display.TicksPerSecond} ticks/sec)",
                                       $"ticks_this_frame: {_ticks[0] - _ticks[1]}",
                                       $"show_grid: {_display.ShowGrid}",
                                       $"fps: {_lastFps.Average():0.000}",
                                       $"x: {_player.Position.X:0.000}",
                                       $"y: {_player.Position.Y:0.000}",
                                       $"block_scale: {_display.BlockScale}",
                                       $"mouse_x: {_mouseBlock.X:0.000} ({_mouseBlockInt.X})",
                                       $"mouse_y: {_mouseBlock.Y:0.000} ({_mouseBlockInt.Y})",
                                       $"player_velocity: {_player.Velocity.Length():0.000}",
                                       $"player_grounded: {_player.IsGrounded}"};
                for (int i = 0; i < debugInfo.Length; i++)
                {
                    drawPos = new Vector2(UI_SPACER, (i * (UI_SPACER + Display.Font.LineSpacing)) + UI_SPACER);
                    _display.DrawString(drawPos, debugInfo[i], Colors.FontDebug);
                }
            }
            // end drawing
            _display.SpriteBatch.End();
            // base call
            base.Draw(gameTime);
        }
    }
}
