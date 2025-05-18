using System;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

// Project -> NuGet package management -> Install Package -> System.IO.Ports

namespace ComSpeedTest
{
    public class Program
    {
        static readonly string ProgramName = "ComSpeedTest.exe";
        static readonly string Version = "v1.0";

        static DateTime startTime;
        static System.IO.Ports.SerialPort port;
        static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        static double[] data;

        struct Parameter
        {
            public Parameter()
            {
                dataLength = 1;
                count = 100;

                portName = null;
                baudRate = 115200; // 変調回数(1秒あたり)
                dataBits = 8; // 1変調あたりのbit数
                startBits = 1;
                stopBits = StopBits.One;
                parity = Parity.None;

                dtrEnable = true;
                rtsEnable = false;

                readTimeout = 1000;
                writeTimeout = 10000;

                debug = false;
                sync = true;
            }
            public bool sync;

            public bool debug;
            public int dataLength;
            public int count;

            public String portName;
            public int baudRate;

            public int dataBits;
            public int startBits;
            public StopBits stopBits;
            public Parity parity;

            public bool dtrEnable;
            public bool rtsEnable;


            public int readTimeout;
            public int writeTimeout;
        }
        static Parameter parameter = new Parameter();

        static void usage()
        {
            Console.WriteLine($"Usage: {ProgramName} <portname>");
            Console.WriteLine("Example: ComIO.exe COM1");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void getArg(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-h") || args[i].Equals("--help"))
                {
                    usage();
                    Environment.Exit(0);
                }
                else if (args[i].Equals("-v") || args[i].Equals("--version"))
                {
                    Console.WriteLine($"{ProgramName} {Version}");
                    Environment.Exit(0);
                }
                else if (args[i].Equals("--debug"))
                {
                    parameter.debug = true;
                }
                else if (args[i].Equals("--async"))
                {
                    Console.WriteLine("Asynchronous transfer enabled.");
                    parameter.sync = false;
                }
                else if (args[i].Equals("--dtr"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: DTR is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.dtrEnable = bool.Parse(args[i]);
                }
                else if (args[i].Equals("--rts"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: RTS is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.rtsEnable = bool.Parse(args[i]);
                }
                else if (args[i].Equals("-l") || args[i].Equals("--length"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Data length is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.dataLength = int.Parse(args[i]);
                }
                else if (args[i].Equals("-p") || args[i].Equals("--port"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Port name is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.portName = args[i];
                }
                else if(args[i].Equals("-c") || args[i].Equals("--count"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Count is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.count = int.Parse(args[i]);
                }
                else if (args[i].Equals("--databits"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Data bits is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.dataBits = int.Parse(args[i]);
                }
                else if (args[i].Equals("--parity"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Parity is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    switch (args[i])
                    {
                        case "none":
                            parameter.parity = Parity.None;
                            break;
                        case "even":
                            parameter.parity = Parity.Even;
                            break;
                        case "mark":
                            parameter.parity = Parity.Mark;
                            break;
                        case "odd":
                            parameter.parity = Parity.Odd;
                            break;
                        case "space":
                            parameter.parity = Parity.Space;
                            break;
                        default:
                            Console.WriteLine("Error: Invalid parity value. Please set none/even/mark/odd/space");
                            usage();
                            Environment.Exit(1);
                            break;
                    }
                }
                else if (args[i].Equals("--stopbits"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Stop bits is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    switch (args[i])
                    {
                        case "0":
                            parameter.stopBits = StopBits.None;
                            break;
                        case "1":
                            parameter.stopBits = StopBits.One;
                            break;
                        case "1.5":
                            parameter.stopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            parameter.stopBits = StopBits.Two;
                            break;
                        default:
                            Console.WriteLine("Error: Invalid stop bits value.");
                            usage();
                            Environment.Exit(1);
                            break;
                    }
                }
                else if (args[i].Equals("--startbits"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Data bits is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.startBits = int.Parse(args[i]);
                }
                else if (args[i].Equals("--readtimeout"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Read timeout is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.readTimeout = int.Parse(args[i]);
                }
                else if (args[i].Equals("--writetimeout"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Write timeout is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.writeTimeout = int.Parse(args[i]);
                }
                else if (args[i].Equals("-b") || args[i].Equals("--baudrate"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Error: Baud rate is required.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.baudRate = int.Parse(args[i]);
                    Console.WriteLine($"BaudRate : {parameter.baudRate}");
                }
                else if (args[i].StartsWith("-"))
                {
                    Console.WriteLine($"Unknown option: {args[i]}");
                    usage();
                    Environment.Exit(1);
                }
                else
                {
                    if (parameter.portName != null)
                    {
                        Console.WriteLine($"Error: Port name is already set to {parameter.portName}.");
                        usage();
                        Environment.Exit(1);
                    }
                    parameter.portName = args[i];
                }
            }
            if (parameter.portName == null)
            {
                Console.WriteLine("Error: Port name is required.");
                usage();
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine($"Port: {parameter.portName}");
            }
        }

        static int sent;
        static int rcvd;
        static void Main(string[] args)
        {
            Console.WriteLine($"Com port speed tester\r");
            getArg(args);

            int count = parameter.count;
            Random rand = new Random();

            port = new SerialPort()
            {
                BaudRate = parameter.baudRate, //変調回数(1秒あたり)
                DataBits = parameter.dataBits, // 1変調あたりのbit数
                StopBits = parameter.stopBits,
                Parity = parameter.parity,
                Handshake = Handshake.None,
                DtrEnable = parameter.dtrEnable,
                RtsEnable = parameter.rtsEnable,
                PortName = parameter.portName,
                ReadTimeout = parameter.readTimeout,
                WriteTimeout = parameter.writeTimeout,
            };
            port.DataReceived += Port_DataReceived;

            try
            {
                port.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }

            Byte[] buf = new byte[parameter.dataLength];
            for (int i = 0; i < buf.Length; i += 2)
            {
                buf[i] = (byte)(i & 0xFF);
            }


            Console.WriteLine($"Test start.");
            if(parameter.sync)
            {
                semaphore.Wait();
            }
            for (int i = 0; i < count; i++)
            {
                //Thread.Sleep((int)(rand.NextDouble() * 300 + 100)); // Wait for 0.1 - 0.3 seconds
                startTime = DateTime.Now;

                DateTime dateTime = DateTime.Now;
                for(int j = 0; j < parameter.dataLength; j++)
                {
                    port.Write(buf, j, 1);
                    sent += 1;
                    Console.Write($"Sent*{sent} Rcvd:{rcvd}\r");
                }
                /*
                int sent = 0;
                while(sent < parameter.dataLength)
                {
                    int len =  (parameter.dataLength - sent) > 10 ? 10 : (parameter.dataLength - sent);
                    port.Write(buf, sent, len);
                    sent += len;
                }
                */
                if (parameter.sync)
                {
                    semaphore.Wait();
                }
                if (failed)
                {
                    break;
                }
            }
            Thread.Sleep(1000); // Wait for 1 second
            Console.WriteLine($"\nTest completed.");
            Console.WriteLine($"Total time        : {total} ms");
            Console.WriteLine($"Average  latency  : {total / count} ms");
            Console.WriteLine($"Total byte sent   : {sent} bytes");
            Console.WriteLine($"Total byte rcvd   : {rcvd} bytes");
            Console.WriteLine($"Transfer rate     : {rcvd / total * 1000} Bytes/sec");
        }

        static bool failed = false;
        static double total = 0;

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buf = new byte[parameter.dataLength];
            int r = parameter.dataLength;
            int r2;

            TimeSpan timespan;
            try
            {
                while (true)
                {
                    r2 = port.Read(buf, 0, r);
                    r -= r2;
                    rcvd += r2;
                    Console.Write($"Sent:{sent} Rcvd*{rcvd}\r");
                    if (parameter.debug)
                    {
                        timespan = DateTime.Now - startTime;
                        Console.Write($"{r2}/{r}. ");
                        Console.WriteLine($"\n Time elapsed: {timespan.TotalMilliseconds} ms");
                    }
                    if (r == 0)
                    {
                        break;
                    }

                }
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                failed = true;
                if (parameter.sync)
                {
                    semaphore.Release();
                }
                return;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                failed = true;
                if (parameter.sync)
                {
                    semaphore.Release();
                }
                return;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                failed = true;
                if (parameter.sync)
                {
                    semaphore.Release();
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                failed = true;
                if (parameter.sync)
                {
                    semaphore.Release();
                }
                return;
            }
            timespan = DateTime.Now - startTime;
            if (parameter.debug)
            {
                Console.WriteLine($"\n Time elapsed: {timespan.TotalMilliseconds} ms\n");
            }
            total += timespan.TotalMilliseconds;
            if (parameter.sync)
            {
                semaphore.Release();
            }
        }
    }
}