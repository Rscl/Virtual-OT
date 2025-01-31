﻿using ModbusServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;

namespace FlowMeter
{

    public class Program
    {
        private static bool _isRunning = true;
        private static int _waterConsumption = 0;
        private static PumpStatus _pumpStatus = new PumpStatus();
        internal static PumpStatus PumpStatus { get { return _pumpStatus; } }

        static void Main(string[] args)
        {
            Console.WriteLine("Virtual OT / FlowMeter");
            _pumpStatus = new PumpStatus()
            {
                PumpEnabled = true,
                OverheatAlarm = false,
                LeakDetected = false,
                RemoteControl = true,
                SafetyMode = true,
                PressureAlarm = false,
                Temperature = 30,
                Pressure = 3,
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
                if (PumpStatus.RemoteControl == false)
                {
                    
                    server.Stop();
                }
                UpdatePumpStatus();
                Thread.Sleep(1000);
            }
            Console.WriteLine("Shutting down Modbus Server...");
            server.Stop();
            Console.WriteLine("Exitng...");
        }

        private static void UpdatePumpStatus()
        {
            if (PumpStatus.PumpEnabled == false)
            {
                PumpStatus.FlowRate = 0;
                PumpStatus.Pressure = 0;
                PumpStatus.RPM = 0;
                PumpStatus.Runtime = 0;
                // Simulate alarms
                // Overheat alarm is triggered if temperature is over 70 degrees
                _pumpStatus.OverheatAlarm = _pumpStatus.Temperature > 70;
                // Pressure alarm is triggered if pressure is over 5
                _pumpStatus.PressureAlarm = _pumpStatus.Pressure > 5;
                // Leak is detected if flowrate is over 500
                _pumpStatus.LeakDetected = _pumpStatus.FlowRate > 500;
                return;

            }
            // Simulate water consumption
            // Water consumption should fluctuate between 50 and 400 liters per minute, based on previous values
            int deltaConsumption = new Random().Next(-25, 25);

            // Water consumption os low, increase water consumption
            if (_waterConsumption < 105)
                deltaConsumption = new Random().Next(0, 50);

            _waterConsumption += deltaConsumption;

               
            // Water consumption is limited to 0 - 1000 liters per minute
            if (_waterConsumption < 0)
            {
                _waterConsumption = 0;
            }
            else if (_waterConsumption > 1000)
            {
                _waterConsumption = 1000;
            }

            _pumpStatus.FlowRate = (short)_waterConsumption;

            // Calculate RPM based on flow rate
            // RPM can fluctuate between 0 and 5000
            // based on previous values. If delta consumption is positive,
            // RPM increases, if negative, RPM decreases
            int deltaRPM = deltaConsumption * 10;
            _pumpStatus.RPM += (short)deltaRPM;

            // Finally calculate pressure based on flow rate and RPM
            // Pressure can fluctuate between 0 and 7 bars
            // based on previous values. If delta consumption is negative, pressure increases,
            // if positive, pressure decreases
            _pumpStatus.Pressure += (short)(deltaConsumption / 5);
            if (_pumpStatus.Pressure < 0)
            {
                _pumpStatus.Pressure = 0;
            }
            else if (_pumpStatus.Pressure > 70)
            {
                _pumpStatus.Pressure = 70;
            }

            // Simulate temperature increase
            // Temperature can fluctuate between 20 and 80 degrees
            // based on previous values. If delta rpm is positive, temperature increases,
            // if negative, temperature decreases
            _pumpStatus.Temperature += (short)(deltaRPM / 100);
            if (_pumpStatus.Temperature < 20)
            {
                _pumpStatus.Temperature = 20;
            }
            else if (_pumpStatus.Temperature > 80)
            {
                _pumpStatus.Temperature = 80;
            }

            // Simulate alarms
            // Overheat alarm is triggered if temperature is over 70 degrees
            _pumpStatus.OverheatAlarm = _pumpStatus.Temperature > 70;
            // Pressure alarm is triggered if pressure is over 5
            _pumpStatus.PressureAlarm = _pumpStatus.Pressure > 5;
            // Leak is detected if flowrate is over 500
            _pumpStatus.LeakDetected = _pumpStatus.FlowRate > 500;

        }

        public static void OnPacketReceived(ModbusPacket packet, NetworkStream stream)
        {
            Console.WriteLine($"New packet received with function: {packet.FunctionCode} from {(stream.Socket?.RemoteEndPoint as IPEndPoint)?.Address}");
            switch (packet.FunctionCode)
            {
                case 0x01:
                    ModbusFunctions.HandleF1(packet, stream);
                    break;
                case 0x02:
                    ModbusFunctions.HandleF2(packet, stream);
                    break;
                case 0x04:
                    ModbusFunctions.HandleF4(packet, stream);
                    break;
                case 0x05:
                    ModbusFunctions.HandleF5(packet, stream);
                    break;
                default:
                    ModbusFunctions.HandleError(packet, stream, 1);
                    break;
            }
        }
    }
}
