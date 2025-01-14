using ModbusServer;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;

namespace FlowMeter
{
    public class Program
    {
        private static bool _isRunning = true;
        private static PumpStatus _pumpStatus = new PumpStatus();
        static void Main(string[] args)
        {
            Console.WriteLine("Virtual OT / FlowMeter");
            _pumpStatus = new PumpStatus()
            {
                IsRunning = true,
                DryRunProtection = false,
                OverheatAlarm = false,
                LeakDetected = false,
                CalibrationMode = false,
                RemoteControlEnabled = true,
                MaintenanceRequired = false,
                SafetyModeEnabled = true,
                AutoStartEnabled = true,
                PressureAlarm = false,
                Temperature = 1,
                Pressure = 2,
                FlowRate = 300,
                Runtime = 0
            };

            // New thread to update runtime every minute
            new Thread(() => {
                while(_isRunning)
                {
                    _pumpStatus.Runtime++;// = _pumpStatus.Runtime++;
                    Thread.Sleep(1000);
                }
            }).Start();
            Console.WriteLine(_pumpStatus.ToString());
            Console.WriteLine("Setting up Modbus Server...");
            ModbusServer.ModbusServer server = new ModbusServer.ModbusServer(502);
            server.PacketReceived += OnPacketReceived;
            Console.WriteLine("Starting Modbus Server...");
            server.Start();
            Console.WriteLine("Server started...");
            while (_isRunning)
            {
                // Update pump status
                //_pumpStatus.FlowRate = (short)((Math.Sin(DateTime.Now.Second) * 100));
                _pumpStatus.FlowRate = ((short)(Random.Shared.Next(50, 300)));
                _pumpStatus.Pressure = 10;// (short)(Math.Cos(DateTime.Now.Second) * 100);
                _pumpStatus.Temperature = 25; // (short)(Math.Tan(DateTime.Now.Second) * 100);
                Thread.Sleep(1000);
            }
            Console.WriteLine("Shutting down Modbus Server...");
            server.Stop();
            Console.WriteLine("Exitng...");
        }

        public static void OnPacketReceived(ModbusPacket packet, NetworkStream stream)
        {
            Console.WriteLine($"New packet received with function: {packet.FunctionCode} from {(stream.Socket?.RemoteEndPoint as IPEndPoint)?.Address}");
            switch(packet.FunctionCode)
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
                default:
                    HandleUnknown(packet, stream);
                    break;
            }
            if(packet.FunctionCode == 0x01)
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
                (byte)(packet.FunctionCode + 0x80), // Illegal funktion code (original + 0x80)
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
            var returnData = _pumpStatus.ReadInputRegisters(registerIndex, registerCount);

            // Varaus vastauksen datalle, lisäämällä rekisterin määrä alkuun
            byte[] returnByteData = new byte[returnData.Length * 2 + 1];

            // Muodostetaan rekisterin määrä big-endian -muodossa (koko = rekisterin määrä * 2)
            var registerCountBytes = BitConverter.GetBytes(registerCount * 2);
            returnByteData[0] = registerCountBytes[0];
            //returnByteData[1] = registerCountBytes[1];

            // Lisätään rekisterit ja varmistetaan big-endian -järjestys
            for (int i = 0; i < returnData.Length; i++)
            {
                var b = BitConverter.GetBytes(returnData[i]);
                byte[] bytes = { b[1], b[0] }; // Big-endian
                bytes.CopyTo(returnByteData, 1 + (i * 2)); // Kopioidaan vastaava data vastaukseen
               
                //Console.WriteLine($"Data {i}: {returnData[i]} - {Convert.ToHexString(new byte[] { returnByteData[1 + i], returnByteData[1 + i+1] })}");
            }

            // Luodaan vastauspaketti Modbusin sääntöjen mukaisesti
            ModbusPacket response = new ModbusPacket(
                packet.TransactionIdentifier,
                packet.ProtocolIdentifier,
                packet.UnitIdentifier,
                packet.FunctionCode,
                returnByteData
            );

            // Lähetetään vastaus asiakasohjelmalle
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }


        private static void HandleF2(ModbusPacket packet, NetworkStream stream)
        {
            var (registerIndex, registerCount) = ModbusServer.Utils.ModbusFunctions.ReadInputData(packet.Data);
            var returnData = _pumpStatus.ReadDiscreteInputs(registerIndex, registerCount);

            ModbusPacket response = new ModbusPacket(
            packet.TransactionIdentifier,
            packet.ProtocolIdentifier,
            packet.UnitIdentifier,
            packet.FunctionCode,
            ModbusServer.Utils.Endian.ConvertToBigEndian(BitConverter.GetBytes(returnData))
            );
            byte[] responseBytes = response.ToByteArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
