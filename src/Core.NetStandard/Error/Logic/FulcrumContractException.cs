﻿using System;

namespace Xlent.Lever.Libraries2.Core.Error.Logic
{
    /// <summary>
    /// There was something wrong with the request itself, i.e. syntax, values out of range, etc.
    /// </summary>
    public class FulcrumContractException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumContractException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>.
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Contract";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties();
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful => false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        private void SetProperties()
        {

            FriendlyMessage =
                "A programmer's code calls another part of the program in a bad way. An end user is never supposed to see this error as it should be converted on the way.";
            FriendlyMessage += " Please report the following:";
            FriendlyMessage += $"\rCorrelactionId: {CorrelationId}";
            FriendlyMessage += $"\rInstanceId: {InstanceId}";

            MoreInfoUrl = "http://lever.xlent-fulcrum.info/FulcrumExceptions#FulcrumContractException";
        }
    }
}
