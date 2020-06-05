// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Msr.Odr.Batch.Shared;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    public class WriteBlocksStream : Stream
    {
        private readonly Stream _stream;
        private bool disposed = false;

        public WriteBlocksStream(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Flush is ignored so that full blocks are written correctly in Azure storage.
        /// </summary>
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Close()
        {
            _stream.Flush();
            _stream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _stream.Dispose();
            }

            disposed = true;
            base.Dispose(disposing);
        }
    }
}
