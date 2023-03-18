using System;

namespace SignatureVerificationSdk.Helpers
{
    /// <summary>
    /// Cosine similarity metric. 
    /// </summary>
    /// 
    /// <remarks><para>This class represents the 
    /// <a href="http://en.wikipedia.org/wiki/Cosine_similarity">Cosine Similarity metric</a>.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // instantiate new similarity class
    /// CosineSimilarity sim = new CosineSimilarity( ); 
    /// // create two vectors for inputs
    /// double[] p = new double[] { 2.5, 3.5, 3.0, 3.5, 2.5, 3.0 };
    /// double[] q = new double[] { 3.0, 3.5, 1.5, 5.0, 3.5, 3.0 };
    /// // get similarity between the two vectors
    /// double similarityScore = sim.GetSimilarityScore( p, q );
    /// </code>    
    /// </remarks>
    /// 
    internal sealed class CosineSimilarity
    {
        /// <summary>
        /// Returns similarity score for two N-dimensional double vectors. 
        /// </summary>
        /// 
        /// <param name="p">1st point vector.</param>
        /// <param name="q">2nd point vector.</param>
        /// 
        /// <returns>Returns Cosine similarity between two supplied vectors.</returns>
        /// 
        /// <exception cref="ArgumentException">Thrown if the two vectors are of different dimensions (if specified
        /// array have different length).</exception>
        /// 
        internal static double GetSimilarityScore(double[] p, double[] q)
        {
            double numerator = 0, pSumSq = 0, qSumSq = 0;
            double pValue, qValue;

            if (p.Length != q.Length)
                throw new ArgumentException("Input vectors must be of the same dimension.");

            for (int x = 0, len = p.Length; x < len; x++)
            {
                pValue = p[x];
                qValue = q[x];

                numerator += pValue * qValue;
                pSumSq += pValue * pValue;
                qSumSq += qValue * qValue;
            }

            var denominator = Math.Sqrt(pSumSq) * Math.Sqrt(qSumSq);

            return (denominator == 0) ? 0 : numerator / denominator;
        }
    }
}