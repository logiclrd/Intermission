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
			RenderMode mode = RenderMode.Standard;

			if (args.Length > 0)
				Enum.TryParse<RenderMode>(args[0], out mode);

			Console.Error.WriteLine("Render mode: {0}", mode);

			var renderer = new Renderer() { Mode = mode };

			if (args.Length > 1)
				renderer.Duration = double.Parse(args[1]);

			Console.Error.WriteLine("Duration: {0}", TimeSpan.FromSeconds(renderer.Duration));

			// Trigger initialization of the pack:// URI scheme.
			new System.Windows.Application();

			var outputStream = Console.OpenStandardOutput();

			byte[] frameBuffer = new byte[1920 * 1080 * 4];

			int frameNumber = 0;

			foreach (var frame in renderer.Render())
			{
				Console.Error.WriteLine("Delivering frame {0}", frameNumber);
				frameNumber++;

				frame.CopyPixels(frameBuffer, 1920 * 4, 0);

				outputStream.Write(frameBuffer, 0, frameBuffer.Length);
			}
		}
	}
}
