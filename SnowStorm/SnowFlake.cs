using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SnowStorm
{
    /// <summary>
    /// A piece of snow drifting through the air.
    /// </summary>
    class SnowFlake
    {
        /// <summary>
        /// Vector to get the acceleration from the windfield, kept in persistent memory.
        /// </summary>
        private static Vector ACCELERATION = new Vector( );

        /// <summary>
        /// Valid flake sizes that snowflakes will keep track of.
        /// </summary>
        public static readonly List<short> FLAKE_SIZES = new List<short>( new short[] {20, 15, 7, 3} );
        /// <summary>
        /// Maximum radius for a given snowflake.
        /// </summary>
        private static readonly int MAX_FLAKE_RADIUS = FLAKE_SIZES.Max( );
        /// <summary>
        /// Maximum vertical or horizontal distance a pixel can be from the center of the snowflake.
        /// </summary>
        private static readonly int MAX_FLAKE_PIXEL_DISTANCE = (int)Math.Ceiling(Math.Sqrt( 2 ) * MAX_FLAKE_RADIUS + 1);

        /// <summary>
        /// Number of patterns there are for all flakes to choose from.
        /// </summary>
        private const int SNOW_FLAKE_PATTERNS = 100;
        /// <summary>
        /// How many patterns per size the snowflake can make.
        /// </summary>
        private const int MAX_PATTERNS_FOR_SIZE = 500;

        /// <summary>
        /// Radius of the spots to put on the snowflake.
        /// </summary>
        private const float SPOT_RADIUS = .25f;

        /// <summary>
        /// The minimum speed a snowflake can fall.
        /// </summary>
        private const float MINIMUM_FALLING_SPEED = .2f;
        
        /// <summary>
        /// Position of the snowflake in the current field.
        /// </summary>
        private Vector position;
        /// <summary>
        /// Velocity of the current vector, or how much it moves every update.
        /// </summary>
        private  Vector movement;
        /// <summary>
        /// Inertia of the snowflake, determines how much the wind gusts affect it.
        /// </summary>
        private short inertia;
        /// <summary>
        /// Size of this flake.
        /// </summary>
        private short size;
        /// <summary>
        /// Angle of rotation for the SnowFlake.
        /// </summary>
        private float rotationAngle = 0;
        /// <summary>
        /// How much the angle changes per update.
        /// </summary>
        private float rotationSpeed = 1;

        /// <summary>
        /// Active wind field that is blowing the snowflake.
        /// </summary>
        private WindField activeWindField;

        /// <summary>
        /// Points that make up the pixels to draw for this snowflake.
        /// </summary>
        private Point[] snowFlakePixels = null;

        /// <summary>
        /// Shared patterns for various flake patterns to draw.
        /// The main array is a hash based on size for snowflakes of different sizes.
        /// The List<Point[]> is a list of various point[]s that define the positions for each crystal on the snowflake.
        /// </summary>
        private static List<Point[]>[] flakePatterns = null;

        /// <summary>
        /// Has data for rotating points.  The first index is the rotation angle,
        /// the second indece pair is the point x and y coordinates with an offset.
        /// </summary>
        private static List<Point>[][,] pointRotationHash = null;

        /// <summary>
        /// Each snowflakes trail, which shows where the snowflake has been the last couple of moves.
        /// </summary>
        private LinkedList<Point> trail = new LinkedList<Point>( );
        
        /// <summary>
        /// Creates a new snowflake.
        /// </summary>
        /// <param name="position">New position for the snowflake to be located at.</param>
        /// <param name="velocity">New direction and speed for the snowflake to be moving.</param>
        /// <param name="inertia">Inertia or how heavy teh snowflake is and resistant to changing direction.</param>
        /// <param name="activeField">Wiendfield that is actively blowing this snowflake.</param>
        /// <param name="flakeSize">Size of this flake.</param>
        public SnowFlake(Vector position, Vector velocity, short inertia, WindField activeField, short flakeSize)
        {
            // Create the flake patterns and rotaional data if it doesn't exist
            if( flakePatterns == null )
            {
                InitSnowFlakePatterns( );
                InitRotationHash( );
            }

            // Initialize this snowfalke
            this.size = flakeSize;
            ReInit( position, velocity, inertia );

            this.activeWindField = activeField;

            // Create the snowflake trail
            for( int i = 0; i < Properties.Settings.Default.TrailLength; i++ )
                trail.AddLast( new Point( (int)position.x, (int)position.y ) );

        }

        /// <summary>
        /// Gets a random valid size for a snowflake.
        /// </summary>
        /// <returns>A valid size for a snowflake</return>
        public static short GetRandomFlakeSize()
        {
            return FLAKE_SIZES[Random.Short( 0, (short)(FLAKE_SIZES.Count - 1))];
        }

        /// <summary>
        /// Creates the SnowFlake patterns and starts a thread to create more, if none currently exist.
        /// </summary>
        /// <param name="numImages">Number of images to add to the existing pattern collection.</param>
        private static void InitSnowFlakePatterns( )
        {
            // Create the image storage array if they don't exist
            if( flakePatterns == null )
            {
                // Create the main pattern list holder.  + 1 so that hash is direct without offset
                flakePatterns = new List<Point[]>[( int )FLAKE_SIZES.Max() + 1];
                for( int i = 0; i < FLAKE_SIZES.Count; i++ )
                    flakePatterns[FLAKE_SIZES[i]] = new List<Point[]>( );

                // Create the initial set
                for( int i = 0; i < FLAKE_SIZES.Count; i++ )
                    AddFlakesToSet( FLAKE_SIZES[i], 1);
                
                // Make patternsin parrallel
                System.Threading.Tasks.Task patternMaker = new System.Threading.Tasks.Task( () => 
                {
                    List<Point[]> largeSet = flakePatterns[( int )FLAKE_SIZES.Max()];

                    // Add pattern sets over time
                    while( largeSet.Count < MAX_PATTERNS_FOR_SIZE )
                    {
                        // Add each type of pattern then take a short nap
                        for( int i = 0; i < FLAKE_SIZES.Count; i++ )
                            AddFlakesToSet( FLAKE_SIZES[i], 1);

                        System.Diagnostics.Debug.WriteLine( "Added a set." + DateTime.Now + "  Count: " + largeSet.Count);
                    }
                    
                } );
                patternMaker.Start( );
            } 
        }

        /// <summary>
        /// Rotates a points around the origin.
        /// </summary>
        /// <param name="angle">Angle in degress to rotate.</param>
        /// <param name="rotatationPoint">Point to rotate.</param>
        /// <returns>rotationPoint rotated by the indicated angle.</returns>
        private static Point RotatePointAroundCenter(float angle, Point rotatationPoint)
        { 
            double pointRadius = Math.Sqrt( rotatationPoint.X * rotatationPoint.X + rotatationPoint.Y * rotatationPoint.Y );
            double pointAngle = Math.Atan2( rotatationPoint.Y, rotatationPoint.X);

            // rotate the angle
            pointAngle = pointAngle + angle * Math.PI / 180.0;
            rotatationPoint.X = ( int )Math.Round( Math.Cos( pointAngle ) * pointRadius , 0);
            rotatationPoint.Y = ( int )Math.Round( Math.Sin( pointAngle ) * pointRadius , 0);

            return rotatationPoint;
        }

        /// <summary>
        /// Maps the indicated point that has been rotated, back to the initial position.
        /// 
        /// If that initial position is not in the first quadrant, it's reflecte back into it.
        /// </summary>
        /// <param name="mapPoint">Point that should be mapped to the rotationMatrix.</param>
        /// <param name="angle">Angle that the point has been rotated.</param>
        /// <param name="rotationMatrix">All the Point lists that make up the rotation data.  Indeces map to initial position values.</param>
        private static void MapRotatedPoint(Point mapPoint, int angle, List<Point>[,] rotationMatrix)
        { 
            // Rotate point back to it's generic grid position
            Point rotated = RotatePointAroundCenter( -angle, mapPoint);

            // Get hash values for the rotated point
            int row = rotated.Y + MAX_FLAKE_RADIUS;
            int col = rotated.X + MAX_FLAKE_RADIUS;

            // If within the bounds of the matrix && the first quadrant
            if( col >= MAX_FLAKE_RADIUS && col < rotationMatrix.GetLength( 1 ) &&
                row >= MAX_FLAKE_RADIUS && row < rotationMatrix.GetLength( 0 ) )
            {
                // If point list hasn't been set, set it
                if( rotationMatrix[row, col] == null )
                    rotationMatrix[row, col] = new List<Point>( );

                // Add the point if it's not already there
                if(!rotationMatrix[row, col].Contains(mapPoint))
                    rotationMatrix[row, col].Add( mapPoint );
            }
        }

        /// <summary>
        /// Maps points rotated at the given angle back to their initial positions.
        /// </summary>
        /// <param name="angle">Angle to map.</param>
        /// <param name="rotationMatrix">Hash set for initial points to their rotated points.</param>
        private static void MapAngle(int angle, List<Point>[,] rotationMatrix)
        { 
            int xMin, xMax;         // Minimum and maximum of the rectangle to average

            xMin = -MAX_FLAKE_RADIUS * 2;
            xMax = MAX_FLAKE_RADIUS * 2;

            // Map all the points in the bounding rectangle
            for( int x = xMin; x <= xMax; x++ )
                for( int y = xMin; y <= xMax; y++ )
                {
                    // Normal position
                    MapRotatedPoint( new Point( x, y ), angle, rotationMatrix );
                }

            // Only do this for the first quadrant
            // Foreach empty list, fill in with the average of the surrounding rotation points
            for( int i = MAX_FLAKE_RADIUS; i < rotationMatrix.GetLength( 0 ); i++ )
                for( int j = MAX_FLAKE_RADIUS; j < rotationMatrix.GetLength( 1 ); j++ )
                    if( rotationMatrix[i, j] == null)
                    {
                        rotationMatrix[i, j] = new List<Point>( );
                        
                        GetAverageOfSurroundingPoints( i, j, rotationMatrix );
                    }

            Point rotate90;
            int temp;

            // Do 90 degree rotation for every point in each list, so each mapping
            // in the first quadrant will map to other 3 rotations in other quadrants
            for( int i = MAX_FLAKE_RADIUS; i < rotationMatrix.GetLength( 0 ); i++ )
                for( int j = MAX_FLAKE_RADIUS; j < rotationMatrix.GetLength( 1 ); j++ )
                {
                    List<Point> rotationList = rotationMatrix[i, j];

                    for( int listIndex = rotationList.Count - 1; listIndex >= 0; listIndex-- )
                    {
                        rotate90 = rotationList[listIndex];

                        for( int rotateIndex = 0; rotateIndex < 3; rotateIndex++ )
                        {
                            temp = rotate90.X;
                            rotate90.X = rotate90.Y;
                            rotate90.Y = -temp;

                            if( !rotationList.Contains( rotate90 ) )
                                rotationList.Add( rotate90 );
                        }
                    }
                }
        }

        /// <summary>
        /// Averages the indicated point in the matrix by it's surrounding points.
        /// Assumes that the indicated point list is already instantiated.
        /// </summary>
        /// <param name="row">Row of the index to average.</param>
        /// <param name="column">Column of the index to average.</param>
        /// <param name="rotationMatrix">Rotation matrix to add an averaged point to.</param>
        private static void GetAverageOfSurroundingPoints(int row, int column, List<Point>[,] rotationMatrix)
        {
            // Point totals and number of points to average
            Point average = new Point(0, 0);
            int numPoints = 0;

            // Min and max indeces for points to average
            int iMin, iMax;
            int jMin, jMax;

            // Determine the bounding rectangle to average the points
            iMin = Math.Max( row - 1, 0 );
            iMax = Math.Min( row + 1, rotationMatrix.GetLength( 0 ) - 1);
            jMin = Math.Max( column - 1, 0 );
            jMax = Math.Min( column + 1, rotationMatrix.GetLength( 1 ) - 1);

            // Average each surrounding point
            for(int i = iMin; i <= iMax; i++)
                for(int j = jMin; j <= jMax; j++)
                    if( i != row && j != column  && rotationMatrix[i, j] != null)
                        foreach( Point p in rotationMatrix[i, j] )
                        {
                            average.Offset( p );
                            numPoints++;
                        }

            // If there were some points to average, average it and add to the hash
            if( numPoints > 0 )
            {
                average.X = (int)Math.Round( ( float )average.X / numPoints, 0 );
                average.Y = ( int )Math.Round( ( float )average.Y / numPoints, 0 );

                rotationMatrix[row, column].Add( average );
            }

        }

        /// <summary>
        /// Creates the rotational data to transform points based on angle of rotation.
        /// </summary>
        private static void InitRotationHash()
        {
            pointRotationHash = new List<Point>[90][,];

            for( int angle = 0; angle < pointRotationHash.Length; angle++)
            {
                // TODO: Reduce the size of this since only one quadrant is used
                // May only needs to do MAX_FLAKE_RADIUS + 1
                List<Point>[,] rotationMatrix = new  List<Point>[MAX_FLAKE_RADIUS * 2 + 1, MAX_FLAKE_RADIUS * 2 + 1];

                MapAngle(angle, rotationMatrix );
                pointRotationHash[angle] = rotationMatrix;
            }
        }

        /// <summary>
        /// Adds some snowflake builds to the set for the given flakeSize.
        /// </summary>
        /// <param name="flakeSize">Size of the snowflake.</param>
        /// <param name="numPatterns">Number of patterns to add to the set.</param>
        private static void AddFlakesToSet(int flakeSize, int numPatterns)
        {
            // Add a flake for each valid size
            List<Point[]> set = flakePatterns[flakeSize];
            for( int i = 0; i < numPatterns; i++ )
                AddFlake( flakeSize, set);
        }

        /// <summary>
        /// Adds a flake pattern to the list of flake patterns.
        /// </summary>
        /// <param name="flakeSize">Radius of the snow flake.</param>
        /// <param name="flakePointList">List of snow flake patterns.</param>
        private static void AddFlake(int flakeSize, List<Point[]> flakePointList)
        {
            switch(Random.Int(0, 1))
            {
                case 0:
                    flakePointList.Add( Build8AngleFlake( flakeSize ) );
                    break;

                case 1:
                    flakePointList.Add(BuildJaggedFlake(flakeSize));
                    break;
            }
        }

        /// <summary>
        /// Builds a SnowFlake pattern that starts as a filled shape, then takes pieces away and rotates that
        /// pattern around in 4 angles.
        /// </summary>
        /// <param name="radius">Radius of the snowflake pattern to make.</param>
        /// <returns>An array of points that defines a SnowFlake pattern.</returns>
        private static Point[] Build8AngleFlake(int radius)
        { 
            List<Point> flakePositions = new List<Point>( );

            // Create a square cutout of points
            flakePositions = new List<Point>( GetRandomQuarterShape( radius ) );

            for( int i = 0; i < 4; i++ )
            {
                // Pick random set of points in PI / 4 area
                int x = Random.Int( 0, radius );
                int yMax = x;
                int y = Random.Int( 0, yMax );

                Point[] shape = GetRandomShape( (int)(radius * SPOT_RADIUS));

                Point[] removingShape = new Point[shape.Length];
                for( int j = 0; j < shape.Length; j++ )
                {
                    removingShape[j] = shape[j];
                    removingShape[j].Offset( x, y );
                }


                RemovePixel( removingShape, flakePositions );
            }

            for( int i = 0; i < 3; i++ )
            {
                // Pick random set of points in PI / 4 area
                int x = radius;
                int yMax = x;
                int y = Random.Int( 0, yMax );

                Point[] shape = GetRandomShape( (int)(radius * SPOT_RADIUS));

                Point[] removingShape = new Point[shape.Length];
                for( int j = 0; j < shape.Length; j++ )
                {
                    removingShape[j] = shape[j];
                    removingShape[j].Offset( x, y );
                }

                RemovePixel( removingShape, flakePositions );
            }

            AddPixel( CreateJaggedLine( radius ), flakePositions );
            RemovePixel( CreateJaggedLine( radius ), flakePositions );

            // Add another set to the snowflake ring
            if(  Random.Boolean() )
            {
                List<Point> rotated = new List<Point>( );
                foreach( Point p in flakePositions )
                {
                    Point rotatedPoint;

                    // Keep the point in the first quadrant
                    if( p.X > p.Y )
                        rotatedPoint = RotatePointAroundCenter( 45, p );
                    else
                        rotatedPoint = RotatePointAroundCenter( -45, p );

                    // If point is so far out that it no longer fits inside the flake range
                    if(rotatedPoint.X <= MAX_FLAKE_RADIUS &&
                        rotatedPoint.Y <= MAX_FLAKE_RADIUS)
                        rotated.Add( rotatedPoint );
                }

                foreach( Point p in rotated )
                {
                    if( !flakePositions.Contains( p ) )
                        flakePositions.Add( p );
                }
            }

            return flakePositions.ToArray( );
        }

        /// <summary>
        /// Creates an array that represents a jagged line between y = 0 and y = x.
        /// </summary>
        /// <param name="radius">Lenght of the x axis to cover.</param>
        /// <returns>A representation of a jagged line.</returns>
        private static Point[] CreateJaggedLine(int radius)
        { 
            List<Point> line = new List<Point>( );

            int y = 0;
            for( int x = 0; x < radius; x++ )
            {
                int maxY = x;

                // Since the line is biased towards being close to the x axis, add a bias towards going up higher
                y += Random.Int( -3, 3 );

                // Make sure the height is under y = x and above y = 0
                if( y > maxY )
                    y = maxY;
                else if( y < 0 )
                    y = 0;

                line.Add( new Point( x, y ) );

            }

            return line.ToArray( );
        }

        /// <summary>
        /// Builds a flake made up of jagged lines.
        /// </summary>
        /// <returns></returns>
        private static Point[] BuildJaggedFlake(int radius)
        {
            // Create a square cutout of points
            List<Point> flakePositions = new List<Point>( );

            // Add a random number of lines to the flake
            int lines = Random.Int( 1, 5 );
            for( int i = 0; i < lines; i++ )
                AddPixel(CreateJaggedLine(radius), flakePositions);

            // Add another set to the snowflake ring
            if(  Random.Boolean() )
            {
                List<Point> rotated = new List<Point>( );
                foreach( Point p in flakePositions )
                    if( Math.Abs( p.X ) < MAX_FLAKE_RADIUS * 3 / 4 && Math.Abs( p.Y ) < MAX_FLAKE_RADIUS * 3 / 4 )
                    {
                        Point rotatedPoint;

                        // Keep the point in the first quadrant
                        if( p.X > p.Y )
                            rotatedPoint = RotatePointAroundCenter( 45, p );
                        else
                            rotatedPoint = RotatePointAroundCenter( -45, p );

                        rotated.Add( rotatedPoint );
                    }

                foreach( Point p in rotated )
                {
                    if( !flakePositions.Contains( p ) )
                        flakePositions.Add( p );
                }
            }

            return flakePositions.ToArray( );
        }

        /// <summary>
        /// Gets a random circle or square.
        /// </summary>
        /// <param name="radius">Radius of the circle or half width of the square.</param>
        /// <returns>A point array representing that shape.</returns>
        private static Point[] GetRandomShape(int radius)
        {
            int shapeType = Random.Int( 1, 3 );


            List<Point> shapePoints;

            switch(shapeType)
            {
                // Square
                case 1:
                    shapePoints = new List<Point>();
                    for( int x = -radius; x <= radius; x++ )
                    {
                        int height = radius;

                        for( int y = -height; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );

                    }

                break;

                // Circle
                case 2:
                    shapePoints = new List<Point>();
                    for( int x = -radius; x <= radius; x++ )
                    {
                        int height = (int)Math.Sqrt( radius * radius - x * x );

                        for( int y = -height; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );

                    }
                break;

                // Diamond
                case 3:
                    shapePoints = new List<Point>();
                    for( int x = -radius; x <= radius; x++ )
                    {
                        int height = radius - Math.Abs( x );

                        for( int y = -height; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );

                    }
                break;


                default:

                return null;
            }

            return shapePoints.ToArray( ); ;
        }

        /// <summary>
        /// Gets a random circle or square, but only the filled in I quadrant.
        /// </summary>
        /// <param name="radius">Radius of the circle or half width of the square.</param>
        /// <returns>A point array representing that shape.</returns>
        private static Point[] GetRandomQuarterShape(int radius)
        {
            int shapeType = Random.Int( 1, 3 );


            List<Point> shapePoints;

            switch(shapeType)
            {
                // Square
                case 1:
                    shapePoints = new List<Point>();
                    for( int x = 0; x <= radius; x++ )
                    {
                        int height = radius;

                        for( int y = 0; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );
                    }

                break;

                // Circle
                case 2:
                    shapePoints = new List<Point>();
                    for( int x = 0; x <= radius; x++ )
                    {
                        int height = (int)Math.Sqrt( radius * radius - x * x );

                        for( int y = 0; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );
                    }
                break;

                // Diamond
                case 3:
                    shapePoints = new List<Point>();
                    for( int x = 0; x <= radius; x++ )
                    {
                        int height = radius - Math.Abs( x );

                        for( int y = 0; y <= height; y++ )
                            shapePoints.Add( new Point( x, y ) );
                    }
                break;


                default:

                return null;
            }

            return shapePoints.ToArray( ); ;
        }

        /// <summary>
        /// Removes pixels based on removing points.  Removing points are reflected accross
        /// y=x, y = 0, and x = 0, then removed from the total pixels for the flake.
        /// </summary>
        /// <param name="removingPoints">Shape to remove, should be between y = 0 and y = x.</param>
        /// <param name="flakePoints">Points of the flake to remove from.</param>
        private static void RemovePixel(Point[] removingPoints, List<Point> flakePoints)
        {
            List<Point> toRemove = new List<Point>( );
            List<Point> reflectedPoints = new List<Point>( );

            // Remove normal point
            toRemove.AddRange(removingPoints);

            // Reflection across y = x to make it possible to give it rotational reflection
            foreach( Point p in toRemove )
                reflectedPoints.Add( new Point( p.Y, p.X ) );
            toRemove.AddRange( reflectedPoints );
            reflectedPoints.Clear( );

            // Remove all the points
            foreach( Point p in toRemove )
                flakePoints.Remove( p );
        }

        /// <summary>
        /// Adds pixels based on addingpoints.  addingPoints are reflected accross
        /// y=x, y = 0, and x = 0, then added to the total pixels for the flake.
        /// </summary>
        /// <param name="removingPoints">Shape to add, should be between y = 0 and y = x.</param>
        /// <param name="flakePoints">Points of the flake to add to.</param>
        private static void AddPixel(Point[] addingPoints, List<Point> flakePoints)
        { 
             List<Point> toAdd = new List<Point>( );
            List<Point> reflectedPoints = new List<Point>( );

            // Remove normal point
            toAdd.AddRange(addingPoints);

            // Reflection across y = x to make it possible to give it rotational reflection
            foreach( Point p in toAdd )
                reflectedPoints.Add( new Point( p.Y, p.X ) );
            toAdd.AddRange( reflectedPoints );
            reflectedPoints.Clear( );

            // Add all the points
            foreach( Point p in toAdd )
                flakePoints.Add( p );
        }

        /// <summary>
        /// REInitializes the snowflake.
        /// </summary>
        /// <param name="position">New position for the snowflake to be located at.</param>
        /// <param name="velocity">New direction and speed for the snowflake to be moving.</param>
        /// <param name="inertia">Inertia or how heavy teh snowflake is and resistant to changing direction.</param>
        public void ReInit(Vector position, Vector velocity, short inertia)
        {
            // Reset the movement stats for the snowflake
            this.position = position;
            this.movement = velocity;
            this.inertia = inertia;

            // Choose a new pattern
            List<Point[]> patternHolder = flakePatterns[( int )size];
            snowFlakePixels = patternHolder[Random.Int(0, patternHolder.Count - 1)];

            rotationSpeed = 0;
        }

        /// <summary>
        /// Accelerates the snowflake based on it's active windfield.
        /// </summary>
        public void Accelerate()
        {
            activeWindField.SetVector( position, ref ACCELERATION );

            movement.x = ( ACCELERATION.x +  movement.x * inertia ) / (inertia + 1);
            movement.y = ( ACCELERATION.y +  movement.y * inertia ) / (inertia + 1);

            // Force the vector to fall no less than the given rate
            if( movement.y < MINIMUM_FALLING_SPEED )
                movement.y = MINIMUM_FALLING_SPEED;
        }

        /// <summary>
        /// Updates the SnowFlake's position based on it's speed.
        /// </summary>
        public void Move()
        {
            position.Add( movement );

            // Adjust the rotation speed, then rotate the flake
            rotationSpeed = (Math.Sign(movement.x) * .6f * movement.Magnitude + 9 * rotationSpeed) / 10;
            rotationAngle += rotationSpeed;
            rotationAngle += 360;
            rotationAngle %= 90;

            // Add the trail
            trail.AddLast( new Point( ( int )position.x, ( int )position.y ) );
            trail.RemoveFirst( );
        }

        /// <summary>
        /// Returns true if the snowflake is inside of the bounds.
        /// </summary>
        /// <param name="bounds">Area that may contain the snowflake.</param>
        /// <returns>True if the snowflake is inside of the area.</returns>
        public bool InBounds(Size bounds)
        { 
            return position.x >= 0 && position.x < bounds.Width &&
                   position.y >= 0 && position.y < bounds.Height;
        }

        /// <summary>
        /// Draws teh snowflake onto the InteractiveImage buffer for the screen.
        /// </summary>
        /// <param name="g">Graphics buffer for the screen.</param>
        public void Draw(Interactive8BitImage g)
        {
            Point drawPoint;        // Point to draw on the buffer

            List<Point>[,] rotationLists = pointRotationHash[(int)rotationAngle];
            List<Point> mappedPoints;       // All the rotated & reflect points that are mapped to the point in the first quadrant  

            Rectangle imageBounds;          // Bounds for the image
            Rectangle flakeBounds;          // Bounding box for all the pixels that could be in this flake
            
            imageBounds = new Rectangle( 0, 0, g.Width, g.Height );
            flakeBounds = new Rectangle( ( int )position.x - MAX_FLAKE_PIXEL_DISTANCE, ( int )position.y - MAX_FLAKE_PIXEL_DISTANCE,
                                                   MAX_FLAKE_PIXEL_DISTANCE * 2, MAX_FLAKE_PIXEL_DISTANCE * 2);

            if( imageBounds.Contains( flakeBounds ) )
            {
                // Draw each point in the pixel set that makes up the snowflake, offset by flakes position
                foreach( Point flakePixel in snowFlakePixels )
                {
                    System.Diagnostics.Debug.Assert( flakePixel.Y + MAX_FLAKE_RADIUS < rotationLists.GetLength( 0 ) &&
                                                    flakePixel.X + MAX_FLAKE_RADIUS < rotationLists.GetLength( 1 ), "Pixel is outside of the rotaiton array" );
                    mappedPoints = rotationLists[flakePixel.Y + MAX_FLAKE_RADIUS, flakePixel.X + MAX_FLAKE_RADIUS];

                    foreach( Point rotated in mappedPoints )
                    {
                        // Rotate the pixel
                        drawPoint = rotated;

                        // Make pixel position relative to the snowflakes position
                        drawPoint.X = drawPoint.X + ( int )position.x;
                        drawPoint.Y = drawPoint.Y + ( int )position.y;

                        // No need to bounds check pixel, it's in the snowflake
                        g.SetPixel8Bit( drawPoint.X, drawPoint.Y, byte.MaxValue );
                    }
                }
            }
            else
            {
                // Draw each point in the pixel set that makes up the snowflake, offset by flakes position
                foreach( Point flakePixel in snowFlakePixels )
                {
                    if(!(flakePixel.Y + MAX_FLAKE_RADIUS < rotationLists.GetLength( 0 ) &&   flakePixel.X + MAX_FLAKE_RADIUS < rotationLists.GetLength( 1 )))
                    {
                    
                    }
                    System.Diagnostics.Debug.Assert( flakePixel.Y + MAX_FLAKE_RADIUS < rotationLists.GetLength( 0 ) &&
                                                    flakePixel.X + MAX_FLAKE_RADIUS < rotationLists.GetLength( 1 ), "Pixel is outside of the rotation array" );
                    mappedPoints = rotationLists[flakePixel.Y + MAX_FLAKE_RADIUS, flakePixel.X + MAX_FLAKE_RADIUS];

                    foreach( Point rotated in mappedPoints )
                    {
                        // Rotate the pixel
                        drawPoint = rotated;

                        // Make pixel position relative to the snowflakes position
                        drawPoint.X = drawPoint.X + ( int )position.x;
                        drawPoint.Y = drawPoint.Y + ( int )position.y;

                        // If pixel is in image bounds, draw it.
                        if( drawPoint.X >= 0 && drawPoint.X < g.Width && drawPoint.Y >= 0 && drawPoint.Y < g.Height )
                            g.SetPixel8Bit( drawPoint.X, drawPoint.Y, byte.MaxValue );
                    }
                }
            }

            foreach(Point p in trail)
                if(p.X >= 0 && p.X < g.Width && p.Y >= 0 && p.Y < g.Height)
                g.SetPixel( p.X, p.Y, Color.White ); 
        }

        /// <summary>
        /// Gets the size of the snowflake, which describes the snowflakes radius.
        /// </summary>
        public int Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Tests how quickly patterns can be made for Snowflakes under the current configuration.
        /// 
        /// Should not be used in the course of a run of the program.
        /// </summary>
        /// <param name="numPatterns">Number of patterns to create for each SnowFlake size.</param>
        /// <returns>Amount of time, in milleseconds, to create the indicated</returns>
        public static int TestPatternCreation(int numPatterns)
        {
            if( numPatterns <= 0 )
                return - 1;

            // Create the main pattern list holder.  + 1 so that hash is direct without offset
            flakePatterns = new List<Point[]>[( int )FLAKE_SIZES.Max() + 1];
            for( int i = 0; i < FLAKE_SIZES.Count; i++ )
                flakePatterns[FLAKE_SIZES[i]] = new List<Point[]>( );

            DateTime start = DateTime.Now;

            for(int patternsDone = 0; patternsDone < numPatterns; patternsDone++)
            {
                // Add each type of pattern then take a short nap
                for( int i = 0; i < FLAKE_SIZES.Count; i++ )
                    AddFlakesToSet( FLAKE_SIZES[i], 1);
            }

            return (int)( DateTime.Now - start ).TotalMilliseconds;
        }

        /// <summary>
        /// True if the SnowFlake should be turned on or off.
        /// </summary>
        public bool IsActive
        {
            get;
            set;
        }
    }
}
