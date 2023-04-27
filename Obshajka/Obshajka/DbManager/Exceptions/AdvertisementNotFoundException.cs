namespace Obshajka.DbManager.Exceptions
{
    [Serializable]
    public class AdvertisementNotFoundException : Exception
    {
        public AdvertisementNotFoundException() { }
        public AdvertisementNotFoundException(string message) : base(message) { }
        public AdvertisementNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected AdvertisementNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
