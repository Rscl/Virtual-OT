using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ModbusServer.Utils
{
    public static class ModbusFunctions
    {
        public static (ushort, ushort) ReadInputData(byte[] data)
        {
            if (data == null || data.Length != 4)
            {
                throw new ArgumentException("Data field must be 4 bytes for function code 1.");
            }
            ushort startRegister = BitConverter.ToUInt16(new byte[] { data[1], data[0] }, 0);
            ushort numberOfRegisters = BitConverter.ToUInt16(new byte[] { data[3], data[2] }, 0);
            return (startRegister, numberOfRegisters);
        }

        public static (object registerIndex, object registerCount) F4_ReadInputRegisters(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
