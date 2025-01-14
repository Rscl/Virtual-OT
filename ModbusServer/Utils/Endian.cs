using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Utils
{
    public static class Endian
    {
        public static byte[] ConvertToBigEndian(byte[] data)
        {
            if (data.Length % 2 != 0)
            {
                throw new ArgumentException("Data length must be even to convert to big-endian.");
            }

            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i += 2)
            {
                result[i] = data[i + 1];
                result[i + 1] = data[i];
            }

            return result;
        }
    }
}
