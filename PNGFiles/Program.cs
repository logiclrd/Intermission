using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Intermission;

namespace PNGFiles
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			RenderMode mode = RenderMode.Standard;

			if (args.Length > 0)
				Enum.TryParse<RenderMode>(args[0], out mode);

			// Trigger initialization of the pack:// URI scheme.
			new System.Windows.Application();

			int frameNumber = 0;

			foreach (var frame in new Renderer() { Mode = mode }.Render())
			{
				Console.Error.WriteLine("Saving frame {0}", frameNumber);

				var encoder = new PngBitmapEncoder();

				encoder.Frames.Add(BitmapFrame.Create(frame));

				using (var outputStream = File.Create("Frame" + frameNumber.ToString("d5") + ".png"))
					encoder.Save(outputStream);

				GC.Collect();

				frameNumber++;
			}
		}
	}
}
