﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignatureVerification.Console.Dto
{
    internal class SignatureVerificationOutputDto
    {
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public bool IsForged { get; set; }
        public bool CalculatedResult { get; set; }
        public bool IsSameResult { get; set; }
        public double Confidence { get; set; }
        public double Similarity { get; set; }

    }
}
