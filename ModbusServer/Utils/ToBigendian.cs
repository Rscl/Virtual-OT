using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Utils
{
    public static class ToBigendian
    {
        public static byte[] From(short value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return ConvertToBigEndian(data);
        }

        public static byte[] From(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return ConvertToBigEndian(data);
        }

        public static byte[] From(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return ConvertToBigEndian(data);
        }
        public static byte[] From(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return ConvertToBigEndian(data);
        }
        public static byte[] ConvertToBigEndian(byte[] data)
        {
            if (data.Length % 2 != 0)
            {
                throw new ArgumentException("Data length must be even to convert to big-endian.");
            }
            byte[] result = data.Reverse().ToArray();
            return result;
        }
    }
}
