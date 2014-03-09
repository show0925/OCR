// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet
{
	using System;

	/// <summary>
	/// Neuron
	/// </summary>
	public class Neuron
	{
		protected int		inputsCount = 1;
		protected float[]	weights = new float[1];	// synapses weights
		protected float		threshold = 0.0f;
		protected IActivationFunction	function = new SigmoidFunction();

		protected float		sum;		// weighted input's sum
		protected float		output;		// neuron's output value

		protected static Random	rand = new Random();

		// Inputs count property
		public int InputsCount
		{
			get { return inputsCount; }
			set
			{
				inputsCount = Math.Max(1, value);
				weights = new float[inputsCount];
			}
		}
		// Threshold property
		public float Threshold
		{
			get { return threshold; }
			set { threshold = value; }
		}
		// Activation function property
		public IActivationFunction ActivationFunction
		{
			get { return function; }
			set { function = value; }
		}
		// Output value
		public float Output
		{
			get { return output; }
		}
		// Get/Set weight value
		public float this[int index]
		{
			get { return weights[index]; }
			set { weights[index] = value; }
		}


		// Constructors
		public Neuron()
		{ }
		public Neuron(int inputs) : this(inputs, new SigmoidFunction())
		{ }
		public Neuron(int inputs, IActivationFunction function)
		{
			this.function = function;
			InputsCount =  inputs;
		}


		// Compute the output value of the neuron
		public float Compute(float[] input)
		{
			if (input.Length != inputsCount)
				throw new ArgumentException();

			sum = 0.0f;

			// compute weighted sum of input
			for (int i = 0; i < inputsCount; i++)
			{
				sum += weights[i] * input[i];
			}
			sum -= threshold;

			return (output = function.Output(sum));
		}

		// Randomize weights
		public void Randomize()
		{
			for (int i = 0; i < inputsCount; i++)
				weights[i] = (float)(rand.NextDouble());

			threshold = (float)(rand.NextDouble());
		}
	}
}
