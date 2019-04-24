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
		RenderMode _mode = RenderMode.DazedAndConfused;

		public RenderMode Mode
		{
			get { return _mode; }
			set { _mode = value; }
		}

		public IEnumerable<RenderTargetBitmap> Render()
		{
			double duration;
			double ptsAdjustment = 0.0;

			int startFrame = 1228; // from Lobby.mkv

			switch (_mode)
			{
				case RenderMode.MontyPython: ptsAdjustment = 8.69; duration = 600 + ptsAdjustment; break;
				case RenderMode.DazedAndConfused: duration = 647.960 + startFrame * 1001d / 30000d; break;
				default: duration = 600; break;
			}

			int endFrame = (int)(duration * 30000 / 1001);

			Noise noise = new Noise();
			Fade fade = new FadeOut();

			if (_mode == RenderMode.MontyPython)
			{
				noise.EnableJiggle = false;
				noise.EnableScratches = false;
				noise.EnableDust = false;
				noise.EnableOpacityMask = false;
			}
			else
			{
				noise.EnableWander = false;
			}

			fade.StartFrame = endFrame - 2 * 30000 / 1001;
			fade.EndFrame = endFrame;

			for (int frameNumber = startFrame; frameNumber <= endFrame; frameNumber++)
			{
				TimeSpan pts = TimeSpan.FromSeconds(frameNumber / (30000.0 / 1001.0) - ptsAdjustment);

				var frame = RenderFrame(frameNumber, pts, duration, noise, fade);

				frame.Freeze();

				yield return frame;
			}
		}

		RenderTargetBitmap RenderFrame(int frameNumber, TimeSpan pts, double duration, Noise noise, Fade fade)
		{
			if (_mode == RenderMode.MontyPython)
			{
				var buffer = new RenderTargetBitmap(7680, 4320, 384.0, 384.0, PixelFormats.Pbgra32);

				var visual = ComposeVisual(pts, duration);

				visual.SnapsToDevicePixels = false;

				visual = noise.ApplyTo(visual);
				visual = fade.ApplyTo(frameNumber, visual);

				visual.Measure(new Size(1920, 1080));
				visual.Arrange(new Rect(0, 0, 1920, 1080));

				buffer.Render(visual);

				var bufferElement = new ImageDrawing(buffer, new Rect(0, 0, 1920, 1080));
				var bufferVisual = new DrawingVisual();

				RenderOptions.SetBitmapScalingMode(bufferElement, BitmapScalingMode.HighQuality);

				using (var context = bufferVisual.RenderOpen())
					context.DrawDrawing(bufferElement);

				var target = new RenderTargetBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Pbgra32);

				target.Render(bufferVisual);

				return target;
			}
			else
			{
				var target = new RenderTargetBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Pbgra32);

				var visual = ComposeVisual(pts, duration);

				visual = noise.ApplyTo(visual);
				visual = fade.ApplyTo(frameNumber, visual);

				visual.Measure(new Size(1920, 1080));
				visual.Arrange(new Rect(0, 0, 1920, 1080));

				target.Render(visual);

				return target;
			}
		}

		static FontFamily s_lhfClassicCapsFontFamily = new FontFamily("LHF Classic Caps");
		static FontFamily s_windsorBTFontFamily = new FontFamily("Windsor BT");

		UIElement ComposeVisual(TimeSpan pts, double duration)
		{
			var layout = new Grid();

			layout.Width = 1920;
			layout.Height = 1080;
			layout.Background = (_mode == RenderMode.MontyPython) ? Brushes.Transparent : Brushes.Black;

			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			layout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

			var font = (_mode == RenderMode.MontyPython) ? s_windsorBTFontFamily : s_lhfClassicCapsFontFamily;

			var intermissionLabel =
				new Label()
				{
					Content = "Intermission",
					FontFamily = font,
					FontSize = 180,
					Foreground = Brushes.WhiteSmoke,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
				};

			var timeRemainingLabel =
				new Label()
				{
					FontFamily = font,
					FontSize = 75,
					Foreground = Brushes.WhiteSmoke,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
				};

			double minutesRemaining = Math.Ceiling(duration / 60.0 - pts.TotalMinutes);

			if (minutesRemaining > 10)
				minutesRemaining = 10;

			if (minutesRemaining <= 1)
				timeRemainingLabel.Content = "The presentation will resume momentarily";
			else
			{
				int spaceForMinutes = (_mode == RenderMode.MontyPython) ? 95 : 75;

				var tbMinutes = new TextBlock() { Text = minutesRemaining.ToString(), Width = spaceForMinutes, TextAlignment = TextAlignment.Right };
				var tbRemaining = new TextBlock() { Text = (minutesRemaining > 1) ? " minutes remaining" : " minute remaining", Margin = new Thickness(0, 0, 30, 0) };

				var spParts = new StackPanel() { Orientation = Orientation.Horizontal };

				spParts.Children.Add(tbMinutes);
				spParts.Children.Add(tbRemaining);

				timeRemainingLabel.Content = spParts;
			}

			Grid.SetRow(intermissionLabel, 1);
			Grid.SetRow(timeRemainingLabel, 2);

			if (_mode == RenderMode.MontyPython)
			{
				layout.Children.Add(intermissionLabel);
				layout.Children.Add(timeRemainingLabel);
			}
			else
			{
				var fancyTop = new FancyTop() { Foreground = Brushes.WhiteSmoke, Margin = new Thickness(22.5) };
				var fancyBottom = new FancyBottom() { Foreground = Brushes.WhiteSmoke, Margin = new Thickness(22.5) };

				Grid.SetRow(fancyTop, 0);
				Grid.SetRow(fancyBottom, 3);

				layout.Children.Add(fancyTop);
				layout.Children.Add(intermissionLabel);
				layout.Children.Add(timeRemainingLabel);
				layout.Children.Add(fancyBottom);
			}

			return layout;
		}
	}
}
