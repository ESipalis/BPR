namespace CommonServices.EndNodeCommunicator.Models
{
    public abstract class EndNodeMessage
    {
        protected EndNodeMessage(EndNodeMessageType messageType)
        {
            MessageType = messageType;
        }

        public string DeviceEui { get; set; }
        public EndNodeMessageType MessageType { get; }
    }

    public class GatewayConfirmationMessage : EndNodeMessage
    {
        public GatewayConfirmationMessage() : base(EndNodeMessageType.GatewayConfirmation)
        {
        }
    }
    
    public class UplinkDataMessage : EndNodeMessage
    {
        public UplinkDataMessage() : base(EndNodeMessageType.UplinkMessage)
        {
        }

        public long Timestamp { get; set; }
        public bool Ack { get; set; }
        public string Data { get; set; }
    }

    public class DownlinkDataMessage : EndNodeMessage
    {
        public DownlinkDataMessage() : base(EndNodeMessageType.DownlinkDataMessage)
        {
        }
        
        public bool Confirmed { get; set; }
        public string Data { get; set; }
    }

    public class SendRequestAckMessage : EndNodeMessage
    {
        public SendRequestAckMessage() : base(EndNodeMessageType.SendRequestAck)
        {
        }
        
        public bool Successful { get; set; }
    }
    

    public enum EndNodeMessageType
    {
        UplinkMessage,
        SendRequestAck,
        GatewayConfirmation,
        DownlinkDataMessage
    }
}