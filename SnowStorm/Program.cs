using ScreenSaver;
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
			// Get the command line arguments
			string firstArgument = null;
			string secondArgument = null;

			Application.EnableVisualStyles( );
			Application.SetCompatibleTextRenderingDefault( false );

			// Parse the arguments
			if( args != null && args.Length > 0 )
			{
				if( args.Length > 2 )
				{
					firstArgument = args[0].Substring( 0, 2 ).ToLower( );
					secondArgument = args[0].Substring( 3 ).ToLower( );
				}
				else
				{
					firstArgument = args[0].ToLower( );
					secondArgument = args.Length > 1 ? args[1].ToLower( ) : null;
				}
			}

			if( firstArgument == null )
			{
				ShowOptionsDialog( );
			}
			else
			{
				switch( firstArgument )
				{ 
					case "/p":
					   
						break;
					case "/s":
						ShowScreenSaver( );
						break;

					case "/c":
						ShowOptionsDialog( );
						break;

					default:
						ShowOptionsDialog( );
						break;
				}
			}
		}

		/// <summary>
		/// Shows the screensaver.
		/// </summary>
		private static void ShowScreenSaver()
		{
			// Run the screen saver
			ScreenSaverForm form = new ScreenSaverForm();

			Application.Run( form );
		}

		/// <summary>
		/// Run the screensaver's option dialog.
		/// </summary>
		private static void ShowOptionsDialog()
		{
			OptionsForm form = new OptionsForm(new SnowStormOptions());
			Application.Run( form );
		}

	}
}
