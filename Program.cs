﻿using System;

namespace Game
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Minicraft())
                game.Run();
        }
    }
}
