using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Utils
{
    public static class ToLittleEndian
    {
        public static ushort ToUInt16(byte[] data)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("Data field must be 2 bytes for UInt16 conversion.");
            }
            return BitConverter.ToUInt16(new byte[] { data[1], data[0] }, 0);
        }

        public static short ToInt16(byte[] data)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("Data field must be 2 bytes for Int16 conversion.");
            }
            return BitConverter.ToInt16(new byte[] { data[1], data[0] }, 0);
        }

        public static uint ToUInt32(byte[] data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException("Data field must be 4 bytes for UInt32 conversion.");
            }
            return BitConverter.ToUInt32(new byte[] { data[3], data[2], data[1], data[0] }, 0);
        }
        public static int ToInt32(byte[] data)
        {
            if(data.Length != 4)
            {
                throw new ArgumentException("Data field must be 4 bytes for Int32 conversion.");
            }
            return BitConverter.ToInt32(new byte[] { data[3], data[2], data[1], data[0] }, 0);
        }
    }
}