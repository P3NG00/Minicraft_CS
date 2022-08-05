using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Minicraft.Utils
{
    public static class Debug
    {
        public static bool Enabled = false;
        public static bool TrackUpdated = false;

        public static readonly HashSet<Point> UpdatedPoints = new HashSet<Point>();
    }
}