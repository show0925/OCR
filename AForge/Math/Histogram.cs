// AForge Math Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.Math
{
	using System;

	/// <summary>
	/// Histogram
	/// </summary>
	public class Histogram
	{
		private int[]	values;
		private double	mean = 0;
		private double	stdDev = 0;
		private int		median = 0;
		private int		min;
		private int		max;

		// Get values
		public int[] Values
		{
			get { return values; }
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
		public int Median
		{
			get { return median; }
		}
		// Get min value
		public int Min
		{
			get { return min; }
		}
		// Get max value
		public int Max
		{
			get { return max; }
		}

		// Constructor
		public Histogram(int[] values)
		{
			this.values = values;

			int i, n = values.Length;

			max = 0;
			min = n;

			// calculate min and max
			for (i = 0; i < n; i++)
			{
				if (values[i] != 0)
				{
					// max
					if (i > max)
						max = i;
					// min
					if (i < min)
						min = i;
				}
			}

			mean	= Statistics.Mean(values);
			stdDev	= Statistics.StdDev(values);
			median	= Statistics.Median(values);
		}

		// Get range around median containing specified
		// percentile of values
		public Range GetRange(double percent)
		{
			return Statistics.GetRange(values, percent);
		}
	}
}
