using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SnowStorm
{
	/// <summary>
	/// Represents the snow on the field, this class brings snow from the sky and gently combines it with snow drifts.
	/// </summary>
	class SnowDrift
	{
		#region Constants

		/// <summary>
		/// Minimum inertia for flakes.
		/// </summary>
		private const short MIN_INERTIA = 25;
		/// <summary>
		/// Maximum inertia for flakes.
		/// </summary>
		private const short MAX_INERTIA = 40;

		#endregion

		#region Fields

		/// <summary>
		/// Different wind fields, either switched out for different snowflakes or for different times.
		/// </summary>
		private WindField[] windVariations;
		private Interactive8BitImage nextBuffer;
		private Interactive8BitImage renderedScene;

		/// <summary>
		/// All the snowflakes in this SnowDrift.
		/// </summary>
		private HashSet<SnowFlake> snowflakes;

		/// <summary>
		/// Size of the screen, adjusted to fit in all the flake.
		/// </summary>
		private Size screenSize;
		/// <summary>
		/// General wind direction of this SnowDrift.
		/// </summary>
		private Vector generalDirection;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of SnowFlakes currently in this SnowDrift.
		/// </summary>
		public int SnowFlakeCount
		{
			get { return snowflakes.Count; }
		}

		/// <summary>
		/// The number of flakes the snowdrift is aiming to have
		/// </summary>
		public int TargetFlakes
		{
			get;
			set;
		}

		#endregion

		/// <summary>
		/// Creates a new SnowDrift to fit the indicated screen size.
		/// </summary>
		/// <param name="screenSize">Size of the screen to fill with snowflakes.</param>
		public SnowDrift(Size screenSize)
		{
			// Add in enough room so that snowflakes can drift off screen without just flashing away
			screenSize.Width += (int)SnowFlake.FLAKE_SIZES.Max() * 2;
			screenSize.Height += (int)SnowFlake.FLAKE_SIZES.Max() * 2;
			this.screenSize = screenSize;

			// Create a black and white buffer based on the adjusted screen sized
			nextBuffer = new Interactive8BitImage(screenSize.Width,
												screenSize.Height,
												System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			nextBuffer.SetAlpha(byte.MaxValue);
			renderedScene = new Interactive8BitImage(screenSize.Width,
												screenSize.Height,
												System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			renderedScene.SetAlpha(byte.MaxValue);

			// Create the wind variations
			generalDirection = new Vector(Random.Int(-5, 5), Random.Int(1, 4));
			Vector variance = new Vector(12, 4);

			windVariations = new WindField[100];
			for (int i = 0; i < windVariations.Length; i++)
			{
				windVariations[i] = new WindField(screenSize.Width, screenSize.Height, generalDirection, variance, new Size(15, 15));
			}

			snowflakes = new HashSet<SnowFlake>(Properties.Settings.Default.MaximumNumberOfFlakes);
		}

		#region Methods

		/// <summary>
		/// Draws the snowdrift onto the graphics buffer.
		/// </summary>
		/// <param name="g">Graphics buffer to draw on.</param>
		public void Draw(Graphics g)
		{
			//Draw to the buffer with enough border for the flakes to move offscreen
			g.DrawImage(renderedScene.GetImage(),
						 -(int)SnowFlake.FLAKE_SIZES.Max(),
						 -(int)SnowFlake.FLAKE_SIZES.Max());
		}

		public void RenderDisplay()
		{
			nextBuffer.Clear(Color.Black);

			// Draw each snowflakes pattern
			Parallel.ForEach(snowflakes, flake =>
			{
				flake.Draw(nextBuffer);
			});

			swapBuffers();
		}

		private void swapBuffers()
		{
			Interactive8BitImage temp = renderedScene;
			renderedScene = nextBuffer;
			nextBuffer = temp;
		}

		/// <summary>
		/// Gets a random position along the edge for a new vector.
		/// </summary>
		/// <returns>A new position vector.</returns>
		private Vector GetNewSnowFlakePosition()
		{
			int position = this.nextBuffer.Width + this.nextBuffer.Height;
			int x, y;
			position = Random.Int(0, position - 1);

			y = position - this.nextBuffer.Width;

			if (y > 0)
			{
				if (generalDirection.x > 0)
					x = 0;
				else
					x = nextBuffer.Width - 1;
			}
			else
			{
				// Set the x position, and make sure it's not greater than the width
				x = position;
				if (x >= this.screenSize.Width)
					x = this.screenSize.Width - 1;

				y = 0;
			}

			return new Vector(x, y);
		}

		/// <summary>
		/// Updates the SnowDrift to the next state.
		/// </summary>
		public void Update()
		{
			int snowflakeDelta = TargetFlakes - SnowFlakeCount;

			// Add any snowflakes as needed
			if(snowflakeDelta > 0)
			{
				for (int i = 0; i < snowflakeDelta; i++)
				{

					SnowFlake newFlake = new SnowFlake(GetNewSnowFlakePosition(),
														new Vector(),
														Random.Short(MIN_INERTIA, MAX_INERTIA),
														windVariations[SnowFlakeCount % windVariations.Length],
														SnowFlake.GetRandomFlakeSize());
					snowflakes.Add(newFlake);
				}
			}

			ConcurrentBag<SnowFlake> toRemove = new ConcurrentBag<SnowFlake>();

			Parallel.ForEach(snowflakes, flake =>
			{
				flake.Move();

				if (!flake.InBounds(screenSize))
				{
					toRemove.Add(flake);
				}
				else
				{
					flake.Accelerate();
				}
			});

			snowflakes.ExceptWith(toRemove.ToArray());
		}

		#endregion
	}
}
