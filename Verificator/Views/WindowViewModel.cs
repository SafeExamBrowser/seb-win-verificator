/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
			if (dialog.TrySelectDirectory(out var path, "Select installation directory...") && repository.IsValidInstallationPath(path))
			{
				InstallationPath = path;
				CanGenerateReference = true;
				GenerateReferenceCommand.RaiseCanExecuteChanged();
			}
			else if (path != default)
			{
				dialog.ShowError($"The selected directory '{path}' does not contain a Safe Exam Browser installation!");
			}
		}

		private void GenerateReference()
		{
			var progress = new Progress { Owner = Application.Current.MainWindow };

			Task.Run(() =>
			{
				Cursor = Cursors.Wait;
				Task.Run(() => Application.Current.Dispatcher.Invoke(() => progress.ShowDialog()));

				try
				{
					var reference = algorithm.GenerateReference(InstallationPath);

					Application.Current.Dispatcher.Invoke(() =>
					{
						if (References.All(r => r.Version != reference.Version))
						{
							References.Add(reference);
						}

						progress.Close();

						if (dialog.TrySelectDirectory(out var path, "Save reference under..."))
						{
							path = repository.Save(reference, path);
							dialog.ShowMessage($"Reference successfully saved as '{path}'.");
						}
					});

					UpdateCanVerify();
					Cursor = Cursors.Arrow;
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() => progress.Close());
					Cursor = Cursors.Arrow;
					dialog.ShowError($"Failed to generate reference for '{InstallationPath}'!", e);
				}
			});
		}

		private void Initialize()
		{
			var referencesTask = Task.Run(new Action(SearchReferences));
			var pathTask = Task.Run(new Action(SearchInstallationPath));

			pathTask.ContinueWith((_) => referencesTask.ContinueWith((__) => UpdateCanVerify()));
		}

		private void LoadReference()
		{
			try
			{
				var success = dialog.TrySelectFile(out var path, "Please select a reference file...");

				if (success && repository.TryLoadReference(path, out var reference) && References.All(r => r.Version != reference.Version))
				{
					References.Add(reference);
					UpdateCanVerify();
				}
				else if (path != default)
				{
					dialog.ShowError($"The selected file '{path}' does not contain a valid Safe Exam Browser reference!");
				}
			}
			catch (Exception e)
			{
				dialog.ShowError($"Failed to load file '{InstallationPath}'!", e);
			}
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
			var progress = new Progress { Cursor = Cursors.Wait, Owner = Application.Current.MainWindow };

			Task.Run(() =>
			{
				Cursor = Cursors.Wait;
				Task.Run(() => Application.Current.Dispatcher.Invoke(() => progress.ShowDialog()));

				Application.Current.Dispatcher.Invoke(() => Results.Clear());

				try
				{
					var results = algorithm.Verify(InstallationPath, References);
					var tampered = new List<ResultItem>();

					foreach (var item in results)
					{
						Application.Current.Dispatcher.Invoke(() => Results.Add(item));

						if (item.Status != ResultItemStatus.OK)
						{
							tampered.Add(item);
						}

						// Looks like we need to let the UI thread catch up before we can continue...
						Thread.Sleep(1);
					}

					Application.Current.Dispatcher.Invoke(() => progress.Close());
					Cursor = Cursors.Arrow;

					if (tampered.Any())
					{
						dialog.ShowError($"Verification finished, {tampered.Count} of {results.Count()} items are not okay!");
					}
					else
					{
						dialog.ShowMessage($"Verification finished, all {results.Count()} items are okay.");
					}
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() => progress.Close());
					Cursor = Cursors.Arrow;
					dialog.ShowError($"Failed to verify '{InstallationPath}'!", e);
				}
			});
		}

		private void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
		{
			property = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
