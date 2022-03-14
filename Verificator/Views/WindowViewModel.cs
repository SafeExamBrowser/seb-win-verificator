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

		private Cursor a;
		private string b, c, d;

		public Cursor Cursor
		{
			get { return a; }
			set { Set(ref a, value); }
		}

		public string InstallationInfo
		{
			get { return b; }
			set { Set(ref b, value); }
		}

		public string InstallationPath
		{
			get { return c; }
			set { Set(ref c, value); }
		}

		public string ReferencesEmptyMessage
		{
			get { return d; }
			set { Set(ref d, value); }
		}

		public bool CanChangePath { get; set; }
		public bool CanLoadReference { get; set; }
		public bool CanGenerateReference { get; set; }
		public bool CanRemoveReferences { get; set; }
		public bool CanVerify { get; set; }
		public string Title => $"SEB {nameof(Verificator)} - Version {Constants.VERSION}";

		public ObservableCollection<Installation> References { get; private set; }
		public ObservableCollection<ResultItem> Results { get; private set; }

		public DelegateCommand ChangeLocalInstallationCommand { get; private set; }
		public DelegateCommand ContentRenderedCommand { get; private set; }
		public DelegateCommand ExitCommand { get; private set; }
		public DelegateCommand GenerateReferenceCommand { get; private set; }
		public DelegateCommand LoadReferenceCommand { get; private set; }
		public DelegateCommand RemoveAllReferencesCommand { get; private set; }
		public DelegateCommand VerifyCommand { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public WindowViewModel(Algorithm algorithm, Dialog dialog, Repository repository)
		{
			this.algorithm = algorithm;
			this.dialog = dialog;
			this.repository = repository;

			ChangeLocalInstallationCommand = new DelegateCommand(ChangeLocalInstallation, () => CanChangePath);
			ContentRenderedCommand = new DelegateCommand(Initialize);
			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());
			GenerateReferenceCommand = new DelegateCommand(GenerateReference, () => CanGenerateReference);
			LoadReferenceCommand = new DelegateCommand(LoadReference, () => CanLoadReference);
			References = new ObservableCollection<Installation>();
			RemoveAllReferencesCommand = new DelegateCommand(RemoveAllReferences, () => CanRemoveReferences);
			Results = new ObservableCollection<ResultItem>();
			VerifyCommand = new DelegateCommand(Verify, () => CanVerify);
		}

		private void ChangeLocalInstallation()
		{
			if (dialog.TrySelectDirectory(out var path, "Select installation directory...") && algorithm.IsValidInstallation(path, out var platform, out var version))
			{
				UpdateLocalInstallation(path, platform, version);
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
			var installationTask = Task.Run(new Action(SearchInstallation));
			var referencesTask = Task.Run(new Action(SearchReferences));

			installationTask.ContinueWith((_) => referencesTask.ContinueWith((__) => UpdateCanVerify()));
		}

		private void LoadReference()
		{
			try
			{
				if (dialog.TrySelectFile(out var path, "Please select a reference file...") && repository.TryLoad(path, out var reference))
				{
					if (References.Any(r => r.Version == reference.Version && r.Platform == reference.Platform))
					{
						dialog.ShowError($"A reference for version {reference.Version} ({reference.Platform}) has already been loaded!");
					}
					else
					{
						References.Add(reference);
						UpdateCanVerify();
					}
				}
				else if (path != default)
				{
					dialog.ShowError($"The selected file '{path}' does not contain a valid Safe Exam Browser reference!");
				}
			}
			catch (Exception e)
			{
				dialog.ShowError($"Failed to load reference file!", e);
			}
		}

		private void RemoveAllReferences()
		{
			if (References.Count > 0 && dialog.ShowQuestion($"Would you really like to remove all {References.Count} currently loaded references?"))
			{
				References.Clear();
				UpdateCanVerify();
			}
		}

		private void SearchInstallation()
		{
			InstallationInfo = "Searching...";

			if (algorithm.TrySearchInstallation(out var path, out var platform, out var version))
			{
				UpdateLocalInstallation(path, platform, version);
			}
			else
			{
				InstallationInfo = "Could not find a Safe Exam Browser installation!";
			}

			CanChangePath = true;
			ChangeLocalInstallationCommand.RaiseCanExecuteChanged();
		}

		private void SearchReferences()
		{
			ReferencesEmptyMessage = "Loading...";

			foreach (var reference in repository.SearchReferences())
			{
				Application.Current.Dispatcher.Invoke(() => References.Add(reference));
			}

			CanLoadReference = true;
			CanRemoveReferences = true;
			LoadReferenceCommand.RaiseCanExecuteChanged();
			RemoveAllReferencesCommand.RaiseCanExecuteChanged();
			ReferencesEmptyMessage = "Could not find any installation references!";
		}

		private void UpdateCanVerify()
		{
			CanVerify = References.Count > 0 && Directory.Exists(InstallationPath);
			VerifyCommand.RaiseCanExecuteChanged();
		}

		private void UpdateLocalInstallation(string path, Platform platform, string version)
		{
			InstallationInfo = $"SEB {version} ({platform}) installed at ";
			InstallationPath = path;
			CanGenerateReference = true;
			GenerateReferenceCommand.RaiseCanExecuteChanged();
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
