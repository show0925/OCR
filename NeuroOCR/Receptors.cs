// Neural Network OCR
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

using System;
using System.Drawing;
using System.Collections;

namespace NeuroOCR
{
	/// <summary>
	/// Receptors collection
	/// </summary>
	public class Receptors : CollectionBase
	{
		private Random	rand = new Random();
		private Size	size = new Size(100, 100);

		// Area size
		public Size AreaSize
		{
			get { return size; }
			set { size = value; }
		}
		// Get receptor
		public Receptor this[int index]
		{
			get { return ((Receptor) InnerList[index]); }
		}

		// Constructor
		public Receptors()
		{
		}

		// Add new receptor to the sequence
		public void Add(Receptor receptor)
		{
			InnerList.Add(receptor);
		}

		// Generate new receptors
		public void Generate(int count)
		{
			int	maxX = size.Width;
			int maxY = size.Height;
			int i = 0;

			while (i < count)
			{
				int x1 = rand.Next(maxX);
				int y1 = rand.Next(maxY);
				int x2 = rand.Next(maxX);
				int y2 = rand.Next(maxY);

				int	dx = x1 - x2;
				int dy = y1 - y2;
				int length = (int) Math.Sqrt(dx * dx + dy * dy);

				// avoid too short and too long receptors
				if ((length < 10) || (length > 50))
					continue;

				InnerList.Add(new Receptor(x1, y1, x2, y2));
				i++;
			}
		}

		// Get receptors state
		public int[] GetReceptorsState(Bitmap image)
		{
			int		width = image.Width;
			int		height = image.Height;
			int		n = InnerList.Count;
			int[]	state = new int[n];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// check for black pixel
					if (image.GetPixel(x, y).R == 0)
					{
						for (int i = 0; i < n; i++)
						{
							// skip already activated receptors
							if (state[i] == 1)
								continue;

							if (((Receptor) InnerList[i]).GetReceptorState(x, y))
								state[i] = 1;
						}
					}
				}
			}

			return state;
		}
	}
}
