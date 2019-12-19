using SnowStorm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
		/// The number of milliesconds per frame to achieve the desired FPS.
		/// </summary>
		private const float MILLISECONDS_PER_FRAME = 1000f / 60 /*60 FPS is our target */;

		/// <summary>
		/// Starting point of the mouse
		/// </summary>
		private Point mouseStartingLocation = Point.Empty;

		/// <summary>
		/// Animator that will draw the screen saver. 
		/// </summary>
		private SnowStormDrawer snowStormDrawer;

		private Thread renderThread;
		private RenderThreadSate threadState;

		public ScreenSaverForm()
		{
			InitializeComponent();

			this.snowStormDrawer = new SnowStormDrawer();
			this.snowStormDrawer.Instantiate(this.ClientSize);
			this.DoubleBuffered = true;
		}

		private void renderLoop()
		{
			Stopwatch screenTimer = new Stopwatch();
			while(!threadState.ShouldCancel)
			{
				screenTimer.Restart();
				snowStormDrawer.Animate();
				snowStormDrawer.Render();
				this.BeginInvoke(new Action(() => 
				{
					this.Invalidate();
				}));

				// Spin lock until we reach the target frame rate
				while (screenTimer.ElapsedMilliseconds < MILLISECONDS_PER_FRAME)
					continue;
			}
		}

		private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
		{
			// Quit if a user presses a key
#if DEBUG
			if (e.KeyCode == Keys.Escape)
#endif
			stopScreenSaver();
		}

		private void ScreenSaverForm_MouseClick(object sender, MouseEventArgs e)
		{
			// Quit if the user clicks the mouse
#if !DEBUG
			stopScreenSaver();
#endif
		}

		private void ScreenSaverForm_Scroll(object sender, ScrollEventArgs e)
		{
			// Quit if the user scrolls the mouse
#if !DEBUG
			stopScreenSaver();
#endif
		}

		private void ScreenSaverForm_Load(object sender, EventArgs e)
		{
			// If debugging, don't prevent user from checking other forms
#if !DEBUG
			this.TopMost = true;
			Cursor.Hide( );
#endif

			threadState = new RenderThreadSate();
			renderThread = new Thread(renderLoop);
			renderThread.Start();
		}

		private void ScreenSaverForm_Deactivate(object sender, EventArgs e)
		{
			// Close if this form gets deactivated
#if !DEBUG
			stopScreenSaver();
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
					stopScreenSaver();

			}

			// Record the mouses location on startup
			mouseStartingLocation = e.Location;
#endif
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			snowStormDrawer.Animate( );
			this.Invalidate( ); 
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			snowStormDrawer.Draw( e.Graphics, e.ClipRectangle.Size );
		}

		private void stopScreenSaver()
		{
			threadState.ShouldCancel = true;
			renderThread.Join(5000);
			this.Close();
		}

		private class RenderThreadSate
		{
			public bool ShouldCancel = false;
		}
	}
}
