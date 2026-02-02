namespace API_Service.Utils
{
    public enum LogType
    {
        ERROR = -1,
        INFO = 0,
        WARNING = 1,
    }
    public class LoggerService<T> where T : class
    {
        private readonly ILogger<T> _logger;
        public LoggerService(ILogger<T> logger)
        {
            this._logger = logger;
        }
        public void LogDetails(LogType type, string message)
        {
            switch (type)
            {
                case LogType.ERROR:
                    _logger.LogError(message);
                    break;
                case LogType.INFO:
                    _logger.LogInformation(message);
                    break;
                case LogType.WARNING:
                    _logger.LogWarning(message);
                    break;
                default:
                    _logger.LogCritical(message);
                    break;
            }
        }
    }
}
