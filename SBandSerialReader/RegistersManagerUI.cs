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
        public static void WriteRegisters(byte startReg, byte[] regs)
        {
            Control.ControlCollection controls = Application.OpenForms["Form1"].Controls["panel1"].Controls["groupBox3"].Controls;
            for (int i = startReg; i < regs.Length + startReg; i++)
            {
                switch (i)
                {
                    case (int)RegMap.DeviceType:
                        SetDeviceType(regs[i - startReg],
                                      controls["textBoxRow0Value"],
                                      controls["textBoxRow0Data"]);
                        break;
                    case (int)RegMap.SerialNumber5:
                        Control[] hexValueSerialNumber5Controls = {
                            controls["textBoxRow1Value1"], controls["textBoxRow1Value2"],
                            controls["textBoxRow1Value3"], controls["textBoxRow1Value4"],
                            controls["textBoxRow1Value5"]
                        };
                        Control[] hexDataSerialNumber5Controls = {
                            controls["textBoxRow1Data1"], controls["textBoxRow1Data2"],
                            controls["textBoxRow1Data3"], controls["textBoxRow1Data4"],
                            controls["textBoxRow1Data5"]
                        };
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                        hexValueSerialNumber5Controls,
                                        hexDataSerialNumber5Controls,
                                        5);
                        break;
                    case (int)RegMap.TransmitAdress5:
                        Control[] hexValueTransmitAdress5Controls = {
                            controls["textBoxRow3Value1"], controls["textBoxRow3Value2"],
                            controls["textBoxRow3Value3"], controls["textBoxRow3Value4"],
                            controls["textBoxRow3Value5"]
                        };
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                           hexValueTransmitAdress5Controls,
                                           null,
                                           5);
                        break;
                    case (int)RegMap.ReceiveAdress5:
                        Control[] hexValueReceiveAdress5Controls = {
                            controls["textBoxRow4Value1"], controls["textBoxRow4Value2"],
                            controls["textBoxRow4Value3"], controls["textBoxRow4Value4"],
                            controls["textBoxRow4Value5"]
                        };
                        SetMultipleRegsRow(ArrayExtensions.SubArray(regs, i - startReg - 4, 5),
                                           hexValueReceiveAdress5Controls,
                                           null,
                                           5);
                        break;
                    case (int)RegMap.TransmitFifoMsgs:
                        SetFifoBytes(regs[i - startReg],
                                      controls["textBoxRow5Value1"],
                                      controls["textBoxRow5Data1"]);
                        break;
                    case (int)RegMap.TransmitFifoLen:
                        SetFifoBytes(regs[i - startReg],
                                      controls["textBoxRow5Value2"],
                                      controls["textBoxRow5Data2"]);
                        break;
                    case (int)RegMap.ReceiveFifoMsgs:
                        SetFifoBytes(regs[i - startReg],
                                      controls["textBoxRow6Value1"],
                                      controls["textBoxRow6Data1"]);
                        break;
                    case (int)RegMap.ReceiveFifoLen:
                        SetFifoBytes(regs[i - startReg],
                                      controls["textBoxRow6Value2"],
                                      controls["textBoxRow6Data2"]);
                        break;
                    case (int)RegMap.Status:
                        SetStatus(regs[i - startReg],
                                      controls["textBoxRow7Value"],
                                      controls["groupBoxRow7Data"]);
                        break;
                    case (int)RegMap.InterruptConfig:
                        SetInterruptConfig(regs[i - startReg],
                                      controls["textBoxRow8Value"],
                                      controls["groupBoxRow8Data"]);
                        break;
                    case (int)RegMap.Config:
                        SetConfig(regs[i - startReg],
                                      controls["textBoxRow9Value"],
                                      controls["groupBoxRow9Data"]);
                        break;
                    case (int)RegMap.Frequency4:
                        Control[] hexValueFrequency4Controls = {
                            controls["textBoxRow10Value1"], controls["textBoxRow10Value2"],
                            controls["textBoxRow10Value3"],controls["textBoxRow10Value4"]
                        };
                        SetFrequency(ArrayExtensions.SubArray(regs, i - startReg - 3, 4),
                                    hexValueFrequency4Controls,
                                    controls["textBoxRow10Data"]);
                        break;
                    case (int)RegMap.Power:
                        setPower(regs[i - startReg],
                                      controls["textBoxRow11Value"],
                                      controls["textBoxRow11Data"]);
                        break;
                    case (int)RegMap.LoraSpreadingFactor:
                        setSpreadingFactor(regs[i - startReg],
                                      controls["textBoxRow12Value"],
                                      controls["comboBoxRow12Data"]);
                        break;
                    case (int)RegMap.LoraCodingRate:
                        setCodingRate(regs[i - startReg],
                                      controls["textBoxRow13Value"],
                                      controls["comboBoxRow13Data"]);
                        break;
                    case (int)RegMap.LoraBandwidth:
                        setBandwidth(regs[i - startReg],
                                      controls["textBoxRow14Value"],
                                      controls["comboBoxRow14Data"]);
                        break;


                    case (int)RegMap.BitRate3:
                        Control[] hexValueBitRate3Controls = {
                            controls["textBoxRow15Value1"], controls["textBoxRow15Value2"],
                            controls["textBoxRow15Value3"]
                        };
                        SetFrequency(ArrayExtensions.SubArray(regs, i - startReg - 2, 3),
                                    hexValueBitRate3Controls,
                                    controls["textBoxRow15Data"]);
                        break;
                    case (int)RegMap.FskFdev:
                        setFskFdev(regs[i - startReg],
                                      controls["textBoxRow16Value"],
                                      controls["textBoxRow16Data"]);
                        break;
                    case (int)RegMap.FskBandwidth:
                        setFskBandwidth(regs[i - startReg],
                                            controls["textBoxRow17Value"],
                                            controls["comboBoxRow17Data"]);
                        break;
                    case (int)RegMap.FskPreambleLength:
                        setFskPreambleLength(regs[i - startReg],
                                      controls["textBoxRow18Value"],
                                      controls["textBoxRow18Data"]);
                        break;
                    case (int)RegMap.FskGaussFilter:
                        setFskGaussFilter(regs[i - startReg],
                                      controls["textBoxRow19Value"],
                                      controls["comboBoxRow19Data"]);
                        break;
                    case (int)RegMap.PioDir:
                        setPioDir(regs[i - startReg],
                                      controls["textBoxRow20Value"],
                                      controls["groupBoxRow20Data"]);
                        break;
                    case (int)RegMap.PioOut:
                        setPioOut(regs[i - startReg],
                                      controls["textBoxRow21Value"],
                                      controls["groupBoxRow21Data"]);
                        break;
                    case (int)RegMap.PioIn:
                        setPioIn(regs[i - startReg],
                                      controls["textBoxRow22Value"],
                                      controls["groupBoxRow22Data"]);
                        break;
                    case (int)RegMap.RadioStackCgf:
                        setRadioConfig(regs[i - startReg],
                                      controls["textBoxRow23Value"],
                                      controls["groupBoxRow23Data"]);
                        break;

                    default:
                        break;
                }
            }
        }

        private static void setPower(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            sbyte power = unchecked((sbyte)data);

            textBoxHexValues.Text = DataConverter.ByteToStringHEX(data);
            textBoxVarDatas.Text = power.ToString();
        }

        private static void setSpreadingFactor(byte data, Control textBoxHexValues, Control textBoxVarDatas)
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
                case 0x09:
                    selectedIndex = 4;
                    break;
                case 0x0A:
                    selectedIndex = 5;
                    break;
                case 0x0B:
                    selectedIndex = 6;
                    break;
                case 0x0C:
                    selectedIndex = 7;
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
                case 0x01:
                    selectedIndex = 0;
                    break;
                case 0x02:
                    selectedIndex = 1;
                    break;
                case 0x03:
                    selectedIndex = 2;
                    break;
                case 0x04:
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
                    selectedIndex = 2;
                    break;
                case 0x02:
                    selectedIndex = 4;
                    break;
                case 0x03:
                    selectedIndex = 6;
                    break;
                case 0x04:
                    selectedIndex = 7;
                    break;
                case 0x05:
                    selectedIndex = 8;
                    break;
                case 0x06:
                    selectedIndex = 9;
                    break;
                case 0x08:
                    selectedIndex = 1;
                    break;
                case 0x09:
                    selectedIndex = 3;
                    break;
                case 0x0A:
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

        private static void setFskBandwidth(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {
            int selectedIndex = -1;

            switch (data)
            {
                case 0x09:
                    selectedIndex = 20;
                    break;
                case 0x11:
                    selectedIndex = 19;
                    break;
                case 0x19:
                    selectedIndex = 18;
                    break;
                case 0x0A:
                    selectedIndex = 17;
                    break;
                case 0x12:
                    selectedIndex = 16;
                    break;
                case 0x1A:
                    selectedIndex = 15;
                    break;
                case 0x0B:
                    selectedIndex = 14;
                    break;
                case 0x13:
                    selectedIndex = 13;
                    break;
                case 0x1B:
                    selectedIndex = 12;
                    break;
                case 0x0C:
                    selectedIndex = 11;
                    break;
                case 0x14:
                    selectedIndex = 10;
                    break;
                case 0x1C:
                    selectedIndex = 9;
                    break;
                case 0x0D:
                    selectedIndex = 8;
                    break;
                case 0x15:
                    selectedIndex = 7;
                    break;
                case 0x1D:
                    selectedIndex = 6;
                    break;
                case 0x0E:
                    selectedIndex = 5;
                    break;
                case 0x16:
                    selectedIndex = 4;
                    break;
                case 0x1E:
                    selectedIndex = 3;
                    break;
                case 0x0F:
                    selectedIndex = 2;
                    break;
                case 0x17:
                    selectedIndex = 1;
                    break;
                case 0x1F:
                    selectedIndex = 0;
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

        private static void setFskGaussFilter(byte data, Control textBoxHexValues, Control textBoxVarDatas)
        {

            int selectedIndex = -1;
            switch (data)
            {
                case 0x00:
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
            for (int i = 0; i < len; i++)
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
                    }
                }
                else if (c is ComboBox cbb)
                {
                    switch (cbb.Name)
                    {
                        case "comboBoxRow8DataBits3_5":
                            int selectedIndex3_5 = ((interruptConfig >> 3) & 0b111);
                            cbb.SelectedIndex = selectedIndex3_5;
                            break;
                        case "comboBoxRow8DataBits0_2":
                            int selectedIndex0_2 = ((interruptConfig) & 0b111);
                            cbb.SelectedIndex = selectedIndex0_2;
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
                else if (c is GroupBox gp)
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
            byte[] freqRevBytes = new byte[4];
            for (int i = 0; i < 3; i++)
            {
                freqRevBytes[i] = freqBytes[i];
            }

            freqRevBytes[3] = freqBytes.Length == 3 ? freqRevBytes[3] = 0 : freqRevBytes[3] = freqRevBytes[3];

            Array.Reverse(freqRevBytes);
            uint frequency = BitConverter.ToUInt32(freqRevBytes, 0);

            for (int i = 0; i < freqBytes.Length; i++)
            {
                textBoxesHexValue[i].Text = DataConverter.ByteToStringHEX(freqBytes[i]);
            }

            textBoxesVarData.Text = frequency.ToString();
        }

        private static void setFskFdev(byte fskFdev, Control textBoxHexValue, Control textBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(fskFdev);
            textBoxVarData.Text = fskFdev.ToString();
        }

        private static void setFskPreambleLength(byte fskPreamble, Control textBoxHexValue, Control textBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(fskPreamble);
            textBoxVarData.Text = fskPreamble.ToString();
        }

        private static void setRadioConfig(byte interruptConfig, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(interruptConfig);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox cb)
                {
                    switch (cb.Name)
                    {
                        case "checkBoxRow23DataBit7":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 7) & 1);
                            break;
                        case "checkBoxRow23DataBit6":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 6) & 1);
                            break;
                        case "checkBoxRow23DataBit5":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 5) & 1);
                            break;
                        case "checkBoxRow23DataBit4":
                            cb.Checked = Convert.ToBoolean((interruptConfig >> 4) & 1);
                            break;
                    }
                }
                else if (c is ComboBox cbb)
                {
                    switch (cbb.Name)
                    {
                        case "comboBoxRow23DataBits0_3":
                            int selectedIndex0_3 = ((interruptConfig) & 0b1111);
                            cbb.SelectedIndex = selectedIndex0_3;
                            break;
                    }
                }
            }
        }
        private static void setPioDir(byte pioDir, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(pioDir);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            { 
                if (c is CheckBox)
                {
                    CheckBox cb = (CheckBox)c;
                    switch (cb.Name)
                    {
                        case "checkBoxRow20DataBit5":
                            cb.Checked = Convert.ToBoolean((pioDir >> 5) & 1);
                            break;
                        case "checkBoxRow20DataBit4":
                            cb.Checked = Convert.ToBoolean((pioDir >> 4) & 1);
                            break;
                        case "checkBoxRow20DataBit3":
                            cb.Checked = Convert.ToBoolean((pioDir >> 3) & 1);
                            break;
                        case "checkBoxRow20DataBit2":
                            cb.Checked = Convert.ToBoolean((pioDir >> 2) & 1);
                            break;
                        case "checkBoxRow20DataBit1":
                            cb.Checked = Convert.ToBoolean((pioDir >> 1) & 1);
                            break;
                        case "checkBoxRow20DataBit0":
                            cb.Checked = Convert.ToBoolean((pioDir >> 0) & 1);
                            break;
                    }
                }
                

            }
        }
        private static void setPioOut(byte pioOut, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(pioOut);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;


            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cb = (CheckBox)c;
                    switch (cb.Name)
                    {
                        case "checkBoxRow21DataBit5":
                            cb.Checked = Convert.ToBoolean((pioOut >> 5) & 1);
                            break;
                        case "checkBoxRow21DataBit4":
                            cb.Checked = Convert.ToBoolean((pioOut >> 4) & 1);
                            break;
                        case "checkBoxRow21DataBit3":
                            cb.Checked = Convert.ToBoolean((pioOut >> 3) & 1);
                            break;
                        case "checkBoxRow21DataBit2":
                            cb.Checked = Convert.ToBoolean((pioOut >> 2) & 1);
                            break;
                        case "checkBoxRow21DataBit1":
                            cb.Checked = Convert.ToBoolean((pioOut >> 1) & 1);
                            break;
                        case "checkBoxRow21DataBit0":
                            cb.Checked = Convert.ToBoolean((pioOut >> 0) & 1);
                            break;
                    }
                }
            }
        }
        private static void setPioIn(byte pioIn, Control textBoxHexValue, Control groupBoxVarData)
        {
            textBoxHexValue.Text = DataConverter.ByteToStringHEX(pioIn);

            GroupBox groupBoxStatus = (GroupBox)groupBoxVarData;

            foreach (Control c in groupBoxStatus.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cb = (CheckBox)c;
                    switch (cb.Name)
                    {
                        case "checkBoxRow22DataBit5":
                            cb.Checked = Convert.ToBoolean((pioIn >> 5) & 1);
                            break;
                        case "checkBoxRow22DataBit4":
                            cb.Checked = Convert.ToBoolean((pioIn >> 4) & 1);
                            break;
                        case "checkBoxRow22DataBit3":
                            cb.Checked = Convert.ToBoolean((pioIn >> 3) & 1);
                            break;
                        case "checkBoxRow22DataBit2":
                            cb.Checked = Convert.ToBoolean((pioIn >> 2) & 1);
                            break;
                        case "checkBoxRow22DataBit1":
                            cb.Checked = Convert.ToBoolean((pioIn >> 1) & 1);
                            break;
                        case "checkBoxRow22DataBit0":
                            cb.Checked = Convert.ToBoolean((pioIn >> 0) & 1);
                            break;
                    }
                }
            }
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
