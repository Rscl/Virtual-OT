FlowMeter = Virtual Water Pump

Listens modbus protocol on port 502.

Reads and writes to holding registers 0x0000 to 0x0003.
0x0001 = Pump enabled
0x0002 = Remote control enabled. Settings this to 0x0000 shutdowns modbus port (server).
0x0003 = Safety mode. Currently does nothing.

Reads from discrete inputs 0x0001 to 0x0005.
0x0001 = Pump running
ox0002 = Overheat Alarm. Set if pump internal temperature exceeds 70 C.
0x0003 = Leak Detected. Set if flow is over set value.
0x0004 = Overpressure Alarm. Set if pressure exceeds set value.

Reads form input registers 0x0000 to 0x0005.
0x0001 = Flow rate in L/s
0x0002 = Pressure in bar
0x0003 = Temperature in C
0x0004 = Runtime in seconds
0x0005 = Pump current RPM.

Internal operation:
The pump operates based on its enabled status and various conditions. Conditions are updated every second.
When the Pump is Disabled:
•	The flow rate, pressure, RPM (revolutions per minute), and runtime are all set to zero.
•	Alarms are simulated based on the following conditions:
•	The overheat alarm is triggered if the temperature exceeds 70 degrees.
•   Other alarms are reset
When the Pump is Enabled:
•	Water consumption is simulated with random fluctuations:
•	It fluctuates between 50 and 400 liters per second.
•   Consumption is randomly decreased or increased (-25 to 25 litres per second change)
•	If water consumption is low (below 105 liters per minute), it is only increased by a random amount.
•	The flow rate is updated based on the current water consumption.
RPM Calculation:
•	The RPM of the pump is calculated based on the flow rate:
•	RPM can fluctuate between 0 and 5000.
•	If water consumption increases, RPM increases; if water consumption decreases, RPM decreases.
Pressure Calculation:
•	The pressure is calculated based on the flow rate and RPM:
•	Pressure can fluctuate between 0 and 7 bars.
•	If water consumption decreases, pressure increases; if water consumption increases, pressure decreases.
Temperature Calculation:
•	The temperature is calculated based on the RPM:
•	Temperature can fluctuate between 20 and 80 degrees.
•	If RPM increases, temperature increases; if RPM decreases, temperature decreases.
•	Temperature is limited to a range of 20 to 80 degrees.
Alarm Simulation:
•	Alarms are simulated based on the updated values of temperature, pressure, and flow rate:
•	The overheat alarm is triggered if the temperature exceeds 70 degrees.
•	The pressure alarm is triggered if the pressure exceeds 5 bars.
•	A leak is detected if the flow rate exceeds 500 liters per minute.
