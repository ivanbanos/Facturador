using System.Collections.Concurrent;

namespace SigesServicio
{
    public interface IWorkerHeartbeatTracker
    {
        void MarkStarted(string workerName);
        void Pulse(string workerName);
        DateTimeOffset? GetLastSeen(string workerName);
    }

    public sealed class WorkerHeartbeatTracker : IWorkerHeartbeatTracker
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _heartbeats = new();

        public void MarkStarted(string workerName)
        {
            _heartbeats[workerName] = DateTimeOffset.UtcNow;
        }

        public void Pulse(string workerName)
        {
            _heartbeats[workerName] = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset? GetLastSeen(string workerName)
        {
            return _heartbeats.TryGetValue(workerName, out var value) ? value : null;
        }
    }
}
