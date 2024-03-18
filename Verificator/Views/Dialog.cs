/*
 * Copyright (c) 2023 ETH Zürich, IT Services
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
		internal void ShowError(string message, string title = "Error")
		{
			Application.Current.Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error));
		}

		internal void ShowError(string message, Exception e, string title = "Error")
		{
			ShowError($"{message} {e.Message}{Environment.NewLine}{Environment.NewLine}{e.StackTrace}", title);
		}

		internal void ShowMessage(string message, string title = "Information")
		{
			Application.Current.Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information));
		}

		internal bool ShowQuestion(string message, string title = "Question")
		{
			return Application.Current.Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)) == MessageBoxResult.Yes;
		}

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

			dialog.Filters.Add(new CommonFileDialogFilter("SEB Reference File", Constants.REFERENCE_FILE_EXTENSION));
			path = default;

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				path = dialog.FileName;
			}

			return path != default;
		}
	}
}
