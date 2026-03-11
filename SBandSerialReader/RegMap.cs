using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    public enum RegMap
    {
        DeviceType =            0,
        SerialNumber1 =         1,
        SerialNumber2 =         2,
        SerialNumber3 =         3,
        SerialNumber4 =         4,
        SerialNumber5 =         5,
        Reset =                 6,
        TransmitAdress1 =       7,
        TransmitAdress2 =       8,
        TransmitAdress3 =       9,
        TransmitAdress4 =       10,
        TransmitAdress5 =       11,
        ReceiveAdress1 =        12,
        ReceiveAdress2 =        13,
        ReceiveAdress3 =        14,
        ReceiveAdress4 =        15,
        ReceiveAdress5 =        16,
        TransmitFifoMsgs =      17,
        TransmitFifoLen =       18,
        ReceiveFifoMsgs =       19,
        ReceiveFifoLen =        20,
        Status =                21,
        InterruptConfig =       22,
        Config =                23,
        Frequency1 =            24,
        Frequency2 =            25,
        Frequency3 =            26,
        Frequency4 =            27,
        Power =                 28,
        LoraSpreadingFactor =   29,
        LoraCodingRate =        30,
        LoraBandwidth =         31,
        BitRate1 =              32,
        BitRate2 =              33,
        BitRate3 =              34,
        FskFdev =               35,
        FskBandwidth =          36,
        FskPreambleLength =     37,
        FskGaussFilter =        38,
        PioDir =                39,
        PioOut =                40,
        PioIn =                 41,
        RadioStackCgf =         42
    }

    public enum Commands
    {
        W_Row = 0,
        R_Row = 1,
        W_FIFO = 2,
        R_FIFO = 3
    }
}
