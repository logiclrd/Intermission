using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Intermission
{
	class Noise
	{
		Random _rnd = new Random();

		bool _enableJiggle = true;
		bool _inJiggle;
		Point _jiggleBaseOffset;
		int _jiggleCooldown;

		bool _enableWander = true;
		double _wanderMagnitude = 0.5;
		double _wanderX;
		double _wanderY;
		double _wanderNextX;
		double _wanderNextY;

		bool _enableScratches = true;
		ScratchLine[] _scratchLines = null;
		double _scratchLineIntensity;
		double _scratchLineDelta;
		int _scratchLineCooldown;

		bool _enableDust = true;
		double _dustLevel = 0.2;
		double _dustIntensity;

		bool _enableOpacityMask = true;

		public bool EnableJiggle
		{
			get { return _enableJiggle; }
			set { _enableJiggle = value; }
		}

		public bool EnableWander
		{
			get { return _enableWander; }
			set { _enableWander = value; }
		}

		public double WanderMagnitude
		{
			get { return _wanderMagnitude; }
			set { _wanderMagnitude = value; }
		}

		public bool EnableScratches
		{
			get { return _enableScratches; }
			set { _enableScratches = value; }
		}

		public bool EnableDust
		{
			get { return _enableDust; }
			set { _enableDust = value; }
		}

		public double DustLevel
		{
			get { return _dustLevel; }
			set { _dustLevel = value; }
		}

		public bool EnableOpacityMask
		{
			get { return _enableOpacityMask; }
			set { _enableOpacityMask = value; }
		}

		class ScratchLine
		{
			public double Intensity;
			public double X;
			public double DX;
		}

		ImageBrush _opacityMask;

		public Noise()
		{
			_opacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Intermission;component/OpacityMask.png")));
			_opacityMask.Stretch = Stretch.Fill;
		}

		public UIElement ApplyTo(UIElement visual)
		{
			Canvas effects = new Canvas();

			effects.Children.Add(visual);

			if (_inJiggle)
			{
				Canvas.SetLeft(visual, _jiggleBaseOffset.X + _rnd.NextDouble() * 2 - 1);
				Canvas.SetTop(visual, _jiggleBaseOffset.Y + _rnd.NextDouble() * 2 - 1);

				_jiggleCooldown--;

				if (_jiggleCooldown <= 0)
				{
					_jiggleCooldown = 300;
					_inJiggle = false;
				}
			}
			else if (_enableJiggle)
			{
				if (_jiggleCooldown > 0)
					_jiggleCooldown--;
				else
				{
					if (_rnd.NextDouble() < 0.002)
					{
						_inJiggle = true;
						_jiggleBaseOffset = new Point(_rnd.Next(-15, -10), _rnd.Next(-15, -10));
						_jiggleCooldown = _rnd.Next(3, 20);
					}
				}
			}

			if (_enableWander)
			{
				var wanderTransformation = new TranslateTransform();

				wanderTransformation.X = _wanderX;
				wanderTransformation.Y = _wanderY;

				visual.RenderTransform = wanderTransformation;

				double dx = _wanderNextX - _wanderX;
				double dy = _wanderNextY - _wanderY;

				double d = Math.Sqrt(dx * dx + dy * dy);

				double step = _wanderMagnitude / 4;

				if (d + _wanderMagnitude * 0.05 < step)
				{
					_wanderX = _wanderNextX;
					_wanderY = _wanderNextY;

					double r = _rnd.NextDouble() * _wanderMagnitude;
					double a = _rnd.NextDouble() * 2 * Math.PI;

					_wanderNextX = Math.Cos(a) * r;
					_wanderNextY = Math.Sin(a) * r;
				}
				else
				{
					_wanderX += dx * step;
					_wanderY += dy * step;
				}
			}

			if (_scratchLines != null)
			{
				_scratchLineIntensity = _scratchLineIntensity + _rnd.NextDouble() * 0.1 * _scratchLineDelta;

				double thisFrameIntensity = _scratchLineIntensity * (_rnd.NextDouble() * 0.2 + 0.9);

				effects.Children.Add(RenderScratchLines(thisFrameIntensity, _scratchLines));

				if (_scratchLineIntensity > 1.0)
				{
					if ((_scratchLineDelta == 0.0) && (_rnd.NextDouble() < 0.1))
						_scratchLineDelta = -1.0;
					else
						_scratchLineDelta = 0.0;
				}
				else if (_scratchLineIntensity < 0.0)
				{
					_scratchLines = null;
					_scratchLineCooldown = 50;
				}

				if (_scratchLines != null)
					UpdateScratchLines(_scratchLines);
			}
			else if (_enableScratches)
			{
				if (_scratchLineCooldown > 0)
					_scratchLineCooldown--;
				else
				{
					if (_rnd.NextDouble() < 0.01)
					{
						_scratchLineIntensity = _rnd.NextDouble() * 0.15;
						_scratchLineDelta = 1.0;
						_scratchLines = Enumerable.Range(0, _rnd.Next(1, 6)).Select((x) =>
							new ScratchLine()
							{
								X = _rnd.Next(-10, 1930),
								DX = (_rnd.NextDouble() - 0.5) * 0.75,
								Intensity = _rnd.NextDouble() * 0.5 + 0.5,
							}).ToArray();
					}
				}
			}

			if (_enableDust)
				effects.Children.Add(CreateDustParticles());

			if (_enableOpacityMask)
				effects.OpacityMask = _opacityMask;

			var background = new Grid();

			background.Children.Add(effects);

			return background;
		}

		private UIElement RenderScratchLines(double intensity, ScratchLine[] scratchLines)
		{
			var output = new Path();

			var geometry = new PathGeometry();

			for (int i = 0; i < scratchLines.Length; i++)
				geometry.Figures.Add(RenderScratchLine(intensity, scratchLines[i]));

			output.Data = geometry;
			output.Fill = Brushes.Ivory;

			return output;
		}

		private PathFigure RenderScratchLine(double intensity, ScratchLine scratchLine)
		{
			double width = intensity * 1.6 + 0.25;

			var figure = new PathFigure();

			figure.StartPoint = new Point(scratchLine.X, 0);

			for (int y = 1; y < 1080; y += _rnd.Next(1, 5))
				figure.Segments.Add(new LineSegment() { Point = new Point(scratchLine.X - _rnd.NextDouble() * width * 0.25, y) });

			figure.Segments.Add(new LineSegment() { Point = new Point(scratchLine.X, 1080) });
			figure.Segments.Add(new LineSegment() { Point = new Point(scratchLine.X - width, 1080) });

			for (int y = 1080 - _rnd.Next(1, 5); y > 0; y -= _rnd.Next(1, 5))
				figure.Segments.Add(new LineSegment() { Point = new Point(scratchLine.X - width + _rnd.NextDouble() * width * 0.25, y) });

			figure.IsClosed = true;

			return figure;
		}

		private void UpdateScratchLines(ScratchLine[] _scratchLines)
		{
			for (int i = 0; i < _scratchLines.Length; i++)
			{
				_scratchLines[i].X = _scratchLines[i].X + _scratchLines[i].DX * (_rnd.NextDouble() * 0.25 + 0.25);
				_scratchLines[i].Intensity = _scratchLines[i].Intensity + (_rnd.NextDouble() * 0.1 - 0.05);
			}
		}

		private UIElement CreateDustParticles()
		{
			var output = new Path();

			var geometry = new PathGeometry();

			while (_rnd.NextDouble() < _dustIntensity)
				geometry.Figures.Add(CreateDustParticle());

			_dustIntensity = _dustIntensity * _rnd.NextDouble() + _rnd.NextDouble() * _dustLevel;

			output.Data = geometry;
			output.Fill = new SolidColorBrush(Color.FromArgb((byte)_rnd.Next(192, 256), 15, 10, 0));

			return output;
		}

		private PathFigure CreateDustParticle()
		{
			var curve = new PolyBezierSegment();

			double centreX = _rnd.NextDouble() * 1920.0;
			double centreY = _rnd.NextDouble() * 1080.0;

			double baseRadius = _rnd.NextDouble() * 7.5 + 2.25;

			for (double angle = _rnd.NextDouble(); angle < 6.283185; angle += _rnd.NextDouble())
			{
				double radius = baseRadius * (_rnd.NextDouble() * 0.4 + 0.8);

				double leadX = centreX + radius * Math.Cos(angle - 0.1);
				double leadY = centreY + radius * Math.Sin(angle - 0.1);

				double x = centreX + radius * Math.Cos(angle);
				double y = centreY + radius * Math.Sin(angle);

				double trailX = centreX + radius * Math.Cos(angle + 0.1);
				double trailY = centreY + radius * Math.Sin(angle + 0.1);

				curve.Points.Add(new Point(leadX, leadY));
				curve.Points.Add(new Point(x, y));
				curve.Points.Add(new Point(trailX, trailY));
			}

			var figure = new PathFigure();

			figure.StartPoint = curve.Points[1];

			curve.Points.Add(curve.Points[0]);
			curve.Points.Add(curve.Points[1]);

			curve.Points.RemoveAt(0);
			curve.Points.RemoveAt(0);

			figure.Segments.Add(curve);

			return figure;
		}
	}
}
