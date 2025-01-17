using System;
using System.Net;
using System.Net.Sockets;
using ModbusServer;

namespace FlowMeter
{
    public static class ModbusFunctions
    {
        public static void HandleError(ModbusPacket packet, NetworkStream stream, byte exceptionCode)
        {
            Console.WriteLine("Unknown function code.");

            // Generate error response packet
            ModbusPacket errorResponse = new ModbusPacket(
                packet.TransactionIdentifier,  // Same Transaction ID
                packet.ProtocolIdentifier,     // Same Protocol ID
                packet.UnitIdentifier,         // Same Unit ID
                (byte)(packet.FunctionCode + 0x80), // Error type (original function code + 0x80)
                new byte[] { exceptionCode }   // Error code)
            );

            // Generate byte array
            byte[] responseBytes = errorResponse.ToByteArray();
            // And send it
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        public static void HandleF4(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerCount) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            // Lue rekisterit pumpustatuksesta
            var returnData = Program.PumpStatus.ReadInputRegisters(registerIndex, registerCount);

            // Varaus vastauksen datalle, lis‰‰m‰ll‰ rekisterin m‰‰r‰ alkuun
            byte[] returnByteData = new byte[returnData.Length * 2 + 1];

            // Muodostetaan rekisterin m‰‰r‰ big-endian -muodossa (koko = rekisterin m‰‰r‰ * 2)
            var registerCountBytes = BitConverter.GetBytes(registerCount * 2);
            returnByteData[0] = registerCountBytes[0];
            //returnByteData[1] = registerCountBytes[1];

            // Lis‰t‰‰n rekisterit ja varmistetaan big-endian -j‰rjestys
            for (int i = 0; i < returnData.Length; i++)
            {
                var b = BitConverter.GetBytes(returnData[i]);
                byte[] bytes = { b[1], b[0] }; // Big-endian
                bytes.CopyTo(returnByteData, 1 + (i * 2)); // Kopioidaan vastaava data vastaukseen

                //Console.WriteLine($"Data {i}: {returnData[i]} - {Convert.ToHexString(new byte[] { returnByteData[1 + i], returnByteData[1 + i+1] })}");
            }

            // Luodaan vastauspaketti Modbusin s‰‰ntˆjen mukaisesti
            ModbusPacket response = new ModbusPacket(
                packet.TransactionIdentifier,
                packet.ProtocolIdentifier,
                packet.UnitIdentifier,
                packet.FunctionCode,
                returnByteData
            );

            // L‰hetet‰‰n vastaus asiakasohjelmalle
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        public static void HandleF1(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerCount) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            var returnData = Program.PumpStatus.ReadCoils(registerIndex, registerCount);

            ModbusPacket response = new ModbusPacket(
            packet.TransactionIdentifier,
            packet.ProtocolIdentifier,
            packet.UnitIdentifier,
            packet.FunctionCode,
            ModbusServer.Utils.ToBigendian.From(returnData)
            );
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
        public static void HandleF2(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerCount) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            var returnData = Program.PumpStatus.ReadDiscreteInputs(registerIndex, registerCount);

            ModbusPacket response = new ModbusPacket(
            packet.TransactionIdentifier,
            packet.ProtocolIdentifier,
            packet.UnitIdentifier,
            packet.FunctionCode,
            ModbusServer.Utils.ToBigendian.From(returnData)
            );
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }


        /// <summary>
        /// F5 function in modbus set's one coil value. Data structure is 
        /// Bigendian encoded ushort coil address
        /// Bigendian encoded ushort / short value. FF00 = On, 0000 = Off. Other values are invalid and are ignored.
        /// 
        /// Response packet returns data as 
        /// Bigendian encoded ushort coil address
        /// Bigendian encoded ushort / short value (FF00 or 0000).
        /// OR
        /// Error code packet
        /// </summary>
        /// <param name="packet">Received modbus packet.</param>
        /// <param name="stream">Stream where to send response packet.</param>
        public static void HandleF5(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerValue) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            bool bit;
            if (registerValue == 0)
            {
                bit = false;
            }
            else
                bit = true;
            Program.PumpStatus.SetCoil(registerIndex, bit);
            //Program._pumpStatus.SetInputRegister(registerIndex, (short)registerValue);
            var returnBit = Program.PumpStatus.GetCoil(registerIndex);
            byte[] returnData = new byte[2];
            if (returnBit)
                returnData = new byte[] { 0xFF, 0x00 };
            else
                returnData = new byte[] { 0x00, 0x00 };
            ModbusPacket response = new ModbusPacket(
            packet.TransactionIdentifier,
            packet.ProtocolIdentifier,
            packet.UnitIdentifier,
            packet.FunctionCode,
            returnData
            );
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
