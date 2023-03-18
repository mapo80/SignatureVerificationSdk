using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignatureVerificationSdk.Dto
{
    public class SignatureVerificationResult
    {
        public double Confidence { get; set; }
        public double Similarity { get; set; }
    }
}
