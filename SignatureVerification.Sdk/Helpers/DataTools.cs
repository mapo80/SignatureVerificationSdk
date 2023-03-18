//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SignatureVerificationSdk.Utility
//{
//    public class DataTools
//    {

//        /// <summary>
//        /// returns the min and max values in a matrix of doubles.
//        /// </summary>
//        public static void MinMax(double[,] data, out double min, out double max)
//        {
//            int rows = data.GetLength(0);
//            int cols = data.GetLength(1);
//            min = data[0, 0];
//            max = data[0, 0];
//            for (int i = 1; i < rows; i++)
//            {
//                for (int j = 1; j < cols; j++)
//                {
//                    if (data[i, j] < min)
//                    {
//                        min = data[i, j];
//                    }
//                    else if (data[i, j] > max)
//                    {
//                        max = data[i, j];
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Normalises a matrix so that all values lie between 0 and 1.
//        /// Min value in matrix set to 0.0.
//        /// Max value in matrix is set to 1.0.
//        /// Rerturns the min and the max.
//        /// </summary>
//        public static double[,] NormaliseInZeroOne(double[,] m, out double min, out double max)
//        {
//            int rows = m.GetLength(0);
//            int cols = m.GetLength(1);
//            MinMax(m, out min, out max);
//            double range = max - min;
//            double[,] m2Return = new double[rows, cols];
//            for (int r = 0; r < rows; r++)
//            {
//                for (int c = 0; c < cols; c++)
//                {
//                    m2Return[r, c] = (m[r, c] - min) / range;
//                    if (m2Return[r, c] > 1.0)
//                    {
//                        m2Return[r, c] = 1.0;
//                    }
//                    else if (m2Return[r, c] < 0.0)
//                    {
//                        m2Return[r, c] = 0.0;
//                    }
//                }
//            }

//            return m2Return;
//        }

//        /// <summary>
//        /// This method assumes that the passed matrix of double already takes values between 0.0 and 1.0.
//        /// </summary>
//        public static byte[,] ConvertMatrixOfDouble2Byte(double[,] matrix)
//        {
//            int rows = matrix.GetLength(0);
//            int cols = matrix.GetLength(1);
//            var outM = new byte[rows, cols];
//            var maxValue = byte.MaxValue;

//            for (int r = 0; r < rows; r++)
//            {
//                for (int c = 0; c < cols; c++)
//                {
//                    outM[r, c] = (byte)(matrix[r, c] * maxValue);
//                }
//            }

//            return outM;
//        }

//        public static double[,] ConvertMatrixOfByte2Double(byte[,] matrix)
//        {
//            int rows = matrix.GetLength(0);
//            int cols = matrix.GetLength(1);
//            var outM = new double[rows, cols];

//            for (int r = 0; r < rows; r++)
//            {
//                for (int c = 0; c < cols; c++)
//                {
//                    outM[r, c] = matrix[r, c];
//                }
//            }

//            return outM;
//        }
//        /// <summary>
//        /// Converts a matrix to a vector by concatenating columns.
//        /// </summary>
//        public static byte[] Matrix2Array(byte[,] m)
//        {
//            int ht = m.GetLength(0);
//            int width = m.GetLength(1);
//            byte[] v = new byte[ht * width];

//            int id = 0;
//            for (int col = 0; col < width; col++)
//            {
//                for (int row = 0; row < ht; row++)
//                {
//                    v[id++] = m[row, col];
//                }
//            }

//            return v;
//        }

//        /*
//         * converts a matrix to a vector by concatenating columns.
//         */
//        public static byte[,] Array2Matrix(byte[] array, int width, int height)
//        {
//            byte[,] M = new byte[height, width];

//            int id = 0;
//            for (int col = 0; col < width; col++)
//            {
//                for (int row = 0; row < height; row++)
//                {
//                    M[row, col] = array[id++];
//                }
//            }

//            return M;
//        }
//    }
//}
