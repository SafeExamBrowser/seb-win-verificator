/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Verificator.Data;

namespace Verificator.Views
{
	internal class WindowViewModel : INotifyPropertyChanged
	{
		private readonly Algorithm algorithm;
		private readonly Dialog dialog;
		private readonly Repository repository;

		private string a, b;
		private Cursor c;

		public Cursor Cursor
		{
			get { return c; }
			set { Set(ref c, value); }
		}

		public string InstallationPath
		{
			get { return a; }
			set { Set(ref a, value); }
		}

		public string ReferencesEmptyMessage
		{
			get { return b; }
			set { Set(ref b, value); }
		}

		public bool CanChangePath { get; set; }
		public bool CanLoadReference { get; set; }
		public bool CanGenerateReference { get; set; }
		public bool CanVerify { get; set; }
		public string Title => $"SEB {nameof(Verificator)} - Version {Repository.VERSION}";

		public ObservableCollection<Installation> References { get; private set; }
		public ObservableCollection<ResultItem> Results { get; private set; }

		public DelegateCommand ChangeInstallationPathCommand { get; private set; }
		public DelegateCommand ContentRenderedCommand { get; private set; }
		public DelegateCommand ExitCommand { get; private set; }
		public DelegateCommand GenerateReferenceCommand { get; private set; }
		public DelegateCommand LoadReferenceCommand { get; private set; }
		public DelegateCommand VerifyCommand { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public WindowViewModel(Algorithm algorithm, Dialog dialog, Repository repository)
		{
			this.algorithm = algorithm;
			this.dialog = dialog;
			this.repository = repository;

			ChangeInstallationPathCommand = new DelegateCommand(ChangeInstallationPath, () => CanChangePath);
			ContentRenderedCommand = new DelegateCommand(Initialize);
			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());
			GenerateReferenceCommand = new DelegateCommand(GenerateReference, () => CanGenerateReference);
			LoadReferenceCommand = new DelegateCommand(LoadReference, () => CanLoadReference);
			References = new ObservableCollection<Installation>();
			Results = new ObservableCollection<ResultItem>();
			VerifyCommand = new DelegateCommand(Verify, () => CanVerify);
		}

		private void ChangeInstallationPath()
		{
			// TODO
			// var path = dialog.SelectFolder();
			// InstallationPath = path;
		}

		private void GenerateReference()
		{
			try
			{
				var reference = algorithm.GenerateReference(InstallationPath);
				// TODO: Show dialog to select path! var path = repository.Save(reference);

				References.Add(reference);

				// dialog.ShowMessage($"File successfully saved as '{path}'.");
				UpdateCanVerify();
			}
			catch (Exception e)
			{
				dialog.ShowError($"Failed to generate reference for '{InstallationPath}'!", e);
			}
		}

		private void Initialize()
		{
			var referencesTask = Task.Run(new Action(SearchReferences));
			var pathTask = Task.Run(new Action(SearchInstallationPath));

			pathTask.ContinueWith((_) => referencesTask.ContinueWith((__) => UpdateCanVerify()));
		}

		private void LoadReference()
		{
			// TODO
			// var file = dialog.SelectFile();
			// if (repository.TryLoadReference(file))
			// {

			// }
		}

		private void SearchReferences()
		{
			ReferencesEmptyMessage = "Loading...";

			foreach (var reference in repository.SearchReferences())
			{
				Application.Current.Dispatcher.Invoke(() => References.Add(reference));
			}

			CanLoadReference = true;
			LoadReferenceCommand.RaiseCanExecuteChanged();
			ReferencesEmptyMessage = "Could not find any installation references!";
		}

		private void SearchInstallationPath()
		{
			InstallationPath = "Searching...";

			if (repository.TrySearchInstallationPath(out var path))
			{
				InstallationPath = path;
				CanGenerateReference = true;
				GenerateReferenceCommand.RaiseCanExecuteChanged();
			}
			else
			{
				InstallationPath = "Could not find a Safe Exam Browser installation!";
			}

			CanChangePath = true;
			ChangeInstallationPathCommand.RaiseCanExecuteChanged();
		}

		private void UpdateCanVerify()
		{
			CanVerify = References.Count > 0 && Directory.Exists(InstallationPath);
			VerifyCommand.RaiseCanExecuteChanged();
		}

		private void Verify()
		{
			Results.Clear();
			Cursor = Cursors.Wait;

			try
			{
				var results = algorithm.Verify(InstallationPath, References);

				foreach (var item in results)
				{
					Results.Add(item);
				}

				dialog.ShowMessage($"Finished verification of '{InstallationPath}'.");
			}
			catch (Exception e)
			{
				dialog.ShowError($"Failed to verify '{InstallationPath}'!", e);
			}

			Cursor = Cursors.Arrow;
		}

		private void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
		{
			property = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
