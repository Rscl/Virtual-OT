using System;
using System.IO;
using System.Net.Sockets;
using FlowMeter;
using ModbusServer;
using Moq;
using NUnit.Framework;

namespace FlowMeterTest
{
    public class ModbusFunctionsTests
    {
        private Mock<NetworkStream> _mockNetworkStream;
        private ModbusPacket _modbusPacket;

        [SetUp]
        public void Setup()
        {
            _mockNetworkStream = new Mock<NetworkStream>(MockBehavior.Strict);
            _modbusPacket = new ModbusPacket(1, 0, 1, 0x04, new byte[] { 0, 1, 0, 2 });
        }

        [Test]
        public void TestOnPacketReceived_HandleF4()
        {
            // Arrange
            _modbusPacket.FunctionCode = 0x04;
            _mockNetworkStream.Setup(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            ModbusFunctions.OnPacketReceived(_modbusPacket, _mockNetworkStream.Object);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void TestOnPacketReceived_HandleUnknown()
        {
            // Arrange
            _modbusPacket.FunctionCode = 0xFF;
            _mockNetworkStream.Setup(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            ModbusFunctions.OnPacketReceived(_modbusPacket, _mockNetworkStream.Object);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void TestHandleF4()
        {
            // Arrange
            _mockNetworkStream.Setup(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            ModbusFunctions.OnPacketReceived(_modbusPacket, _mockNetworkStream.Object);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void TestHandleF2()
        {
            // Arrange
            _modbusPacket.FunctionCode = 0x02;
            _mockNetworkStream.Setup(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            ModbusFunctions.OnPacketReceived(_modbusPacket, _mockNetworkStream.Object);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void TestHandleF5()
        {
            // Arrange
            _modbusPacket.FunctionCode = 0x05;
            _mockNetworkStream.Setup(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            ModbusFunctions.OnPacketReceived(_modbusPacket, _mockNetworkStream.Object);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}
