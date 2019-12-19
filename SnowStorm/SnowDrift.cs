using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

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
		/// <summary>
		/// Drawing buffer for all the snowflakes.
		/// </summary>
		private Interactive8BitImage flakeBuffer;
		/// <summary>
		/// All the snowflakes in this SnowDrift.
		/// </summary>
		private SnowFlake[] snowflakes;
		/// <summary>
		/// Number of flakes created in this SnowDrfit, not neccessarily the
		/// number of active flakes.
		/// </summary>
		private int numFlakes = 0;

		/// <summary>
		/// Size of the screen, adjusted to fit in all the flake.
		/// </summary>
		private Size screenSize;
		/// <summary>
		/// General wind direction of this SnowDrift.
		/// </summary>
		private Vector generalDirection;

		/// <summary>
		/// Number of SnowFlakes to turn off as soon as possible
		/// </summary>
		private int snowFlakesToRemove = 0;
		/// <summary>
		/// Lowest index of any SnowFlake that is turned off.
		/// </summary>
		private int lowestTurnedOfIndex = int.MaxValue;
		/// <summary>
		/// Number of SnowFlakes that are turne
		/// </summary>
		private int snowFlakesTurnedOff = 0;

		/// <summary>
		/// Lock for removing snowflakes from the drift.
		/// </summary>
		private readonly object removalLock = new object();

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
			flakeBuffer = new Interactive8BitImage(screenSize.Width,
												screenSize.Height,
												System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			flakeBuffer.SetAlpha(byte.MaxValue);

			// Create the wind variations
			generalDirection = new Vector(Random.Int(-5, 5), Random.Int(1, 4));
			Vector variance = new Vector(12, 4);

			windVariations = new WindField[100];
			for (int i = 0; i < windVariations.Length; i++)
			{
				windVariations[i] = new WindField(screenSize.Width, screenSize.Height, generalDirection, variance, new Size(15, 15));
			}

			snowflakes = new SnowFlake[Properties.Settings.Default.MaximumNumberOfFlakes];
			AddFlakes();
		}


		/// <summary>
		/// Adds another snowflake to the SnowDrift.
		/// </summary>
		public void AddFlakes()
		{
			// If some are going to be removed, just don't remove them
			if (snowFlakesToRemove > 0)
			{
				snowFlakesToRemove--;
			}
			// Add back a previously created SnowFlake
			else if (snowFlakesTurnedOff > 0)
			{
				// Reactivate a turned off flake
				snowflakes[lowestTurnedOfIndex].IsActive = true;
				snowFlakesTurnedOff--;

				// Find the next flake that is turned off
				while (lowestTurnedOfIndex < numFlakes && snowflakes[lowestTurnedOfIndex].IsActive)
					lowestTurnedOfIndex++;

				// Set the index out of bounds again so that adding more snowflakes doesn't invalidate the index
				if (lowestTurnedOfIndex >= numFlakes)
					lowestTurnedOfIndex = int.MaxValue;
			}
			else  // Create a new SnowFlake
			{
				// Don't overflow the array
				if (numFlakes < snowflakes.Length)
				{
					// Create a snowflake and add it, giving it a somewhat random wind field
					SnowFlake newFlake = new SnowFlake(GetNewSnowFlakePosition(),
														new Vector(),
														Random.Short(MIN_INERTIA, MAX_INERTIA),
														windVariations[numFlakes % windVariations.Length],
														 SnowFlake.GetRandomFlakeSize());
					newFlake.IsActive = true;
					snowflakes[numFlakes++] = newFlake;
				}
			}
		}

		public void RemoveFlakes(int numRemove)
		{
			snowFlakesToRemove += numRemove;
			if (snowFlakesToRemove > numFlakes)
				snowFlakesToRemove = numFlakes;
		}

		/// <summary>
		/// Draws the snowdrift onto the graphics buffer.
		/// </summary>
		/// <param name="g">Graphics buffer to draw on.</param>
		public void Draw(Graphics g)
		{
			flakeBuffer.Clear(Color.Black);

			// Draw each snowflakes pattern
			Parallel.For(0, numFlakes, (i) =>
			{
				if (snowflakes[i].IsActive)
					snowflakes[i].Draw(flakeBuffer);
			});

			//Draw to the buffer with enough border for the flakes to move offscreen
			g.DrawImage(flakeBuffer.GetImage(),
						 -(int)SnowFlake.FLAKE_SIZES.Max(),
						 -(int)SnowFlake.FLAKE_SIZES.Max());
		}

		/// <summary>
		/// Gets a random position along the edge for a new vector.
		/// </summary>
		/// <returns>A new position vector.</returns>
		private Vector GetNewSnowFlakePosition()
		{
			int position = this.flakeBuffer.Width + this.flakeBuffer.Height;
			int x, y;
			position = Random.Int(0, position - 1);

			y = position - this.flakeBuffer.Width;

			if (y > 0)
			{
				if (generalDirection.x > 0)
					x = 0;
				else
					x = flakeBuffer.Width - 1;
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
			Parallel.For(0, numFlakes, (i) =>
			{
				UpdateFlake(i);
			});
		}

		/// <summary>
		/// Updates the flake and the indicated index.
		/// Removes some flakes if they're now out of bounds and
		/// flakes need to be withdrawn.
		/// </summary>
		/// <param name="snowFlakeIndex">Index of flake to update.</param>
		private void UpdateFlake(int snowFlakeIndex)
		{
			SnowFlake updating = snowflakes[snowFlakeIndex];
			if (updating.IsActive)
			{
				if (!updating.InBounds(screenSize))
				{
					updating.ReInit(GetNewSnowFlakePosition(),
										new Vector(),
										Random.Short(MIN_INERTIA, MAX_INERTIA));

					// If SnowFlakes should be removed, remove one
					if (snowFlakesToRemove > 0)
					{
						lock (removalLock)
						{
							// We'll repeat this test, just so the outer one can skip
							// the lock if there are no snowflakes to remove 
							if (snowFlakesToRemove > 0)
							{
								updating.IsActive = false;
								snowFlakesToRemove--;
								snowFlakesTurnedOff++;

								// Update the index if necessary
								if (snowFlakeIndex < lowestTurnedOfIndex)
									lowestTurnedOfIndex = snowFlakeIndex;
							}
						}
					}
				}

				updating.Accelerate();
				updating.Move();
			}
		}

		/// <summary>
		/// Gets the number of SnowFlakes currently in this SnowDrift.
		/// </summary>
		public int SnowFlakeCount
		{
			get { return numFlakes - snowFlakesTurnedOff; }
		}

		/// <summary>
		/// Percentage of the capacity of the SnowDrift used up.
		/// </summary>
		public float Fullness
		{
			get { return (float)SnowFlakeCount / snowflakes.Length; }
		}
	}
}
