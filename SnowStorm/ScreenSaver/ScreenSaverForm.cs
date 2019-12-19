using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenSaver
{
    public partial class ScreenSaverForm : Form
    {       
        /// <summary>
        /// How many points the mouse can move by before the form closes.
        /// </summary>
        private const int MOUSE_MOVEMENT_TOLERANCE = 3;

        /// <summary>
        /// Starting point of the mouse
        /// </summary>
        private Point mouseStartingLocation = Point.Empty;


        /// <summary>
        /// Animator that will draw the screen saver. 
        /// </summary>
        private ScreenDrawer screenAnimator;

        public ScreenSaverForm(ScreenDrawer screenAnimator, int timerSpeed)
        {
            InitializeComponent( );
  
            this.screenAnimator = screenAnimator;
            this.updateTimer.Interval = timerSpeed;
            this.screenAnimator.Instantiate( this.ClientSize );
        }

        private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Quit if a user presses a key
#if DEBUG
            if(e.KeyCode == Keys.Escape)              
#endif
            this.Close( );
        }

        private void ScreenSaverForm_MouseClick(object sender, MouseEventArgs e)
        {
            // Quit if the user clicks the mouse
#if !DEBUG
            this.Close( );
#endif
        }

        private void ScreenSaverForm_Scroll(object sender, ScrollEventArgs e)
        {
            // Quit if the user scrolls the mouse
#if !DEBUG
            this.Close( );
#endif
        }

        private void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            // Close if there is no animator
            if( this.screenAnimator == null )
            {
                Console.WriteLine( "A null screen animator was passed in, so this screen saver is closing" );
                this.Close( );
            }

            // If debugging, don't prevent user from checking other forms
#if !DEBUG
            this.TopMost = true;
            Cursor.Hide( );
#endif

            updateTimer.Enabled = true;
        }

        private void ScreenSaverForm_Deactivate(object sender, EventArgs e)
        {
            // Close if this form gets deactivated
#if !DEBUG
            this.Close( );
#endif
        }

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {
#if !DEBUG
            // Close if the user moves the mouse again
            if( !mouseStartingLocation.IsEmpty )
            {
                Point movement = mouseStartingLocation;
                movement.X -= e.X;
                movement.Y -= e.Y;

                // Close the form if the mouse has moved outside of the tolerance range
                if( Math.Abs(movement.X) >= MOUSE_MOVEMENT_TOLERANCE ||
                    Math.Abs(movement.Y) >= MOUSE_MOVEMENT_TOLERANCE)
                    this.Close( );

            }

            // Record the mouses location on startup
            mouseStartingLocation = e.Location;
#endif
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            screenAnimator.Update( );
            this.Invalidate( ); 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            screenAnimator.Draw( e.Graphics, e.ClipRectangle.Size );
        }

        
    }
}
