using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SnowStorm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SnowStormScreenSaver instance = new SnowStormScreenSaver( );
            instance.Run( args );
        }
    }
}
