using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Indexers.Events
{
    public class IndexerAuthEvent : IEvent
    {
        public int IndexerId { get; set; }
        public bool Successful { get; set; }
        public long ElapsedTime { get; set; }

        public IndexerAuthEvent(int indexerId, bool successful, long elapsedTime)
        {
            IndexerId = indexerId;
            Successful = successful;
            ElapsedTime = elapsedTime;
        }
    }
}
