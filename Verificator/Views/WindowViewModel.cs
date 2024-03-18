/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Commands;
using Verificator.Data;
using Verificator.Logging;

namespace Verificator.Views
{
	internal class WindowViewModel : ILogObserver, INotifyPropertyChanged
	{
		private readonly Algorithm algorithm;
		private readonly Dialog dialog;
		private readonly Logger logger;
		private readonly Repository repository;
		private readonly SystemInfo systemInfo;

		private bool a;
		private Configuration g;
		private Cursor c;
		private string b, d, e, f, h;

		public bool AutoStart
		{
			get { return a; }
			set { Set(ref a, value); }
		}

		public string AutoStartInfo
		{
			get { return b; }
			set { Set(ref b, value); }
		}

		public string ConfigurationsMessage
		{
			get { return h; }
			set { Set(ref h, value); }
		}

		public Cursor Cursor
		{
			get { return c; }
			set { Set(ref c, value); }
		}

		public string InstallationInfo
		{
			get { return d; }
			set { Set(ref d, value); }
		}

		public string InstallationPath
		{
			get { return e; }
			set { Set(ref e, value); }
		}

		public string ReferencesMessage
		{
			get { return f; }
			set { Set(ref f, value); }
		}

		public Configuration SelectedConfiguration
		{
			get { return g; }
			set { Set(ref g, value); }
		}

		public bool CanChangePath { get; set; }
		public bool CanLoadReference { get; set; }
		public bool CanGenerateReference { get; set; }
		public bool CanRemoveReferences { get; set; }
		public bool CanVerify { get; set; }
		public string Title => $"SEB {nameof(Verificator)} - Version {Constants.VERSION}";

		public ObservableCollection<Configuration> Configurations { get; private set; }
		public ObservableCollection<LogEntry> Log { get; private set; }
		public ObservableCollection<Installation> References { get; private set; }
		public ObservableCollection<ResultItem> Results { get; private set; }

		public DelegateCommand ChangeLocalInstallationCommand { get; private set; }
		public DelegateCommand ContentRenderedCommand { get; private set; }
		public DelegateCommand ExitCommand { get; private set; }
		public DelegateCommand GenerateReferenceCommand { get; private set; }
		public DelegateCommand LoadReferenceCommand { get; private set; }
		public DelegateCommand RemoveAllReferencesCommand { get; private set; }
		public DelegateCommand SearchConfigurationsCommand { get; private set; }
		public DelegateCommand SelectedConfigurationChangedCommand { get; private set; }
		public DelegateCommand VerifyCommand { get; private set; }
		public DelegateCommand WindowClosingCommand { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public WindowViewModel(Algorithm algorithm, Dialog dialog, Logger logger, Repository repository, SystemInfo systemInfo)
		{
			this.algorithm = algorithm;
			this.dialog = dialog;
			this.logger = logger;
			this.repository = repository;
			this.systemInfo = systemInfo;

			ChangeLocalInstallationCommand = new DelegateCommand(ChangeLocalInstallation, () => CanChangePath);
			Configurations = new ObservableCollection<Configuration>();
			ContentRenderedCommand = new DelegateCommand(Initialize);
			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());
			GenerateReferenceCommand = new DelegateCommand(GenerateReference, () => CanGenerateReference);
			LoadReferenceCommand = new DelegateCommand(LoadReference, () => CanLoadReference);
			Log = new ObservableCollection<LogEntry>();
			References = new ObservableCollection<Installation>();
			RemoveAllReferencesCommand = new DelegateCommand(RemoveAllReferences, () => CanRemoveReferences);
			Results = new ObservableCollection<ResultItem>();
			SearchConfigurationsCommand = new DelegateCommand(SearchConfigurations);
			SelectedConfigurationChangedCommand = new DelegateCommand(SelectedConfigurationChanged);
			VerifyCommand = new DelegateCommand(Verify, () => CanVerify);
			WindowClosingCommand = new DelegateCommand(LogShutdownInformation);
		}

