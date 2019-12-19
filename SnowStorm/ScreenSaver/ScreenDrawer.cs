using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScreenSaver
{
    /// <summary>
    /// A class that will draw information to the screen at certain time intervals.
    /// </summary>
    public interface ScreenDrawer
    {
        /// <summary>
        /// Instantiates possible data in the drawer based off the given screensaver.
        /// </summary>
        /// <param name="screenSize">Size of the screen</param>
        void Instantiate(Size screenSize);

        /// <summary>
        /// Draw whatever needs to be drawn to the screen.
        /// </summary>
        /// <param name="screenBuffer">Buffer to draw on the screen.</param>
        /// <param name="screenSize">Size of the screen.</param>
        void Draw(Graphics screenBuffer, Size screenSize);

        /// <summary>
        /// Updates the internal data for drawing the screen saver, occurs once in between drawings.
        /// </summary>
        void Animate();
    }
}
