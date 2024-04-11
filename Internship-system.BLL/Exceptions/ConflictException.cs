namespace Internship_system.BLL.Exceptions;

[Serializable]
public class ConflictException : Exception {
    /// <summary>
    /// Constructor
    /// </summary>
    public ConflictException() {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ConflictException(string message)
        : base(message) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ConflictException(string message, Exception inner)
        : base(message, inner) {
    }
}