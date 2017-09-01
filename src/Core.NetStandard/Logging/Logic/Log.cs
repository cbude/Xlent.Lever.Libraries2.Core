﻿using System;
using System.Diagnostics;
using Xlent.Lever.Libraries2.Core.Application;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Context;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Logging.Model;
using Xlent.Lever.Libraries2.Core.Threads;

namespace Xlent.Lever.Libraries2.Core.Logging.Logic
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The chosen <see cref="IValueProvider"/> to use.
        /// </summary>
        /// <remarks>There are overrides for this, see e.g. in Xlent.Lever.Libraries2.WebApi.ContextValueProvider.</remarks>
        [Obsolete("Use ApplicationSetup.Logger", true)]
#pragma warning disable 169
        private static IFulcrumLogger _chosenLogger;
#pragma warning restore 169

        /// <summary>
        /// The chosen <see cref="IValueProvider"/> to use.
        /// </summary>
        /// <remarks>There are overrides for this, see e.g. in Xlent.Lever.Libraries2.WebApi.ContextValueProvider.</remarks>
        [Obsolete("Use ApplicationSetup.Logger", true)]
        public static IFulcrumLogger LoggerForApplication
        {
            get => ApplicationSetup.Logger;
            set => ApplicationSetup.Logger = value;
        }

        /// <summary>
        /// Recommended <see cref="IFulcrumLogger"/> for developing an application. For testenvironments and production, we recommend the Xlent.Lever.Logger capability.
        /// </summary>
        public static IFulcrumLogger RecommendedForNetFramework { get; } = new TraceSourceLogger();

        /// <summary>
        /// Verbose logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        public static void LogVerbose(string message, Exception exception = null)
        {
            LogInBackground(LogSeverityLevel.Verbose, message, exception);
        }

        /// <summary>
        /// Information logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        public static void LogInformation(string message, Exception exception = null)
        {
            LogInBackground(LogSeverityLevel.Information, message, exception);
        }

        /// <summary>
        /// Warning logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        public static void LogWarning(string message, Exception exception = null)
        {
            LogInBackground(LogSeverityLevel.Warning, message, exception);
        }

        /// <summary>
        /// Error logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        public static void LogError(string message, Exception exception = null)
        {
            LogInBackground(LogSeverityLevel.Error, message, exception);
        }

        /// <summary>
        /// Critical logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        public static void LogCritical(string message, Exception exception = null)
        {
            LogInBackground(LogSeverityLevel.Critical, message, exception);
        }

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log (will be concatenated with any <paramref name="exception"/> information).</param>
        /// <param name="exception">Optional exception</param>
        private static void LogInBackground(LogSeverityLevel severityLevel, string message, Exception exception = null)
        {
            ThreadHelper.FireAndForget(() => SafeLog(severityLevel, message, exception));
        }

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log (will be concatenated with any <paramref name="exception"/> information).</param>
        /// <param name="exception">Optional exception</param>
        private static void SafeLog(LogSeverityLevel severityLevel, string message, Exception exception = null)
        {
            try
            {
                var formattedMessage = FormatMessage(message, exception);
                ApplicationSetup.Logger.Log(severityLevel, formattedMessage);
            }
            catch (Exception e1)
            {
                try
                {
                    FallbackLoggingWhenAllElseFails($"{e1.Message}\r{message}");
                }
                catch (Exception e2)
                {
                    try
                    {
                        Debug.WriteLine($"{e2.Message}\r{e1.Message}\r{message}");
                    }
                    catch (Exception)
                    {
                        // We give up
                    }
                }
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
                RecommendedForNetFramework.LogAsync(LogSeverityLevel.Critical, message);
            }
            catch (Exception)
            {
                // This method must never fail.
            }
        }
    }
}
