// AForge Math Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.Math
{
	using System;

	/// <summary>
	/// HistogramD
	/// </summary>
	public class HistogramD
	{
		private int[]	values;
		private RangeD	range;

		private double	mean = 0;
		private double	stdDev = 0;
		private double	median = 0;
		private double	min;
		private double	max;

		private int		total = 0;

		// Get values
		public int[] Values
		{
			get { return values; }
		}
		// Get range
		public RangeD Range
		{
			get { return range; }
		}
		// Get mean
		public double Mean
		{
			get { return mean; }
		}
		// Get standard deviation
		public double StdDev
		{
			get { return stdDev; }
		}
		// Get median
		public double Median
		{
			get { return median; }
		}
		// Get min value
		public double Min
		{
			get { return min; }
		}
		// Get max value
		public double Max
		{
			get { return max; }
		}

		// Constructor
		public HistogramD(int[] values, RangeD range)
		{
			this.values	= values;
			this.range	= range;

			int v, i, l = values.Length;
			int lM1 = l - 1;
			double d = range.Max - range.Min;

			max = 0;
			min = l;

			// calculate mean, min, max
			for (i = 0; i < l; i++)
			{
				v = values[i];

				if (v != 0)
				{
					// max
					if (i > max)
						max = i;
					// min
					if (i < min)
						min = i;
				}

				// accumulate total value
				total += v;
				// accumulate mean value
				mean += (((double) i / lM1) * d + range.Min) * v;
			}
			mean /= total;

			min = (min / lM1) * d + range.Min;
			max = (max / lM1) * d + range.Min;

			// calculate stadard deviation
			for (i = 0; i < l; i++)
			{
				v = values[i];
				stdDev += Math.Pow((((double) i / lM1) * d + range.Min) - mean, 2) * v;
			}
			stdDev = Math.Sqrt(stdDev / total);
			
			// calculate median
			int m, h = total / 2;

			for (m = 0, v = 0; median < l; m++)
			{
				v += values[m];
				if (v >= h)
					break;
			}
			median = ((double) m / lM1) * d + range.Min;
		}

		// Get range around median containing specified
		// percentile of values
		public RangeD GetRange(double percent)
		{
			int min, max, v;
			int h = (int)(total * (percent + (1 - percent) / 2));
			int l = values.Length;
			int lM1 = l - 1;
			double d = range.Max - range.Min;

			for (min = 0, v = total; min < l; min++)
			{
				v -= values[min];
				if (v < h)
					break;
			}
			for (max = l - 1, v = total;  max >= 0; max--)
			{
				v -= values[max];
				if (v < h)
					break;
			}
			return new RangeD(((double) min / lM1) * d + range.Min, ((double) max / lM1) * d + range.Min);
		}
	}
}
