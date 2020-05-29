using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.EndNodeCommunicator.Models;

namespace CommonServices.EndNodeCommunicator
{
    public class EndNodeCommunicatorWebSocket : IEndNodeCommunicator
    {
        private readonly EndNodeCommunicatorWebSocketConfiguration _configuration;
        private readonly Dictionary<int, Action<EndNodeMessage>> _listeners = new Dictionary<int, Action<EndNodeMessage>>();
        private readonly ClientWebSocket _webSocket = new ClientWebSocket();
        private int _idCounter = 0;

        public EndNodeCommunicatorWebSocket(EndNodeCommunicatorWebSocketConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Start()
        {
            await _webSocket.ConnectAsync(new Uri(_configuration.Url), CancellationToken.None);
        }

        public async Task Stop()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        
        public int AddListener(Action<EndNodeMessage> listener)
        {
            _listeners.Add(_idCounter, listener);
            return _idCounter++;
        }

        public void RemoveListener(int listenerId)
        {
            _listeners.Remove(listenerId);
        }

        public void SendMessage(DownlinkDataMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class EndNodeCommunicatorWebSocketConfiguration
    {
        public string Url { get; set; }
    }
}