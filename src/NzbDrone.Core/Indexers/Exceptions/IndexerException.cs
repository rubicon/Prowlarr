using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Exceptions
{
    public class IndexerException : NzbDroneException
    {
        public IndexerResponse Response { get; }

        public IndexerException(IndexerResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        public IndexerException(IndexerResponse response, string message, Exception innerException)
            : base(message, innerException)
        {
            Response = response;
        }

        public IndexerException(IndexerResponse response, string message, params object[] args)
            : base(message, args)
        {
            Response = response;
        }

        public IndexerException(IndexerResponse response, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            Response = response;
        }
    }
}
