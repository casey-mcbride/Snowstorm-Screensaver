using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;

namespace SnowStorm
{
    /// <summary>
    /// Implements a 32bppArgb or 24bpprgb image that's easy to manipulate.
    /// </summary>
    public class Interactive8BitImage
    {
        /// <summary>
        /// Where blue resides in the pixel.
        /// </summary>
        private const byte BLUE_BIT = 0;
        /// <summary>
        /// Where green resides in the pixel.
        /// </summary>
        private const byte GREEN_BIT = 1;
        /// <summary>
        /// Where red resides in the pixel.
        /// </summary>
        private const byte RED_BIT = 2;
        /// <summary>
        /// Where alpha resides in the pixel.
        /// </summary>
        private const byte ALPHA_BIT = 3;
        /// <summary>
        /// The pixels in the image.
        /// byte 0:Blue, byte 1:Green, byte 2:Red, byte 3:Alpha
        /// </summary>
        private byte[] pixels = new byte[0];
        /// <summary>
        /// The width of the image.
        /// </summary>
        private int width = 0;
        /// <summary>
        /// The height of the image.
        /// </summary>
        private int height = 0;
        /// <summary>
        /// The number of bytes in the image.
        /// </summary>
        private int bytes = 0;
        /// <summary>
        /// The number of bytes per vertical line.
        /// </summary>
        private int bytesLine = 0;
        /// <summary>
        /// Byte boundary for offsets for the padding added onto images.
        /// </summary>
        private int byteBoundary;
        /// <summary>
        /// The format of the image.
        /// </summary>
        private System.Drawing.Imaging.PixelFormat format;
        /// <summary>
        /// The number of bytes per pixel.
        /// </summary>
        private int bytesPixel;
        /// <summary>
        /// The image for interaction.
        /// </summary>
        private Bitmap bitImage = null;
        /// <summary>
        /// True if the bitImage needs to be refreshed.
        /// </summary>
        private bool needsRefresh = true;

        /// <summary>
        /// Creates a new instance of an interactive image.
        /// </summary>
        /// <param name="Width">Width of the image.</param>
        /// <param name="Height">Height of the image.</param>
        /// <param name="bytesPixel">Number of bytes per pixel</param>
        public Interactive8BitImage(int Width,
                                 int Height,
                                 System.Drawing.Imaging.PixelFormat Format)
        {
            width = Width;
            height = Height;

            format = Format;

            SetBytesPixel(format);


            //bytes = width * height * bytesPixel;
            bytesLine = width * bytesPixel;

            // Set the byte boundary
            byteBoundary = ( 4 - bytesLine % 4 ) % 4;

            // Calculate total number of bytes used by this 8 bit image
            bytes = bytesLine * height + height * byteBoundary;



           

            pixels = new byte[bytes];
            
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 150;

            bitImage = new Bitmap( Width, Height, format );


        }

        /// <summary>
        /// Creates an Interactive_Image from a bitmap.
        /// </summary>
        /// <param name="Original"></param>
        public Interactive8BitImage(Bitmap Original)
        {
            width = Original.Width;
            height = Original.Height;

            format = Original.PixelFormat;

            if (format != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                throw new System.FormatException("Bitmap was in the wrong format.");

            SetBytesPixel(format);

            bytes = width * height * bytesPixel;
            bytesLine = width * bytesPixel;

            byte[] pixels_to_copy = new byte[bytes];

            System.Drawing.Imaging.BitmapData bmpData =
            Original.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
            Original.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels_to_copy, 0, bytes);

            pixels = (byte[])pixels_to_copy.Clone();

            System.Runtime.InteropServices.Marshal.Copy(pixels_to_copy, 0, bmpData.Scan0, bytes);

            Original.UnlockBits(bmpData);
        }

        /// <summary>
        /// Returns an image based on the pixels.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetImage()
        {
            /*int bytes = width * height * bytesPixel;
            Bitmap bmp = new Bitmap(width, height, format);

            System.Drawing.Imaging.BitmapData bmpData =
               bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
               bmp.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy((byte[])pixels.Clone(), 0, bmpData.Scan0, bytes);

            bmp.UnlockBits(bmpData);

            return bmp;*/

            if (needsRefresh)
                SetBitImage();

            return bitImage;
        }

