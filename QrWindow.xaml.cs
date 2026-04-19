using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using QRCoder;

namespace WgcfGUI
{
    public partial class QrWindow : Wpf.Ui.Controls.FluentWindow
    {
        public QrWindow(string configData)
        {
            InitializeComponent();
            GenerateQr(configData);
        }

        private void GenerateQr(string data)
        {
            try
            {
                using var generator = new QRCodeGenerator();
                var qrData = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                using var ms = new MemoryStream(qrBytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                QrImage.Source = image;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to generate QR Code: {ex.Message}", "QR Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
