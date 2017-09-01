﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Context;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Logging.Model;

namespace Xlent.Lever.Libraries2.Core.Logging.Logic
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// The chosen <see cref="IValueProvider"/> to use.
        /// </summary>
        /// <remarks>There are overrides for this, see e.g. in Xlent.Lever.Libraries2.WebApi.Context.</remarks>
        private static IFulcrumLogger _chosenLogger;

        /// <summary>
        /// The chosen <see cref="IValueProvider"/> to use.
        /// </summary>
        /// <remarks>There are overrides for this, see e.g. in Xlent.Lever.Libraries2.WebApi.Context.</remarks>
        public static IFulcrumLogger LoggerForApplication
        {
            get
            {
                // TODO: Link to Lever WIKI
                FulcrumAssert.IsNotNull(_chosenLogger, null, $"The application must at startup set {nameof(LoggerForApplication)} to the appropriate {nameof(IFulcrumLogger)}.");
                return _chosenLogger;
            }
            set
            {
                InternalContract.RequireNotNull(value, nameof(value));
                _chosenLogger = value;
            }
        }

        /// <summary>
        /// Recommended <see cref="IFulcrumLogger"/> for developing an application. For testenvironments and production, we recommend the Xlent.Lever.Logger capability.
        /// </summary>
        public static IFulcrumLogger RecommendedForDevelopment { get; } = new TraceSourceLogger();

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log (will be concatenated with any <paramref name="exception"/> information).</param>
        /// <param name="exception">Optional exception</param>
        public static async Task LogAsync(LogSeverityLevel severityLevel, string message, Exception exception = null)
        {
            try
            {
                var formattedMessage = FormatMessage(message, exception);
                await LoggerForApplication.LogAsync(severityLevel, formattedMessage);
            }
            catch (Exception e)
            {
                FallbackLoggingWhenAllElseFails(e.Message);
                FallbackLoggingWhenAllElseFails(message);
            }
        }

        /// <summary>
        /// Create a formatted message based on <paramref name="message"/> and <paramref name="exception"/>
        /// </summary>
        /// <param name="message">The message. Can be null or empty if exception is not null.</param>
        /// <param name="exception">Optional exception</param>
        /// <returns>A formatted message, never null or empty</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null AND <paramref name="message"/> is null or empty.</exception>
        public static string FormatMessage(string message, Exception exception)
        {
            if (exception == null && string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message));
            }
            return exception != null ? FormatMessage(exception) : message;
        }

        /// <summary>
        /// Create a formatted message based on <paramref name="exception"/>
        /// </summary>
        /// <param name="exception">The exception that we will create a log message for.</param>
        /// <returns>A formatted message, never null or empty.</returns>
        /// <remarks>This method should never throw an exception. If </remarks>
        public static string FormatMessage(Exception exception)
        {
            // This method should never fail, so if no exception was given, we will create an exception.
            try
            {
                InternalContract.RequireNotNull(exception, nameof(exception));
            }
            catch (Exception e)
            {
                exception = e;
            }
            var formatted = $"Exception type: {exception.GetType().FullName}";
            var fulcrumException = exception as FulcrumException;
            if (fulcrumException != null) formatted += $"\r{fulcrumException}";
            formatted += $"\rException message: {exception.Message}";
            formatted += $"\r{exception.StackTrace}";
            if (exception.InnerException != null)
            {
                formatted += $"\r--Inner exception--\r{FormatMessage(exception.InnerException)}";
            }
            return formatted;
        }

        
        /// <summary>
        /// Use this method to log when the original logging method fails.
        /// </summary>
        /// <param name="message">The original message to log.</param>
        private static void FallbackLoggingWhenAllElseFails(string message)
        {
            try
            {
                Debug.WriteLine(message);
                RecommendedForDevelopment.LogAsync(LogSeverityLevel.Critical, message);
            }
            catch (Exception)
            {
                // This method must never fail.
            }
        }
    }
}

