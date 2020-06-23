using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.EndNodeCommunicator.Models;
using Microsoft.Extensions.Logging;

namespace CommonServices.EndNodeCommunicator
{
    public class EndNodeCommunicatorWebSocket : IEndNodeCommunicator
    {
        private readonly EndNodeCommunicatorWebSocketConfiguration _configuration;
        private readonly ILogger<EndNodeCommunicatorWebSocket> _logger;

        private readonly ConcurrentDictionary<int, Func<EndNodeMessage, Task>> _listeners = new ConcurrentDictionary<int, Func<EndNodeMessage, Task>>();
        private readonly ConcurrentQueue<DownlinkDataMessage> _messagesToSend = new ConcurrentQueue<DownlinkDataMessage>();
        private readonly ConcurrentQueue<string> _messagesToHandle = new ConcurrentQueue<string>();
        private int _idCounter = 0;
        private readonly AutoResetEvent _sendMessageEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _handleMessageEvent = new AutoResetEvent(false);

        public EndNodeCommunicatorWebSocket(EndNodeCommunicatorWebSocketConfiguration configuration, ILogger<EndNodeCommunicatorWebSocket> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        public async Task Start(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var socket = new ClientWebSocket();
                try
                {
                    await socket.ConnectAsync(new Uri(_configuration.Url), stoppingToken);
                    _logger.LogInformation("Connected to the web socket");

                    _logger.LogInformation("Starting send next message thread...");
                    new Thread(async () => await SendNextMessage(socket, stoppingToken)).Start();
                    _logger.LogInformation("SendNextMessage thread started...");
                    _logger.LogInformation("Starting handle received messages thread...");
                    new Thread(async () => await HandleReceivedMessages()).Start();
                    _logger.LogInformation("HandleReceivedMessages thread started...");
                    _logger.LogInformation("Listening for messages...");
                    await ListenForMessages(socket, stoppingToken);
                    _logger.LogError("Should not reach here");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR - {ex.Message}");
                }
            }
        }

        private async Task SendNextMessage(ClientWebSocket socket, CancellationToken stoppingToken)
        {
            while (true)
            {
                _logger.LogInformation("Waiting for messages to send...");
                _sendMessageEvent.WaitOne();
                while (_messagesToSend.TryDequeue(out DownlinkDataMessage message))
                {
                    var downlinkMessage = new
                    {
                        cmd = "tx",
                        EUI = message.DeviceEui,
                        port = 1,
                        confirmed = message.Confirmed,
                        data = message.Data
                    };
                    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(downlinkMessage);
                    _logger.LogInformation("Sending message: " + JsonSerializer.Serialize(downlinkMessage));
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, stoppingToken);
                }
            }
        }


        private async Task ListenForMessages(ClientWebSocket socket, CancellationToken stoppingToken)
        {
            while (true)
            {
                _logger.LogInformation("Getting ready to read next message");
                var buffer = new ArraySegment<byte>(new byte[2048]);
                WebSocketReceiveResult result;
                await using var ms = new MemoryStream();
                do
                {
                    result = await socket.ReceiveAsync(buffer, stoppingToken);
                    _logger.LogTrace($"Read {result.Count} bytes");
                    await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count, stoppingToken);
                } while (!result.EndOfMessage);

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    _logger.LogInformation("Message type not text: " + result.MessageType);
                    return;
                }

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                string messageAsString = await reader.ReadToEndAsync();
                _logger.LogInformation("Read message from websocket:" + messageAsString);
                _messagesToHandle.Enqueue(messageAsString);
                _handleMessageEvent.Set();
            }
        }

        private async Task HandleReceivedMessages()
        {
            while (true)
            {
                _logger.LogInformation("Waiting for messages to handle...");
                _handleMessageEvent.WaitOne();
                while (_messagesToHandle.TryDequeue(out string messageAsString))
                {
                    EndNodeMessage message = DeserializeMessage(messageAsString);
                    _logger.LogInformation("Deserialized message: " + JsonSerializer.Serialize<object>(message));
                    if (message != null)
                    {
                        await InformListeners(message);
                    }
                }
            }
        }

        private EndNodeMessage DeserializeMessage(string messageAsString)
        {
            JsonElement element = JsonDocument.Parse(messageAsString).RootElement;
            return element.Gsp("cmd") switch
            {
                "rx" => new UplinkDataMessage {DeviceEui = element.Gsp("EUI"), Data = element.Gsp("data"), Ack = element.GetProperty("ack").GetBoolean(), Timestamp = element.GetProperty("ts").GetInt64()},
                "tx" => new SendRequestAckMessage {DeviceEui = element.Gsp("EUI"), Successful = element.Gsp("success") != null},
                "txd" => new GatewayConfirmationMessage {DeviceEui = element.Gsp("EUI")},
                _ => null
            };
        }


        private async Task InformListeners(EndNodeMessage message)
        {
            foreach (var listener in _listeners.Values)
            {
                await listener(message);
            }
        }

        public int AddListener(Func<EndNodeMessage, Task> listener)
        {
            _listeners.TryAdd(_idCounter, listener);
            return Interlocked.Increment(ref _idCounter);
        }

        public void RemoveListener(int listenerId)
        {
            _listeners.TryRemove(listenerId, out Func<EndNodeMessage, Task> _);
        }

        public void SendMessage(DownlinkDataMessage message)
        {
            _logger.LogInformation("Enqueing downlink data message: " + JsonSerializer.Serialize(message));
            _messagesToSend.Enqueue(message);
            _sendMessageEvent.Set();
        }
    }

    public class EndNodeCommunicatorWebSocketConfiguration
    {
        public string Url { get; set; }
    }
}