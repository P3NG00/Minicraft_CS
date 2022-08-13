using System;
using Microsoft.Xna.Framework;
using Minicraft.Game.Blocks;
using Minicraft.Utils;

namespace Minicraft.Game.Worlds
{
    public sealed partial class World
    {
        public const float WORLD_UPDATED_PER_SECOND = 1f / 32f;
        public const int TICKS_PER_SECOND = 32;
        public const float GRAVITY = 10f;
        public const string SAVE_FILE = "save";
        public const int WIDTH = 1024;
        public const int HEIGHT = 512;
        public const float TickStep = 1f / TICKS_PER_SECOND;

        private const int BLOCK_UPDATES_PER_TICK = (int)(((WIDTH * HEIGHT) * WORLD_UPDATED_PER_SECOND) / World.TICKS_PER_SECOND);

        private BlockType[,] _blockGrid = new BlockType[HEIGHT, WIDTH];

        private ref BlockType BlockTypeAt(int x, int y) => ref _blockGrid[y, x];

        public BlockType GetBlockType(Point point) => GetBlockType(point.X, point.Y);

        public BlockType GetBlockType(int x, int y) => BlockTypeAt(x, y);

        public void SetBlockType(Point point, BlockType blockType) => SetBlockType(point.X, point.Y, blockType);

        public void SetBlockType(int x, int y, BlockType blockType) => BlockTypeAt(x, y) = blockType;

        public (BlockType block, int y) GetTop(int x)
        {
            for (int y = HEIGHT - 1; y >= 0; y--)
            {
                var _block = GetBlockType(x, y);
                if (!_block.GetBlock().CanWalkThrough)
                    return (_block, y);
            }
            return (BlockType.Air, 0);
        }

        public void Update()
        {
            for (int i = 0; i < BLOCK_UPDATES_PER_TICK; i++)
            {
                // get random point
                var pos = Util.Random.NextPoint(new Point(WIDTH, HEIGHT));
                // update block at that point
                GetBlockType(pos).GetBlock().Update(pos, this);
            }
        }

        public void Draw(Entity player)
        {
            var drawScale = Display.ShowGrid ? new Vector2(Display.BlockScale - 1) : new Vector2(Display.BlockScale);
            // find edge to start drawing
            var visualWidth = (int)Math.Ceiling((double)Display.WindowSize.X / (double)Display.BlockScale) + 4;
            var visualHeight = (int)Math.Ceiling((double)Display.WindowSize.Y / (double)Display.BlockScale) + 4;
            var visualStartX = (int)Math.Floor(player.Position.X - (visualWidth / 2f));
            var visualStartY = (int)Math.Ceiling(player.Position.Y - (visualHeight / 2f)) + 2;
            // fix variables if out of bounds
            if (visualStartX < 0)
            {
                visualWidth += visualStartX;
                visualStartX = 0;
            }
            if (visualStartY < 0)
            {
                visualHeight += visualStartY;
                visualStartY = 0;
            }
            if (visualWidth >= WIDTH - visualStartX)
                visualWidth = WIDTH - visualStartX - 1;
            if (visualHeight >= HEIGHT - visualStartY)
                visualHeight = HEIGHT - visualStartY - 1;
            // draw visible blocks
            for (int y = 0; y < visualHeight; y++)
            {
                var blockY = y + visualStartY;
                var drawY = (-1 - blockY) * Display.BlockScale;
                for (int x = 0; x < visualWidth; x++)
                {
                    var blockX = x + visualStartX;
                    var drawPos = new Vector2(blockX * Display.BlockScale, drawY) - Display.CameraOffset;
                    var blockPos = new Point(blockX, blockY);
                    Color color = GetBlockType(blockPos).GetBlock().Color;
                    if (Debug.Enabled && Debug.TrackUpdated)
                    {
                        var debugColor = Debug.CheckDebugColor(blockPos);
                        if (debugColor.HasValue)
                            color = debugColor.Value;
                    }
                    Display.Draw(drawPos, drawScale, color);
                }
            }
        }
    }
}
