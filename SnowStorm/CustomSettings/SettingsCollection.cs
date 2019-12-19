using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Windows.Forms;
using System.IO;

namespace CustomSettings
{
    /// <summary>
    /// A singleton class for custom user settings.
    /// </summary>
    [Serializable]
    class GlobalSettings
    {
        /// <summary>
        /// Shared settings accross multiple contexts.
        /// </summary>
        [NonSerialized]
        private static GlobalSettings globalSettings;

        /// <summary>
        /// Hashtable of user settings based off the users ID.
        /// </summary>
        private System.Collections.Hashtable userSettings;


        private static void Load(byte[] resourceData)
        {     
            if( globalSettings == null )
            {
                GlobalSettings loadedCollection = new GlobalSettings( );

                if( resourceData != null && resourceData.Length > 0)
                {
                    try
                    {
                        MemoryStream flStream = new MemoryStream( resourceData, false );
                        BinaryFormatter settingsReader = new BinaryFormatter( );

                        loadedCollection = settingsReader.Deserialize( flStream ) as GlobalSettings;

                        flStream.Close( );
                    }
                    catch( Exception )
                    {
                        // If an error occured, just set as emptpy settings
                        loadedCollection.userSettings = new System.Collections.Hashtable( );
                    }
                }
                else
                {
                    // Create a new list of empty settings
                    loadedCollection.userSettings = new System.Collections.Hashtable( );
                }

                globalSettings = loadedCollection;
            }
        }

        private static byte[] Save()
        {
            byte[] resourceData = null;

            if( globalSettings != null )
            {
                try
                {
                    MemoryStream flsStream = new MemoryStream( ); 

                    BinaryFormatter settingsWriter = new BinaryFormatter( );

                    settingsWriter.Serialize( flsStream, globalSettings );

                    resourceData = flsStream.GetBuffer();

                    flsStream.Close( );
                }
                catch( Exception )
                {
                    MessageBox.Show( "An error occured saving user settings.  Those settings either weren't saved or have become corrupted" );
                }
            }

            return resourceData;
        }


        /// <summary>
        /// Loads user Global user settings data from the given file path, or creates
        /// a new set of global settings.
        /// </summary>
        /// <param name="filePath">File path to load settings from.</param>
        public static void Load(string filePath)
        {     
            if( globalSettings == null )
            {
                GlobalSettings loadedCollection = new GlobalSettings( );

                if( File.Exists( filePath ) )
                {
                    try
                    {
                        FileStream flStream = new FileStream( filePath,
                                                             FileMode.Open, FileAccess.Read );
                        BinaryFormatter settingsReader = new BinaryFormatter( );

                        loadedCollection = settingsReader.Deserialize( flStream ) as GlobalSettings;

                        flStream.Close( );
                    }
                    catch( Exception )
                    {
                        // If an error occured, just set as emptpy settings
                        loadedCollection.userSettings = new System.Collections.Hashtable( );
                    }
                }
                else
                {
                    // Create a new list of empty settings
                    loadedCollection.userSettings = new System.Collections.Hashtable( );
                }

                globalSettings = loadedCollection;
            }
        }

        /// <summary>
        /// Saves the current user global settings to the indicated file.
        /// If the file does not exist, it is created.
        /// </summary>
        /// <param name="filePath">File path to save at.</param>
        public static void Save(string filePath)
        {
            if( globalSettings != null )
            {
                try
                {
                    FileStream flsStream = new FileStream( filePath,
                                                           FileMode.Create,
                                                           FileAccess.Write );


                    BinaryFormatter settingsWriter = new BinaryFormatter( );

                    settingsWriter.Serialize( flsStream, globalSettings );

                    flsStream.Close( );
                }
                catch( Exception e)
                {
                    MessageBox.Show( "An error occured saving user settings.\n" + 
                                     "Those settings either weren't saved or have become corrupted.\n" +
                                     "Error message: \n___________________________\n" + e.Message + "\n_______________________" +
                                     "Stack tracking:\n___________________________\n" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Gets user settings under the indicated name.
        /// </summary>
        /// <param name="userName">User name to get settings for.</param>
        /// <returns>Some settings for the given user</returns>
        public static Hashtable GetUsersSettings(string userName)
        {
            // Don't try to fetch settings if the global settings are null.
            if( globalSettings == null )
                throw new Exception( "Settings not yet loaded or created" );

            if( globalSettings.userSettings.ContainsKey( userName ) )
                return globalSettings.userSettings[userName] as Hashtable;
            else
                return null;
        }

        /// <summary>
        /// Add a user settings if they don't already exist.
        /// </summary>
        /// <param name="userName">User name to save settings under.</param>
        /// <param name="settings">Settings to save.</param>
        public static void AddUserSettings(string userName, Hashtable settings)
        {
            // Add the settings if the global settings doesn't already contain them
            if( settings != null && !globalSettings.userSettings.ContainsKey( userName ) )
                globalSettings.userSettings.Add( userName, settings );
        }
    }
}
