/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;

namespace Verificator.Views
{
	internal class Dialog
	{
		internal void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		internal void ShowError(string message, Exception e, string title = "Error")
		{
			ShowError($"{message}{Environment.NewLine}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.StackTrace}", title);
		}

		internal void ShowMessage(string message, string title = "Information")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
