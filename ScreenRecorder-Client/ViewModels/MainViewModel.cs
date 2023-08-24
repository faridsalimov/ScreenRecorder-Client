using ScreenRecorder_Client.Commands;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ScreenRecorder_Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand ConnectServerCommand { get; set; }

        public bool IsConnected { get; set; } = false;
        public Socket Socket { get; set; }
        private NetworkStream stream;

        [Obsolete]
        public MainViewModel()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            ConnectServerCommand = new RelayCommand((obj) =>
            {
                var ipAddress = IPAddress.Parse(myIP);
                var port = 22003;

                if (!IsConnected)
                {
                    Task.Run(() =>
                    {
                        var endPoint = new IPEndPoint(ipAddress, port);

                        try
                        {
                            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Socket.Connect(endPoint);

                            if (Socket.Connected)
                            {
                                IsConnected = true;
                                System.Windows.MessageBox.Show("Connected!", "Successfully!", MessageBoxButton.OK, MessageBoxImage.Information);

                                Thread sendThread = new Thread(SendScreenData);
                                sendThread.Start();
                            }
                        }

                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }

                else
                {
                    System.Windows.MessageBox.Show("You are already connected to the server.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void SendScreenData()
        {
            while (true)
            {
                Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using (Graphics gfx = Graphics.FromImage(screenshot))
                {
                    gfx.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenshot.Size);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    screenshot.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] imageData = ms.ToArray();
                    stream.Write(imageData, 0, imageData.Length);
                }

                Thread.Sleep(100);
            }
        }   
    }
}
