/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Verificator.Views
{
	internal class Dialog
	{
		internal bool TrySelectDirectory(out string path, string title = default)
		{
			var dialog = new CommonOpenFileDialog
			{
				EnsurePathExists = true,
				IsFolderPicker = true,
				Title = title
			};

			path = default;

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				path = dialog.FileName;
			}

			return path != default;
		}

		internal bool TrySelectFile(out string path, string title = default)
		{
			var dialog = new CommonOpenFileDialog
			{
				EnsurePathExists = true,
				EnsureFileExists = true,
				NavigateToShortcut = false,
				Title = title
			};

			dialog.Filters.Add(new CommonFileDialogFilter("SEB Reference File", Repository.REFERENCE_FILE_EXTENSION));
			path = default;

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				path = dialog.FileName;
			}

			return path != default;
		}

		internal void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		internal void ShowError(string message, Exception e, string title = "Error")
		{
			ShowError($"{message} {e.Message}{Environment.NewLine}{Environment.NewLine}{e.StackTrace}", title);
		}

		internal void ShowMessage(string message, string title = "Information")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
