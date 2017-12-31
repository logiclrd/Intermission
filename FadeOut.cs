using System;

namespace Intermission
{
	public class FadeOut : Fade
	{
		protected override double Opacity(int frameNumber)
		{
			return 1.0 - T(frameNumber);
		}
	}
}
