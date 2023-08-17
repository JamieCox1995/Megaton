using System;

namespace TotalDistraction.ServiceLocation.Exceptions
{
    /// <summary>
    /// The exception that is thrown when no instances of a service are found.
    /// </summary>
    public class ServiceNotFoundException : Exception
    {
        /// <summary>
        /// The format string for the exception message.
        /// </summary>
        private static readonly string FormatString = "The service \"{0}\" has no registered instance. " +
            "If this service has a scene-based lifetime, check that it has been registered to this scene.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class, specifying the
        /// service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        public ServiceNotFoundException(Type serviceType) : base(string.Format(FormatString, serviceType.Name)) { }
    }
}
