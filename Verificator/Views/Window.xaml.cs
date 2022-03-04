/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Specialized;
using Verificator.Data;

namespace Verificator.Views
{
	public partial class Window : System.Windows.Window
	{
		public Window()
		{
			var viewModel = new WindowViewModel(new Algorithm(), new Dialog(), new Repository());

			InitializeComponent();

			DataContext = viewModel;
			viewModel.Results.CollectionChanged += Results_CollectionChanged;
		}

		private void Results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResultListView.Items.MoveCurrentToLast();
			ResultListView.ScrollIntoView(ResultListView.Items.CurrentItem);
		}
	}
}
