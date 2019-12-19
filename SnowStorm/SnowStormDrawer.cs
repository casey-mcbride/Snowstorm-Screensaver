#define STATS
//#define QUICK_FILL

using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

using System.IO;
using CustomSettings;
using SnowStorm.Utility;

namespace SnowStorm
{
	class SnowStormDrawer : ScreenSaver.ScreenDrawer
	{
		// Frame times for ramping up then down, then a little empty screen time to change
		// the wind settings
		const int RAMP_UP_FRAMES = 3000;
		const int RAMP_DOWN_FRAMES = 5000;
		const int RAMP_FRAMES_TOTAL = RAMP_UP_FRAMES + RAMP_DOWN_FRAMES;
		const int EMPTY_SCREEN_FRAMES = 1000;

		const int MIN_RAMP_DOWN_FLAKES = 8;

		/// <summary>
		/// The SnowDrift being animated by this drawer.
		/// </summary>
		private SnowDrift animatedDrift;

		private readonly FpsCounter fpsCounter = new FpsCounter(30);

		/// <summary>
		/// Size of the screen.
		/// </summary>
		private Size screenSize;

		private int frameCount = 0;

		/// <summary>
		/// Current user's account name.
		/// </summary>
		private string userName;

		public void Instantiate(Size notUsed)
		{
			LoadUserSettings();

			this.screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
			animatedDrift = new SnowDrift(this.screenSize);

			// TODO: REMOVE QUICK FILL
#if QUICK_FILL
			for( int i = 0; i < 1000; i++ )
				animatedDrift.AddFlakes( );
#endif

			fpsCounter.Start();
		}

		/// <summary>
		/// Load user settings from the custom file.
		/// </summary>
		private void LoadUserSettings()
		{
			// Load settings
			GlobalSettings.Load(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" +
								 Properties.Settings.Default.SettingsFilePath);

			// Get current users settings
			userName = System.Windows.Forms.SystemInformation.UserName;
			Hashtable settings = GlobalSettings.GetUsersSettings(userName);

			// Use default settings if non exist for the user
			if (settings == null)
			{
				// Default settings
				Properties.Settings.Default.Reset();
			}
			else
			{
				// Load save settings
				if (settings.ContainsKey("Opacity"))
					Properties.Settings.Default.Opacity = (float)settings["Opacity"];
				if (settings.ContainsKey("TrailLength"))
					Properties.Settings.Default.TrailLength = (int)settings["TrailLength"];
				if (settings.ContainsKey("MaximumNumberOfFlakes"))
					Properties.Settings.Default.MaximumNumberOfFlakes = (int)settings["MaximumNumberOfFlakes"];
			}
		}

		public void Draw(System.Drawing.Graphics screenBuffer, System.Drawing.Size screenSize)
		{
			animatedDrift.Draw(screenBuffer);

#if STATS
			screenBuffer.DrawString($"FPS: {fpsCounter.Fps}  Flakes/Target: {animatedDrift.SnowFlakeCount}/{animatedDrift.TargetFlakes}",
									 new Font(FontFamily.GenericSansSerif, 40),
									 Brushes.Red, 0, 0);
#endif
		}

		public void Update()
		{
			// Do performance check
			fpsCounter.MarkFrame();

			bool rampUp = frameCount < RAMP_UP_FRAMES;
			bool rampDown = !rampUp && frameCount < RAMP_DOWN_FRAMES + RAMP_UP_FRAMES;
			int targetFlakes;

			if(rampUp)
			{
				double x = (double)frameCount / RAMP_UP_FRAMES;
				targetFlakes = (int)(x * x * Properties.Settings.Default.MaximumNumberOfFlakes);

				frameCount++;
			}
			else if(rampDown)
			{
				double x = ((double)frameCount - RAMP_UP_FRAMES) / RAMP_DOWN_FRAMES;
				targetFlakes = (int)((1 - x) * (1 - x) * Properties.Settings.Default.MaximumNumberOfFlakes + MIN_RAMP_DOWN_FLAKES);

				frameCount++;
			}
			else
			{
				targetFlakes = 0;
				if(animatedDrift.SnowFlakeCount <= 0)
				{
					frameCount++;

					if(frameCount - RAMP_FRAMES_TOTAL > EMPTY_SCREEN_FRAMES)
					{
						// Reset
						animatedDrift = new SnowDrift(screenSize);
						frameCount = 0;
					}
				}
			}

			animatedDrift.TargetFlakes = targetFlakes;
			animatedDrift.Update();
		}
	}
}
