﻿#if __WASM__
using System;
using System.Threading.Tasks;
using Uno.Foundation;

#if NET7_0_OR_GREATER
using NativeMethods = __Windows.__System.Launcher.NativeMethods;
#endif

namespace Windows.System
{
	public static partial class Launcher
	{
		public static Task<bool> LaunchUriPlatformAsync(Uri uri)
		{
			var command = $"Uno.UI.WindowManager.current.open(\"{uri.OriginalString}\");";
			var result = WebAssemblyRuntime.InvokeJS(command);
			return Task.FromResult(result == "True");
		}
	}
}
#endif
