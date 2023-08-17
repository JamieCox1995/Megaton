using System;

namespace TotalDistraction.ServiceLocation.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an unregistered service is requested from the service locator.
    /// </summary>
    public class UnregisteredServiceException : Exception
    {
        /// <summary>
        /// The format string for the exception message.
        /// </summary>
        private static readonly string FormatString = "The type \"{0}\" has not been registered as a service.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisteredServiceException"/> class, specifying the
        /// service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        public UnregisteredServiceException(Type serviceType) : base(string.Format(FormatString, serviceType.Name)) { }
    }
}
