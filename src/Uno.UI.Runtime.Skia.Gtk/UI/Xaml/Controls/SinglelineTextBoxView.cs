﻿#nullable enable

using System;
using Gtk;
using Uno.Disposables;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Runtime.Skia.UI.Xaml.Controls;

internal class SinglelineTextBoxView : GtkTextBoxView
{
	private readonly Entry _entry = new();
	private readonly bool _isPassword;

	public SinglelineTextBoxView(bool isPassword)
	{
		_isPassword = isPassword;
	}

	protected override Widget InputWidget => _entry;

	protected override Widget RootWidget => _entry;

	public override string Text
	{
		get => _entry.Text;
		set => _entry.Text = value;
	}

	public override bool IsCompatible(TextBox textBox) => !textBox.AcceptsReturn || textBox is PasswordBox;

	public override (int start, int length) Selection
	{
		get
		{
			_entry.GetSelectionBounds(out var start, out var end);
			return (start, end - start);
		}
		set
		{
			_entry.SelectRegion(value.start, value.start + value.length);
		}
	}

	public override void UpdateProperties(TextBox textBox)
	{
		base.UpdateProperties(textBox);

		_entry.IsEditable = !textBox.IsReadOnly;
		_entry.MaxLength = textBox.MaxLength;
		if (_isPassword && textBox is PasswordBox passwordBox)
		{
			_entry.Visibility = passwordBox.PasswordRevealMode == PasswordRevealMode.Visible;
		}
	}

	public override IDisposable ObserveTextChanges(EventHandler onChanged)
	{
		_entry.Changed += onChanged;
		return Disposable.Create(() => _entry.Changed -= onChanged);
	}
}
