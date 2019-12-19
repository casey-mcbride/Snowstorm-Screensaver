#define STATS
//#define QUICK_FILL

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using ScreenSaver;
using System.Drawing;
using System.Diagnostics;

using System.IO;
using CustomSettings;
using System.Diagnostics;

namespace SnowStorm
{
    class SnowStormDrawer: ScreenSaver.ScreenDrawer
    {
        /// <summary>
        /// The SnowDrift being animated by this drawer.
        /// </summary>
        private SnowDrift animatedDrift;
        /// <summary>
        /// When the last cycle of the update and draw cycle started.
        /// </summary>
        private DateTime algorithmStart;
        /// <summary>
        /// How long it took to draw and update the animateDrift.
        /// </summary>
        private long calculationTime;

        private System.Diagnostics.Stopwatch algorithmTimer;
        /// <summary>
        /// Size of the screen.
        /// </summary>
        private Size screenSize;

        /// <summary>
        /// How long it should snow before the drift starts emptying.  Time in milleseconds.
        /// </summary>
        private const int SNOWING_TIME = 90 * 1000; 
        /// <summary>
        /// Number of milleseconds the program should try to maintain for one cycle
        /// of updating and drawing snowflakes.
        /// </summary>
        private const int PERFORMANCE_TIME = 45;
        /// <summary>
        /// Number of flakes to add per cycle.
        /// </summary>
        private const int MAX_FLAKES_RATE = 20;
        /// <summary>
        /// Number of flakes to remove during when slowing near the end
        /// </summary>
        private const int FLAKES_TO_REMOVE = 15;
        /// <summary>
        /// How many flakes to remove in a large chunk while winding down.
        /// </summary>
        private const int FLAKE_GROUP_REMOVAL = 10;
        /// <summary>
        /// How many milleseconds between removals while winding down.
        /// </summary>
        private const int FLAKE_REMOVAL_TIME = 500;
        /// <summary>
        /// Limit under which to adjust flake rates to give it a slow start and finish.
        /// </summary>
        private const int FLAKE_REDUCTION_LIMIT = 200;
        /// <summary>
        /// How many seconds after the screen has cleared before restarting.
        /// </summary>
        private const int RESTART_TIME = 10 * 1000;



        /// <summary>
        /// Times how long it's been since the snowdrift was last set.
        /// </summary>
        private Stopwatch snowDriftTimer;

        /// <summary>
        /// Current user's account name.
        /// </summary>
        private string userName;
        /// <summary>
        /// Number of clock ticks it takes to reach the performance time.
        /// </summary>
        private int algorithmTicks;
        /// <summary>
        /// Number of times in a row the speed goal hasn't been achieve by the algorithm
        /// </summary>
        private int underSpeedCounter = 0;


        private bool stopSnow = false;

        public void Instantiate(Size notUsed)
        {
           // System.Windows.Forms.MessageBox.Show( "Total test time= " + SnowFlake.TestPatternCreation( 10 ).ToString( ) );

            //SnowFlake.TestPatternCreation( 1000 );
            //throw new Exception();

            LoadUserSettings( );

            this.screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
            animatedDrift = new SnowDrift( this.screenSize );
            algorithmStart = DateTime.Now;
            snowDriftTimer = new Stopwatch();
            snowDriftTimer.Start();
            algorithmTimer = new System.Diagnostics.Stopwatch( );
            algorithmTimer.Start( );

            algorithmTicks = TimerSampling( PERFORMANCE_TIME * 10 ) / 10;

            // TODO: REMOVE QUICK FILL
#if QUICK_FILL
            for( int i = 0; i < 1000; i++ )
                animatedDrift.AddFlakes( );
#endif

        }

        private static int TimerSampling(int milleseconds)
        {
            Stopwatch sleepWatcher = new Stopwatch( );
            sleepWatcher.Start( );
            System.Threading.Thread.Sleep( milleseconds );
            return (int)sleepWatcher.ElapsedTicks;
        }

