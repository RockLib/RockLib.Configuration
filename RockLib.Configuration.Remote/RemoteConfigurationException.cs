using System;
using System.Runtime.Serialization;

namespace RockLib.Configuration.Remote;

/// <summary>
/// An exception related to a remote configuration source.
/// </summary>
[Serializable]
public class RemoteConfigurationException : Exception
{
    /// <summary>
    /// Create a RemoteConfigurationException instance.
    /// </summary>
    public RemoteConfigurationException()
    {
    }

    /// <summary>
    /// Create a RemoteConfigurationException instance.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RemoteConfigurationException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Create a RemoteConfigurationException instance.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null
    /// reference if no inner exception is specified.
    /// </param>
    public RemoteConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Create a RemoteConfigurationException instance.
    /// </summary>
    /// <param name="info">
    /// The System.Runtime.Serialization.SerializationInfo that holds the
    /// serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    /// The System.Runtime.Serialization.StreamingContext that contains
    /// contextual information about the source or destination.
    /// </param>
    protected RemoteConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
