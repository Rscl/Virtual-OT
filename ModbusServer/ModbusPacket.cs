using Microsoft.Win32;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace ModbusServer
{
    // Modbus packet structure
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModbusPacket
    {
        // Transaction Identifier (2 bytes)
        public ushort TransactionIdentifier;

        // Protocol Identifier (2 bytes)
        public ushort ProtocolIdentifier;

        // Length (2 bytes)
        public ushort Length;

        // Unit Identifier (1 byte)
        public byte UnitIdentifier;

        // Function Code (1 byte)
        public byte FunctionCode;

        // Data
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]  // Defaults to 256 bytes
        public byte[] Data;


        public override string ToString()
        {
            return $"  Transaction identifier: {TransactionIdentifier}\n" +
                $"  Protocol identifier: {ProtocolIdentifier}\n" +
                $"  Length: {Length}\n" +
                $"  Unit identifier: {UnitIdentifier}\n" +
                $"  Function code: {FunctionCode}\n" +
                $"  Data: {Convert.ToHexString(Data)}\n" +
                $"  Hex-coded: {Convert.ToHexString(ToByteArray())}";
        }

        // Ganerate packet
        public ModbusPacket(ushort transactionId, ushort protocolId, byte unitId, byte functionCode, byte[] data)
        {
            TransactionIdentifier = transactionId;
            ProtocolIdentifier = protocolId;
            Length = (ushort)(2+data.Length); // 6 bytes on header (Transaction + Protocol + Length + Unit + Function)
            UnitIdentifier = unitId;
            FunctionCode = functionCode;
            Data = data;
        }

        // Packet to byte array
        public byte[] ToByteArray()
        {
            // Calculate byte array length
            int arrayLength = 8 + Data.Length;
            byte[] byteArray = new byte[arrayLength];
            int offset = 0;

            // Transaction Identifier
            byte[] transactionIdentifierBytes = BitConverter.GetBytes(TransactionIdentifier).Reverse().ToArray();
            transactionIdentifierBytes.CopyTo(byteArray, offset);
            offset += 2;

            // Protocol Identifier
            byte[] protocolIdentifierBytes = BitConverter.GetBytes(ProtocolIdentifier).Reverse().ToArray();
            protocolIdentifierBytes.CopyTo(byteArray, offset);
            offset += 2;

            // Length
            byte[] lengthBytes = BitConverter.GetBytes(Length).Reverse().ToArray();
            lengthBytes.CopyTo(byteArray, offset);
            offset += 2;

            // Unit Identifier
            byteArray[offset++] = UnitIdentifier;

            // Function Code
            byteArray[offset++] = FunctionCode;
            // Data
            Data.CopyTo(byteArray, offset);
            return byteArray;
        }

        // Byte array to packet
        public static ModbusPacket FromByteArray(byte[] byteArray)
        {
            if (byteArray.Length < 8) // Check if byte array is too short
            {
                throw new ArgumentException("Byte array is too short to be a valid Modbus packet.");
            }
            // Generate packet
            ModbusPacket packet = new ModbusPacket
            {
                TransactionIdentifier = BitConverter.ToUInt16(new byte[] { byteArray[1], byteArray[0] }),//BitConverter.ToUInt16(byteArray, 0),
                ProtocolIdentifier = BitConverter.ToUInt16(new byte[] { byteArray[3], byteArray[2] }),//BitConverter.ToUInt16(byteArray, 2),
                Length = BitConverter.ToUInt16(new byte[]{ byteArray[5], byteArray[4] }), //BitConverter.ToUInt16(byteArray, 4),
                UnitIdentifier = byteArray[6],
                FunctionCode = byteArray[7]
            };

            // Calculate data length
            int dataLength = packet.Length -2;//byteArray.Length - 8;
            if(byteArray.Length<dataLength+8)
            {
                // Packet is too short
                throw new Exception("Invalid packet length received");
                
            }
            // Initalize data array
            packet.Data = new byte[dataLength];

            // Copy data to packet
            Array.Copy(byteArray, 8, packet.Data, 0, dataLength);
            return packet; // And finally return the packet
        }
    }
}