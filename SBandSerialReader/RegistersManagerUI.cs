using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.ComboBox;

namespace SBandSerialReader
{
    internal class RegistersManagerUI
    {
        public static void WriteRegisters(byte startReg, byte[] regs,
                                          Control[] hexValueControls,
                                          Control[] varDataControls)
        {
            for (int i = startReg; i < regs.Length + startReg; i++)
            {
                switch (i)
                {
                    case (int)RegMap.DeviceType:
                        SetDeviceType(regs[i - startReg],
                                      hexValueControls[i],
                                      varDataControls[i]);
                        break;
                    case (int)RegMap.SerialNumber5:
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                        ArrayExtensions.SubArray(hexValueControls, i - 4, 5),
                                        ArrayExtensions.SubArray(varDataControls, i - 4, 5),
                                        5);
                        break;
                    case (int)RegMap.TransmitAdress5:
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                           ArrayExtensions.SubArray(hexValueControls, i - 4, 5),
                                           null,
                                           5);
                        break;
                    case (int)RegMap.ReceiveAdress5:
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                           ArrayExtensions.SubArray(hexValueControls, i - 4, 5),
                                           null,
                                           5);
                        break;
                    case (int)RegMap.TransmitFifoMsgs:
                        SetFifoBytes(regs[i - startReg],
                                     hexValueControls[i],
                                     varDataControls[i]);
                        break;
                    case (int)RegMap.TransmitFifoLen:
                        SetFifoBytes(regs[i - startReg],
                                     hexValueControls[i],
                                     varDataControls[i]);
                        break;
                    case (int)RegMap.ReceiveFifoMsgs:
                        SetFifoBytes(regs[i - startReg],
                                     hexValueControls[i],
                                     varDataControls[i]);
                        break;
                    case (int)RegMap.ReceiveFifoLen:
                        SetFifoBytes(regs[i - startReg],
                                     hexValueControls[i],
                                     varDataControls[i]);
                        break;
                    case (int)RegMap.Status:
                        SetStatus(regs[i - startReg],
                                  hexValueControls[i],
                                  varDataControls[i]);
                        break;
                    case (int)RegMap.InterruptConfig:
                        SetInterruptConfig(regs[i - startReg],
                                           hexValueControls[i],
                                           varDataControls[i]);
                        break;
                    case (int)RegMap.Config:
                        SetConfig(regs[i - startReg],
                                  hexValueControls[i],
                                  varDataControls[i]);
                        break;
                    case (int)RegMap.Frequency4:
                        SetFrequency(ArrayExtensions.SubArray(regs, i - startReg - 3, 4),
                                    ArrayExtensions.SubArray(hexValueControls, i - 3, 4),
                                    varDataControls[i]);
                        break;
                    case (int)RegMap.Power:
                        setPower(regs[i - startReg],
                            hexValueControls[i],
                            varDataControls[i]);
                        break;
                    case (int)RegMap.SpreadingFactor:
                        setSpreadingFactor(regs[i - startReg],
                        hexValueControls[i],
                        varDataControls[i]);
                        break;
                    case (int)RegMap.CodingRate:
                        setCodingRate(regs[i - startReg],
                        hexValueControls[i],
                        varDataControls[i]);
                        break;
                    case (int)RegMap.Bandwidth:
                        setBandwidth(regs[i - startReg],
                        hexValueControls[i],
                        varDataControls[i]);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void setPower(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            int power = (int) data - 10;

            textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);
            textBoxVarDatas.Text = power.ToString();
        }

