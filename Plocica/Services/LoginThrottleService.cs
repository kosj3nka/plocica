namespace Plocica.Services;

public class LoginThrottleService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(1);

    private readonly object _lock = new();
    private int _failedAttempts;
    private DateTimeOffset? _lockedUntil;

    public bool IsLockedOut(out TimeSpan remaining)
    {
        lock (_lock)
        {
            if (_lockedUntil.HasValue && _lockedUntil.Value > DateTimeOffset.UtcNow)
            {
                remaining = _lockedUntil.Value - DateTimeOffset.UtcNow;
                return true;
            }

            remaining = TimeSpan.Zero;
            return false;
        }
    }

    public void RegisterFailure()
    {
        lock (_lock)
        {
            _failedAttempts++;
            if (_failedAttempts >= MaxAttempts)
            {
                _lockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
                _failedAttempts = 0;
            }
        }
    }

    public void RegisterSuccess()
    {
        lock (_lock)
        {
            _failedAttempts = 0;
            _lockedUntil = null;
        }
    }
}
