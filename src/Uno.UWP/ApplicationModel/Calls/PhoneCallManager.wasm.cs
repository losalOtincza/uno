﻿#if __WASM__
using System;
using System.Collections.Generic;
using System.Text;

#if NET7_0_OR_GREATER
using NativeMethods = __Windows.ApplicationModel.Calls.PhoneCallManager.NativeMethods;
#endif

namespace Windows.ApplicationModel.Calls
{
	public partial class PhoneCallManager
	{
		private static void ShowPhoneCallUIImpl(string phoneNumber, string displayName)
		{
			var uri = new Uri($"tel:{phoneNumber}");
			var command = $"Uno.UI.WindowManager.current.open(\"{uri.AbsoluteUri}\");";
			Uno.Foundation.WebAssemblyRuntime.InvokeJS(command);
		}
	}
}
#endif
