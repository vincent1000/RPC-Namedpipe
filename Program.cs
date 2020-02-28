using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace testPipeServer
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern uint GetLastError();
        int MAX = 100;
        static StreamReader reader;
        static private BlockingCollection<int> msgCollection;
        static bool isRunning = false;
        public enum EventTag
        {
            T_RangeMin,
            T_VoiceTrigger = 0x01,
            T_ButtonTrigger,
            T_StartListen,
            T_ActiveListen,
            T_EndListen,
            T_ASR,
            T_TTS,
            T_IDLE,
            T_TEXTRES,
            T_Reset,
            T_RangeMax
        };
        static public bool ReadMsg()
        {
            int len;
            char[] buf = new char[3];
            int cnt = reader.Read(buf, 0, 3);
            if (cnt == 0)
            {
                return false;
            }
            else
            {
                EventTag tag = (EventTag)buf[0];
                switch (tag)
                {
                    case EventTag.T_VoiceTrigger:
                        Console.WriteLine("T_VoiceTrigger");
                        break;
                    case EventTag.T_ButtonTrigger:
                        Console.WriteLine("T_ButtonTrigger");
                        break;
                    case EventTag.T_StartListen:
                        Console.WriteLine("T_StartListen");
                        break;
                    case EventTag.T_ActiveListen:
                        Console.WriteLine("T_ActiveListen");
                        break;
                    case EventTag.T_EndListen:
                        Console.WriteLine("T_EndListen");
                        break;
                    case EventTag.T_ASR:
                        Console.WriteLine("T_ASR");
                        break;
                    case EventTag.T_TTS:
                        Console.WriteLine("T_TTS");
                        break;
                    case EventTag.T_IDLE:
                        Console.WriteLine("T_IDLE");
                        break;
                    case EventTag.T_TEXTRES:
                        Console.WriteLine("T_TEXTRES");
                        len = buf[1];// (buf[1] << 8)&0xff00 + buf[2]&0xff;
                        len = (len << 8) & 0xff00;
                        len = len + buf[2];
                        char[] msg = new char[len];
                        cnt = reader.Read(msg, 0, len);
                        byte[] byteData = Encoding.Default.GetBytes(msg);
                        string str = Encoding.Unicode.GetString(byteData);
                        
                        Console.WriteLine(str);
                        break;
                    case EventTag.T_Reset:
                        Console.WriteLine("T_Reset");
                        break;
                    default:
                        Console.WriteLine("Wrong");
                        break;
                }
            }
            return true;
        }

        static public void FirstThread()
        {
            char[] buf = new char[10];
            msgCollection.Take();
            while (true)
            {
                int cnt = reader.Read(buf, 0, 3);
                if (cnt == 0)
                {
                    isRunning = false;
                    msgCollection.Take();
                }
                else
                {
                    EventTag tag = (EventTag)buf[0];
                    switch (tag)
                    {
                        case EventTag.T_VoiceTrigger:
                            Console.WriteLine("T_VoiceTrigger");
                            break;
                        case EventTag.T_ButtonTrigger:
                            Console.WriteLine("T_ButtonTrigger");
                            break;
                        case EventTag.T_StartListen:
                            Console.WriteLine("T_StartListen");
                            break;
                        case EventTag.T_ActiveListen:
                            Console.WriteLine("T_ActiveListen");
                            break;
                        case EventTag.T_EndListen:
                            Console.WriteLine("T_EndListen");
                            break;
                        case EventTag.T_ASR:
                            Console.WriteLine("T_ASR");
                            break;
                        case EventTag.T_TTS:
                            Console.WriteLine("T_TTS");
                            break;
                        case EventTag.T_IDLE:
                            Console.WriteLine("T_IDLE");
                            break;
                        case EventTag.T_TEXTRES:
                            Console.WriteLine("T_TEXTRES");
                            int len = buf[1] << 8 + buf[2];
                            cnt = reader.Read(buf, 0, len);
                            break;
                        case EventTag.T_Reset:
                            Console.WriteLine("T_Reset");
                            break;
                        default:
                            Console.WriteLine("Wrong");
                            break;
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            PipeSecurity pipeSa = new PipeSecurity();
            pipeSa.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, System.Security.AccessControl.AccessControlType.Allow));
            pipeSa.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(@"S-1-15-2-1"), PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, System.Security.AccessControl.AccessControlType.Allow));
            msgCollection = new BlockingCollection<int>();
            // Create the named pipe 
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                "starpipe",                    // The unique pipe name. 
                PipeDirection.InOut,            // The pipe is bi-directional 
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,   // Message type pipe 
                PipeOptions.None,               // No additional parameters 
                2048,                    // Input buffer size 
                2048,                    // Output buffer size 
                pipeSa,                         // Pipe security attributes 
                HandleInheritability.None       // Not inheritable 
            );
            Console.WriteLine("Listening\n");
           // Thread tFirst = new Thread(new ThreadStart(FirstThread));
           // tFirst.Start();
CONNECTION:
            pipeServer.WaitForConnection();
            Console.WriteLine("[server] a client connected");
            Console.WriteLine("[server] receive message:" + pipeServer.IsConnected);
            pipeServer.Disconnect();
            goto CONNECTION;
            reader = new StreamReader(pipeServer);
            StreamWriter writer = new StreamWriter(pipeServer);
            writer.AutoFlush = true;
            string str = "server";
            char[] buf = new char[10];
            byte[] send = {0x01,0x00,0x00};
            int times = 0;
            isRunning = true;
            //msgCollection.Add(1);
            while (true)
            {
                //if(!isRunning)
                //{
                //    Console.WriteLine("Disconnect ,reconnecting");
                //    pipeServer.Disconnect();

                //    goto CONNECTION;
                //}
               // int cnt = reader.Read(buf, 0, 3);
               // Console.WriteLine("[server] receive message:" + buf[0] + " "+ buf[1] + " " + buf[2]);
               try
                {
                    //Console.WriteLine("Waiting read key");
                    //Console.ReadKey();
                    times++;
                    //Console.WriteLine("Times:" + times);
                    //str = Encoding.UTF8.GetString(send);
                    //writer.Write(str);

                    //writer.Flush();
                    //pipeServer.WaitForPipeDrain();
                    if (!ReadMsg())
                    {
                        pipeServer.Disconnect();

                        goto CONNECTION;
                    }
                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}

                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}
                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}
                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}

                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}
                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}
                    //if (!ReadMsg())
                    //{
                    //    pipeServer.Disconnect();

                    //    goto CONNECTION;
                    //}
                    Console.WriteLine("One loop:" + times);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("[server] receive message:" +ex.ToString());
                    Console.WriteLine("[server] isconnenct:" + pipeServer.IsConnected);
                    Console.WriteLine("[server] IO code:" + GetLastError());

                    //reader.Close();
                    //reader.Dispose();
                    //writer.Close();
                    //writer.Dispose();
                    pipeServer.Disconnect();

                    goto CONNECTION;
                }

            }

        }
    }
}
