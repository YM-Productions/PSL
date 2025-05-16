namespace StdbModule.Utils;

public enum Status
{
    // Defaults
    Success = 0,
    UnknownError = 1,

    // Login 1
    InvalidUserName = 11,
    InvalidPassword = 12,

    // Account creation 100
    UserNameTaken = 1001,
    InvalidMail = 1002,
    MustAcceptAGB = 1003,
}
