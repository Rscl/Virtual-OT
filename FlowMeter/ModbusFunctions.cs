using System;
using System.Net;
using System.Net.Sockets;
using ModbusServer;

namespace FlowMeter
{
    public static class ModbusFunctions
    {
        public static void OnPacketReceived(ModbusPacket packet, NetworkStream stream)
        {
            Console.WriteLine($"New packet received with function: {packet.FunctionCode} from {(stream.Socket?.RemoteEndPoint as IPEndPoint)?.Address}");
            switch (packet.FunctionCode)
            {
                case 0x01:
                    HandleUnknown(packet, stream);
                    break;
                case 0x02:
                    HandleF2(packet, stream);
                    break;
                case 0x04:
                    HandleF4(packet, stream);
                    break;
                case 0x05:
                    HandleF5(packet, stream);
                    break;
                default:
                    HandleUnknown(packet, stream);
                    break;
            }
            if (packet.FunctionCode == 0x01)
            {
                HandleF2(packet, stream);
            }
        }

        private static void HandleUnknown(ModbusPacket packet, NetworkStream stream)
        {
            Console.WriteLine("Unknown function code.");

            // Error code "Illegal Function" (0x01)
            byte exceptionCode = 0x01;

            // Generate error response packet
            ModbusPacket errorResponse = new ModbusPacket(
                packet.TransactionIdentifier,  // Same Transaction ID
                packet.ProtocolIdentifier,     // Same Protocol ID
                packet.UnitIdentifier,         // Same Unit ID
                (byte)(packet.FunctionCode + 0x80), // Illegal function code (original + 0x80)
                new byte[] { exceptionCode }   // Error code (0x01 - "Illegal Function")
            );

            // Generate byte array
            byte[] responseBytes = errorResponse.ToByteArray();
            // And send it
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        private static void HandleF4(ModbusPacket packet, NetworkStream stream)
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

        private static void HandleF2(ModbusPacket packet, NetworkStream stream)
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

        private static void HandleF5(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerValue) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            Program.PumpStatus.SetCoil(registerIndex, registerValue);
            //Program._pumpStatus.SetInputRegister(registerIndex, (short)registerValue);
            var returnData = Program.PumpStatus.ReadDiscreteInputs(registerIndex, registerValue);
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
    }
}
