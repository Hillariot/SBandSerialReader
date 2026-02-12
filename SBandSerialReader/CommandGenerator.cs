using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    internal class CommandGenerator
    {

        public static byte[] FifoRead(byte deviceAddress)
        {
            byte[] transmitData = new byte[3];
            transmitData[0] = deviceAddress;
            transmitData[1] = 0x03;
            transmitData[2] = CRC(transmitData, 2);

            return transmitData;
        }

        public static byte[] FifoWrite(byte deviceAddress, byte[] data)
        {
            byte[] transmitData = new byte[4 + data.Length];
            transmitData[0] = deviceAddress;
            transmitData[1] = 0x02;
            transmitData[2] = (byte)(data.Length);

            for(int i = 0; i < data.Length; i++)
            {
                transmitData[3 + i] = data[i];
            }

            transmitData[3 + data.Length] = CRC(transmitData, 3 + data.Length);

            return transmitData;
        }

        public static byte[] reset(byte deviceAddress)
        {
            byte[] transmitData = new byte[7];
            transmitData[0] = deviceAddress;
            transmitData[1] = 2;
            transmitData[2] = (byte) Commands.W_Row;
            transmitData[3] = 0x06;
            transmitData[4] = 1;
            transmitData[5] = 0x55;
            transmitData[6] = CRC(transmitData, 6);

            return transmitData;
        }

        public static byte[] RegisterRead(byte deviceAddress, byte RegisterStart, byte RegistersAmount)
        {
            byte[] transmitData = new byte[5];
            transmitData[0] = deviceAddress;
            transmitData[1] = 0x01; //Read reg cmd
            transmitData[2] = RegisterStart;
            transmitData[3] = RegistersAmount;
            transmitData[4] = CRC(transmitData, 4);

            return transmitData;
        }

        public static byte[] RegisterWrite(byte deviceAddress, byte RegisterStart, byte RegistersAmount, byte[] RegisterData)
        {
            byte[] transmitData = new byte[5 + RegistersAmount];
            transmitData[0] = deviceAddress;
            transmitData[1] = 0x00; //Write reg cmd
            transmitData[2] = RegisterStart;            
            transmitData[3] = RegistersAmount;
            
            for(int i = 0; i < RegistersAmount; i++)
            {
                transmitData[4 + i] = RegisterData[i];
            }

            transmitData[4 + RegistersAmount] = CRC(transmitData, 4 + RegistersAmount);

            return transmitData;
        }


        public static byte CRC(byte[] data, int length)
        {
            byte crc = 0;

            for (int i = 0; i < length; i++)
            {
                crc = (byte)(crc + data[i]);
            }

            return crc;
        }

    }
}
