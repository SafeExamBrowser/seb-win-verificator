/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Prism.Commands;
using Verificator.Data;

namespace Verificator.Views
{
	internal class WindowViewModel : INotifyPropertyChanged
	{
		private static readonly string VERSION = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

		private readonly Repository repository;

		private string ip, rem;

		public string InstallationPath
		{
			get { return ip; }
			set { Set(ref ip, value); }
		}

		public string ReferencesEmptyMessage
		{
			get { return rem; }
			set { Set(ref rem, value); }
		}

		public bool CanAddReference { get; set; }
		public bool CanChangePath { get; set; }
		public bool CanVerify { get; set; }
		public string Title => $"SEB {nameof(Verificator)} - Version {VERSION}";

		public ObservableCollection<Reference> References { get; private set; }
		public ObservableCollection<ResultItem> Results { get; private set; }

		public DelegateCommand AddReferenceCommand { get; private set; }
		public DelegateCommand ChangeInstallationPathCommand { get; private set; }
		public DelegateCommand ContentRenderedCommand { get; private set; }
		public DelegateCommand VerifyCommand { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public WindowViewModel(Repository repository)
		{
			this.repository = repository;

			AddReferenceCommand = new DelegateCommand(AddReference, () => CanAddReference);
			ChangeInstallationPathCommand = new DelegateCommand(ChangeInstallationPath, () => CanChangePath);
			ContentRenderedCommand = new DelegateCommand(Initialize);
			References = new ObservableCollection<Reference>();
			Results = new ObservableCollection<ResultItem>();
			VerifyCommand = new DelegateCommand(Verify, () => CanVerify);
		}

		private void AddReference()
		{
			// TODO
			// var file = dialog.SelectFile();
			// if (repository.TryLoadReference(file))
			// {
				
			// }
		}

		private void ChangeInstallationPath()
		{
			// TODO
			// var path = dialog.SelectFolder();
			// InstallationPath = path;
		}

		private void Initialize()
		{
			var referencesTask = Task.Run(() =>
			{
				ReferencesEmptyMessage = "Loading...";

				foreach (var reference in repository.SearchReferences())
				{
					System.Windows.Application.Current.Dispatcher.Invoke(() => References.Add(reference));
				}

				CanAddReference = true;
				AddReferenceCommand.RaiseCanExecuteChanged();
				ReferencesEmptyMessage = "Could not find any installation references!";
			});

			var pathTask = Task.Run(() =>
			{
				InstallationPath = "Searching...";

				if (repository.TrySearchInstallationPath(out var path))
				{
					InstallationPath = path;
				}
				else
				{
					InstallationPath = "Could not find a Safe Exam Browser installation!";
				}

				CanChangePath = true;
				ChangeInstallationPathCommand.RaiseCanExecuteChanged();
			});

			pathTask.ContinueWith((_) => referencesTask.ContinueWith((__) =>
			{
				CanVerify = true;
				VerifyCommand.RaiseCanExecuteChanged();
			}));
		}

		private void Verify()
		{
			// TODO
		}

		private void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
		{
			property = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
