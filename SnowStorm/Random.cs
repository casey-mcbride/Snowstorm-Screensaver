using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnowStorm
{
    /// <summary>
    /// Wraps a global random number generator as well as provides
    /// some added functions.
    /// </summary>
    abstract class Random
    {
        private static System.Random GLOBAL_RANDOM = new System.Random( );

        /// <summary>
        /// Gets a random float inside the given range
        /// </summary>
        /// <param name="minRange">Minimum value.</param>
        /// <param name="maxRange">Maximum value.</param>
        /// <returns>Random float inside the given range inclusively.</returns>
        public static float Float(float minRange, float maxRange)
        {
            return (float)(GLOBAL_RANDOM.NextDouble( )) * (maxRange - minRange) + minRange;
        }

        /// <summary>
        /// Gets a random int inside the given range.
        /// </summary>
        /// <param name="minRange">Minimum value.</param>
        /// <param name="maxRange">Maximum value.</param>
        /// <returns>Random int inside the given range inclusively.</returns>
        public static int Int(int minRange, int maxRange)
        {
            return GLOBAL_RANDOM.Next( minRange, maxRange + 1 );
        }

        /// <summary>
        /// Gets a random int inside the given range.
        /// </summary>
        /// <param name="minRange">Minimum value.</param>
        /// <param name="maxRange">Maximum value.</param>
        /// <returns>Random int inside the given range inclusively.</returns>
        public static short Short(short minRange, short maxRange)
        {
            return (short)GLOBAL_RANDOM.Next( minRange, maxRange + 1 );
        }

        /// <summary>
        /// Gets a random boolean value, that has equal chance of being true or false.
        /// </summary>
        /// <returns>A random boolean.</returns>
        public static bool Boolean()
        {
            return GLOBAL_RANDOM.Next( 0, 2 ) == 1;
        }
    }
}
