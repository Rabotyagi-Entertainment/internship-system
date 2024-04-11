namespace Internship_system.BLL.Exceptions;

[Serializable]
public class BadRequestException : Exception {
    /// <summary>
    /// Constructor
    /// </summary>
    public BadRequestException() {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public BadRequestException(string message)
        : base(message) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public BadRequestException(string message, Exception inner)
        : base(message, inner) {
    }
}