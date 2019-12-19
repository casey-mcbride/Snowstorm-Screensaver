using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowStorm.Utility
{
	public sealed class TimingScope : IDisposable
	{
		Stopwatch timing;
		string tag;

		public TimingScope(string tag)
		{
			this.tag = tag;
			timing = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			Console.WriteLine($"{tag}: {timing.Elapsed}");
		}
	}
}
