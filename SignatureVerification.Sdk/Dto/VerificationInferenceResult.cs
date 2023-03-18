using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace SignatureVerificationSdk.Dto
{
    internal class VerificationInferenceResult
    {
        internal IEnumerable<float> Output0 { get; set; }
        internal IEnumerable<float> Output1 { get; set; }
        internal Tensor<float> Output2 { get; set; }
    }
}
