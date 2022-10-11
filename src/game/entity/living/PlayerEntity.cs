using Microsoft.Xna.Framework;
using Minicraft.Game.Worlds;
using Minicraft.Input;
using Minicraft.Utils;

namespace Minicraft.Game.Entities.Living
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

        public void SpawnIntoWorld(World world) => Position = world.GetSpawnPosition().ToVector2();

        public sealed override void Update(World world)
        {
            // set horizontal movement
            RawVelocity.X = 0;
            if (Keybinds.MoveLeft.Held)
                RawVelocity.X--;
            if (Keybinds.MoveRight.Held)
                RawVelocity.X++;
            // check running
            Running = Keybinds.Run.Held;
            // check jump
            if (Keybinds.Jump.Held)
                Jump();
            // base call
            base.Update(world);
        }
    }
}
