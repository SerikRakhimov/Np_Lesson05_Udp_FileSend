using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UdpFileClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Socket srvSock;
        static string FileName;
        static string FileFullName;
        static int OneBlockSize;
        static IPEndPoint ClientEndPoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbIpAdress.Text == "")
            {
                MessageBox.Show("Введите IP-адрес. Повторите ввод.");
                return;
            }

            if (tbPort.Text == "")
            {
                MessageBox.Show("Введите порт. Повторите ввод.");
                return;
            }

            try
            {
                OneBlockSize = int.Parse(tbOnBlockSize.Text);
                if (OneBlockSize <= 0)
                {
                    MessageBox.Show("Введите число в OnBlockSize. Повторите ввод.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Введите число в OnBlockSize. Повторите ввод.");
                return;
            }

            if (tbFileName.Text == "")
            {
                MessageBox.Show("Введите имя файла. Повторите ввод.");
                return;
            }

            srvSock = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);

            ClientEndPoint = new IPEndPoint(IPAddress.Parse(tbIpAdress.Text),
                                                      Convert.ToInt32(tbPort.Text));

            ThreadPool.QueueUserWorkItem(ThreadRoutine);

        }

        static void ThreadRoutine(object obj)
        {

        FileStream outFile = new FileStream(FileFullName, FileMode.Open);

        // количество посылок
        int cntRecive = (int)outFile.Length / OneBlockSize;
        // размер в байтах последней посылки
        int Remainder = (int)outFile.Length % OneBlockSize;

        // посылаем имя файла
        byte[] data = Encoding.UTF8.GetBytes(FileName);
        srvSock.SendTo(data, ClientEndPoint);

            // посылаем количество посылок
            int BlockCountToSend = cntRecive;
            if (Remainder > 0) BlockCountToSend++;
            data = BitConverter.GetBytes(BlockCountToSend);
            srvSock.SendTo(data, ClientEndPoint);

            // посылаем части файла
            byte[] buf = new byte[OneBlockSize];
            for (int i = 0; i<cntRecive; i++)
            {
                outFile.Read(buf, 0, OneBlockSize);
                srvSock.SendTo(buf, 0, OneBlockSize, SocketFlags.None, ClientEndPoint);
                Thread.Sleep(1000);  // без этой задержки большие файлы не пересылаются до конца
            }
            if (Remainder > 0)
            {
                outFile.Read(buf, 0, Remainder);
                srvSock.SendTo(buf, 0, Remainder, SocketFlags.None, ClientEndPoint);
            }
            outFile.Flush();
            outFile.Close();
}

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Jpeg Files (*.jpg)|*.jpg";

            if (ofd.ShowDialog() == true)
            {
                tbFileName.Text = ofd.FileName;
                FileFullName = ofd.FileName;
                FileName = ofd.SafeFileName;
            }
        }
    }

}
