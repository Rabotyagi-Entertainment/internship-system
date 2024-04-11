namespace Internship_system.BLL.Exceptions;

[Serializable]
public class NotFoundException : Exception {
    /// <summary>
    /// Constructor
    /// </summary>
    public NotFoundException() {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public NotFoundException(string message)
        : base(message) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public NotFoundException(string message, Exception inner)
        : base(message, inner) {
    }
}