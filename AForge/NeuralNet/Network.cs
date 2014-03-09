// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet
{
	using System;

	/// <summary>
	/// Network - represent a collection of connected layers
	/// </summary>
	public class Network
	{
		protected int	inputsCount;	// inputs count of the net
		protected int	layersCount;	// layers count in the net
		protected float[]	output;		// network output

		protected Layer[]	layers;

		// Layers count property
		public int LayersCount
		{
			get { return layersCount; }
		}
		// Get layer at the specified index
		public Layer this[int index]
		{
			get { return (layers[index]); }
		}
		// Get network output
		public float[] Output
		{
			get { return output; }
		}

		// Constructors
		public Network(int inputsCount, params int[] neuronsCountPerLayer)
			: this(new SigmoidFunction(), inputsCount, neuronsCountPerLayer)
		{ }
		public Network(IActivationFunction function, int inputsCount, params int[] neuronsCountPerLayer)
		{
			this.inputsCount = Math.Max(1, inputsCount);
			this.layersCount = neuronsCountPerLayer.Length;

			// create collection of layers
			layers = new Layer[layersCount];
			// create each layer
			for (int i = 0; i < layersCount; i++)
			{
				layers[i] = new Layer(
					neuronsCountPerLayer[i],
					(i == 0) ? inputsCount : neuronsCountPerLayer[i - 1],
					function);
			}
		}


		// Compute the output value of the net
		public float[] Compute(float[] input)
		{
			output = input;

			// compute each layer
			for (int i = 0; i < layersCount; i++)
			{
				output = layers[i].Compute(output);
			}

			return output;
		}

		// Randomize the network
		public void Randomize()
		{
			foreach (Layer layer in layers)
				layer.Randomize();
		}
	}
}
