using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace SBandSerialReader
{
    public partial class Form1 : Form
    {
        private const int deviceAddress = 0x80;
        private static volatile SerialPort serialPort;

        private TcpServer server = new TcpServer();
        private List<int> _clients = new List<int>();
        int ReceivedPackets = 0;
        int SentPackets = 0;

        private Control[] HexValueControls;
        private Control[] VarDataControls;

        private bool isReadingRegs = false;

        private static System.Timers.Timer aTimer;

        private Queue<List<byte>> rxFifo = new Queue<List<byte>>();
        private Queue<List<byte>> txFifo = new Queue<List<byte>>();

        private int _uiUpdateDepth = 0;

        private IDisposable BeginUiUpdate()
        {
            _uiUpdateDepth++;
            return new UiUpdateScope(() => _uiUpdateDepth--);
        }

        private bool IsUiUpdating => _uiUpdateDepth > 0;

        private sealed class UiUpdateScope : IDisposable
        {
            private readonly Action _onDispose;
            public UiUpdateScope(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }


        public void addToRxFifo(byte[] data)
        {
            rxFifo.Enqueue(new List<byte>(data));
            //изменить textBox'ы
        }
        public void removeFromRxFifo()
        {
            List<byte> data = rxFifo.Dequeue();
            //Сделать что-то с данными
            //изменить textBox'ы
        }

        public void addToTxFifo(byte[] data)
        {
            txFifo.Enqueue(new List<byte>(data));
            //изменить textBox'ы
        }
        public void removeFromTxFifo()
        {
            List<byte> data = txFifo.Dequeue();
            //Сделать что-то с данными
            //изменить textBox'ы
        }


        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort();
            serialPort.BaudRate = 115200;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;

            aTimer = new System.Timers.Timer(500);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            //aTimer.Enabled = true;

            panel1.Controls.Add(groupBox3);

            refreshSerialPorts();
            if(comboBoxSelectedPort.Items.Count != 0)
            {
                comboBoxSelectedPort.SelectedIndex = 0;
                serialPort.PortName = comboBoxSelectedPort.SelectedItem.ToString();
            }

            pictureBox1.BackColor = Color.OrangeRed;
            comboBoxBaudRate.SelectedIndex = 1;
            comboBoxParity.SelectedIndex = 0;
            comboBoxStopBits.SelectedIndex = 1;

            server.DataReceived += OnDataReceived;
            server.ClientConnected += id =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => _clients.Add(id)));
                    return;
                }
                _clients.Add(id);
            };

            server.ClientDisconnected += id =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => _clients.Remove(id)));
                    return;
                }
                _clients.Remove(id);
            };

            HexValueControls = new Control[] {
                textBoxRow0Value,
                textBoxRow1Value1, textBoxRow1Value2, textBoxRow1Value3, textBoxRow1Value4, textBoxRow1Value5,
                new Control(), //Это RESET, ничего нет
                textBoxRow3Value1, textBoxRow3Value2, textBoxRow3Value3, textBoxRow3Value4, textBoxRow3Value5,
                textBoxRow4Value1, textBoxRow4Value2, textBoxRow4Value3, textBoxRow4Value4, textBoxRow4Value5,
                textBoxRow5Value1, textBoxRow5Value2,
                textBoxRow6Value1, textBoxRow6Value2,
                textBoxRow7Value,
                textBoxRow8Value,
                textBoxRow9Value,
                textBoxRow10Value1, textBoxRow10Value2, textBoxRow10Value3, textBoxRow10Value4,
                textBoxRow11Value,
                textBoxRow12Value,
                textBoxRow13Value,
                textBoxRow14Value,
                textBoxRow15Value1, textBoxRow15Value2, textBoxRow15Value3,
                textBoxRow16Value,
                textBoxRow17Value,
                textBoxRow18Value,
                textBoxRow19Value,
                textBoxRow20Value,
                textBoxRow21Value,
                textBoxRow22Value,
                textBoxRow23Value,
                textBoxRow24Value
            };

            VarDataControls = new Control[] {
                textBoxRow0Data,
                textBoxRow1Data1, textBoxRow1Data2, textBoxRow1Data3, textBoxRow1Data4, textBoxRow1Data5,
                buttonRow2Data,
                textBoxRow3Data1, textBoxRow3Data2, textBoxRow3Data3, textBoxRow3Data4, textBoxRow3Data5,
                textBoxRow4Data1, textBoxRow4Data2, textBoxRow4Data3, textBoxRow4Data4, textBoxRow4Data5,
                textBoxRow5Data1, textBoxRow5Data2,
                textBoxRow6Data1, textBoxRow6Data2,
                groupBoxRow7Data,
                groupBoxRow8Data,
                groupBoxRow9Data,
                new Control(), new Control(), new Control(), textBoxRow10Data,
                textBoxRow11Data,
                comboBoxRow12Data,
                comboBoxRow13Data,
                comboBoxRow14Data,
                new Control(), new Control(), textBoxRow15Data,
                textBoxRow16Data,
                comboBoxRow17Data,
                textBoxRow18Data,
                comboBoxRow19Data,
                checkBoxRow20Data,
                checkBoxRow21Data,
                checkBoxRow22Data,
                new Control(),
                textBoxRow24Data
            };
        }

        private void OnDataReceived(int clientId, byte[] data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnDataReceived(clientId, data)));
                return;
            }

            // Здесь ты уже в UI-потоке
            ReceivedPackets++;
            labelServerReceived.Text = "Сообщений принято: " + ReceivedPackets;
            WriteTxBuffer(data);
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            byte[] read = CommandGenerator.RegisterRead(deviceAddress, 0, 41);
            if (serialPort.IsOpen)
            {
                serialPort.Write(read, 0, read.Length);
            }
        }



        public void WriteRegsAsync(byte startReg, byte[] regs)
        {
            isReadingRegs = true;
            using (BeginUiUpdate())
            {
                RegistersManagerUI.WriteRegisters(startReg, regs, HexValueControls, VarDataControls);
            }
            isReadingRegs = false;
        }

        public void SetReadFifo(byte[] data)
        {
            isReadingRegs = true;

            string hexData = DataConverter.ByteArrayToStringHEX(data);
            string asciiData = DataConverter.ByteArrayToStringASCII(data);

            textBoxRxBufferASCII.Text = asciiData;
            textBoxRxBufferHEX.Text = hexData;

            WriteTxToServer(data);

            isReadingRegs = false;
        }

        private async void WriteTxToServer(byte[] data)
        {
            if (server.IsRunning)
            {
                foreach (var id in _clients)
                    await server.SendAsync(id, data);

                SentPackets++;
                labelServerTransmitted.Text = "Сообщений отправлено: " + SentPackets;
            }
        }



        private Parity GetParity(string selectedP)
        {
            switch (selectedP)
            {
                case "None":
                    return Parity.None;
                case "Odd":
                    return Parity.Odd;
                case "Even":
                    return Parity.Even;
                case "Mark":
                    return Parity.Mark;
                case "Space":
                    return Parity.Space;
            }

            return Parity.None;
        }

        private StopBits GetStopBits(string selectedSB)
        {
            switch (double.Parse(selectedSB))
            {
                case 0:
                    return StopBits.None;
                case 1:
                    return StopBits.One;
                case 1.5:
                    return StopBits.OnePointFive;
                case 2:
                    return StopBits.Two;
            }

            return StopBits.None;
        }

        private void buttonRefreshPorts_Click(object sender, EventArgs e)
        {
            refreshSerialPorts();
        }

        private void refreshSerialPorts()
        {
            comboBoxSelectedPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            comboBoxSelectedPort.Items.AddRange(ports);
        }

        private void buttonConnectComPort_Click(object sender, EventArgs e)
        {
            if (!serialPort.IsOpen)
            {
                if (comboBoxSelectedPort.SelectedItem != null)
                {
                    serialPort.PortName = comboBoxSelectedPort.SelectedItem.ToString();
                    serialPort.Open();

                    SerialReader serialReader = new SerialReader(serialPort, this);
                    Thread readingThread = new Thread(serialReader.ReadBytes);
                    readingThread.IsBackground = true;
                    readingThread.Start();

                    pictureBox1.BackColor = Color.Green;
                    buttonConnectComPort.Text = "Отключиться";
                    labelConnectionStatus.Text = "Подключён к " + serialPort.PortName;
                }
                else
                {
                    MessageBox.Show("Не выбран COM Port!");
                }
            }
            else
            {
                serialPort.Close();

                pictureBox1.BackColor = Color.OrangeRed;
                buttonConnectComPort.Text = "Подключиться";
                labelConnectionStatus.Text = "Отключён";
            }
        }

        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.BaudRate = int.Parse(comboBoxBaudRate.SelectedItem.ToString());
        }

        private void comboBoxParity_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.Parity = GetParity(comboBoxParity.SelectedItem.ToString());
        }

        private void numericUpDownDataBits_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownDataBits.Value >= 5 && numericUpDownDataBits.Value <= 8)
            {
                serialPort.DataBits = (int)numericUpDownDataBits.Value;
            }
        }

        private void comboBoxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.StopBits = GetStopBits(comboBoxStopBits.SelectedItem.ToString());
        }




        private void controlsHexValueChanged(RegMap reg)
        {
            try
            {
                if (HexValueControls[(int)reg].Text.Length == 0 || HexValueControls[(int)reg].Text.Length > 2)
                {
                    throw new Exception();
                }

                string originHexData = HexValueControls[(int)reg].Text;
                string createdVarValue = DataConverter.HEXStringToASCIIString(HexValueControls[(int)reg].Text);

                if (createdVarValue == "?" && originHexData != "3F" || originHexData == "00")
                {
                    HexValueControls[(int)reg].BackColor = Color.White;
                    VarDataControls[(int)reg].BackColor = Color.BlueViolet;
                    VarDataControls[(int)reg].Text = "";
                }
                else
                {
                    VarDataControls[(int)reg].Text =
                        DataConverter.HEXStringToASCIIString(HexValueControls[(int)reg].Text);
                    HexValueControls[(int)reg].BackColor = Color.White;
                    VarDataControls[(int)reg].BackColor = Color.White;
                }




                if(HexValueControls[(int)reg].BackColor == Color.White && !isReadingRegs)
                {
                    //Здесь отправить в serialPort НО не забыть проверку на open
                    byte[] read = CommandGenerator.RegisterWrite(deviceAddress, (byte)reg, 1,
                        DataConverter.HEXStringToByteArray(originHexData));
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(read, 0, read.Length);
                    }
                }
            }
            catch
            {
                HexValueControls[(int)reg].BackColor = Color.OrangeRed;
            }
        }

        private void controlsVarDataChanged(RegMap reg)
        {
            try
            {
                if (VarDataControls[(int)reg].Text.Length > 1)
                {
                    throw new Exception();
                }

                string createdHexValue = DataConverter.ASCIIStringToHexString(VarDataControls[(int)reg].Text);
                string currentHexValue = HexValueControls[(int)reg].Text;

                if(VarDataControls[(int)reg].BackColor == Color.BlueViolet && VarDataControls[(int)reg].Text == "")
                {
                    return;
                }

                if (createdHexValue != currentHexValue)
                {
                    HexValueControls[(int)reg].Text = createdHexValue;
                }
            }
            catch
            {
                VarDataControls[(int)reg].BackColor = Color.OrangeRed;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            byte[] read = CommandGenerator.RegisterRead(deviceAddress, 0, 41);
            if (serialPort.IsOpen)
            {
                serialPort.Write(read, 0, read.Length);
            }
        }


        private void textBoxRow3Value1_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.TransmitAdress1);
        }

        private void textBoxRow3Data1_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.TransmitAdress1);
        }

        private void textBoxRow3Value2_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.TransmitAdress2);
        }

        private void textBoxRow3Data2_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.TransmitAdress2);
        }

        private void textBoxRow3Value3_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.TransmitAdress3);
        }

        private void textBoxRow3Data3_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.TransmitAdress3);
        }

        private void textBoxRow3Value4_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.TransmitAdress4);
        }

        private void textBoxRow3Data4_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.TransmitAdress4);
        }

        private void textBoxRow3Value5_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.TransmitAdress5);
        }

        private void textBoxRow3Data5_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.TransmitAdress5);
        }

        private void textBoxRow4Value1_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.ReceiveAdress1);
        }

        private void textBoxRow4Data1_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.ReceiveAdress1);
        }

        private void textBoxRow4Value2_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.ReceiveAdress2);
        }

        private void textBoxRow4Data2_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.ReceiveAdress2);
        }

        private void textBoxRow4Value3_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.ReceiveAdress3);
        }

        private void textBoxRow4Data3_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.ReceiveAdress3);
        }

        private void textBoxRow4Value4_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.ReceiveAdress4);
        }

        private void textBoxRow4Data4_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.ReceiveAdress4);
        }

        private void textBoxRow4Value5_TextChanged(object sender, EventArgs e)
        {
            controlsHexValueChanged(RegMap.ReceiveAdress5);
        }

        private void textBoxRow4Data5_TextChanged(object sender, EventArgs e)
        {
            controlsVarDataChanged(RegMap.ReceiveAdress5);
        }


        private void textBoxTxBufferHEX_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string data = DataConverter.HEXStringToASCIIString(textBoxTxBufferHEX.Text);
                textBoxTxBufferHEX.BackColor = Color.White;

                if (data != textBoxTxBufferASCII.Text)
                {
                    if (!data.Contains("?") && !data.Contains("\0"))
                    {
                        textBoxTxBufferASCII.Text = data;
                    }
                    else
                    {
                        textBoxTxBufferASCII.Text = "";
                        textBoxTxBufferASCII.BackColor = Color.BlueViolet;
                    }
                }
            }
            catch (Exception)
            {
                textBoxTxBufferHEX.BackColor = Color.OrangeRed;
            }
        }

        private void textBoxTxBufferASCII_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string data = DataConverter.ASCIIStringToHexString(textBoxTxBufferASCII.Text);
                textBoxTxBufferASCII.BackColor = Color.White;

                if (data != textBoxTxBufferHEX.Text && data != "")
                {
                    textBoxTxBufferHEX.Text = data;
                }
            }
            catch (Exception)
            {
                textBoxTxBufferASCII.BackColor = Color.OrangeRed;
            }
        }

        private void buttonWriteTxBuffer_Click(object sender, EventArgs e)
        {
            byte[] data = DataConverter.HEXStringToByteArray(textBoxTxBufferHEX.Text);
            byte[] write = CommandGenerator.FifoWrite(deviceAddress, data);
            if (serialPort.IsOpen)
            {
                serialPort.Write(write, 0, write.Length);
            }
        }

        private void WriteTxBuffer(byte[] data)
        {
            byte[] write = CommandGenerator.FifoWrite(deviceAddress, data);
            if (serialPort.IsOpen)
            {
                serialPort.Write(write, 0, write.Length);
            }
        }

        private void checkBoxReadOnly_CheckChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            CheckBox checkBox = sender as CheckBox;
            isReadingRegs = true;
            checkBox.Checked = !checkBox.Checked;
            isReadingRegs = false;
        }



        private void ChangeConfig(bool isChecked, int bitIndex)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            try
            {
                byte currentConfig = 0;
                if (HexValueControls[(int)RegMap.Config].Text != "")
                {
                    currentConfig = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Config].Text);
                }
                byte bitStatus = Convert.ToByte(isChecked);

                byte mask = (byte)(1 << bitIndex);
                currentConfig = (byte)((currentConfig & ~mask) | ((bitStatus << bitIndex) & mask));

                if (bitIndex != 0 && bitIndex != 1)
                {
                    HexValueControls[(int)RegMap.Config].Text = DataConverter.ByteToStringHEX(currentConfig);
                }

                if (serialPort.IsOpen)
                {
                    byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (int)RegMap.Config, 1, new byte[] { currentConfig });
                    serialPort.Write(cmd, 0, cmd.Length);
                }
            }
            catch { }
        }
        private void checkBoxRow9DataBit7_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig((sender as CheckBox).Checked, 7);
        }

        private void checkBoxRow9DataBit6_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig((sender as CheckBox).Checked, 6);
        }

        private void checkBoxRow9DataBit4_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig((sender as CheckBox).Checked, 4);
        }

        private void checkBoxRow9DataBit3_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig((sender as CheckBox).Checked, 3);
        }

        private void checkBoxRow9DataBit2_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig((sender as CheckBox).Checked, 2);
        }

        private void buttonRow9DataBit1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig(true, 1);
        }

        private void buttonRow9DataBit0_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConfig(true, 0);
        }

        private void radioButtonRow9DataBitLora_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRow9DataBitLora.Checked)
            {
                ChangeConfig(true, 5);
            }
        }

        private void radioButtonRow9DataBitFsk_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRow9DataBitFsk.Checked)
            {
                ChangeConfig(false, 5);
            }
        }

        private void textBoxRow10Data_TextChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            if (!uint.TryParse(VarDataControls[(int)RegMap.Frequency4].Text, out uint frequency))
            {
                VarDataControls[(int)RegMap.Frequency4].BackColor = Color.OrangeRed;
                return;
            }
            VarDataControls[(int)RegMap.Frequency4].BackColor = Color.White;

            string freqData1 = DataConverter.ByteToStringHEX((byte)(frequency >> 24));
            string freqData2 = DataConverter.ByteToStringHEX((byte)(frequency >> 16));
            string freqData3 = DataConverter.ByteToStringHEX((byte)(frequency >> 8));
            string freqData4 = DataConverter.ByteToStringHEX((byte)(frequency));

            HexValueControls[(int)RegMap.Frequency1].Text = freqData1;
            HexValueControls[(int)RegMap.Frequency2].Text = freqData2;
            HexValueControls[(int)RegMap.Frequency3].Text = freqData3;
            HexValueControls[(int)RegMap.Frequency4].Text = freqData4;
        }

        private void setFrequency()
        {
            byte freqByte1 = 0;
            byte freqByte2 = 0;
            byte freqByte3 = 0;
            byte freqByte4 = 0;
            bool isError = false;

            try
            {
                freqByte1 = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Frequency1].Text);
                HexValueControls[(int)RegMap.Frequency1].BackColor = Color.White;
            }
            catch
            {
                HexValueControls[(int)RegMap.Frequency1].BackColor = Color.OrangeRed;
                isError = true;
            }
            try
            {
                freqByte2 = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Frequency2].Text);
                HexValueControls[(int)RegMap.Frequency2].BackColor = Color.White;
            }
            catch
            {
                HexValueControls[(int)RegMap.Frequency2].BackColor = Color.OrangeRed;
                isError = true;
            }
            try
            {
                freqByte3 = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Frequency3].Text);
                HexValueControls[(int)RegMap.Frequency3].BackColor = Color.White;
            }
            catch
            {
                HexValueControls[(int)RegMap.Frequency3].BackColor = Color.OrangeRed;
                isError = true;
            }
            try
            {
                freqByte4 = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Frequency4].Text);
                HexValueControls[(int)RegMap.Frequency4].BackColor = Color.White;
            }
            catch
            {
                HexValueControls[(int)RegMap.Frequency4].BackColor = Color.OrangeRed;
                isError = true;
            }

            if (!isError)
            {
                byte[] freqData = new byte[] { freqByte4, freqByte3, freqByte2, freqByte1 };
                int frequency = BitConverter.ToInt32(freqData, 0);

                int.TryParse(VarDataControls[(int)RegMap.Frequency4].Text, out int currentFrequency);

                if (frequency != currentFrequency)
                {
                    VarDataControls[(int)RegMap.Frequency4].Text = frequency.ToString();

                    System.Windows.Forms.TextBox freqDataTextBox = (System.Windows.Forms.TextBox)VarDataControls[(int)RegMap.Frequency4];
                    freqDataTextBox.SelectionStart = freqDataTextBox.Text.Length;
                }
            }
        }

        private void checkFreqByte(RegMap reg)
        {
            try
            {
                if (HexValueControls[(int)reg].Text.Length == 0 || HexValueControls[(int)reg].Text.Length > 2)
                {
                    throw new Exception();
                }
                byte byteToWrite = DataConverter.HEXStringToByte(HexValueControls[(int)reg].Text);

                if (!isReadingRegs)
                {
                    //Здесь отправить в serialPort НО не забыть проверку на open
                    byte[] read = CommandGenerator.RegisterWrite(deviceAddress, (byte)reg, 1, 
                        DataConverter.HEXStringToByteArray(HexValueControls[(int)reg].Text));
                    //if (serialPort.IsOpen)
                    
                        serialPort.Write(read, 0, read.Length);
                    
                }

                setFrequency();

                HexValueControls[(int)reg].BackColor = Color.White;
            }
            catch
            {
                HexValueControls[(int)reg].BackColor = Color.OrangeRed;
            }
        }

        private void textBoxRow10Value1_TextChanged(object sender, EventArgs e)
        {
            //checkFreqByte(RegMap.Frequency1);
        }

        private void textBoxRow10Value2_TextChanged(object sender, EventArgs e)
        {
            //checkFreqByte(RegMap.Frequency2);
        }

        private void textBoxRow10Value3_TextChanged(object sender, EventArgs e)
        {
            //checkFreqByte(RegMap.Frequency3);
        }

        private void textBoxRow10Value4_TextChanged(object sender, EventArgs e)
        {
            //checkFreqByte(RegMap.Frequency4);
        }

        private void textBoxRow11Value_TextChanged(object sender, EventArgs e)
        {
            if (VarDataControls[(int)RegMap.Power].Text == "")
            {
                isReadingRegs = true;
                VarDataControls[(int)RegMap.Power].Text = "0";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }

            //тут 0;32
            try
            {
                int value = DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Power].Text);

                if (value < 0 || value > 32)
                {
                    HexValueControls[(int)RegMap.Power].BackColor = Color.OrangeRed;
                    return;
                }
                HexValueControls[(int)RegMap.Power].BackColor = Color.White;

                if (!isPowerEqual(value, int.Parse(VarDataControls[(int)RegMap.Power].Text)))
                {
                    VarDataControls[(int)RegMap.Power].Text = (value - 10).ToString();
                }

                if (!isReadingRegs)
                {
                    //Здесь отправить в serialPort НО не забыть проверку на open
                    if (serialPort.IsOpen)
                    {
                        byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Power, 1, new byte[] {(byte)value});
                    }
                }
            }
            catch
            {
                HexValueControls[(int)RegMap.Power].BackColor = Color.OrangeRed;
            }
        }

        private void textBoxRow11Data_TextChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.Power].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.Power].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            //тут -10;22
            try
            {
                int data = int.Parse(VarDataControls[(int)RegMap.Power].Text);

                if (data < -10 || data > 22)
                {
                    VarDataControls[(int)RegMap.Power].BackColor = Color.OrangeRed;
                    return;
                }
                VarDataControls[(int)RegMap.Power].BackColor = Color.White;

                if (!isPowerEqual(DataConverter.HEXStringToByte(HexValueControls[(int)RegMap.Power].Text), data))
                {
                    HexValueControls[(int)RegMap.Power].Text = DataConverter.ByteToStringHEX((byte)(data + 10));
                }
            }
            catch
            {
                VarDataControls[(int)RegMap.Power].BackColor = Color.OrangeRed;
            }
        }

        private bool isPowerEqual(int value, int data)
        {
            int diff = 10;
            return value - diff == data;
        }

        private void comboBoxRow12Data_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }


            byte spreadingFactor;

            switch (comboBoxRow12Data.SelectedIndex)
            {
                case 0:
                    spreadingFactor = 5;
                    break;
                case 1:
                    spreadingFactor = 6;
                    break;
                case 2:
                    spreadingFactor = 7;
                    break;
                case 3:
                    spreadingFactor = 8;
                    break;
                case 4:
                    spreadingFactor = 9;
                    break;
                case 5:
                    spreadingFactor = 10;
                    break;
                case 6:
                    spreadingFactor = 11;
                    break;
                case 7:
                    spreadingFactor = 12;
                    break;
                default:
                    spreadingFactor = 7;
                    break;
            }

            HexValueControls[(int)RegMap.LoraSpreadingFactor].Text = DataConverter.ByteToStringHEX(spreadingFactor);

            if (!isReadingRegs)
            {
                //Здесь отправить в serialPort НО не забыть проверку на open
                if (serialPort.IsOpen)
                {
                    byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.LoraSpreadingFactor, 1, new byte[] { spreadingFactor });
                    serialPort.Write(data, 0, data.Length);
                }
            }
        }

        private void comboBoxRow13Data_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            byte codingRate;

            switch (comboBoxRow13Data.SelectedIndex)
            {
                case 0:
                    codingRate = 0x01;
                    break;
                case 1:
                    codingRate = 0x02;
                    break;
                case 2:
                    codingRate = 0x03;
                    break;
                case 3:
                    codingRate = 0x04;
                    break;
                default:
                    codingRate = 0x01;
                    break;
            }

            HexValueControls[(int)RegMap.LoraCodingRate].Text = DataConverter.ByteToStringHEX(codingRate);

            if (!isReadingRegs)
            {
                //Здесь отправить в serialPort НО не забыть проверку на open
                if (serialPort.IsOpen)
                {
                    byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.LoraCodingRate, 1, new byte[] { codingRate });
                    serialPort.Write(data, 0, data.Length);
                }
            }
        }

        private void comboBoxRow14Data_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            byte bandwidth;

            switch (comboBoxRow14Data.SelectedIndex)
            {
                case 0:
                    bandwidth = 0x00;
                    break;
                case 1:
                    bandwidth = 0x08;
                    break;
                case 2:
                    bandwidth = 0x01;
                    break;
                case 3:
                    bandwidth = 0x09;
                    break;
                case 4:
                    bandwidth = 0x02;
                    break;
                case 5:
                    bandwidth = 0x0A;
                    break;
                case 6:
                    bandwidth = 0x03;
                    break;
                case 7:
                    bandwidth = 0x04;
                    break;
                case 8:
                    bandwidth = 0x05;
                    break;
                case 9:
                    bandwidth = 0x06;
                    break;
                default:
                    bandwidth = 0x06;
                    break;
            }

            HexValueControls[(int)RegMap.LoraBandwidth].Text = DataConverter.ByteToStringHEX(bandwidth);

            if (!isReadingRegs)
            {
                //Здесь отправить в serialPort НО не забыть проверку на open
                if (serialPort.IsOpen)
                {
                    byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.LoraBandwidth, 1, new byte[] { bandwidth });
                    serialPort.Write(data, 0, data.Length);
                }
            }
        }

        private void buttonReadRxBuffer_Click(object sender, EventArgs e)
        {
            byte[] read = CommandGenerator.FifoRead(deviceAddress);
            if (serialPort.IsOpen)
            {
                serialPort.Write(read, 0, read.Length);
            }
        }

        private void buttonSetFrequency_Click(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            if (!uint.TryParse(VarDataControls[(int)RegMap.Frequency4].Text, out uint frequency))
            {
                VarDataControls[(int)RegMap.Frequency4].BackColor = Color.OrangeRed;
                return;
            }
            VarDataControls[(int)RegMap.Frequency4].BackColor = Color.White;

            byte[] freq = new byte[] { (byte)(frequency >> 24),
                                       (byte)(frequency >> 16),
                                       (byte)(frequency >> 8),
                                       (byte)(frequency) };

            if (serialPort.IsOpen)
            {
                byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Frequency1, 4, freq);
                serialPort.Write(cmd, 0, cmd.Length);
            }
        }

        private void buttonRow2Data_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Reset, 1, new byte[] { 0xAA });
                serialPort.Write(cmd, 0, cmd.Length);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Reset, 1, new byte[] { 0xBB });
                serialPort.Write(cmd, 0, cmd.Length);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Reset, 1, new byte[] { 0xCC });
                serialPort.Write(cmd, 0, cmd.Length);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }
            if(textBoxRow24Value.BackColor != Color.White)
            {
                return;
            }

            byte[] key = HexToFixed16Bytes(textBoxRow24Value.Text);


            if (serialPort.IsOpen)
            {
                byte[] cmd = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.Frequency1, 16, key);
                serialPort.Write(cmd, 0, cmd.Length);
                textBoxRow24Data.Text = "";
                textBoxRow24Value.Text = "";
            }
        }

        private void textBoxRow24Data_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string data = DataConverter.ASCIIStringToHexString(textBoxRow24Data.Text);
                textBoxRow24Data.BackColor = Color.White;

                if (data != textBoxRow24Value.Text && data != "")
                {
                    textBoxRow24Value.Text = data;
                }
            }
            catch (Exception)
            {
                textBoxRow24Data.BackColor = Color.OrangeRed;
            }
        }

        public byte[] HexToFixed16Bytes(string hex)
        {
            byte[] result = new byte[16];

            int byteCount = hex.Length / 2;

            for (int i = 0; i < byteCount; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return result;
        }

        private void textBoxRow24Value_TextChanged(object sender, EventArgs e)
        {
            if(textBoxRow24Value.Text.Length > 32)
            {
                textBoxRow24Value.BackColor = Color.OrangeRed;
                return;
            }

            try
            {
                string data = DataConverter.HEXStringToASCIIString(textBoxRow24Value.Text);
                textBoxRow24Value.BackColor = Color.White;

                if (data != textBoxRow24Data.Text)
                {
                    if (!data.Contains("?") && !data.Contains("\0"))
                    {
                        textBoxRow24Data.Text = data;
                    }
                    else
                    {
                        textBoxRow24Data.Text = "";
                        textBoxRow24Data.BackColor = Color.BlueViolet;
                    }
                }
            }
            catch (Exception)
            {
                textBoxRow24Value.BackColor = Color.OrangeRed;
            }
        }

        private async void buttonOpenConnect_Click(object sender, EventArgs e)
        {
            await server.StartAsync("0.0.0.0", 8924);
            labelServerStatus.Text = "Сервер включён";
        }

        private void buttonCloseConnect_Click(object sender, EventArgs e)
        {
            server.Stop();
            labelServerStatus.Text = "Сервер отключён";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0xAA, 0xBB };

            SetReadFifo(data);
        }

        private void comboBoxRow17Data_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            byte bandwidth;

            switch (comboBoxRow17Data.SelectedIndex)
            {
                case 0:
                    bandwidth = 0x1F;
                    break;
                case 1:
                    bandwidth = 0x17;
                    break;
                case 2:
                    bandwidth = 0x0F;
                    break;
                case 3:
                    bandwidth = 0x1E;
                    break;
                case 4:
                    bandwidth = 0x16;
                    break;
                case 5:
                    bandwidth = 0x0E;
                    break;
                case 6:
                    bandwidth = 0x1D;
                    break;
                case 7:
                    bandwidth = 0x15;
                    break;
                case 8:
                    bandwidth = 0x0D;
                    break;
                case 9:
                    bandwidth = 0x1C;
                    break;
                case 10:
                    bandwidth = 0x14;
                    break;
                case 11:
                    bandwidth = 0x0C;
                    break;
                case 12:
                    bandwidth = 0x1B;
                    break;
                case 13:
                    bandwidth = 0x13;
                    break;
                case 14:
                    bandwidth = 0x0B;
                    break;
                case 15:
                    bandwidth = 0x1A;
                    break;
                case 16:
                    bandwidth = 0x12;
                    break;
                case 17:
                    bandwidth = 0x0A;
                    break;
                case 18:
                    bandwidth = 0x19;
                    break;
                case 19:
                    bandwidth = 0x11;
                    break;
                case 20:
                    bandwidth = 0x09;
                    break;
                default:
                    bandwidth = 0x09;
                    break;
            }

            HexValueControls[(int)RegMap.FskBandwidth].Text = DataConverter.ByteToStringHEX(bandwidth);

            if (!isReadingRegs)
            {
                //Здесь отправить в serialPort НО не забыть проверку на open
                if (serialPort.IsOpen)
                {
                    byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.LoraBandwidth, 1, new byte[] { bandwidth });
                    serialPort.Write(data, 0, data.Length);
                }
            }

        }

        private void textBoxRow15Data_TextChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            if (!uint.TryParse(VarDataControls[(int)RegMap.BitRate3].Text, out uint frequency))
            {
                VarDataControls[(int)RegMap.BitRate3].BackColor = Color.OrangeRed;
                return;
            }
            VarDataControls[(int)RegMap.BitRate3].BackColor = Color.White;

            string freqData1 = DataConverter.ByteToStringHEX((byte)(frequency >> 16));
            string freqData2 = DataConverter.ByteToStringHEX((byte)(frequency >> 8));
            string freqData3 = DataConverter.ByteToStringHEX((byte)(frequency));

            HexValueControls[(int)RegMap.BitRate1].Text = freqData1;
            HexValueControls[(int)RegMap.BitRate2].Text = freqData2;
            HexValueControls[(int)RegMap.BitRate3].Text = freqData3;
        }

        private void textBoxRow16Data_TextChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.FskFdev].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.FskFdev].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            //тут 0;200
            try
            {
                int data = int.Parse(VarDataControls[(int)RegMap.FskFdev].Text);

                if (data < 0 || data > 201)
                {
                    VarDataControls[(int)RegMap.FskFdev].BackColor = Color.OrangeRed;
                    return;
                }
                VarDataControls[(int)RegMap.FskFdev].BackColor = Color.White;


                HexValueControls[(int)RegMap.FskFdev].Text = DataConverter.ByteToStringHEX((byte)(data));

            }
            catch
            {
                VarDataControls[(int)RegMap.FskFdev].BackColor = Color.OrangeRed;
            }
        }

        private void textBoxRow18Data_TextChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.FskPreambleLength].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.FskPreambleLength].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            //тут 0;255
            try
            {
                int data = int.Parse(VarDataControls[(int)RegMap.FskPreambleLength].Text);

                if (data < 0 || data > 255)
                {
                    VarDataControls[(int)RegMap.FskPreambleLength].BackColor = Color.OrangeRed;
                    return;
                }
                VarDataControls[(int)RegMap.FskPreambleLength].BackColor = Color.White;


                HexValueControls[(int)RegMap.FskPreambleLength].Text = DataConverter.ByteToStringHEX((byte)(data));

            }
            catch
            {
                VarDataControls[(int)RegMap.FskPreambleLength].BackColor = Color.OrangeRed;
            }
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxRow19Data_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            byte bandwidth;

            switch (comboBoxRow19Data.SelectedIndex)
            {
                case 0:
                    bandwidth = 0x00;
                    break;
                case 1:
                    bandwidth = 0x08;
                    break;
                case 2:
                    bandwidth = 0x09;
                    break;
                case 3:
                    bandwidth = 0x0A;
                    break;
                case 4:
                    bandwidth = 0x0B;
                    break;
                default:
                    bandwidth = 0x00;
                    break;
            }

            HexValueControls[(int)RegMap.FskGaussFilter].Text = DataConverter.ByteToStringHEX(bandwidth);

            if (!isReadingRegs)
            {
                //Здесь отправить в serialPort НО не забыть проверку на open
                if (serialPort.IsOpen)
                {
                    byte[] data = CommandGenerator.RegisterWrite(deviceAddress, (byte)RegMap.FskGaussFilter, 1, new byte[] { bandwidth });
                    serialPort.Write(data, 0, data.Length);
                }
            }

        }

        private void checkBoxRow20Data_CheckedChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.PioDir].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.PioDir].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            HexValueControls[(int)RegMap.PioDir].Text= ((CheckBox)sender).Checked?"1":"0";
        }

        private void checkBoxRow21Data_CheckedChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.PioOut].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.PioOut].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            HexValueControls[(int)RegMap.PioOut].Text = ((CheckBox)sender).Checked ? "1" : "0";
        }

        private void checkBoxRow22Data_CheckedChanged(object sender, EventArgs e)
        {
            if (HexValueControls[(int)RegMap.PioIn].Text == "")
            {
                isReadingRegs = true;
                HexValueControls[(int)RegMap.PioIn].Text = "00";
                isReadingRegs = false;
            }

            if (isReadingRegs)
            {
                return;
            }
            if (IsUiUpdating)
            {
                return;
            }

            HexValueControls[(int)RegMap.PioIn].Text = ((CheckBox)sender).Checked ? "1" : "0";
        }
    }
}
