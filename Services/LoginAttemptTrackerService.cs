using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Services
{
    public static class LoginAttemptTrackerService
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(1);

        private static readonly Dictionary<string, (int attempts, DateTime? lockedUntil)> _tracker = new();

        public static bool IsLocked(string login)
        {
            if (_tracker.TryGetValue(login, out var entry))
            {
                if (entry.lockedUntil.HasValue && entry.lockedUntil.Value > DateTime.Now)
                    return true;

                if (entry.lockedUntil.HasValue && entry.lockedUntil.Value <= DateTime.Now)
                    _tracker.Remove(login);
            }
            return false;
        } //Проверка заблокирован ли пользователь

        public static int GetRemainingAttempts(string login)
        {
            if (_tracker.TryGetValue(login, out var entry))
                return Math.Max(0, MaxAttempts - entry.attempts);
            return MaxAttempts;
        } //Счётчик кол-ва попыток

        public static void RecordFailedAttempt(string login)
        {
            if (_tracker.TryGetValue(login, out var entry))
            {
                int newAttempts = entry.attempts + 1;
                DateTime? lockedUntil = newAttempts >= MaxAttempts ? DateTime.Now.Add(LockoutDuration) : null;
                _tracker[login] = (newAttempts, lockedUntil);
            }
            else
            {
                _tracker[login] = (1, null);
            }
        } //Установка временной блокировки при достижении лимита

        public static void Reset(string login)
        {
            _tracker.Remove(login);
        } //Сброс счётчика попыток
    }
}
