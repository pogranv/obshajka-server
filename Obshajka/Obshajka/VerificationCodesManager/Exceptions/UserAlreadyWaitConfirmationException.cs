namespace Obshajka.VerificationCodesManager.Exceptions
{

    [Serializable]
    public class UserAlreadyWaitConfirmationException : Exception
    {
        public UserAlreadyWaitConfirmationException() { }
        public UserAlreadyWaitConfirmationException(string message) : base(message) { }
        public UserAlreadyWaitConfirmationException(string message, Exception inner) : base(message, inner) { }
        protected UserAlreadyWaitConfirmationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
