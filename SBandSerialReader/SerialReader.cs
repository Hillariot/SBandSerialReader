using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    internal class SerialReader
    {
        private volatile SerialPort serialPort;
        private Form1 form1;
        private CancellationToken cst;

        public SerialReader(SerialPort serialPort, Form1 form1, CancellationToken cst)
        {
            this.serialPort = serialPort;
            this.form1 = form1;
            this.cst = cst;
        }

        public void ReadBytes()
        {
            while (!cst.IsCancellationRequested)
            {
                if (serialPort.IsOpen)
                {
                    var ports = SerialPort.GetPortNames();
                    if (!ports.Contains(serialPort.PortName))
                    {
                        form1.BeginInvoke(new Action(() =>
                        {
                            form1.OnPortDisconnected();
                            form1.refreshSerialPorts();
                        }));
                    }


                    try
                    {
                        int bytesToRead = serialPort.BytesToRead;
                        int startBytesToRead;
                        if (bytesToRead > 0)
                        {
                            do
                            {
                                startBytesToRead = bytesToRead;
                                Thread.Sleep(20);
                                bytesToRead = serialPort.BytesToRead;
                            }
                            while (bytesToRead != startBytesToRead);


                            byte[] bytes = new byte[bytesToRead];
                            int bytesRead = serialPort.Read(bytes, 0, bytesToRead);

                            if (bytesRead > 0)
                            {
                                byte cmd = bytes[1];
                                byte startReg = bytes[2];
                                byte length = bytes[3];

                                if (cmd == 0x01)
                                {
                                    form1.BeginInvoke(new Action(() =>
                                    {
                                        byte[] data = new byte[length];
                                        Array.Copy(bytes, 4, data, 0, data.Length);
                                        form1.WriteRegsAsync(startReg, data);
                                    }));
                                }

                                else if (cmd == 0x00)
                                {
                                    form1.BeginInvoke(new Action(() =>
                                    {
                                        byte[] data = new byte[bytes[1] - 2];
                                        Array.Copy(bytes, 5, data, 0, data.Length);
                                        form1.WriteRegsAsync(startReg, data);
                                    }));
                                }

                                else if (cmd == 0x03)
                                {
                                    form1.BeginInvoke(new Action(() =>
                                    {
                                        byte[] data = new byte[bytes[2]];
                                        Array.Copy(bytes, 3, data, 0, data.Length);
                                        form1.SetReadFifo(data);
                                    }));
                                }
                            }
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Порт внезапно пропал во время чтения");
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("Попытка читать закрытый порт");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Попытка читать закрытый порт");
                    }
                }
                else
                {
                    form1.BeginInvoke(new Action(() =>
                    {
                        form1.OnPortDisconnected();
                        form1.refreshSerialPorts();
                    }));
                    return;
                }
            }
        }
    }
}
