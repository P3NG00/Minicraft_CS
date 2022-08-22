using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Minicraft.Font;
using Minicraft.Game.Blocks;
using Minicraft.Game.Entities;
using Minicraft.Game.Inventories;
using Minicraft.Game.Worlds;
using Minicraft.UI;
using Minicraft.Utils;

namespace Minicraft.Scenes
{
    public sealed class GameScene : Scene
    {
        private const string TEXT_DEATH = "you died!";
        private const string TEXT_MAIN_MENU = "main menu";
        private const string TEXT_PAUSE = "paused...";
        private const string TEXT_RESPAWN = "respawn";
        private const string TEXT_RESUME = "resume";

        private static readonly Vector2 BarSize = new Vector2(150, 30);

        private int Ticks => _ticks[0];
        private float AverageFramesPerSecond => _lastFps.Average();
        private float AverageTicksPerFrame => (float)_lastTickDifferences.Average();

        private float _tickDelta = 0f;
        private int[] _ticks = new [] {0, 0};
        private int[] _lastTickDifferences = new int[World.TICKS_PER_SECOND];
        private float[] _lastFps = new float[Display.FRAMES_PER_SECOND];

        private readonly Button _buttonRespawn = new Button(new Vector2(0.5f, 0.6f), new Point(250, 50), TEXT_RESPAWN, Colors.Game_Button_Respawn, Colors.Game_Text_Respawn);
        private readonly Button _buttonResume = new Button(new Vector2(0.5f, 0.6f), new Point(250, 50), TEXT_RESUME, Colors.Game_Button_Resume, Colors.Game_Text_Resume);
        private readonly Button _buttonMainMenu = new Button(new Vector2(0.5f, 0.7f), new Point(250, 50), TEXT_MAIN_MENU, Colors.Game_Button_MainMenu, Colors.Game_Text_MainMenu);
        private readonly List<NPCEntity> _npcList = new List<NPCEntity>();
        private readonly PlayerEntity _player;
        private readonly World _world;
        private readonly Inventory _inventory = new Inventory();

        private Vector2 _lastMouseBlock;
        private Point _lastMouseBlockInt;
        private bool _paused = false;

        public GameScene(World world) : base(BlockType.Air.GetBlock().Color)
        {
            // initialize buttons
            _buttonRespawn.Action = RespawnPlayer;
            _buttonRespawn.ColorBoxHighlight = Colors.Game_Button_Respawn_Highlight;
            _buttonRespawn.ColorTextHighlight = Colors.Game_Text_Respawn_Highlight;
            _buttonResume.Action = ResumeGame;
            _buttonResume.ColorBoxHighlight = Colors.Game_Button_Resume_Highlight;
            _buttonResume.ColorTextHighlight = Colors.Game_Text_Resume_Highlight;
            _buttonMainMenu.Action = SaveAndMainMenu;
            _buttonMainMenu.ColorBoxHighlight = Colors.Game_Button_MainMenu_Highlight;
            _buttonMainMenu.ColorTextHighlight = Colors.Game_Text_MainMenu_Highlight;
            // cache world
            _world = world;
            // create player
            _player = new PlayerEntity(_world);
        }

        public sealed override void Update(GameTime gameTime)
        {
            // handle input
            HandleInput();
            // check pause
            if (_paused)
            {
                // update pause menu
                _buttonResume.Update();
                _buttonMainMenu.Update();
            }
            else
            {
                UpdateTicks((float)gameTime.ElapsedGameTime.TotalSeconds * Debug.TimeScale);
                // update ui buttons
                if (!_player.Alive)
                {
                    // update buttons
                    _buttonRespawn.Update();
                    _buttonMainMenu.Update();
                }
                // update for every tick step
                while (Tick())
                {
                    // update debug
                    Debug.Update();
                    // update world
                    _world.Update();
                    // update player
                    if (_player.Alive)
                        _player.Update(_world);
                    // update npc's
                    _npcList.ForEach(npc => npc.Update(_world));
                    // remove dead npc's
                    _npcList.RemoveAll(npc => !npc.Alive);
                }
            }
        }

