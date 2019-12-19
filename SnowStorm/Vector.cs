using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SnowStorm
{
    /// <summary>
    /// Represents a direction on a field.
    /// </summary>
    [DebuggerDisplay("<{x}, {y}>")]
    public struct Vector
    {
        /// <summary>
        /// X direction value.
        /// </summary>
        public float x;

        /// <summary>
        /// Y direction value.
        /// </summary>
        public float y;

        private static readonly Vector X_AXIS = new Vector( 1, 0 );

        private static readonly float X_AXIS_DOT_PRODUCT = X_AXIS.DotProduct( X_AXIS );

        /// <summary>
        /// Creates a new vector of the given values.
        /// </summary>
        /// <param name="X">X direction value.</param>
        /// <param name="Y">Y direction value.</param>
        public Vector(float X, float Y)
        {
            this.x = X;
            this.y = Y;
        }

        /// <summary>
        /// True if the vector indicates no direction.
        /// </summary>
        public bool IsDirectionless
        {
            get { return x == 0 && y == 0; }
        }

        /// <summary>
        /// Gets the magnitude of the vector.
        /// </summary>
        public float Magnitude
        { 
            get{return (float)Math.Sqrt(DotProduct(this));}
        }

        /// <summary>
        /// Gets the dot product with the other vector.
        /// </summary>
        /// <param name="operand">Vector to get the dot product with.</param>
        /// <returns>Dot product of this and operand.</returns>
        public float DotProduct(Vector operand)
        {
            return x * operand.x + y * operand.y;
        }

        /// <summary>
        /// Adds toAdd to this vector.
        /// </summary>
        /// <param name="toAdd">Vector to add to this.</param>
        public void Add(Vector toAdd)
        {
            x += toAdd.x;
            y += toAdd.y;
        }

        /// <summary>
        /// Gets the angle in radians of this vector.
        /// </summary>
        public float Angle
        {
            get
            {
                //http://www.daniweb.com/forums/thread165353.html
                float ang = (float)Math.Sqrt( this.DotProduct( this ) * X_AXIS_DOT_PRODUCT );

                return (float)Math.Acos(this.DotProduct(X_AXIS) / ang);
            }
        }
    }
}
