using System;
namespace TotalDistraction.ServiceLocation.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a service could not be configured.
    /// </summary>
    public class ServiceConfigurationException : Exception
    {
        /// <summary>
        /// The format string for the exception message.
        /// </summary>
        private static readonly string FormatString = "The service \"{0}\" could not be configured. " +
            "Ensure that the configuration asset is in a Resources folder.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConfigurationException"/> class, specifying the
        /// service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        public ServiceConfigurationException(Type serviceType) : base(string.Format(FormatString, serviceType.Name)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConfigurationException"/> class, specifying the
        /// service type and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or <c>null</c> if no inner exception is specified.</param>
        public ServiceConfigurationException(Type serviceType, Exception innerException) : base(string.Format(FormatString, serviceType.Name), innerException) { }
    }
}
