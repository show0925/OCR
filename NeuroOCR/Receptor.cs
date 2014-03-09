// Neural Network OCR
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

using System;

namespace NeuroOCR
{
	/// <summary>
	/// Summary description for Receptor.
	/// </summary>
	public class Receptor
	{
		private int		x1, y1, x2, y2;
		private int		left, top, right, bottom;
		private float	k, z;
		private float	a, b, c, d;

		// X1 property
		public int X1
		{
			get { return x1; }
		}
		// Y1 property
		public int Y1
		{
			get { return y1; }
		}
		// X2 property
		public int X2
		{
			get { return x2; }
		}
		// Y2 property
		public int Y2
		{
			get { return y2; }
		}

		// Constructor
		public Receptor(int x1, int y1, int x2, int y2)
		{			
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;

			left	= Math.Min(x1, x2);
			right	= Math.Max(x1, x2);
			top		= Math.Min(y1, y2);
			bottom	= Math.Max(y1, y2);

			if (x1 != x2)
			{
				k = (float) (y2 - y1) / (float) (x2 - x1);
				z = (float) y1 - k * x1;

				a = y1 - y2;
				b = x2 - x1;
				c = y1 * (x1 - x2) + x1 * (y2 - y1);
				d = (float) Math.Sqrt(a * a + b * b);
			}
		}

		// Check receptor state
		public bool GetReceptorState(int x, int y)
		{
			// check, if the point is in receptors bounds
			if ((x < left) || (y < top) || (x > right) || (y > bottom))
				return false;

			// check for horizontal and vertical receptors
			if ((x1 == x2) || (y1 == y2))
				return true;

			// check if the point is on the receptors line

			// more fast, but not very accurate
//			if ((int)(k * x + z - y) == 0)
//				return true;

			// more accurate version
			if (Math.Abs(a * x + b * y + c) / d < 1)
				return true;

			return false;
		}
	}
}
