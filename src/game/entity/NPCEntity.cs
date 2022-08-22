using System;
using Microsoft.Xna.Framework;
using Minicraft.Game.Worlds;
using Minicraft.Utils;

namespace Minicraft.Game.Entities
{
    public sealed class NPCEntity : Entity
    {
        private const float NPC_SPEED = 3f;
        private const float NPC_JUMP = 3f;
        private const float NPC_LIFE = 2f;
        private const float NPC_AI_GOAL_DISTANCE_MIN = 0.5f;
        private const int NPC_AI_UPDATE_TICKS_MIN = World.TICKS_PER_SECOND * 3;
        private const int NPC_AI_UPDATE_TICKS_MAX = World.TICKS_PER_SECOND * 5;
        private static readonly Vector2 NPCSize = new Vector2(1.5f, 2.2f);

        private int? _goalX = null;
        private int _aiUpdateTicks;

        public NPCEntity(Vector2 position) : base(position, NPC_LIFE, Colors.Entity_NPC, NPCSize, NPC_SPEED, NPC_JUMP) => ResetAIUpdateTimer();

        private void ResetAIUpdateTimer() => _aiUpdateTicks = Util.Random.Next(NPC_AI_UPDATE_TICKS_MIN, NPC_AI_UPDATE_TICKS_MAX + 1);

        public sealed override void Update(World world)
        {
            // decrement update ticks
            _aiUpdateTicks--;
            // test update
            if (_aiUpdateTicks == 0)
            {
                _goalX = _goalX.HasValue ? null : (int?)Util.Random.Next(World.WIDTH);
                ResetAIUpdateTimer();
            }
            // test goal
            if (_goalX.HasValue)
            {
                // if goal reached
                if (Math.Abs(Position.X - _goalX.Value) <= NPC_AI_GOAL_DISTANCE_MIN)
                {
                    _goalX = null;
                    ResetAIUpdateTimer();
                }
                else
                {
                    var goalDirectionLeftElseRight = Position.X > _goalX.Value;
                    var goalDirection = goalDirectionLeftElseRight ? -1f : 1f;
                    // set velocity towards goal
                    Velocity.X = goalDirection;
                    // jump if block in way
                    var sides = GetSides();
                    int checkSide;
                    if (goalDirectionLeftElseRight)
                        checkSide = sides.Left - 1;
                    else
                        checkSide = sides.Right + 1;
                    if (!world.GetBlockType(new Point(checkSide, sides.Bottom)).GetBlock().CanWalkThrough)
                        Jump();
                }
            }
            else
                Velocity.X = 0f;
            // base call
            base.Update(world);
        }
    }
}