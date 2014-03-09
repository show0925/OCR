// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet
{
	using System;

	/// <summary>
	/// Layer - represent a collection of neurons
	/// </summary>
	public class Layer
	{
		protected int	inputsCount;	// inputs count of the layer
		protected int	neuronsCount;	// neurons count in the layer
		protected IActivationFunction	function;	// activation function of the layer
		protected Neuron[]	neurons;
		protected float[]	output;

		// Inputs count property
		public int InputsCount
		{
			get { return inputsCount; }
			set
			{
				inputsCount = Math.Max(1, value);
				InitLayer();
			}
		}
		// Neurons count property
		public int NeuronsCount
		{
			get { return neuronsCount; }
			set
			{
				neuronsCount = Math.Max(1, value);
				InitLayer();
			}
		}
		// Activation function property
		public IActivationFunction ActivationFunction
		{
			get { return function; }
			set
			{
				function = value;

				for (int i = 0; i < neuronsCount; i++)
					neurons[i].ActivationFunction = value;
			}
		}
		// Get neuron at the specified index
		public Neuron this[int index]
		{
			get { return (neurons[index]); }
		}
		// Get layer output
		public float[] Output
		{
			get { return output; }
		}

		
		// Constructors
		public Layer()
			: this (1, 1, new SigmoidFunction())
		{ }
		public Layer(int neuronsCount)
			: this (neuronsCount, 1, new SigmoidFunction())
		{ }
		public Layer(int neuronsCount, int inputsCount)
			: this (neuronsCount, inputsCount, new SigmoidFunction())
		{ }
		public Layer(int neuronsCount, int inputsCount, IActivationFunction function)
		{
			this.inputsCount = Math.Max(1, inputsCount);
			this.neuronsCount = Math.Max(1, neuronsCount);
			this.function = function;

			InitLayer();
		}

		// Compute the output value of the layer
		public float[] Compute(float[] input)
		{
			// compute each neuron
			for (int i = 0; i < neuronsCount; i++)
				output[i] = neurons[i].Compute(input);

			return output;
		}

		// Randomize the layer
		public void Randomize()
		{
			foreach (Neuron neuron in neurons)
				neuron.Randomize();
		}


		#region Private Members

		// Initialize layer
		private void InitLayer()
		{
			// create collection of neurons
			neurons = new Neuron[neuronsCount];
			// create each neuron
			for (int i = 0; i < neuronsCount; i++)
				neurons[i] = new Neuron(inputsCount, function);
			// allocate output array
			output = new float[neuronsCount];
		}

		#endregion
	}
}
