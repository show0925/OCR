// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet.Learning
{
	using System;

	/// <summary>
	/// Back Propagation learning
	/// </summary>
	public class BackPropagationLearning : ISupervisedLearning
	{
		private Network		net;	// network to learn
		private float		learningRate = 0.1f;
		private float		momentum = 0.0f;
		private float		learningLimit = 0.1f;

		private bool		converged = false;

		private float[][]	errors;
		private float[][][]	deltas;
		private float[][]	thresholdDeltas;

		// Learning Rate property
		public float LearningRate
		{
			get { return learningRate; }
			set { learningRate = value; }
		}
		// Momentum property
		public float Momentum
		{
			get { return momentum; }
			set { momentum = value; }
		}
		// Learning Limit property
		public float LearningLimit
		{
			get { return learningLimit; }
			set { learningLimit = value; }
		}
		// Is converged property
		public bool IsConverged
		{
			get { return converged; }
		}

		// Constructor
		public BackPropagationLearning(Network net)
		{
			this.net = net;

			// create error and deltas arrays
			errors = new float[net.LayersCount][];
			deltas = new float[net.LayersCount][][];
			thresholdDeltas = new float[net.LayersCount][];
			
			for (int i = 0; i < net.LayersCount; i++)
			{
				Layer layer = net[i];

				errors[i] = new float[layer.NeuronsCount];
				deltas[i] = new float[layer.NeuronsCount][];
				thresholdDeltas[i] = new float[layer.NeuronsCount];

				for (int j = 0; j < layer.NeuronsCount; j++)
				{
					deltas[i][j] = new float[layer.InputsCount];
				}
			}
		}

		// Perform learning epoch and return errors sum
		public float LearnEpoch(float[][] input, float[][] output)
		{
			int		i, n = input.Length;
			float	error = 0.0f;

			// for all training patterns
			for (i = 0; i < n; i++)
			{
				error += Learn(input[i], output[i]);
			}
			// determine if we converged
			converged = (error < learningLimit);

			// return error
			return error;
		}


		// Perform one learning iteration and return network error
		public float Learn(float[] input, float[] output)
		{
			// compute the network
			float[] nout = net.Compute(input);

			// calculate network error
			float error = CalculateError(output);

			// calculate weights updates
			CalculateUpdates(input);

			// update the network
			UpdateNetwork();

			// return error level
			return error;
		}

		// Calculate network errors
		private float CalculateError(float[] desiredOutput)
		{
			Layer		layer, layerNext;
			float[]		err, errNext;
			float		error = 0, e;
			float		output, sum;
			int			i, j, k, n, m, layersCount = net.LayersCount;

			// assume, that all neurons of the network have
			// the same activation function
			IActivationFunction	function = net[0][0].ActivationFunction;

			// calculate error for the last layer
			layer = net[layersCount - 1];
			err = errors[layersCount - 1];

			for (i = 0, n = layer.NeuronsCount; i < n; i++)
			{
				output = layer[i].Output;
				// error of the neuron
				e = desiredOutput[i] - output;
				// error multiplied with first derivative
				err[i] = e * function.OutputPrime2(output);
				// squre the error and sum it
				error += (e * e);
			}

			// calculate error for other layers
			for (j = layersCount - 2; j >= 0; j--)
			{
				layer		= net[j];
				layerNext	= net[j + 1];
				err			= errors[j];
				errNext		= errors[j + 1];

				// for all neurons of the layer
				for (i = 0, n = layer.NeuronsCount; i < n; i++)
				{
					sum = 0.0f;
					// for all neurons of the next layer
					for (k = 0, m = layerNext.NeuronsCount; k < m; k++)
					{
						sum += errNext[k] * layerNext[k][i];
					}
					err[i] = sum * function.OutputPrime2(layer[i].Output);
				}
			}

			// return squared error of the last layer
			return error;
		}

		// Calculate synapse (neurons weights) updates
		private void CalculateUpdates(float[] input)
		{
			Neuron		neuron;
			Layer		layer, layerPrev;
			float[][]	lDeltas;
			float[]		err, del, tdel;
			float		e;
			int			i, j, k, n, m, l;

			// 1 - for the first layer
			layer	= net[0];
			lDeltas	= deltas[0];
			err		= errors[0];
			tdel	= thresholdDeltas[0];

			// for each neuron of the layer
			for (i = 0, n = layer.NeuronsCount; i < n; i++)
			{
				neuron	= layer[i];
				del		= lDeltas[i];
				e		= err[i];

				// for each synapse of the neuron
				for (j = 0, m = neuron.InputsCount; j < m; j++)
				{
					// calculate weight update
					del[j] = learningRate * (
							momentum * del[j] +
							(1.0f - momentum) * e * input[j]
						);
				}

				// calculate treshold update
				tdel[i] = learningRate * (
					momentum * tdel[i] +
					(1.0f - momentum) * e
					);
			}

			// 2 - for all other layers
			for (k = 1, l = net.LayersCount; k < l; k++)
			{
				layerPrev = net[k - 1];
				layer	= net[k];
				lDeltas	= deltas[k];
				err		= errors[k];
				tdel	= thresholdDeltas[k];

				// for each neuron of the layer
				for (i = 0, n = layer.NeuronsCount; i < n; i++)
				{
					neuron	= layer[i];
					del		= lDeltas[i];
					e		= err[i];

					// for each synapse of the neuron
					for (j = 0, m = neuron.InputsCount; j < m; j++)
					{
						// calculate weight update
						del[j] = learningRate * (
							momentum * del[j] +
							(1.0f - momentum) * e * layerPrev[j].Output
							);
					}

					// calculate treshold update
					tdel[i] = learningRate * (
						momentum * tdel[i] +
						(1.0f - momentum) * e
						);
				}
			}
		}

		// Update weights of network
		private void UpdateNetwork()
		{
			Neuron		neuron;
			Layer		layer;
			float[][]	lDeltas;
			float[]		del, tdel;
			int			i, j, k, n, m, s;

			// for each layer of the network
			for (i = 0, n = net.LayersCount; i < n; i++)
			{
				layer = net[i];
				lDeltas = deltas[i];
				tdel = thresholdDeltas[i];

				// for each neuron of the layer
				for (j = 0, m = layer.NeuronsCount; j < m; j++)
				{
					neuron = layer[j];
					del = lDeltas[j];

					// for each weight of the neuron
					for (k = 0, s = neuron.InputsCount; k < s; k++)
					{
						// update weight
						neuron[k] += del[k];
					}
					// update treshold
					neuron.Threshold -= tdel[j];
				}
			}
		}
	}
}
