using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowStorm.Utility
{
	/// <summary>
	/// Keeps a running average of the frames per second.
	/// </summary>
	public class FpsCounter
	{
		int[] timePerFrameInMilliseconds;
		int totalMilliseconds;
		int currentFrameIndex = 0;
		Stopwatch stopwatch;

		public int Fps { get; private set; }

		/// <summary>
		/// Creates a new instance of a frame counter that will keep the results for <paramref name="frameCount"/> number of frames.
		/// </summary>
		/// <param name="frameCount">The number of frames to keep in the window, the large it 
		/// is the slower it changes but the more resistant it is to spikes.</param>
		public FpsCounter(int frameCount)
		{
			if (frameCount <= 0)
				throw new ArgumentException($"{nameof(frameCount)} must be larger than 0");

			timePerFrameInMilliseconds = new int[frameCount];
			stopwatch = new Stopwatch();
		}

		public void Start()
		{
			stopwatch.Start();
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		/// <summary>
		/// Updates the running total, and returns the current frames per second.
		/// </summary>
		/// <param name="currentFrameMilliseconds"></param>
		/// <returns></returns>
		public void MarkFrame()
		{
			int currentFramesMilliseconds = (int)stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();

			totalMilliseconds -= timePerFrameInMilliseconds[currentFrameIndex];
			totalMilliseconds += currentFramesMilliseconds;

			timePerFrameInMilliseconds[currentFrameIndex] = currentFramesMilliseconds;

			currentFrameIndex = (currentFrameIndex + 1) % timePerFrameInMilliseconds.Length;

			const int millisecondsPerSecond = 1000;
			if (totalMilliseconds == 0)
				Fps = 0;
			else
				Fps = millisecondsPerSecond * timePerFrameInMilliseconds.Length / totalMilliseconds; 
		}
	}
}