        public sealed override void Draw(GameTime gameTime)
        {
            UpdateFramesPerSecond((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            // update display handler
            Display.UpdateCameraOffset(_player);
            // draw world
            _world.Draw(_player, _lastMouseBlockInt);
            // draw player
            if (_player.Alive)
                _player.Draw();
            // draw npc's
            _npcList.ForEach(npc => npc.Draw());
            // draw ui
            DrawUI();
        }

        private void ResumeGame() => _paused = false;

        private void SaveAndMainMenu()
        {
            _world.Save();
            MinicraftGame.SetScene(new MainMenuScene());
        }

        private void RespawnPlayer()
        {
            // reset health
            _player.ResetHealth();
            // respawn
            _player.Respawn(_world);
        }

        private bool Tick()
        {
            if (_tickDelta >= World.TICK_STEP)
            {
                // decrement delta time by tick step
                _tickDelta -= World.TICK_STEP;
                // increment tick counter
                _ticks[0]++;
                // return success
                return true;
            }
            return false;
        }

        private void UpdateTicks(float timeThisUpdate)
        {
            // add delta time
            _tickDelta += timeThisUpdate;
            // move last tick count down
            for (int i = _lastTickDifferences.Length - 2; i >= 0; i--)
                _lastTickDifferences[i + 1] = _lastTickDifferences[i];
            // set last tick difference
            _lastTickDifferences[0] = _ticks[0] - _ticks[1];
            // update last tick count
            _ticks[1] = _ticks[0];
        }

        private void UpdateFramesPerSecond(float timeThisFrame)
        {
            // move values down
            for (int i = _lastFps.Length - 2; i >= 0; i--)
                _lastFps[i + 1] = _lastFps[i];
            // store fps value
            _lastFps[0] = 1000f / timeThisFrame;
        }

        private void HandleInput()
        {
            if (Input.KeyFirstDown(Keys.Escape) && _player.Alive)
                _paused = !_paused;
            if (Input.KeyFirstDown(Keys.Tab))
                Display.ShowGrid = !Display.ShowGrid;
            // increase/decrease time scale
            if (Input.KeyFirstDown(Keys.F1))
                Debug.TimeScale -= Debug.TIME_SCALE_STEP;
            if (Input.KeyFirstDown(Keys.F2))
                Debug.TimeScale += Debug.TIME_SCALE_STEP;
            // manually step time
            if (Input.KeyFirstDown(Keys.F3))
                _tickDelta += World.TICK_STEP;
            // debug
            if (Debug.Enabled && Input.KeyFirstDown(Keys.F9))
                Debug.DisplayBlockChecks = !Debug.DisplayBlockChecks;
            if (Input.KeyFirstDown(Keys.F12))
                Debug.Enabled = !Debug.Enabled;
            // update if not paused
            if (!_paused)
            {
                for (int i = 0; i < Inventory.SLOTS; i++)
                    if (Input.KeyFirstDown(Keys.D1 + i))
                        _inventory.SetActiveSlot(i);
                Display.BlockScale = (Display.BlockScale + Input.ScrollWheelDelta).Clamp(Display.BLOCK_SCALE_MIN, Display.BLOCK_SCALE_MAX);
                // get block position from mouse
                var mousePos = Input.MousePosition.ToVector2();
                mousePos.Y = Display.WindowSize.Y - mousePos.Y - 1;
                _lastMouseBlock = ((mousePos - (Display.WindowSize.ToVector2() / 2f)) / Display.BlockScale) + (_player.Position + new Vector2(0, _player.Dimensions.Y / 2f));
                _lastMouseBlockInt = _lastMouseBlock.ToPoint();
                // catch out of bounds
                if (_player.Alive &&
                    _lastMouseBlockInt.X >= 0 && _lastMouseBlockInt.X < World.WIDTH &&
                    _lastMouseBlockInt.Y >= 0 && _lastMouseBlockInt.Y < World.HEIGHT)
                {
                    // TODO limit distance youre able to interact with blocks from player
                    // TODO implement block hit breaking system instead of instantly breaking
                    var blockType = _world.GetBlockType(_lastMouseBlockInt);
                    // handle block breaking
                    if (Input.MouseLeftFirstDown() && blockType != BlockType.Air)
                    {
                        _inventory.Add(blockType);
                        _world.SetBlockType(_lastMouseBlockInt, BlockType.Air);
                    }
                    // handle block placing
                    if (Input.MouseRightFirstDown() && !_player.GetSides().Contains(_lastMouseBlockInt))
                        _inventory.Place(_world, _lastMouseBlockInt);
                    if (Input.MouseMiddleFirstDown())
                        _npcList.Add(new NPCEntity(_lastMouseBlock));
                }
            }
        }

        private void DrawUI()
        {
            // TODO create a ui system for easier management and placement
            // TODO allow player to edit position of ui elements

            // draw inventory hotbar
            _inventory.Draw();
            // TODO lower position of health bar because the thick black border between both looks ugly
            // draw health bar
            var drawPos = new Vector2((Display.WindowSize.X / 2f) - (BarSize.X / 2f), Display.WindowSize.Y - Inventory.HotbarSize.Y - BarSize.Y);
            Display.Draw(drawPos, BarSize, Colors.UI_Bar);
            // adjust size to fit within bar
            drawPos += new Vector2(Util.UI_SPACER);
            var healthSize = BarSize - (new Vector2(Util.UI_SPACER) * 2);
            // readjust size to display real health
            healthSize.X *= _player.Life / _player.MaxLife;
            Display.Draw(drawPos, healthSize, Colors.UI_Life);
            // draw health numbers on top of bar
            var healthString = $"{_player.Life:0.#}/{_player.MaxLife:0.#}";
            var textSize = FontSize._12.MeasureString(healthString);
            drawPos = new Vector2((Display.WindowSize.X / 2f) - (textSize.X / 2f), Display.WindowSize.Y - Inventory.HotbarSize.Y - 22);
            Display.DrawStringWithShadow(FontSize._12, drawPos, healthString, Colors.UI_TextLife);
            // draw currently selected block
            // draw death screen overlay
            if (!_player.Alive)
            {
                Display.DrawOverlay();
                // draw text
                Display.DrawCenteredText(FontSize._24, new Vector2(0.5f, 0.35f), TEXT_DEATH, Colors.UI_YouDied, Display.DrawStringWithShadow);
                // draw buttons to restart game
                _buttonRespawn.Draw();
                _buttonMainMenu.Draw();
            }
            else if (_paused)
            {
                Display.DrawOverlay();
                // draw text
                Display.DrawCenteredText(FontSize._12, new Vector2(0.5f, 0.35f), TEXT_PAUSE, Colors.UI_Pause, Display.DrawStringWithShadow);
                // draw buttons
                _buttonResume.Draw();
                _buttonMainMenu.Draw();
            }
            // draw debug
            if (Debug.Enabled)
            {
                drawPos = new Vector2(Util.UI_SPACER);
                foreach (var debugInfo in new[] {
                    $"window_size: {Display.WindowSize.X}x{Display.WindowSize.Y}",
                    $"world_size: {World.WIDTH}x{World.HEIGHT}",
                    $"debug_blocks: {Debug.DisplayBlockChecks}",
                    $"show_grid: {Display.ShowGrid}",
                    $"paused: {_paused}",
                    $"time_scale: {Debug.TimeScale:0.00}",
                    $"time: {(Ticks / (float)World.TICKS_PER_SECOND):0.000}",
                    $"ticks: {Ticks} ({World.TICKS_PER_SECOND} ticks/sec)",
                    $"frames_per_second: {AverageFramesPerSecond:0.000}",
                    $"ticks_per_frame: {AverageTicksPerFrame:0.000}",
                    $"x: {_player.Position.X:0.000}",
                    $"y: {_player.Position.Y:0.000}",
                    $"block_scale: {Display.BlockScale}",
                    $"mouse_x: {_lastMouseBlock.X:0.000} ({_lastMouseBlockInt.X})",
                    $"mouse_y: {_lastMouseBlock.Y:0.000} ({_lastMouseBlockInt.Y})",
                    $"npc_count: {_npcList.Count}",
                    $"player_velocity: {_player.Velocity.Length() * _player.MoveSpeed:0.000}",
                    $"player_grounded: {_player.IsGrounded}"})
                {
                    Display.DrawStringWithBackground(FontSize._12, drawPos, debugInfo, Colors.UI_TextDebug);
                    drawPos.Y += Util.UI_SPACER + FontSize._12.GetFont().LineSpacing;
                }
            }
        }
    }
}
