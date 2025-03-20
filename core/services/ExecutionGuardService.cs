namespace FashionApp.core.services
{
    public class ExecutionGuardService
    {
        private readonly HashSet<string> _activeTask = new HashSet<string>();
        private readonly object _lock = new object();

        public bool TryStartTask(string taskKey)
        {
            lock (_lock)
            {
                if(_activeTask.Contains(taskKey)) return false;

                _activeTask.Add(taskKey);
                return true;
            }
        }

        public void FinishTask(string taskKey)
        {
            lock (_lock)
            {
                _activeTask.Remove(taskKey);
            }
        }
    }
}
