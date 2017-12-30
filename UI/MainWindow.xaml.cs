using System;
using System.Threading;
using System.Windows;

using Intermission;

namespace UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void cmdBoom_Click(object sender, RoutedEventArgs e)
		{
			((dynamic)cmdBoom.Parent).Children.Remove(cmdBoom);

			var renderThread = new Thread(RenderThread);

			renderThread.SetApartmentState(ApartmentState.STA);
			renderThread.Start();
		}

		void RenderThread()
		{
			var renderer = new Renderer();

			foreach (var frame in renderer.Render())
			{
				Dispatcher.Invoke((Action)(
					() =>
					{
						imgDisplay.Source = frame;
					}));
			}
		}
	}
}
