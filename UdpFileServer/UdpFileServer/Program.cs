using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

// Сервер, принимающий по Udp имя файла, размер файла, содержимое файла
namespace UdpFileServer
{
    class Program
    {
        static Socket srvSock;
        static string ipServer = "0.0.0.0";
        static int portServer = 12345;  // 0..65535
        static EndPoint ClientEndPoint;
        static void Main(string[] args)
        {
            srvSock = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
            srvSock.Bind(new IPEndPoint(IPAddress.Parse(ipServer), portServer));

            ClientEndPoint = new IPEndPoint(0, 0);

            ThreadPool.QueueUserWorkItem(ThreadRoutine);
         
            Console.ReadLine();

        }

        static void ThreadRoutine(object obj)
        {
            // получить имя файла
            byte[] buf = new byte[64 * 1024];  // рекомендуется для UDP
            int recSize1 = srvSock.ReceiveFrom(buf, ref ClientEndPoint);
            string FileName = Encoding.UTF8.GetString(buf, 0, recSize1);
            Console.WriteLine("\nFile name = " + FileName);
            FileStream outFile = new FileStream(FileName, FileMode.Create);

            // получить кол-во посылок
            srvSock.ReceiveFrom(buf, ref ClientEndPoint);
            int cntRecive = BitConverter.ToInt32(buf, 0);
            Console.WriteLine($"cntRecive = {cntRecive}");

            // прием частей файла
            int fullSize = 0;
            for (int i = 0; i < cntRecive; i++)
            {
                int recSize = srvSock.ReceiveFrom(buf, ref ClientEndPoint);
                if (recSize <= 0) break;
                Console.WriteLine($"i = {i}, {recSize} bytes   \r");
                outFile.Write(buf, 0, recSize);
                fullSize += recSize;
            }

            Console.WriteLine($"\nFull size of file  = {fullSize}");
            outFile.Flush(true);  // принудительный сброс данных на диск
                                  //outFile.Seek()
            outFile.Close();  // принудительное закрытие потоков
                              //outFile.Dispose();
        }
    }
}
