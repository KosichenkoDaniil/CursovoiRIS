using SkiaSharp;
using System;
using System.IO;
using System.Net.Sockets;

namespace Solver
{
    class Solver
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Solver запускается...");
            TcpClient client = new TcpClient();

            try
            {
                client.Connect("127.0.0.1", 9000);
                Console.WriteLine("Solver подключился к серверу.");

                while (true)
                {
                    NetworkStream stream = client.GetStream();
                                        
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = stream.Read(lengthBuffer, 0, 4);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Соединение с сервером закрыто.");
                        break;
                    }
                    int segmentLength = BitConverter.ToInt32(lengthBuffer, 0);
                                        
                    byte[] segmentData = new byte[segmentLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < segmentLength)
                    {
                        totalBytesRead += stream.Read(segmentData, totalBytesRead, segmentLength - totalBytesRead);
                    }

                    using var segmentStream = new MemoryStream(segmentData);
                    using var segmentBitmap = SKBitmap.Decode(segmentStream);
                    Console.WriteLine("Сегмент изображения получен. Выполняется обработка...");
                                        
                    var processedSegment = AdaptiveBlur.ApplyAdaptiveBlurToSegment(segmentBitmap, false, false, 0.0000001);
                                        
                    using var outputStream = new MemoryStream();
                    processedSegment.Encode(outputStream, SKEncodedImageFormat.Png, 100);
                    byte[] responseData = outputStream.ToArray();

                    stream.Write(BitConverter.GetBytes(responseData.Length), 0, 4);
                    stream.Write(responseData, 0, responseData.Length);

                    Console.WriteLine("Обработанный сегмент отправлен серверу.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Solver завершил работу.");
            }
        }
    }
}