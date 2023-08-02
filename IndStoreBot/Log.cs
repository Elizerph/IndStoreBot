using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace IndStoreBot
{
    public static class Log
    {
        private class Appender : IAppender
        {
            public string Name { get; set; }

            public void Close()
            {
            }

            public void DoAppend(LoggingEvent loggingEvent)
            {
                Console.WriteLine($"{loggingEvent.TimeStamp} [{loggingEvent.Level.DisplayName}]: {loggingEvent.MessageObject}");
            }
        }

        private readonly static ILog _instance = LogManager.GetLogger(typeof(Program));

        static Log()
        {
            BasicConfigurator.Configure(new Appender());
        }

        public static void WriteInfo(string text)
        {
            _instance.Info(text);
        }

        public static void WriteError(string text)
        {
            _instance.Error(text);
        }

        public static void WriteError(string text, Exception exception)
        {
            _instance.Error(text, exception);
        }
    }
}