		public void Notify(LogContent content)
		{
			if (content is LogMessage message)
			{
				var severity = message.Severity.ToString().ToUpper();
				var threadId = message.ThreadInfo.Id < 10 ? $"0{message.ThreadInfo.Id}" : message.ThreadInfo.Id.ToString();
				var threadName = message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty;
				var threadInfo = $"[{threadId}{threadName}]";
				var time = message.DateTime.ToString("HH:mm:ss.fff");

				Application.Current.Dispatcher.Invoke(() => Log.Add(new LogEntry
				{
					Color = GetBrushFor(message.Severity),
					Text = $"{time} {threadInfo} - {severity}: {message.Message}"
				}));
			}

			if (content is LogText text)
			{
				Application.Current.Dispatcher.Invoke(() => Log.Add(new LogEntry { Color = GetBrushFor(text.Text), Text = text.Text }));
			}
		}

		private Brush GetBrushFor(LogLevel severity)
		{
			switch (severity)
			{
				case LogLevel.Debug:
					return Brushes.DarkGray;
				case LogLevel.Error:
					return Brushes.Red;
				case LogLevel.Warning:
					return Brushes.DarkOrange;
				default:
					return Brushes.Black;
			}
		}

		private Brush GetBrushFor(string text)
		{
			if (text.StartsWith("#"))
			{
				return Brushes.Green;
			}
			else
			{
				return Brushes.Black;
			}
		}

		private void ActivateAutoStart()
		{
			AutoStart = true;
			AutoStartInfo = $"Auto-start SEB (using '{SelectedConfiguration.RelativePath}')";
		}

		private void ChangeLocalInstallation()
		{
			logger.Debug("Attempting to change local installation...");

			if (dialog.TrySelectDirectory(out var path, "Please select an installation directory...") && algorithm.IsValidInstallation(path, out var platform, out var version))
			{
				UpdateLocalInstallation(path, platform, version);
			}
			else if (path != default)
			{
				logger.Error($"The selected directory '{path}' does not contain a Safe Exam Browser installation!");
				dialog.ShowError($"The selected directory '{path}' does not contain a Safe Exam Browser installation!");
			}
			else
			{
				logger.Debug("User aborted when prompted to select installation directory.");
			}
		}

		private void DeactivateAutoStart()
		{
			AutoStart = false;
			AutoStartInfo = "Auto-start SEB (using the local client or default configuration)";
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
						else
						{
							logger.Debug("User aborted when prompted to save reference.");
						}
					});

