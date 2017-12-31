using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Intermission
{
	public class Renderer
	{
		public IEnumerable<RenderTargetBitmap> Render()
		{
			int startFrame = 1228; // from Lobby.mkv
			int endFrame = 600 * 30000 / 1001;

			Noise noise = new Noise();
			Fade fade = new FadeOut();

			fade.StartFrame = endFrame - 2 * 30000 / 1001;
			fade.EndFrame = endFrame;

			for (int frameNumber = startFrame; frameNumber <= endFrame; frameNumber++)
			{
				TimeSpan pts = TimeSpan.FromSeconds(frameNumber / (30000.0 / 1001.0));

				var frame = RenderFrame(frameNumber, pts, noise, fade);

				frame.Freeze();

				yield return frame;
			}
		}

		RenderTargetBitmap RenderFrame(int frameNumber, TimeSpan pts, Noise noise, Fade fade)
		{
			var target = new RenderTargetBitmap(1280, 720, 96.0, 96.0, PixelFormats.Pbgra32);

			var visual = ComposeVisual(pts);

			visual = noise.ApplyTo(visual);
			visual = fade.ApplyTo(frameNumber, visual);

			visual.Measure(new Size(1280, 720));
			visual.Arrange(new Rect(0, 0, 1280, 720));

			target.Render(visual);

			return target;
		}

		static FontFamily s_lhfClassicCapsFontFamily = new FontFamily("LHF Classic Caps");

		UIElement ComposeVisual(TimeSpan pts)
		{
			var layout = new Grid();

			layout.Width = 1280;
			layout.Height = 720;
			layout.Background = Brushes.Black;

			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

			var intermissionLabel =
				new Label()
				{
					Content = "Intermission",
					FontFamily = s_lhfClassicCapsFontFamily,
					FontSize = 120,
					Foreground = Brushes.WhiteSmoke,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
				};

			var timeRemainingLabel =
				new Label()
				{
					FontFamily = s_lhfClassicCapsFontFamily,
					FontSize = 50,
					Foreground = Brushes.WhiteSmoke,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
				};

			double minutesRemaining = Math.Floor(10 - pts.TotalMinutes);

			if (minutesRemaining < 1)
				timeRemainingLabel.Content = "The presentation will resume momentarily";
			else
			{
				var tbMinutes = new TextBlock() { Text = minutesRemaining.ToString(), Width = 30, TextAlignment = TextAlignment.Right };
				var tbRemaining = new TextBlock() { Text = (minutesRemaining > 1) ? " minutes remaining" : " minute remaining" };

				var spParts = new StackPanel() { Orientation = Orientation.Horizontal };

				spParts.Children.Add(tbMinutes);
				spParts.Children.Add(tbRemaining);

				timeRemainingLabel.Content = spParts;
			}

			var fancyTop = new FancyTop() { Foreground = Brushes.WhiteSmoke, Margin = new Thickness(15) };
			var fancyBottom = new FancyBottom() { Foreground = Brushes.WhiteSmoke, Margin = new Thickness(15) };

			Grid.SetRow(fancyTop, 0);
			Grid.SetRow(intermissionLabel, 1);
			Grid.SetRow(timeRemainingLabel, 2);
			Grid.SetRow(fancyBottom, 3);

			layout.Children.Add(fancyTop);
			layout.Children.Add(intermissionLabel);
			layout.Children.Add(timeRemainingLabel);
			layout.Children.Add(fancyBottom);

			return layout;
		}
	}
}
