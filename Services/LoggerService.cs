using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Services
{
    public enum LogLevel { Info, Warn, Error }
    internal class LoggerService
    {
        private readonly SupabaseClient _client;

        public LoggerService(SupabaseClient client)
        {
            _client = client;
        }

        public async Task LogAsync(LogLevel level, string message, string userLogin = null, string machineName = null, string appVersion = null) //Создание лога
        {
            try
            {
                _ = _client.InsertLog(
                    level.ToString().ToUpper(),
                    message,
                    userLogin ?? AppState.CurrentUser?.login ?? "unknown",
                    machineName ?? Environment.MachineName,
                    appVersion ?? "1.0.0"
                );
            }
            catch // Оставил пустым так как логгер не должен уранить приложение
            {

            }
        }

        // Уровни логгирования
        public Task Info(string message) => LogAsync(LogLevel.Info, message);
        public Task Warn(string message) => LogAsync(LogLevel.Warn, message);
        public Task Error(string message) => LogAsync(LogLevel.Error, message);
        public Task Error(Exception ex) => LogAsync(LogLevel.Error, $"{ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
    }
}
