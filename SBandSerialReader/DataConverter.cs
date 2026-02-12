using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    internal static class DataConverter
    {

        public static string ByteArrayToStringASCII(byte[] data)
        {
            return System.Text.Encoding.ASCII.GetString(data);
        }
        public static string ByteToStringASCII(byte data)
        {
            return System.Text.Encoding.ASCII.GetString(new byte[] { data });
        }


        public static string ByteArrayToStringHEX(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
        public static string ByteToStringHEX(byte data)
        {
            return BitConverter.ToString(new byte[] { data }).Replace("-", "");
        }


        public static byte[] ASCIIStringToByteArray(string ascii)
        {
            return Encoding.ASCII.GetBytes(ascii);
        }
        public static byte ASCIIStringToByte(string ascii)
        {
            byte[] data = Encoding.ASCII.GetBytes(ascii);
            return data[0];
        }


        public static byte[] HEXStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static byte HEXStringToByte(string hex)
        {
            byte[] data = Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
            return data[0];
        }

        public static string HEXStringToASCIIString(string hex)
        {
            byte[] data = HEXStringToByteArray(hex);
            return ByteArrayToStringASCII(data);
        }

        public static string ASCIIStringToHexString(string ascii)
        {
            byte[] data = ASCIIStringToByteArray(ascii);
            return ByteArrayToStringHEX(data);
        }
    }
}
