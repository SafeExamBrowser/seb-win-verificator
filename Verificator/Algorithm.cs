/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Verificator.Data;

namespace Verificator
{
	internal class Algorithm
	{
		internal Installation GenerateReference(string path)
		{
			var root = new DirectoryInfo(path);
			var reference = new Installation();
			var mainExecutable = GetMainExecutable(path);

			reference.Info = $"Generated at {DateTime.Now}";
			reference.Root = AnalyzeDirectory(root, root.FullName);
			reference.Version = FileVersionInfo.GetVersionInfo(mainExecutable).FileVersion;

			return reference;
		}

		internal IEnumerable<ResultItem> Verify(string installationPath, IEnumerable<Installation> references)
		{
			var mainExecutable = GetMainExecutable(installationPath);
			var installedVersion = FileVersionInfo.GetVersionInfo(mainExecutable).FileVersion;
			var reference = references.FirstOrDefault(r => installedVersion.Equals(r.Version, StringComparison.OrdinalIgnoreCase));

			if (reference != default)
			{
				var installation = new Installation();
				var root = AnalyzeDirectory(new DirectoryInfo(installationPath), installationPath);

				return Compare(root, reference.Root).Skip(1);
			}
			else
			{
				throw new InvalidOperationException($"No reference found for installed version {installedVersion}!");
			}
		}

		private InstallationFolder AnalyzeDirectory(DirectoryInfo directory, string rootPath)
		{
			var folder = new InstallationFolder
			{
				Path = directory.FullName.Replace(rootPath, "")
			};

			foreach (var subdirectory in directory.GetDirectories())
			{
				folder.Add(AnalyzeDirectory(subdirectory, rootPath));
			}

			foreach (var file in directory.GetFiles())
			{
				folder.Add(new InstallationFile
				{
					// TODO: Checksum = ,
					// TODO: OriginalName = ,
					Path = file.FullName.Replace(rootPath, ""),
					// TODO: Signature = new X509Certificate2(file.FullName).,
					Size = file.Length,
					Version = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion
				});
			}

			return folder;
		}

		private string GetMainExecutable(string rootPath)
		{
			return Path.Combine(rootPath, "Application", "SafeExamBrowser.exe");
		}

		private IEnumerable<ResultItem> Compare(InstallationFolder installed, InstallationFolder reference)
		{
			var installedFolders = installed.Folders;
			var installedFiles = installed.Files;
			var status = installed == default ? ResultItemStatus.Missing : (reference == default ? ResultItemStatus.Added : ResultItemStatus.OK);

			yield return new ResultItem
			{
				Path = installed?.Path ?? reference.Path,
				Remarks = BuildRemarks(ResultItemType.Folder, status),
				Status = status,
				Type = ResultItemType.Folder
			};

			foreach (var referenceFolder in reference?.Folders)
			{
				var installedFolder = installedFolders.FirstOrDefault(f => f.Path.Equals(referenceFolder.Path, StringComparison.OrdinalIgnoreCase));

				foreach (var result in Compare(installedFolder, referenceFolder))
				{
					yield return result;
				}

				installedFolders.Remove(installedFolder);
			}

			foreach (var folder in installedFolders)
			{
				foreach (var result in Compare(folder, default))
				{
					yield return result;
				}
			}

			foreach (var referenceFile in reference?.Files)
			{
				var installedFile = installedFiles.FirstOrDefault(f => f.Path.Equals(referenceFile.Path, StringComparison.OrdinalIgnoreCase));

				yield return Compare(installedFile, referenceFile);

				installedFiles.Remove(installedFile);
			}

			foreach (var file in installedFiles)
			{
				yield return Compare(file, default);
			}
		}

		private ResultItem Compare(InstallationFile installed, InstallationFile reference)
		{
			var details = default(string);
			var status = installed == default ? ResultItemStatus.Missing : (reference == default ? ResultItemStatus.Added : ResultItemStatus.OK);

			if (installed != default && reference != default)
			{
				// TODO: Validate!!
				status = ResultItemStatus.Changed;
			}

			return new ResultItem
			{
				Path = installed?.Path ?? reference.Path,
				Remarks = BuildRemarks(ResultItemType.Folder, status, details),
				Status = status,
				Type = ResultItemType.File
			};
		}

		private string BuildRemarks(ResultItemType type, ResultItemStatus status, string details = default)
		{
			var itemName = type == ResultItemType.File ? "file" : "folder";

			switch (status)
			{
				case ResultItemStatus.Added:
					return $"This {itemName} has been added!";
				case ResultItemStatus.Changed:
					return $"This {itemName} has been changed! {details}";
				case ResultItemStatus.Missing:
					return $"This {itemName} is missing!";
				default:
					return "";
			}
		}
	}
}
