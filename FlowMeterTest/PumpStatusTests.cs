using FlowMeter;

namespace FlowMeterTest;

public class PumpStatusTests
{
    private PumpStatus _pumpStatus;

    [SetUp]
    public void Setup()
    {
        _pumpStatus = new PumpStatus();
    }

    [Test]
    public void TestFlowRate()
    {
        _pumpStatus.FlowRate = 100;
        Assert.That(_pumpStatus.FlowRate, Is.EqualTo(100));
    }

    [Test]
    public void TestPressure()
    {
        _pumpStatus.Pressure = 5;
        Assert.That(_pumpStatus.Pressure, Is.EqualTo(5));
    }

    [Test]
    public void TestTemperature()
    {
        _pumpStatus.Temperature = 30;
        Assert.That(_pumpStatus.Temperature, Is.EqualTo(30));
    }

    [Test]
    public void TestRuntime()
    {
        _pumpStatus.Runtime = 60;
        Assert.That(_pumpStatus.Runtime, Is.EqualTo(60));
    }

    [Test]
    public void TestPumpEnabled()
    {
        _pumpStatus.PumpEnabled = true;
        Assert.IsTrue(_pumpStatus.PumpEnabled);
        _pumpStatus.PumpEnabled = false;
        Assert.IsFalse(_pumpStatus.PumpEnabled);
    }

    [Test]
    public void TestRemoteControl()
    {
        _pumpStatus.RemoteControl = true;
        Assert.IsTrue(_pumpStatus.RemoteControl);
        _pumpStatus.RemoteControl = false;
        Assert.IsFalse(_pumpStatus.RemoteControl);
    }

    [Test]
    public void TestSafetyMode()
    {
        _pumpStatus.SafetyMode = true;
        Assert.IsTrue(_pumpStatus.SafetyMode);
        _pumpStatus.SafetyMode = false;
        Assert.IsFalse(_pumpStatus.SafetyMode);
    }

    [Test]
    public void TestIsRunning()
    {
        _pumpStatus.RPM = 1000;
        Assert.IsTrue(_pumpStatus.IsRunning);
        _pumpStatus.RPM = 0;
        Assert.IsFalse(_pumpStatus.IsRunning);
    }

    [Test]
    public void TestOverheatAlarm()
    {
        _pumpStatus.OverheatAlarm = true;
        Assert.IsTrue(_pumpStatus.OverheatAlarm);
        _pumpStatus.OverheatAlarm = false;
        Assert.IsFalse(_pumpStatus.OverheatAlarm);
    }

    [Test]
    public void TestLeakDetected()
    {
        _pumpStatus.LeakDetected = true;
        Assert.IsTrue(_pumpStatus.LeakDetected);
        _pumpStatus.LeakDetected = false;
        Assert.IsFalse(_pumpStatus.LeakDetected);
    }

    [Test]
    public void TestPressureAlarm()
    {
        _pumpStatus.PressureAlarm = true;
        Assert.IsTrue(_pumpStatus.PressureAlarm);
        _pumpStatus.PressureAlarm = false;
        Assert.IsFalse(_pumpStatus.PressureAlarm);
    }

