import time
from smbus2 import SMBus

ADC_ADDRESS = 0x48
CONFIG_REGISTER = 0x01
CONVERSION_REGISTER = 0x00

#==========================================================================
#WILL NOT WORK IF YOU DO NOT HAS SENSOR CONNECTED 
#==========================================================================

with SMBus(1) as bus:
    print("Reading soil moisture sensor...")
    print("Press Ctrl+C to stop.\n")

    try:
        while True:
            # A0, single-ended, +/-4.096V range,
            # single-shot conversion, 128 samples/sec
            config = 0xC383

            bus.write_i2c_block_data(
                ADC_ADDRESS,
                CONFIG_REGISTER,
                [(config >> 8) & 0xFF, config & 0xFF]
            )

            time.sleep(0.02)

            data = bus.read_i2c_block_data(
                ADC_ADDRESS,
                CONVERSION_REGISTER,
                2
            )

            raw = (data[0] << 8) | data[1]

            if raw >= 32768:
                raw -= 65536

            voltage = raw * 4.096 / 32768

            print(f"Raw ADC: {raw:6d} | Voltage: {voltage:.3f} V")

            time.sleep(1)

    except KeyboardInterrupt:
        print("\nSensor test stopped.")