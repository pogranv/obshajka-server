namespace Obshajka.VerificationCodesManager.Exceptions
{

    [Serializable]
    public class FailSendCodeException : Exception
    {
        public FailSendCodeException() { }
        public FailSendCodeException(string message) : base(message) { }
        public FailSendCodeException(string message, Exception inner) : base(message, inner) { }
        protected FailSendCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