        private static void setSpreadingFactor(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            int selectedIndex = -1;
            switch (data)
            {
                case 0x07:
                    selectedIndex = 0;
                    break;
                case 0x08:
                    selectedIndex = 1;
                    break;
                case 0x09:
                    selectedIndex = 2;
                    break;
                case 0x0A:
                    selectedIndex = 3;
                    break;
                case 0x0B:
                    selectedIndex = 4;
                    break;
                case 0x0C:
                    selectedIndex = 5;
                    break;
            }

            if (selectedIndex != -1)
            {
                textBoxHexValues.BackColor = Color.White;
                textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);

                ComboBox comboBox = (ComboBox)textBoxVarDatas;
                comboBox.SelectedIndex = selectedIndex;
            }
            else
            {
                textBoxHexValues.BackColor = Color.Orange;
            }
        }

        private static void setCodingRate(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            int selectedIndex = -1;
            switch (data)
            {
                case 0x05:
                    selectedIndex = 0;
                    break;
                case 0x06:
                    selectedIndex = 1;
                    break;
                case 0x07:
                    selectedIndex = 2;
                    break;
                case 0x08:
                    selectedIndex = 3;
                    break;
            }

            if (selectedIndex != -1)
            {
                textBoxHexValues.BackColor = Color.White;
                textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);

                ComboBox comboBox = (ComboBox)textBoxVarDatas;
                comboBox.SelectedIndex = selectedIndex;
            }
            else
            {
                textBoxHexValues.BackColor = Color.Orange;
            }
        }

        private static void setBandwidth(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            int selectedIndex = -1;
            switch (data)
            {
                case 0x00:
                    selectedIndex = 0;
                    break;
                case 0x01:
                    selectedIndex = 1;
                    break;
                case 0x02:
                    selectedIndex = 2;
                    break;
            }

            if (selectedIndex != -1)
            {
                textBoxHexValues.BackColor = Color.White;
                textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);

                ComboBox comboBox = (ComboBox)textBoxVarDatas;
                comboBox.SelectedIndex = selectedIndex;
            }
            else
            {
                textBoxHexValues.BackColor = Color.Orange;
            }
        }

        private static void SetDeviceType(byte deviceType, Control textBoxHexValue, Control textBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(deviceType);

            DeviceType device = (DeviceType)deviceType;
            switch (device)
            {
                case DeviceType.SX1280:
                    textBoxVarData.Text = "SX1280";
                    break;
                case DeviceType.SX1268:
                    textBoxVarData.Text = "SX1268";
                    break;
                default:
                    textBoxVarData.Text = "Неизвестно";
                    break;
            }
        }

        private static void SetMultipleRegsRow(byte[] data, Control[] textBoxHexValues, Control[] textBoxVarDatas, int len)
        {
            for(int i = 0; i < len; i++)
            {
                textBoxHexValues[i].Text = DataConverter.ByteToStringHEX(data[i]);
                if (textBoxVarDatas != null)
                {
                    textBoxVarDatas[i].Text = DataConverter.ByteToStringASCII(data[i]);
                }
            }
        }

        private static void SetFifoBytes(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);
            textBoxVarDatas.Text = data.ToString();
        }

        private static void SetStatus(byte status, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(status);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox cb)
                {
                    switch (cb.Name)
                    {
                        case "checkBoxRow7DataBit7":
                            cb.Checked = Convert.ToBoolean((status >> 7) & 1);
                            break;
                        case "checkBoxRow7DataBit6":
                            cb.Checked = Convert.ToBoolean((status >> 6) & 1);
                            break;
                        case "checkBoxRow7DataBit5":
                            cb.Checked = Convert.ToBoolean((status >> 5) & 1);
                            break;
                        case "checkBoxRow7DataBit4":
                            cb.Checked = Convert.ToBoolean((status >> 4) & 1);
                            break;
                        case "checkBoxRow7DataBit3":
                            cb.Checked = Convert.ToBoolean((status >> 3) & 1);
                            break;
                        case "checkBoxRow7DataBit2":
                            cb.Checked = Convert.ToBoolean((status >> 2) & 1);
                            break;
                        case "checkBoxRow7DataBit1":
                            cb.Checked = Convert.ToBoolean((status >> 1) & 1);
                            break;
                        case "checkBoxRow7DataBit0":
                            cb.Checked = Convert.ToBoolean((status >> 0) & 1);
                            break;
                    }
                }
            }
        }

        private static void SetInterruptConfig(byte interruptConfig, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(interruptConfig);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox cb)
                {
                    switch (cb.Name)
                    {
                        case "checkBoxRow8DataBit7":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 7) & 1);
                            break;
                        case "checkBoxRow8DataBit6":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 6) & 1);
                            break;
                        case "checkBoxRow8DataBit5":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 5) & 1);
                            break;
                        case "checkBoxRow8DataBit4":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 4) & 1);
                            break;
                        case "checkBoxRow8DataBit3":
                            cb.Checked = Convert.ToBoolean((interruptConfig  >> 3) & 1);
                            break;
                        case "checkBoxRow8DataBit2":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 2) & 1);
                            break;
                        case "checkBoxRow8DataBit1":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 1) & 1);
                            break;
                        case "checkBoxRow8DataBit0":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 0) & 1);
                            break;
                    }
                }
            }
        }

        private static void SetConfig(byte config, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(config);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox cb)
                {
                    switch (cb.Name)
                    {
                        case "checkBoxRow9DataBit7":
                            cb.Checked = Convert.ToBoolean((config >> 7) & 1);
                            break;
                        case "checkBoxRow9DataBit6":
                            cb.Checked = Convert.ToBoolean((config >> 6) & 1);
                            break;
                        case "checkBoxRow9DataBit4":
                            cb.Checked = Convert.ToBoolean((config >> 4) & 1);
                            break;
                        case "checkBoxRow9DataBit3":
                            cb.Checked = Convert.ToBoolean((config >> 3) & 1);
                            break;
                        case "checkBoxRow9DataBit2":
                            cb.Checked = Convert.ToBoolean((config >> 2) & 1);
                            break;
                        case "checkBoxRow9DataBit1":
                            cb.Checked = Convert.ToBoolean((config >> 1) & 1);
                            break;
                        case "checkBoxRow9DataBit0":
                            cb.Checked = Convert.ToBoolean((config >> 0) & 1);
                            break;
                    }
                }
                else if(c is GroupBox gp)
                {
                    bool bit5 = Convert.ToBoolean((config >> 5) & 1);

                    foreach (Control c2 in gp.Controls)
                    {
                        if (c2 is RadioButton rb)
                        {
                            switch (rb.Name)
                            {
                                case "radioButtonRow9DataBitLora":
                                    if (bit5)
                                    {
                                        rb.Checked = true;
                                    }
                                    break;
                                case "radioButtonRow9DataBitFsk":
                                    if (!bit5)
                                    {
                                        rb.Checked = true;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static void SetFrequency(byte[] freqBytes, Control[] textBoxesHexValue, Control textBoxesVarData)
        {
            byte[] freqRevBytes = new byte[freqBytes.Length];
            for(int  i = 0; i < freqBytes.Length; i++)
            {
                freqRevBytes[i] = freqBytes[i];
            }

            Array.Reverse(freqRevBytes);
            uint frequency = BitConverter.ToUInt32(freqRevBytes, 0);

            for(int i = 0; i < 4; i++)
            {
                textBoxesHexValue[i].Text = DataConverter.ByteToStringHEX(freqBytes[i]);
            }

            textBoxesVarData.Text = frequency.ToString();
        }

    }

    public static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

}
