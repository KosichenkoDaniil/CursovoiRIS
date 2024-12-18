using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Client
{
    public class TcpManager
    {

        public BitmapImage ApplyFilter(BitmapImage sourceImage)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 8000);
                NetworkStream stream = client.GetStream();

                                
                byte[] imageData = ConvertBitmapImageToByteArray(sourceImage);
                                
                stream.Write(BitConverter.GetBytes(imageData.Length), 0, 4);
                stream.Write(imageData, 0, imageData.Length);
                                
                Console.WriteLine("Получение обработанного изображения...");
                byte[] lengthBuffer = new byte[4];
                stream.Read(lengthBuffer, 0, 4);
                int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] responseData = new byte[responseLength];
                int bytesRead = 0;
                while (bytesRead < responseLength)
                {
                    bytesRead += stream.Read(responseData, bytesRead, responseLength - bytesRead);
                }
                                
                client.Close();
                return ConvertByteArrayToBitmapImage(responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return sourceImage;
            }
        }

        private byte[] ConvertBitmapImageToByteArray(BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
                return null;
                        
            using (MemoryStream memoryStream = new MemoryStream())
            {                
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memoryStream); 
                return memoryStream.ToArray(); 
            }
        }

        private BitmapImage ConvertByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return null;
                        
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; 
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); 
                return bitmapImage;
            }
        }
    }
}

