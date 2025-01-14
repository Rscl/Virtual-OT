using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FlowMeter
{
    public class PumpStatus
    {
        [Flags]
        private enum DiscreteFlags : ushort
        {
            None = 0,
            PumpRunning = 1 << 0,         // 0b0000_0001
            DryRunProtection = 1 << 1,    // 0b0000_0010
            OverheatAlarm = 1 << 2,       // 0b0000_0100
            LeakDetected = 1 << 3,        // 0b0000_1000
            CalibrationMode = 1 << 4,     // 0b0001_0000
            RemoteControlEnabled = 1 << 5, // 0b0010_0000
            MaintenanceRequired = 1 << 6, // 0b0100_0000
            SafetyModeEnabled = 1 << 7,   // 0b1000_0000
            AutoStartEnabled = 1 << 8,    // 0b1_0000_0000
            PressureAlarm = 1 << 9        // 0b10_0000_0000
        }
        private DiscreteFlags _flags = DiscreteFlags.None;

        private Dictionary<ushort, short> inputRegisters = new Dictionary<ushort, short>();

        private const ushort _flowRate = 0x01;
        private const ushort _pressure = 0x02;
        private const ushort _temperature = 0x03;
        private const ushort _runtime = 0x04;

        public short FlowRate
        {
            get => GetInputRegister(_flowRate);
            set => SetInputRegister(_flowRate, value);
        }

        public short Pressure
        {
            get => GetInputRegister(_pressure);
            set => SetInputRegister(_pressure, value);
        }

        public short Temperature
        {
            get => GetInputRegister(_temperature);
            set => SetInputRegister(_temperature, value);
        }

        public short Runtime
        {
            get => GetInputRegister(_runtime);
            set => SetInputRegister(_runtime, value);
        }

        public short GetInputRegister(ushort registerIndex)
        {
            return inputRegisters.GetValueOrDefault<ushort, short>(registerIndex, 0);
        }

        public void SetInputRegister(ushort registerIndex, short value)
        {
            inputRegisters[registerIndex] = value;
        }
        
        public bool IsRunning
        {
            get => _flags.HasFlag(DiscreteFlags.PumpRunning);
            set => _flags = value ? _flags | DiscreteFlags.PumpRunning : _flags & ~DiscreteFlags.PumpRunning;
        }

        public bool DryRunProtection
        {
            get => _flags.HasFlag(DiscreteFlags.DryRunProtection);
            set => _flags = value ? _flags | DiscreteFlags.DryRunProtection : _flags & ~DiscreteFlags.DryRunProtection;
        }

        public bool OverheatAlarm
        {
            get => _flags.HasFlag(DiscreteFlags.OverheatAlarm);
            set => _flags = value ? _flags | DiscreteFlags.OverheatAlarm : _flags & ~DiscreteFlags.OverheatAlarm;
        }

        public bool LeakDetected
        {
            get => _flags.HasFlag(DiscreteFlags.LeakDetected);
            set => _flags = value ? _flags | DiscreteFlags.LeakDetected : _flags & ~DiscreteFlags.LeakDetected;
        }

        public bool CalibrationMode
        {
            get => _flags.HasFlag(DiscreteFlags.CalibrationMode);
            set => _flags = value ? _flags | DiscreteFlags.CalibrationMode : _flags & ~DiscreteFlags.CalibrationMode;
        }

        public bool RemoteControlEnabled
        {
            get => _flags.HasFlag(DiscreteFlags.RemoteControlEnabled);
            set => _flags = value ? _flags | DiscreteFlags.RemoteControlEnabled : _flags & ~DiscreteFlags.RemoteControlEnabled;
        }

        public bool MaintenanceRequired
        {
            get => _flags.HasFlag(DiscreteFlags.MaintenanceRequired);
            set => _flags = value ? _flags | DiscreteFlags.MaintenanceRequired : _flags & ~DiscreteFlags.MaintenanceRequired;
        }

        public bool SafetyModeEnabled
        {
            get => _flags.HasFlag(DiscreteFlags.SafetyModeEnabled);
            set => _flags = value ? _flags | DiscreteFlags.SafetyModeEnabled : _flags & ~DiscreteFlags.SafetyModeEnabled;
        }

        public bool AutoStartEnabled
        {
            get => _flags.HasFlag(DiscreteFlags.AutoStartEnabled);
            set => _flags = value ? _flags | DiscreteFlags.AutoStartEnabled : _flags & ~DiscreteFlags.AutoStartEnabled;
        }

        public bool PressureAlarm
        {
            get => _flags.HasFlag(DiscreteFlags.PressureAlarm);
            set => _flags = value ? _flags | DiscreteFlags.PressureAlarm : _flags & ~DiscreteFlags.PressureAlarm;
        }

        public ushort ToUShort()
        {
            return (ushort)_flags;
        }

        public ushort ReadDiscreteInputs(int startRegister, int count)
        {
            // Get temporary variable
            ushort status = (ushort)_flags;

            // Shift to correct position
            status >>= startRegister;

            // Mask excess bits
            ushort mask = (ushort)((1 << count) - 1);

            // return masked value
            return (ushort)(status & mask);
        }

        public List<string> GetActiveDiscreteFlags()
        {
            var activeFlags = new List<string>();

            foreach (DiscreteFlags flag in Enum.GetValues<DiscreteFlags>())
            {
                if (flag != DiscreteFlags.None && _flags.HasFlag(flag))
                {
                    activeFlags.Add(flag.ToString());
                }
            }

            return activeFlags;
        }

        public string GetDiscreteFlagStatus()
        {
            var flagStatuses = new List<string>();

            foreach (DiscreteFlags flag in Enum.GetValues<DiscreteFlags>())
            {
                if(flag == DiscreteFlags.None)
                {
                    continue;
                }
                string status = _flags.HasFlag(flag) ? "Set" : "Not Set";
                flagStatuses.Add($"{flag}: {status}");
            }

            return string.Join(Environment.NewLine, flagStatuses);
        }


        public override string ToString()
        {
            return $"Pump Status:\n" +
                $"Discete flags:\n{GetDiscreteFlagStatus()}";
        }

        public short[] ReadInputRegisters(ushort registerIndex, ushort registerCount)
        {
            short[] registers = new short[registerCount];
            for(ushort i = 0; i<registerCount; i++)
            {
                if (inputRegisters.ContainsKey((ushort)(registerIndex + i)))
                {
                    registers[i] = inputRegisters[((ushort)(registerIndex+i))];
                }
                else
                    registers[i] = 0;
            }
            return registers;
        }
    }
}
