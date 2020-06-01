using System;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.EndNodeCommunicator.Models;

namespace CommonServices.EndNodeCommunicator
{
    public interface IEndNodeCommunicator
    {
        public Task Start(CancellationToken stoppingToken);
        public int AddListener(Func<EndNodeMessage, Task> listener);
        public void RemoveListener(int listenerId);
        public void SendMessage(DownlinkDataMessage message);
    }
}