        public bool CopyToBitmap(Bitmap copyImage)
        {
            return false;
        }

        /// <summary>
        /// Sets the bitImage.  Only used when it's being called to use.
        /// </summary>
        private void SetBitImage()
        {
            int bytes = width * height * bytesPixel;
       
            System.Drawing.Imaging.BitmapData bmpData =
               bitImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
               bitImage.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy((byte[])pixels.Clone(), 0, bmpData.Scan0, bytes);

            bitImage.UnlockBits(bmpData);
        }

        /// <summary>
        /// Removes all red from the image.
        /// </summary>
        public void RemoveRed()
        {
            for (int i = RED_BIT; i < bytes; i += bytesPixel)
                pixels[i] = 0;

            needsRefresh = true;
        }

        /// <summary>
        /// Removes all green from the image.
        /// </summary>
        public void RemoveGreen()
        {
            for (int i = GREEN_BIT; i < bytes; i += bytesPixel)
                pixels[i] = 0;

            needsRefresh = true;
        }

        /// <summary>
        /// Removes all blue from the iamge.
        /// </summary>
        public void RemoveBlue()
        {
            for (int i = BLUE_BIT; i < bytes; i += bytesPixel)
                pixels[i] = 0;

            needsRefresh = true;
        }

        /// <summary>
        /// Removes the alpha component from the image.
        /// </summary>
        public void RemoveAlpha()
        {
            if (bytesPixel == 4)
            {
                for (int i = ALPHA_BIT; i < bytes; i += bytesPixel)
                    pixels[i] = 0;
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Shifts every Blue value by the named amount.
        /// </summary>
        /// <param name="shift">The value to shift by.</param>
        public void ShiftBlue(int shift)
        {
            if (bytesPixel == 4)
            {
                for (int i = BLUE_BIT; i < bytes; i += bytesPixel)
                    if (pixels[i] + shift < 0)
                        pixels[i] = 0;
                    else if (pixels[i] + shift > byte.MaxValue)
                        pixels[i] = byte.MaxValue;
                    else
                        pixels[i] = (byte)(pixels[i] + shift);
            }


            needsRefresh = true;
        }

        /// <summary>
        /// Shifts every green value by the named amount.
        /// </summary>
        /// <param name="shift">The value to shift by.</param>
        public void ShiftGreen(int shift)
        {
            if (bytesPixel == 4)
            {
                for (int i = GREEN_BIT; i < bytes; i += bytesPixel)
                    if (pixels[i] + shift < 0)
                        pixels[i] = 0;
                    else if (pixels[i] + shift > byte.MaxValue)
                        pixels[i] = byte.MaxValue;
                    else
                        pixels[i] = (byte)(pixels[i] + shift);
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Shifts every Red value by the named amount.
        /// </summary>
        /// <param name="shift">The value to shift by.</param>
        public void ShiftRed(int shift)
        {
            if (bytesPixel == 4)
            {
                for (int i = RED_BIT; i < bytes; i += bytesPixel)
                    if (pixels[i] + shift < 0)
                        pixels[i] = 0;
                    else if (pixels[i] + shift > byte.MaxValue)
                        pixels[i] = byte.MaxValue;
                    else
                        pixels[i] = (byte)(pixels[i] + shift);
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Shifts every alpha value by the named amount.
        /// </summary>
        /// <param name="shift">The value to shift by.</param>
        public void ShiftAlpha(int shift)
        {
            if (bytesPixel == 4)
            {
                for (int i = ALPHA_BIT; i < bytes; i += bytesPixel)
                    if (pixels[i] + shift < 0)
                        pixels[i] = 0;
                    else if (pixels[i] + shift > byte.MaxValue)
                        pixels[i] = byte.MaxValue;
                    else
                        pixels[i] = (byte)(pixels[i] + shift);
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Sets the alpha component for every pixel.
        /// </summary>
        /// <param name="new_alpha">The new alpha component.</param>
        public void SetAlpha(byte new_alpha)
        {
            if (bytesPixel == 4)
            {
                for (int i = 3; i < bytes; i += 4)
                    pixels[i] = new_alpha;
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Sets every byte except the Alpha component to a random byte.
        /// </summary>
        public void RandomizeBytes()
        {
            System.Random r = new System.Random();

            for (int i = 0; i < bytes; i += 4)
                pixels[i] = (byte)r.Next(0, byte.MaxValue + 1);

            for (int i = 1; i < bytes; i += 4)
                pixels[i] = (byte)r.Next(0, byte.MaxValue + 1);

            for (int i = 2; i < bytes; i += 4)
                pixels[i] = (byte)r.Next(0, byte.MaxValue + 1);

            needsRefresh = true;
        }

        /// <summary>
        /// Sets the bytes per pixel based on the pixel format.
        /// </summary>
        /// <param name="Format">The pixel format of the image.</param>
        private void SetBytesPixel(System.Drawing.Imaging.PixelFormat Format)
        {
            switch (Format)
            { 
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:

                    bytesPixel = 4;

                    break;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:

                    bytesPixel = 3;

                    break;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    bytesPixel = 1;
                    break;

                default:
                    throw new NotImplementedException( );
                    break;
            }
        
        }

        /// <summary>
        /// Gets the color of the given pixel
        /// </summary>
        /// <param name="Width">Where to get on the bitmaps width.</param>
        /// <param name="Height">Where to get on the bitmaps height.</param>
        /// <returns>The color of the pixel.</returns>
        public Color GetPixel(int Width, int Height)
        {
            Color toReturn = Color.Black;

            if (Width < 0 || Width >= width || Height < 0 || Height >= height)
                throw new System.Exception();

            int start_index = bytesPixel * Width + Height * bytesLine;

            if (bytesPixel == 4)
                toReturn = Color.FromArgb(pixels[start_index + 3],
                                           pixels[start_index + 2],
                                           pixels[start_index + 1],
                                           pixels[start_index]);
            else
                toReturn = Color.FromArgb(pixels[start_index + 2],
                                           pixels[start_index + 1],
                                           pixels[start_index]);
            
            return toReturn;
        }

        /// <summary>
        /// Sets the specified pixel.
        /// </summary>
        /// <param name="Width">Width to set the pixel at.</param>
        /// <param name="Height">Height to set the pixel at.</param>
        /// <param name="To_Set">Color to set it too.</param>
        public void SetPixel(int Width, int Height, Color To_Set)
        {
            if (Width < 0 || Width >= width || Height < 0 || Height >= height)
                throw new System.Exception();

            int startIndex = bytesPixel * Width + Height * bytesLine;

            if( bytesPixel == 1 )
            {
                int byteBoundary = ( 4 - bytesLine % 4 ) % 4;

                startIndex = bytesPixel * Width + Height * bytesLine + byteBoundary * Height;

                if( startIndex >= pixels.Length )
                    return;

                byte value = ( To_Set.R > 0 || To_Set.G > 0 || To_Set.B > 0 ) ? byte.MaxValue : byte.MinValue;
                pixels[startIndex] = value;
            }
            else if( bytesPixel == 3 || bytesPixel == 4 )
            {
                pixels[startIndex] = To_Set.B;
                pixels[startIndex + 1] = To_Set.G;
                pixels[startIndex + 2] = To_Set.R;
            }

            if(bytesPixel == 4)
                pixels[startIndex + 3] = To_Set.A;

            needsRefresh = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="color"></param>
        public void SetPixel8Bit(int Width, int Height, byte color)
        {
            System.Diagnostics.Debug.Assert( !( Width < 0 || Width >= width || Height < 0 || Height >= height ),
                                             "Pixel setting was outside the bounds of the image" );

            // Map the height and width into an index into the pixel data
            //int startIndex = bytesPixel * Width + Height * bytesLine;
            int startIndex = bytesPixel * Width + Height * bytesLine + byteBoundary * Height;

            System.Diagnostics.Debug.Assert( startIndex >= 0 && startIndex < pixels.Length,
                                             "Pixel value has gone outside of the images data range" );

            // Set the pixel
            pixels[startIndex] = color;
        }

        /// <summary>
        /// Sets the specified color to be transparent.
        /// </summary>
        /// <param name="toChange">The color to change.</param>
        public void SetTransparent(Color toChange)
        { 
            if(bytesPixel == 4)
            {
                for (int i = 0; i < bytes; i += bytesPixel)
                    if (toChange.B == pixels[i] &&
                        toChange.G == pixels[i + 1] &&
                        toChange.R == pixels[i + 2])
                    {
                        pixels[i + 3] = 0;
                    }
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Changes all pixels of one color, to another but ignores the alpha component.
        /// </summary>
        /// <param name="To_Replace">Color to replace.</param>
        /// <param name="To_Replace_With">Color to replace it with.</param>
        public void ChangeColor(Color To_Replace, Color To_Replace_With)
        {
            if (bytesPixel == 4)
            {
                for (int i = 0; i < bytes; i += bytesPixel)
                    if (pixels[i] == To_Replace.B &&
                        pixels[i + 1] == To_Replace.G &&
                        pixels[i + 2] == To_Replace.R &&
                        pixels[i + 3] == To_Replace.A)
                    {
                        pixels[i] = To_Replace_With.B;
                        pixels[i + 1] = To_Replace_With.G;
                        pixels[i + 2] = To_Replace_With.R;
                        pixels[i + 3] = To_Replace_With.A;
                    }
            }
            else
            {
                for (int i = 0; i < bytes; i += bytesPixel)
                    if (pixels[i] == To_Replace.B &&
                       pixels[i + 1] == To_Replace.G &&
                       pixels[i + 2] == To_Replace.R)
                    {
                        pixels[i] = To_Replace_With.B;
                        pixels[i + 1] = To_Replace_With.G;
                        pixels[i + 2] = To_Replace_With.R;
                    }
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Changes one set of colors to another.  Doesn't affect the alpha component.
        /// </summary>
        /// <param name="To_Replace">Parrallel array of colors to replace.</param>
        /// <param name="To_Replace_With">Parrallel array of colors to replace with.</param>
        public void ChangeColors(Color[] To_Replace, Color[] To_Replace_With)
        {
            if (To_Replace.Length != To_Replace_With.Length)
                throw new Exception("Arrays must be of equal length");

            for (int i = 0; i < bytes; i += bytesPixel)
            {
                byte B = pixels[i];
                byte G = pixels[i + 1];
                byte R = pixels[i + 2];

                for(int i2 = 0; i2 < To_Replace.Length; i2++)
                if (B == To_Replace[i2].B &&
                    G == To_Replace[i2].G &&
                    R == To_Replace[i2].R)
                {
                    pixels[i] = To_Replace_With[i2].B;
                    pixels[i + 1] = To_Replace_With[i2].G;
                    pixels[i + 2] = To_Replace_With[i2].R;
                    break;
                }
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Sets all the colors to a grayScale
        /// </summary>
        public void SetAsGrayScale()
        {
            for (int i = 0; i < bytes; i += bytesPixel)
            {
                byte newColor = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);
                pixels[i] = newColor;
                pixels[i + 1] = newColor;
                pixels[i + 2] = newColor;
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Returns an array of every color that this image contains.
        /// </summary>
        /// <returns></returns>
        public Color[] GetColors()
        {
            ArrayList colors = new ArrayList();

            int start_index;
            Color to_add = Color.Black;

            if (bytesPixel == 4)
                for (int column = 0; column < width; column++)
                    for (int row = 0; row < height; row++)
                    {
                        start_index = bytesPixel * column + row * bytesLine;

                        to_add = Color.FromArgb(pixels[start_index + 3],
                                                pixels[start_index + 2],
                                                pixels[start_index + 1],
                                                pixels[start_index]);

                        if (!colors.Contains(to_add))
                            colors.Add(to_add);
                    }
            else
                for (int column = 0; column < width; column++)
                    for (int row = 0; row < height; row++)
                    {
                        start_index = bytesPixel * column + row * bytesLine;

                        to_add = Color.FromArgb(pixels[start_index + 2],
                                                pixels[start_index + 1],
                                                pixels[start_index]);
                        if (!colors.Contains(to_add))
                            colors.Add(to_add);
                    }

            return (Color[])colors.ToArray(to_add.GetType());
        }

        /// <summary>
        /// Inverts all the colors in the InteractiveImage.
        /// </summary>
        public void InvertColors()
        {
            if (bytesPixel == 4)
            {
                for (int i = 0; i < bytes; i += bytesPixel)
                {
                    pixels[i] = (byte)(byte.MaxValue - pixels[i]);
                    pixels[i + 1] = (byte)(byte.MaxValue - pixels[i + 1]);
                    pixels[i + 2] = (byte)(byte.MaxValue - pixels[i + 2]);
                }
            }
            else
            {
                throw new NotImplementedException( );
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Draws an image onto the current image.  Transparencies do not apply, but it's fast.
        /// </summary>
        /// <param name="toDraw">The image to draw.</param>
        /// <param name="x">The location of x to draw at.</param>
        /// <param name="y">The location of y to draw at.</param>
        public void OverlayImage(Interactive8BitImage toDraw, int x, int y)
        {
            int columnWidth = toDraw.width;
            int rowHeight = toDraw.height;
            int heightCutoff = 0;
            int widthCutoff = 0;

            if(x < 0)
            {
                widthCutoff = x;
                columnWidth -= widthCutoff;
            }
            if(y < 0)
            {  
                heightCutoff = y;
                rowHeight += heightCutoff;
            }

            if (x + toDraw.width > this.width)
                columnWidth -= (x + toDraw.width - this.width);
            if (y + toDraw.height > this.height)
                rowHeight -= y + toDraw.height - this.height;

            if(bytesPixel == 4 && toDraw.bytesPixel == 4 && columnWidth > 0 && rowHeight > 0)
            {

                for (int row = 0; row < rowHeight; row++)
                    Array.Copy(toDraw.pixels, (row - heightCutoff) * toDraw.bytesLine, this.pixels,
                        (y + row - heightCutoff) * this.bytesLine + x * this.bytesPixel, columnWidth * toDraw.bytesPixel);
            }



            needsRefresh = true;
           }

        public void Clear(Color rgb)
        {
            byte A,R,G,B;

            R = rgb.R;
            G = rgb.G;
            B = rgb.B;

            if( bytesPixel == 1 )
            {
                A = 0;
                if( R > 0 || G > 0 || B > 0 )
                    A = byte.MaxValue;
                for( int i = 0; i < pixels.Length; i++ )
                    pixels[i] = A;
            }
            else if( bytesPixel == 3 )
            {
                for( int i = 0; i < pixels.Length; )
                {
                    pixels[i + RED_BIT] = R;
                    pixels[i + BLUE_BIT] = B;
                    pixels[i + GREEN_BIT] = G;
                    i += bytesPixel;
                }
            }
            else if( bytesPixel == 4 )
            {
                A = rgb.A;

                for( int i = 0; i < pixels.Length; )
                {
                    pixels[i + RED_BIT] = R;
                    pixels[i + BLUE_BIT] = B;
                    pixels[i + GREEN_BIT] = G;
                    pixels[i + ALPHA_BIT] = A;
                    i += bytesPixel;
                }
            }
        }

        /// <summary>
        /// Draws an image onto the current image.  Transparencies do apply but it's slower than OverlayImage.
        /// </summary>
        /// <param name="toDraw">The image to draw.</param>
        /// <param name="x">The location of x to draw at.</param>
        /// <param name="y">The location of y to draw at.</param>
        public void DrawImage(Interactive8BitImage toDraw, int x, int y)
        {
            //Source for blending equations
            //http://en.wikipedia.org/wiki/Alpha_compositing
            //http://en.wikipedia.org/wiki/Transparency_%28graphic%29
            int columnWidth = toDraw.width;
            int rowHeight = toDraw.height;
            int heightCutoff = 0;
            int widthCutoff = 0;

            if (x < 0)
            {
                widthCutoff = x;
                columnWidth -= widthCutoff;
            }
            if (y < 0)
            {
                heightCutoff = y;
                rowHeight += heightCutoff;
            }

            if (x + toDraw.width > this.width)
                columnWidth -= (x + toDraw.width - this.width);
            if (y + toDraw.height > this.height)
                rowHeight -= y + toDraw.height - this.height;

            byte thisA;
            byte thatA;
            int A;
            int R;
            int G;
            int B;
            int sourceRow;
            int targetRow;
            int thisCurrentColor;
            int thatCurrentColor;


            if (bytesPixel == 4 && toDraw.bytesPixel == 4 && columnWidth > 0 && rowHeight > 0)
            {
                for (int row = 0; row < rowHeight; row++)
                {
                    sourceRow = (row - heightCutoff) * toDraw.bytesLine;
                    targetRow = (y + row - heightCutoff) * this.bytesLine + x * this.bytesPixel;

                    for (int column = 0; column < columnWidth; column++)
                    {
                        thisCurrentColor = targetRow + column * this.bytesPixel;
                        thatCurrentColor = sourceRow + column * toDraw.bytesPixel;

                        thisA = this.pixels[thisCurrentColor + ALPHA_BIT];
                        thatA = toDraw.pixels[thatCurrentColor + ALPHA_BIT];

                        A = (thatA + thisA * (byte.MaxValue - thatA));// thisA + thatA;

                        R = (this.pixels[thisCurrentColor + RED_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + RED_BIT] * thatA) / A;

                        G = (this.pixels[thisCurrentColor + GREEN_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + GREEN_BIT] * thatA)/A;

                        B = (this.pixels[thisCurrentColor + BLUE_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + BLUE_BIT] * thatA)/A;


                        /* R = (this.pixels[thisCurrentColor + RED_BIT] * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + RED_BIT] * (thatA))/byte.MaxValue;

                        G = (this.pixels[thisCurrentColor + GREEN_BIT] * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + GREEN_BIT] * (thatA)) / byte.MaxValue;

                        B = (this.pixels[thisCurrentColor + BLUE_BIT] * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + BLUE_BIT] * (thatA)) / byte.MaxValue;
                         
                         R = (this.pixels[thisCurrentColor + RED_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + RED_BIT] * thatA) / A;

                        G = (this.pixels[thisCurrentColor + GREEN_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + GREEN_BIT] * thatA)/A;

                        B = (this.pixels[thisCurrentColor + BLUE_BIT] * thisA * (byte.MaxValue - thatA) +
                            toDraw.pixels[thatCurrentColor + BLUE_BIT] * thatA)/A;
                         */

                        if (A > byte.MaxValue)
                            A = byte.MaxValue;
                        else if (A < byte.MinValue)
                            A = byte.MinValue;

                        this.pixels[thisCurrentColor + ALPHA_BIT] = (byte)(A);
                        this.pixels[thisCurrentColor + BLUE_BIT] = (byte)B;
                        this.pixels[thisCurrentColor + GREEN_BIT] = (byte)G;
                        this.pixels[thisCurrentColor + RED_BIT] = (byte)R;

                    }
                }
            }



            needsRefresh = true;
        }

        /// <summary>
        /// Rotates the image 180 degrees.
        /// </summary>
        public void Rotate180()
        {
            byte holder = 0;

            for (int i = 0; i < pixels.Length / 2; i += bytesPixel)
            {
                holder = pixels[i + ALPHA_BIT];
                pixels[i + ALPHA_BIT] = pixels[pixels.Length - i  + ALPHA_BIT - bytesPixel];
                pixels[pixels.Length - i + ALPHA_BIT - bytesPixel] = holder;

                holder = pixels[i + RED_BIT];
                pixels[i + RED_BIT] = pixels[pixels.Length - i + RED_BIT - bytesPixel];
                pixels[pixels.Length - i + RED_BIT - bytesPixel] = holder;

                holder = pixels[i + GREEN_BIT];
                pixels[i + GREEN_BIT] = pixels[pixels.Length - i + GREEN_BIT - bytesPixel];
                pixels[pixels.Length - i + GREEN_BIT - bytesPixel] = holder;

                holder = pixels[i + BLUE_BIT];
                pixels[i + BLUE_BIT] = pixels[pixels.Length - i + BLUE_BIT - bytesPixel];
                pixels[pixels.Length - i + BLUE_BIT - bytesPixel] = holder;
            }

            needsRefresh = true;
        }

        /// <summary>
        /// Width of the image.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Height of the image.
        /// </summary>
        public int Height
        {
            get { return height; }
        }
    }
}
