using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

class Server
{
    private static readonly List<TcpClient> solvers = new List<TcpClient>();
    private static readonly object solverLock = new object();

    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8000);
        server.Start();
        Console.WriteLine("Сервер запущен, ожидание клиентов...");

        _ = Task.Run(AcceptSolversAsync);

        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            Console.WriteLine("Клиент подключился!");
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private static async Task AcceptSolversAsync()
    {
        TcpListener solverListener = new TcpListener(IPAddress.Any, 9000);
        solverListener.Start();
        Console.WriteLine("Сервер готов принимать solver'ы...");

        while (true)
        {
            var solverClient = await solverListener.AcceptTcpClientAsync();
            lock (solverLock)
            {
                solvers.Add(solverClient);
            }
            Console.WriteLine("Solver подключился!");
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();

                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer, 0, 4);
                int imageLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] imageData = new byte[imageLength];
                int bytesRead = 0;
                while (bytesRead < imageLength)
                {
                    bytesRead += await stream.ReadAsync(imageData, bytesRead, imageLength - bytesRead);
                }

                using var inputStream = new MemoryStream(imageData);
                using var inputBitmap = SKBitmap.Decode(inputStream);
                Console.WriteLine("Получено изображение. Разделение на области...");

                var segments = SplitImage(inputBitmap, solvers.Count);

                SKBitmap resultBitmap = new SKBitmap(inputBitmap.Width, inputBitmap.Height);
                var tasks = new List<Task>();

                for (int i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    int solverIndex = i;

                    tasks.Add(Task.Run(async () =>
                    {
                        var processedSegment = await SendToSolverAsync(solvers[solverIndex], segment.segment);
                        MergeSegment(resultBitmap, processedSegment, segment.startY);
                    }));
                }

                await Task.WhenAll(tasks);

                Console.WriteLine("Все сегменты обработаны. Сбор итогового изображения...");

                using var outputStream = new MemoryStream();
                resultBitmap.Encode(outputStream, SKEncodedImageFormat.Jpeg, 100);
                byte[] responseData = outputStream.ToArray();

                await stream.WriteAsync(BitConverter.GetBytes(responseData.Length), 0, 4);
                await stream.WriteAsync(responseData, 0, responseData.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static List<(SKBitmap segment, int startY)> SplitImage(SKBitmap bitmap, int numSegments)
    {
        int segmentHeight = bitmap.Height / numSegments;
        var segments = new List<(SKBitmap, int)>();

        for (int i = 0; i < numSegments; i++)
        {
            int startY = i * segmentHeight;
            int endY = (i == numSegments - 1) ? bitmap.Height : startY + segmentHeight;

            SKBitmap segment = new SKBitmap(bitmap.Width, endY - startY);

            for (int y = startY; y < endY; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    segment.SetPixel(x, y - startY, bitmap.GetPixel(x, y));
                }
            }

            segments.Add((segment, startY));
        }

        return segments;
    }

    private static async Task<SKBitmap> SendToSolverAsync(TcpClient solver, SKBitmap segment)
    {
        try
        {
            NetworkStream stream = solver.GetStream();

            using var memoryStream = new MemoryStream();
            segment.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
            byte[] data = memoryStream.ToArray();

            await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
            await stream.WriteAsync(data, 0, data.Length);

            byte[] lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer, 0, 4);
            int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] responseData = new byte[responseLength];
            int bytesRead = 0;
            while (bytesRead < responseLength)
            {
                bytesRead += await stream.ReadAsync(responseData, bytesRead, responseLength - bytesRead);
            }

            return SKBitmap.Decode(new MemoryStream(responseData));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке solver'ом: {ex.Message}");
            return segment;
        }
    }

    private static void MergeSegment(SKBitmap resultBitmap, SKBitmap segment, int startY)
    {
        if (segment.Width != resultBitmap.Width)
        {
            throw new InvalidOperationException("Ширина сегмента не совпадает с шириной итогового изображения.");
        }

        int endY = startY + segment.Height;
        if (endY > resultBitmap.Height)
        {
            throw new InvalidOperationException("Сегмент выходит за границы итогового изображения.");
        }

        for (int y = 0; y < segment.Height; y++)
        {
            for (int x = 0; x < segment.Width; x++)
            {
                resultBitmap.SetPixel(x, startY + y, segment.GetPixel(x, y));
            }
        }
    }
}
