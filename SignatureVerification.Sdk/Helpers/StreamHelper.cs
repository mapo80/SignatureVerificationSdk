using System.IO;

namespace SignatureVerificationSdk.Helpers
{
    internal static class StreamHelper
    {
        internal static byte[] ToByteArray(this Stream inputStream)
        {
            var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            return ms.ToArray();
        }

        internal static Stream ToStream(this byte[] input)
        {
            return new MemoryStream(input);
        }
    }
}
