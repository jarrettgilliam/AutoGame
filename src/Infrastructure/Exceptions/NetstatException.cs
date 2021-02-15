namespace AutoGame.Infrastructure.Exceptions
{
    using System;

    internal sealed class NetstatException : Exception
    {
        public NetstatException()
        {
        }

        public NetstatException(string message) : base(message)
        {
        }

        public NetstatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
