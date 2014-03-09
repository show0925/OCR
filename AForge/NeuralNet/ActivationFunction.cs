// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet
{
	using System;

	/// <summary>
	/// ActivationFunction interface
	/// </summary>
	public interface IActivationFunction
	{
		// Calculate function value
		float Output(float input);

		// Calculate differential of the function value
		float OutputPrime(float input);

		// Calculate differential of the function value
		// using function value as input
		float OutputPrime2(float input);
	}

	// Sigmoid activation function
	//
	//                1
	// f(x) = ------------------
	//        1 + exp(-alfa * x)
	//
	// Outpur range: [0, 1]
	//
	public class SigmoidFunction : IActivationFunction
	{
		private float alfa = 2;

		// Alfa property
		public float Alfa
		{
			get { return alfa; }
			set { alfa = value; }
		}

		// Constructors
		public SigmoidFunction()
		{ }
		public SigmoidFunction(float alfa)
		{
			this.alfa = alfa;
		}

		
		// Calculate function value
		public float Output(float x)
		{
			return (float) (1 / (1 + Math.Exp(-alfa * x)));
		}

		// Calculate differential of the function value
		public float OutputPrime(float x)
		{
			float y = Output(x);

			return (float) (alfa * y * (1 - y));
		}

		// Calculate differential of the function value
		// using function value as input
		public float OutputPrime2(float y)
		{
			return (float) (alfa * y * (1 - y));
		}
	}


	// Bipolar Sigmoid activation function
	//
	//                1
	// f(x) = ------------------ - 0.5
	//        1 + exp(-alfa * x)
	//
	// Outpur range: [-0.5, 0.5]
	//
	public class BipolarSigmoidFunction : IActivationFunction
	{
		private float alfa = 2;

		// Alfa property
		public float Alfa
		{
			get { return alfa; }
			set { alfa = value; }
		}

		// Constructors
		public BipolarSigmoidFunction()
		{ }
		public BipolarSigmoidFunction(float alfa)
		{
			this.alfa = alfa;
		}

		
		// Calculate function value
		public float Output(float x)
		{
			return (float) ((1 / (1 + Math.Exp(-alfa * x))) - 0.5);
		}

		// Calculate differential of the function value
		public float OutputPrime(float x)
		{
			float y = Output(x);

			return (float) (alfa * (0.25 - y * y));
		}

		// Calculate differential of the function value
		// using function value as input
		public float OutputPrime2(float y)
		{
			return (float) (alfa * (0.25 - y * y));
		}
	}


	// Hyperbolic Tangens activation function
	//
	//                         exp(alfa * x) - exp(-alfa * x)
	// f(x) = tanh(alfa * x) = ------------------------------
	//                         exp(alfa * x) + exp(-alfa * x)
	//
	// Outpur range: [-1, 1]
	//
	public class HyperbolicTangensFunction : IActivationFunction
	{
		private float alfa = 1;

		// Alfa property
		public float Alfa
		{
			get { return alfa; }
			set { alfa = value; }
		}

		// Constructors
		public HyperbolicTangensFunction()
		{ }
		public HyperbolicTangensFunction(float alfa)
		{
			// dividing alfa by two gives us the same function
			// as sigmoid function
			this.alfa = alfa;
		}

		// Calculate function value
		public float Output(float x)
		{
			return (float) (Math.Tanh(alfa * x));
		}

		// Calculate differential of the function value
		public float OutputPrime(float x)
		{
			float y = Output(x);

			return (float) (alfa * (1 - y * y));
		}

		// Calculate differential of the function value
		// using function value as input
		public float OutputPrime2(float y)
		{
			return (float) (alfa * (1 - y * y));
		}
	}
}