        /// <summary>
        /// Load user settings from the custom file.
        /// </summary>
        private void LoadUserSettings()
        { 
            // Load settings
            GlobalSettings.Load( Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" +
                                 Properties.Settings.Default.SettingsFilePath );

            // Get current users settings
            userName = System.Windows.Forms.SystemInformation.UserName;
            Hashtable settings = GlobalSettings.GetUsersSettings( userName );

            // Use default settings if non exist for the user
            if( settings == null )
            {
                // Default settings
                Properties.Settings.Default.Reset( );
            }
            else
            { 
                // Load save settings
                if(settings.ContainsKey("Opacity"))
                    Properties.Settings.Default.Opacity = (float)settings["Opacity"];
                if( settings.ContainsKey( "TrailLength" ) )
                    Properties.Settings.Default.TrailLength = ( int )settings["TrailLength"];
                if( settings.ContainsKey( "MaximumNumberOfFlakes" ) )
                    Properties.Settings.Default.MaximumNumberOfFlakes = ( int )settings["MaximumNumberOfFlakes"];
            }
        }

        public void Draw(System.Drawing.Graphics screenBuffer, System.Drawing.Size screenSize)
        {
            animatedDrift.Draw( screenBuffer );

#if STATS
            screenBuffer.DrawString( userName + " Time: " + calculationTime.ToString().PadLeft(10, '0') +
                                                ":" + algorithmTicks.ToString().PadLeft(10, '0') + "    # of Flakes: " + animatedDrift.SnowFlakeCount,
                                     new Font( FontFamily.GenericSansSerif, 40 ),
                                     Brushes.Red, 0, 0 );
#endif
        }

        public void Update()
        {
            // Do performance check
            calculationTime = algorithmTimer.ElapsedTicks;
            algorithmTimer.Restart( );

            // If still animating the snow drift
            if( !stopSnow )
            {
                animatedDrift.Update( );

                if( calculationTime < algorithmTicks )
                {
                    int flakesToAdd = MAX_FLAKES_RATE;

                    // If just starting up, reduce the rate at which flakes are added
                    if(animatedDrift.SnowFlakeCount < FLAKE_REDUCTION_LIMIT)
                    {
                        flakesToAdd = (MAX_FLAKES_RATE * animatedDrift.SnowFlakeCount) / FLAKE_REDUCTION_LIMIT;
                        flakesToAdd = Math.Max( 1, flakesToAdd );
                    }

                    // Quickly reach performance limit
                    for( int i = 0; i < flakesToAdd; i++ )
                        animatedDrift.AddFlakes( );

                    // Algorithm done, reset counter
                    underSpeedCounter = 0;
                }
                else
                {
                    // If we haven't been able to add a snowflake recently we should start
                    // throttling back to improve performance
                    underSpeedCounter++;
                    if( underSpeedCounter > 10 )
                        animatedDrift.RemoveFlakes( 1 );
                }

                // Is it time to reset the snow drift?
                stopSnow = snowDriftTimer.ElapsedMilliseconds >= SNOWING_TIME;
            }
            else
            {
                // Is snowdrift full, start removing flakes
                if( animatedDrift.SnowFlakeCount > 0 )
                {
                    // If almost done, slow down the removal asymptopically
                    if( snowDriftTimer.ElapsedMilliseconds > FLAKE_REMOVAL_TIME )
                    {
                        if( animatedDrift.SnowFlakeCount < FLAKE_REDUCTION_LIMIT )
                        {
                            int flakesToRemove = ( FLAKES_TO_REMOVE * animatedDrift.SnowFlakeCount ) / FLAKE_REDUCTION_LIMIT;
                            flakesToRemove = Math.Max( 1, flakesToRemove );

                            // Update the snow drift and wind it down
                            animatedDrift.RemoveFlakes( flakesToRemove );
                        }
                        else
                        {
                            animatedDrift.RemoveFlakes( FLAKE_GROUP_REMOVAL );
                        }

                        snowDriftTimer.Restart();
                    }


                    animatedDrift.Update( ); 
                }
                    // If empty, wait about some odd time seconds before restarting
                else if(snowDriftTimer.ElapsedMilliseconds > RESTART_TIME)
                {                     
                    // Re-check the algorithm ticks
                    algorithmTicks = ( TimerSampling( PERFORMANCE_TIME * 10 ) / 10 + algorithmTicks ) / 2;

                    // Reset the drift
                    animatedDrift = new SnowDrift( screenSize );
                    snowDriftTimer.Restart();
                    stopSnow = false;
                }
            }
        }
    }
}
