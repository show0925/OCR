// AForge Neural Net Library
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

namespace AForge.NeuralNet.Learning
{
	using System;

	/// <summary>
	/// SupervisedLearning interface
	/// </summary>
	public interface ISupervisedLearning
	{
		/// <summary>
		/// Is converged property
		/// </summary>
		bool IsConverged{get;}

		/// <summary>
		/// Perform one learning iteration and return network error
		/// </summary>
		float Learn(float[] input, float[] output);

		/// <summary>
		/// Perform learning epoch and return errors sum
		/// </summary>
		float LearnEpoch(float[][] input, float[][] output);
	}
}
