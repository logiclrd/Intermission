using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Intermission
{
	public abstract class Fade
	{
		public int StartFrame;
		public int EndFrame;

		protected double T(int frameNumber)
		{
			if (frameNumber < StartFrame)
				return 0.0;
			if (frameNumber > EndFrame)
				return 1.0;

			return (frameNumber - StartFrame) / (double)(EndFrame - StartFrame);
		}

		protected abstract double Opacity(int frameNumber);

		public UIElement ApplyTo(int frameNumber, UIElement visual)
		{
			double opacity = Opacity(frameNumber);

			if (opacity == 1)
				return visual;
			else
			{
				var fader = new Grid();

				fader.Background = Brushes.Black;
				fader.Children.Add(visual);

				visual.Opacity = opacity;

				return fader;
			}
		}
	}
}
