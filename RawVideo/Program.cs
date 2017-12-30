using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using Intermission;

namespace RawVideo
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// Trigger initialization of the pack:// URI scheme.
			new System.Windows.Application();

			var outputStream = Console.OpenStandardOutput();

			byte[] frameBuffer = new byte[1280 * 720 * 4];

			int frameNumber = 0;

			foreach (var frame in new Renderer().Render())
			{
				Console.Error.WriteLine("Delivering frame {0}", frameNumber);
				frameNumber++;

				frame.CopyPixels(frameBuffer, 1280 * 4, 0);

				outputStream.Write(frameBuffer, 0, frameBuffer.Length);
			}
		}
	}
}
