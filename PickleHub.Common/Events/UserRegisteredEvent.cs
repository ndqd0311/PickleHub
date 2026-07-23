namespace PickleHub.Common.Events;

/// <summary>
/// Event được publish bởi Auth Service khi có user mới đăng ký thành công.
/// Customer Service subscribe event này để tự động tạo CustomerProfile.
/// </summary>
public record UserRegisteredEvent(
    Guid UserId,
    string Email
);
