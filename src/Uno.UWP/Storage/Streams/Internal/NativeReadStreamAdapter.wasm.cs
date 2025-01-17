﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace Uno.Storage.Streams.Internal
{
	internal partial class NativeReadStreamAdapter : Stream
	{
#if !NET7_0_OR_GREATER
		private const string JsType = "Uno.Storage.Streams.NativeFileReadStream";
#endif

		private readonly Guid _streamId;

		private long _length;
		private long _position;

		public static async Task<NativeReadStreamAdapter> CreateAsync(Guid fileId)
		{
			var streamId = Guid.NewGuid();
			var result = await
#if NET7_0_OR_GREATER
				NativeMethods.OpenAsync(streamId.ToString(), fileId.ToString());
#else
				WebAssemblyRuntime.InvokeAsync($"{JsType}.openAsync('{streamId}', '{fileId}')");
#endif

			if (result == null || !long.TryParse(result, out var length))
			{
				throw new InvalidOperationException("Could not create a writable stream.");
			}

			return new NativeReadStreamAdapter(streamId, length);
		}

		private NativeReadStreamAdapter(Guid streamId, long length)
		{
			_streamId = streamId;
			_length = length;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length => _length;

		public override long Position
		{
			get => _position;
			set => _position = value;
		}

		public override void Flush()
		{
			// No-op - flush is done when stream is fully closed.
		}

		public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("This stream is asynchronous-only.");

		public override long Seek(long offset, SeekOrigin origin) =>
			origin switch
			{
				SeekOrigin.Begin => Position = offset,
				SeekOrigin.Current => Position += offset,
				SeekOrigin.End => Position = Length + offset,
				_ => throw new ArgumentException("Invalid SeekOrigin value.", nameof(origin)),
			};

		public override void SetLength(long value) => throw new NotSupportedException("This stream is read-only");

		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("This stream is read-only");

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException("This stream is read-only.");

		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try
			{
				var pinnedData = handle.AddrOfPinnedObject();
				// TODO: Handle case of reading beyond end of file!
				var countReadString = await
#if NET7_0_OR_GREATER
					NativeMethods.ReadAsync(_streamId.ToString(), pinnedData, offset, count, Position);
#else
					WebAssemblyRuntime.InvokeAsync($"{JsType}.readAsync('{_streamId}', {pinnedData}, {offset}, {count}, {Position})");
#endif

				var countRead = int.Parse(countReadString, CultureInfo.InvariantCulture);
				Position += countRead;
				return countRead;
			}
			finally
			{
				handle.Free();
			}
		}

		protected override void Dispose(bool disposing)
		{
#if NET7_0_OR_GREATER
			NativeMethods.Close(_streamId.ToString());
#else
			WebAssemblyRuntime.InvokeJS($"{JsType}.close('{_streamId}')");
#endif
		}

#if NET7_0_OR_GREATER
		internal static partial class NativeMethods
		{
			private const string JsType = "globalThis.Uno.Storage.Streams.NativeFileReadStream";

			[JSImport($"{JsType}.close")]
			internal static partial void Close(string streamId);

			[JSImport($"{JsType}.openAsync")]
			internal static partial Task<string> OpenAsync(string streamId, string fileId);

			[JSImport($"{JsType}.readAsync")]
			internal static partial Task<string> ReadAsync(string streamId, nint pData, int offset, int count, double position);
		}
#endif
	}
}
