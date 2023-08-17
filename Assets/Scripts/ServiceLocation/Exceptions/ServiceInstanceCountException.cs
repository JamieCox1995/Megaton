using System;

namespace TotalDistraction.ServiceLocation.Exceptions
{
    /// <summary>
    /// The exception that is thrown when multiple instances of a service were found.
    /// </summary>
    public class ServiceInstanceCountException : Exception
    {
        /// <summary>
        /// The format string for the exception message.
        /// </summary>
        private static readonly string FormatString = "There should be exactly one instance of {0} available but {1} were found.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInstanceCountException"/> class, specifying the
        /// service instance count.
        /// </summary>
        /// <param name="count">The number of found instances of the service.</param>
        public ServiceInstanceCountException(int count) : base(string.Format(FormatString, "a service", count)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInstanceCountException"/> class, specifying the
        /// service type and instance count.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="count">The number of found instances of the service.</param>
        public ServiceInstanceCountException(Type serviceType, int count) : base(string.Format(FormatString, serviceType.Name, count)) { }
    }
}
