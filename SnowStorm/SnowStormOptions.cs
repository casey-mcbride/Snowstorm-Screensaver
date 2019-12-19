using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CustomSettings;
using System.Collections;
using System.IO;

namespace SnowStorm
{
    public partial class SnowStormOptions : UserControl
    {
        /// <summary>
        /// Current user's program settings.
        /// </summary>
        private Hashtable userSettings;

        public SnowStormOptions()
        {
            InitializeComponent( );
        }

        private void SnowStormOptions_Load(object sender, EventArgs e)
        {
            // Load the global user settings
            GlobalSettings.Load( Path.GetDirectoryName(Application.ExecutablePath) + "\\" +
                                 Properties.Settings.Default.SettingsFilePath );

            // Get the current user's saved settings if any exist
            string userName = System.Windows.Forms.SystemInformation.UserName;
            userSettings = GlobalSettings.GetUsersSettings( userName );

            // Add the settings if they don't already exist
            if( userSettings == null )
            {
                // Add a set of values for the user
                userSettings = new Hashtable( );
                GlobalSettings.AddUserSettings( userName, userSettings );

                // Use default settings
                Properties.Settings.Default.Reset( );
            }
            else
            { 
                // Load saved settings
                if(userSettings.ContainsKey("Opacity"))
                    Properties.Settings.Default.Opacity = (float)userSettings["Opacity"];
                if( userSettings.ContainsKey( "TrailLength" ) )
                    Properties.Settings.Default.TrailLength = ( int )userSettings["TrailLength"];
                if( userSettings.ContainsKey( "MaximumNumberOfFlakes" ) )
                    Properties.Settings.Default.MaximumNumberOfFlakes = ( int )userSettings["MaximumNumberOfFlakes"];
            }

            // Set the controls based off user saved settings
            this.opacityValue.Value = (decimal)Properties.Settings.Default.Opacity;
            this.trailLengthValue.Value = Properties.Settings.Default.TrailLength;
            this.maximumFlakesUpDown.Value = Properties.Settings.Default.MaximumNumberOfFlakes;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            // Write back the property values
            userSettings["Opacity"] = (float)this.opacityValue.Value;
            userSettings["TrailLength"] = (int)this.trailLengthValue.Value;
            userSettings["MaximumNumberOfFlakes"] = ( int )this.maximumFlakesUpDown.Value;

            // Save the settings
            GlobalSettings.Save( Path.GetDirectoryName(Application.ExecutablePath) + "\\" +
                                 Properties.Settings.Default.SettingsFilePath );

            // Close the option form
            ( ( Form )this.Parent ).Close( );
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Close the option form
            ( ( Form )this.Parent ).Close( );
        }


    }
}
