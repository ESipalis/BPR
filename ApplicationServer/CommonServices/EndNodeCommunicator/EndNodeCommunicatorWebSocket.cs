using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.EndNodeCommunicator.Models;

namespace CommonServices.EndNodeCommunicator
{
    public class EndNodeCommunicatorWebSocket : IEndNodeCommunicator
    {
        private readonly EndNodeCommunicatorWebSocketConfiguration _configuration;
        private readonly ConcurrentDictionary<int, Action<EndNodeMessage>> _listeners = new ConcurrentDictionary<int, Action<EndNodeMessage>>();
        private readonly ConcurrentQueue<DownlinkDataMessage> _messagesToSend = new ConcurrentQueue<DownlinkDataMessage>();
        private int _idCounter = 0;

        public EndNodeCommunicatorWebSocket(EndNodeCommunicatorWebSocketConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task Start(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var socket = new ClientWebSocket();
                try
                {
                    await socket.ConnectAsync(new Uri(_configuration.Url), stoppingToken);

                    await SendNextMessage(socket, stoppingToken);
                    // await Receive(socket, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR - {ex.Message}");
                }
            }
        }

        private async Task SendNextMessage(ClientWebSocket socket, CancellationToken stoppingToken)
        {
            if (_messagesToSend.IsEmpty)
            {
                return;
            }

            if (_messagesToSend.TryDequeue(out DownlinkDataMessage message))
            {
                
                await socket.SendAsync(Encoding.UTF8.GetBytes(""), WebSocketMessageType.Text, true, stoppingToken);
            }
        }

        private async Task ListenForMessages(ClientWebSocket socket, CancellationToken stoppingToken)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            while (!stoppingToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                await using var ms = new MemoryStream();
                do
                {
                    result = await socket.ReceiveAsync(buffer, stoppingToken);
                    await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count, stoppingToken);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                Console.WriteLine(await reader.ReadToEndAsync());
            }
        }

        public int AddListener(Action<EndNodeMessage> listener)
        {
            _listeners.TryAdd(_idCounter, listener);
            return Interlocked.Increment(ref _idCounter);
        }

        public void RemoveListener(int listenerId)
        {
            _listeners.TryRemove(listenerId, out Action<EndNodeMessage> _);
        }

        public void SendMessage(DownlinkDataMessage message)
        {
            _messagesToSend.Enqueue(message);
        }
    }

    public class EndNodeCommunicatorWebSocketConfiguration
    {
        public string Url { get; set; }
    }
}