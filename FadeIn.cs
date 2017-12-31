using System;

namespace Intermission
{
	public class FadeIn : Fade
	{
		protected override double Opacity(int frameNumber)
		{
			return T(frameNumber);
		}
	}
}
