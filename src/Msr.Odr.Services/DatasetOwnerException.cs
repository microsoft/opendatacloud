// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Msr.Odr.Services
{
    public class DatasetOwnerException : Exception
    {
        public DatasetOwnerException() : base("User is not authorized to perform this operation.")
        {
        }

        public DatasetOwnerException(string message) : base(message)
        {
        }

        public DatasetOwnerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DatasetOwnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
