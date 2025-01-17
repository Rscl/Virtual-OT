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
        public enum CoilFlags : ushort
        {
            None = 0,
            PumpEnabled = 1 << 1,      
            RemoteControl = 1 << 2,     
            SafetyMode = 1 << 3,         
        }

        [Flags]
        public enum DiscreteFlags : ushort
        {
            None = 0,
            PumpRunning = 1 << 1,         
            OverheatAlarm = 1 << 2,       
            LeakDetected = 1 << 3,        
            PressureAlarm = 1 << 4        
        }
        private DiscreteFlags _flags = DiscreteFlags.None;
        private CoilFlags _coils = CoilFlags.None;

        public CoilFlags Coils
        {
            get => _coils; set => _coils = value;
        }

        public DiscreteFlags Discretes
        {
            get => _flags; set => _flags = value;
        }

        private Dictionary<ushort, short> inputRegisters = new Dictionary<ushort, short>();

        private const ushort _flowRate = 0x01;
        private const ushort _pressure = 0x02;
        private const ushort _temperature = 0x03;
        private const ushort _runtime = 0x04;
        private const ushort _rpm = 0x05;

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
            //Console.WriteLine($"Input register {registerIndex} set to value: {value}");
            inputRegisters[registerIndex] = value;
        }
        

        public bool PumpEnabled
        {
            get => _coils.HasFlag(CoilFlags.PumpEnabled);
            set => _coils = value ? _coils | CoilFlags.PumpEnabled : _coils & ~CoilFlags.PumpEnabled;
        }

        public bool RemoteControl
        {
            get => _coils.HasFlag(CoilFlags.RemoteControl);
            set => _coils = value ? _coils | CoilFlags.RemoteControl : _coils & ~CoilFlags.RemoteControl;
        }

        public bool SafetyMode
        {
            get => _coils.HasFlag(CoilFlags.SafetyMode);
            set => _coils = value ? _coils | CoilFlags.SafetyMode : _coils & ~CoilFlags.SafetyMode;
        }

        public bool IsRunning
        {
            get => RPM > 0;
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

        public bool PressureAlarm
        {
            get => _flags.HasFlag(DiscreteFlags.PressureAlarm);
            set => _flags = value ? _flags | DiscreteFlags.PressureAlarm : _flags & ~DiscreteFlags.PressureAlarm;
        }
        public short RPM {
            get => GetInputRegister(_rpm);
            set => SetInputRegister(_rpm, value);
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

        public ushort ReadCoils(int startRegister, int count)
        {
            // Get temporary variable
            ushort status = (ushort)_coils;

            // Shift to correct position
            status >>= startRegister;

            // Mask excess bits
            ushort mask = (ushort)((1 << count) - 1);

            // return masked value
            return (ushort)(status & mask);
        }

        public void SetCoil(ushort registerIndex, bool registerValue)
        {
            Console.WriteLine($"Setting coil {(CoilFlags)registerIndex} ({registerIndex}) to value: {registerValue}");
            ushort mask = (ushort)(1 << registerIndex);
            if (!registerValue)
            {
                _coils &= (CoilFlags)~mask; // Set register OFF
            }
            else
            {
                _coils |= (CoilFlags)mask; // Set register ON
            }
        }

        public bool GetCoil(ushort registerIndex)
        {
            ushort mask = (ushort)(1 << registerIndex);
            return _coils.HasFlag((CoilFlags)mask);
        }
    }
}
