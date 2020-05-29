using System;
using System.Runtime.Serialization;

namespace CommonServices.DetectionSystemServices.KommuneService
{
    public class KommuneCommunicationException : Exception
    {
        public KommuneCommunicationException()
        {
        }

        protected KommuneCommunicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public KommuneCommunicationException(string message) : base(message)
        {
        }

        public KommuneCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}