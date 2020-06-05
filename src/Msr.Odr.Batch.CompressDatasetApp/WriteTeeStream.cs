// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    public class WriteTeeStream : Stream
    {
        private readonly Stream[] _streams;

        public WriteTeeStream(params Stream[] streams)
        {
            _streams = streams;
        }

        public override void Flush()
        {
            WithStreams(stream => stream.Flush());
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
            WithStreams(stream => stream.Write(buffer, offset, count));
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

        private void WithStreams(Action<Stream> action)
        {
            foreach (var stream in _streams)
            {
                action(stream);
            }
        }
    }
}
