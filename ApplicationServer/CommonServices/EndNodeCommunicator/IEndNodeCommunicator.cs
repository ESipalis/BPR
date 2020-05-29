using System;
using CommonServices.EndNodeCommunicator.Models;

namespace CommonServices.EndNodeCommunicator
{
    public interface IEndNodeCommunicator
    {
        public int AddListener(Action<EndNodeMessage> listener);
        public void RemoveListener(int listenerId);
        public void SendMessage(DownlinkDataMessage message);
    }
}