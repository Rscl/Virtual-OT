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

    [Test]
    public void TestReadDiscreteInputs()
    {
        _pumpStatus.OverheatAlarm = true;
        _pumpStatus.LeakDetected = true;
        ushort result = _pumpStatus.ReadDiscreteInputs(3, 2);
        Assert.That(result, Is.EqualTo(3)); // 0b11
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

    [Test]
    public void TestReadCoils()
    {
        _pumpStatus.PumpEnabled = true;
        _pumpStatus.RemoteControl = true;
        ushort result = _pumpStatus.ReadCoils(0, 2);
        Assert.That(result, Is.EqualTo(3)); // 0b11
    }

    [Test]
    public void TestWriteCoils()
    {
        _pumpStatus.WriteCoils(0, 2);
        Assert.IsFalse(_pumpStatus.PumpEnabled);
        Assert.IsFalse(_pumpStatus.RemoteControl);
    }

    [Test]
    public void TestSetCoil()
    {
        _pumpStatus.SetCoil(0, 1);
        Assert.IsTrue(_pumpStatus.PumpEnabled);
        _pumpStatus.SetCoil(0, 0);
        Assert.IsFalse(_pumpStatus.PumpEnabled);
        _pumpStatus.SetCoil(1, 1);
        Assert.IsTrue(_pumpStatus.RemoteControl);
        _pumpStatus.SetCoil(1, 0);
        Assert.IsFalse(_pumpStatus.RemoteControl);
    }

    [Theory]
    [TestCase((ushort)0, (ushort)1, (ushort)1)]
    [TestCase((ushort)1, (ushort)0, (ushort)0)]
    [TestCase((ushort)2, (ushort)1, (ushort)4)]
    [TestCase((ushort)1, (ushort)1, (ushort)2)]
    public void TestGetCoil(ushort registerIndex, ushort registerValue, ushort expected)
    {
        _pumpStatus.SetCoil(registerIndex, registerValue);
        ushort result = _pumpStatus.GetCoil(registerIndex);
        Assert.That(result, Is.EqualTo(expected));
    }
}