    [Theory]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning, (ushort)1, (ushort)1, (ushort)1)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.OverheatAlarm, (ushort)1, (ushort)1, (ushort)1)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.OverheatAlarm | PumpStatus.DiscreteFlags.LeakDetected, (ushort)1, (ushort)1, (ushort)1)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.OverheatAlarm | PumpStatus.DiscreteFlags.LeakDetected | PumpStatus.DiscreteFlags.PressureAlarm, (ushort)1, (ushort)1, (ushort)1)]
    [TestCase(PumpStatus.DiscreteFlags.LeakDetected | PumpStatus.DiscreteFlags.OverheatAlarm, (ushort)1, (ushort)1, (ushort)0)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.OverheatAlarm, (ushort)2, (ushort)1, (ushort)1)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.PressureAlarm, (ushort)2, (ushort)1, (ushort)0)]
    [TestCase(PumpStatus.DiscreteFlags.PumpRunning | PumpStatus.DiscreteFlags.OverheatAlarm, (ushort)1, (ushort)2, (ushort)3)]
    public void TestReadDiscreteInputs(PumpStatus.DiscreteFlags flags, ushort start, ushort count, ushort expected)
    {
        _pumpStatus.Discretes = flags;
        ushort result = _pumpStatus.ReadDiscreteInputs(start, count);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestGetActiveDiscreteFlags()
    {
        _pumpStatus.OverheatAlarm = true;
        _pumpStatus.LeakDetected = true;
        var activeFlags = _pumpStatus.GetActiveDiscreteFlags();
        Assert.Contains("OverheatAlarm", activeFlags);
        Assert.Contains("LeakDetected", activeFlags);
    }

    [Test]
    public void TestGetDiscreteFlagStatus()
    {
        _pumpStatus.OverheatAlarm = true;
        _pumpStatus.LeakDetected = true;
        string status = _pumpStatus.GetDiscreteFlagStatus();
        Assert.IsTrue(status.Contains("OverheatAlarm: Set"));
        Assert.IsTrue(status.Contains("LeakDetected: Set"));
    }

    [Test]
    public void TestReadInputRegisters()
    {
        _pumpStatus.FlowRate = 100;
        _pumpStatus.Pressure = 5;
        short[] registers = _pumpStatus.ReadInputRegisters(1, 2);
        Assert.That(registers[0], Is.EqualTo(100));
        Assert.That(registers[1], Is.EqualTo(5));
    }

    [Theory]
    [TestCase(PumpStatus.CoilFlags.PumpEnabled)]
    public void TestReadCoils(PumpStatus.CoilFlags flags)
    {
        _pumpStatus.Coils = flags;
        foreach(PumpStatus.CoilFlags flag in Enum.GetValues(typeof(PumpStatus.CoilFlags)))
        {
            bool isFlagSet = _pumpStatus.Coils.HasFlag(flag);
            if (flags.HasFlag(flag))
            {
                Assert.IsTrue(isFlagSet, $"{flag} should be set.");
            }
            else
            {
                Assert.IsFalse(isFlagSet, $"{flag} should not be set.");
            }
        }
    }

    [Theory]
    [TestCase((ushort)1, false, PumpStatus.CoilFlags.PumpEnabled, PumpStatus.CoilFlags.None)]
    [TestCase((ushort)1, true, PumpStatus.CoilFlags.PumpEnabled, PumpStatus.CoilFlags.PumpEnabled | PumpStatus.CoilFlags.RemoteControl | PumpStatus.CoilFlags.SafetyMode)]
    [TestCase((ushort)2, false, PumpStatus.CoilFlags.RemoteControl, PumpStatus.CoilFlags.RemoteControl)]
    [TestCase((ushort)2, true, PumpStatus.CoilFlags.RemoteControl, PumpStatus.CoilFlags.None)]
    [TestCase((ushort)3, false, PumpStatus.CoilFlags.SafetyMode, PumpStatus.CoilFlags.None)]
    [TestCase((ushort)3, true, PumpStatus.CoilFlags.SafetyMode, PumpStatus.CoilFlags.None)]
    public void TestSetCoil(ushort registerIndex, bool value, PumpStatus.CoilFlags flag, PumpStatus.CoilFlags initialFlags)
    {
        _pumpStatus.Coils = initialFlags;
        _pumpStatus.SetCoil(registerIndex, value);
        if(value)
        {
            Assert.That(_pumpStatus.Coils.HasFlag(flag));
        }
        else
        {
            Assert.That(!_pumpStatus.Coils.HasFlag(flag));
        }
    }

    [Theory]
    [TestCase((ushort)0, false)]
    [TestCase((ushort)1, true)]
    [TestCase((ushort)2, false)]
    [TestCase((ushort)3, true)]
    public void TestGetCoil(ushort registerIndex, bool registerValue)
    {
        _pumpStatus.Coils = PumpStatus.CoilFlags.PumpEnabled | PumpStatus.CoilFlags.SafetyMode;
        bool result = _pumpStatus.GetCoil(registerIndex);
        Assert.That(result, Is.EqualTo(registerValue));
    }
}
