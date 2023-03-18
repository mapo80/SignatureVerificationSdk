using System;

namespace SignatureVerificationSdk.Utility
{
    internal class Sigmoid
    {
        internal static double CalculateSigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
    }
}
