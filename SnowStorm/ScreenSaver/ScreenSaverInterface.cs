using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenSaver
{
    /// <summary>
    /// Screen saver class that handles instantiation and handling of ScreenSaving events.
    /// </summary>
    public abstract class ScreenSaverInterface
    {
        /// <summary>
        /// Screen that can be drawn to do the animation for the screen.
        /// </summary>
        private ScreenSaverForm screenSaverInstance;
        /// <summary>
        /// User form to edit screen saver settings.
        /// </summary>
        private OptionsForm optionsInstance;

        /// <summary>
        /// Runs the screensaver.
        /// </summary>
        /// <param name="args">Screen saver arguement paramters.</param>
        public void Run(string[] args)
        {
            // Get the command line arguments
            string firstArgument = null;
            string secondArgument = null;

            Application.EnableVisualStyles( );
            Application.SetCompatibleTextRenderingDefault( false );

            // Argument formats
            // /c:1234567 = [option]:[screen handle]
            // /c, 123456 = string[]{"/c", "123456"}

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
        private void ShowScreenSaver()
        {
            // Run the screen saver
            screenSaverInstance = new ScreenSaverForm( this.ScreenAnimator, this.UpdateSpeed );
            screenSaverInstance.Opacity = this.Opacity;

            Application.Run( screenSaverInstance );
        }

        /// <summary>
        /// Run the screensaver's option dialog.
        /// </summary>
        private void ShowOptionsDialog()
        {  
            optionsInstance = new OptionsForm( this.OptionsPanel);
            Application.Run( optionsInstance );
        }

        /// <summary>
        /// Gets the class that draws to the screen in this screensaver
        /// </summary>
        protected abstract ScreenDrawer ScreenAnimator
        {
            get;
        }
        /// <summary>
        /// Allows the user to manipulate settings for this screen saver.
        /// </summary>
        protected abstract UserControl OptionsPanel
        {
            get;
        }

        /// <summary>
        /// How many milleseconds before the screen should be updated.
        /// </summary>
        protected abstract int UpdateSpeed
        {
            get;
        }

        /// <summary>
        /// How Opaque the screen is.
        /// </summary>
        protected virtual float Opacity
        {
            get { return 1f; }
        }
    }
}