					UpdateCanVerify();
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() => progress.Close());
					logger.Error($"Failed to generate reference for '{InstallationPath}'!", e);
					dialog.ShowError($"Failed to generate reference for '{InstallationPath}'!", e);
				}
				finally
				{
					Cursor = Cursors.Arrow;
				}
			});
		}

		private void Initialize()
		{
			InitializeLogging();
			LogStartupInformation();

			var installationTask = Task.Run(new Action(SearchInstallation));
			var referencesTask = Task.Run(new Action(SearchReferences));

			installationTask.ContinueWith((_) => referencesTask.ContinueWith((__) =>
			{
				SearchConfigurations();
				UpdateCanVerify();
			}));
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new LogContentFormatter(), Constants.LOG_FILE);

			logFileWriter.Initialize();
			logger.LogLevel = LogLevel.Debug;
			logger.Subscribe(logFileWriter);
			logger.Subscribe(this);
		}

		private void LoadReference()
		{
			try
			{
				if (dialog.TrySelectFile(out var path, "Please select a reference file...") && repository.TryLoad(path, out var reference))
				{
					if (References.Any(r => r.Version == reference.Version && r.Platform == reference.Platform))
					{
						logger.Error($"A reference for version {reference.Version} ({reference.Platform}) has already been loaded!");
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
				else
				{
					logger.Debug("User aborted when prompted to select reference file.");
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to load reference file!", e);
				dialog.ShowError("Failed to load reference file!", e);
			}
		}

		private void LogStartupInformation()
		{
			logger.Log($"                                 SEB {nameof(Verificator)}, Version {Constants.VERSION}, Build {Constants.BUILD}");
			logger.Log($"                    Copyright © 2024 ETH Zürich, IT Services");
			logger.Log(string.Empty);
			logger.Log($"# Application started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
			logger.Log($"# Running on {systemInfo.OperatingSystemInfo} with user '{Environment.UserName}'");
			logger.Log($"# Computer '{systemInfo.Name}' is a {systemInfo.Model} manufactured by {systemInfo.Manufacturer}");
			logger.Log(string.Empty);
		}

		private void LogShutdownInformation()
		{
			logger.Log(string.Empty);
			logger.Log($"# Application terminated at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
			logger.Log(string.Empty);
		}

		private void RemoveAllReferences()
		{
			if (References.Count > 0 && dialog.ShowQuestion($"Would you really like to remove all {References.Count} currently loaded references?"))
			{
				References.Clear();
				UpdateCanVerify();
			}
		}

		private void SearchConfigurations()
		{
			ConfigurationsMessage = "Searching...";

			if (repository.TrySearchConfigurations(out var configurations))
			{
				Application.Current.Dispatcher.Invoke(Configurations.Clear);
				SelectedConfiguration = default;

				foreach (var configuration in configurations)
				{
					Application.Current.Dispatcher.Invoke(() => Configurations.Add(configuration));
				}

				SelectedConfiguration = Configurations.First();
				ActivateAutoStart();
			}
			else
			{
				ConfigurationsMessage = "Could not find any configuration files.";
				DeactivateAutoStart();
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
			ReferencesMessage = "Loading...";

			foreach (var reference in repository.SearchReferences())
			{
				Application.Current.Dispatcher.Invoke(() => References.Add(reference));
			}

			CanLoadReference = true;
			CanRemoveReferences = true;
			LoadReferenceCommand.RaiseCanExecuteChanged();
			RemoveAllReferencesCommand.RaiseCanExecuteChanged();
			ReferencesMessage = "Could not find any installation references!";
		}

		private void SelectedConfigurationChanged()
		{
			if (SelectedConfiguration != default)
			{
				ActivateAutoStart();
			}
			else
			{
				DeactivateAutoStart();
			}
		}

		private void StartSafeExamBrowser()
		{
			var process = new Process();

			process.StartInfo.Arguments = $"{'"' + SelectedConfiguration?.AbsolutePath + '"'}";
			process.StartInfo.FileName = algorithm.GetMainExecutable(InstallationPath);
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

			logger.Info($"Starting SEB with {(SelectedConfiguration == default ? "local client or default configuration" : $"'{SelectedConfiguration.AbsolutePath}'")}.");

			process.Start();
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

			logger.Info("Starting verification...");

			Task.Run(() =>
			{
				Cursor = Cursors.Wait;
				Task.Run(() => Application.Current.Dispatcher.Invoke(() => progress.ShowDialog()));

				Application.Current.Dispatcher.Invoke(() => Results.Clear());

				try
				{
					var results = algorithm.Verify(InstallationPath, References).ToList();
					var tampered = new List<ResultItem>();

					foreach (var item in results)
					{
						Application.Current.Dispatcher.Invoke(() => Results.Add(item));

						if (item.Status != ResultItemStatus.OK)
						{
							tampered.Add(item);
						}
					}

					Application.Current.Dispatcher.Invoke(() => progress.Close());
					Cursor = Cursors.Arrow;

					if (tampered.Any())
					{
						logger.Error($"Verification finished, {tampered.Count} of {results.Count()} items are not okay!");
						dialog.ShowError($"Verification finished, {tampered.Count} of {results.Count()} items are not okay!");
					}
					else if (AutoStart)
					{
						StartSafeExamBrowser();
						Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
					}
					else
					{
						logger.Info($"Verification finished, all {results.Count()} items are okay.");
						dialog.ShowMessage($"Verification finished, all {results.Count()} items are okay.");
					}
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() => progress.Close());
					Cursor = Cursors.Arrow;
					logger.Error($"Failed to verify '{InstallationPath}'!", e);
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
