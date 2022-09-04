using System;
using Microsoft.Xna.Framework;
using Minicraft.Game.Worlds;
using Minicraft.Utils;

namespace Minicraft.Game.Entities
{
    public sealed class PlayerEntity : AbstractLivingEntity
    {
        private const float PLAYER_SPEED = 5f;
        private const float PLAYER_JUMP = 3.5f;
        private const float PLAYER_RUN_MULT = 1.5f;
        private const float PLAYER_LIFE = 10f;
        private static readonly Vector2 PlayerSize = new Vector2(1.8f, 2.8f);

        public PlayerEntity(Vector2 position) : base(position, PLAYER_LIFE, Colors.Entity_Player, PlayerSize, PLAYER_SPEED, PLAYER_RUN_MULT, PLAYER_JUMP) {}

        public PlayerEntity(World world) : this(Vector2.Zero) => SpawnIntoWorld(world);

        public void SpawnIntoWorld(World world)
        {
            var x = (int)(World.WIDTH / 2f);
            var y = Math.Max(world.GetTopPosition(x - 1), world.GetTopPosition(x));
            Position = new Vector2(x, y);
        }

        public sealed override void Update(World world)
        {
            // set horizontal movement
            RawVelocity.X = 0;
            if (Input.KeyHeld(Keybinds.MoveLeft))
                RawVelocity.X--;
            if (Input.KeyHeld(Keybinds.MoveRight))
                RawVelocity.X++;
            // check running
            Running = Input.KeyHeld(Keybinds.Run);
            // check jump
            if (Input.KeyHeld(Keybinds.Jump))
                Jump();
            // base call
            base.Update(world);
        }
    }
}
