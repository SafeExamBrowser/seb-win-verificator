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
using System.Security.Cryptography;
using Verificator.Data;
using File = Verificator.Data.File;

namespace Verificator
{
	internal class Algorithm
	{
		internal Installation GenerateReference(string path)
		{
			var mainExecutable = GetMainExecutable(path);
			var root = new DirectoryInfo(path);
			var reference = new Installation();

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

		private Folder AnalyzeDirectory(DirectoryInfo directory, string rootPath)
		{
			var folder = new Folder
			{
				Path = directory.FullName.Replace(rootPath, "")
			};

			foreach (var subdirectory in directory.GetDirectories())
			{
				folder.Add(AnalyzeDirectory(subdirectory, rootPath));
			}

			foreach (var file in directory.GetFiles())
			{
				var checksum = default(string);
				var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);

				using (var stream = System.IO.File.OpenRead(file.FullName))
				using (var algorithm = new SHA256Managed())
				{
					checksum = BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty);
				}

				folder.Add(new File
				{
					Checksum = checksum,
					OriginalName = versionInfo.OriginalFilename,
					Path = file.FullName.Replace(rootPath, ""),
					// TODO: Signature = new X509Certificate2(file.FullName).,
					Size = file.Length,
					Version = versionInfo.FileVersion
				});
			}

			return folder;
		}

		private IEnumerable<ResultItem> Compare(Folder installed, Folder reference)
		{
			var installedFolders = installed?.Folders ?? new List<Folder>();
			var installedFiles = installed?.Files ?? new List<File>();
			var referenceFolders = reference?.Folders ?? new List<Folder>();
			var referenceFiles = reference?.Files ?? new List<File>();
			var status = installed == default ? ResultItemStatus.Missing : (reference == default ? ResultItemStatus.Added : ResultItemStatus.OK);

			yield return new ResultItem
			{
				Path = installed?.Path ?? reference.Path,
				Remarks = BuildRemarks(ResultItemType.Folder, status),
				Status = status,
				Type = ResultItemType.Folder
			};

			foreach (var referenceFolder in referenceFolders)
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

			foreach (var referenceFile in referenceFiles)
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

		private ResultItem Compare(File installed, File reference)
		{
			var details = "";
			var status = installed == default ? ResultItemStatus.Missing : (reference == default ? ResultItemStatus.Added : ResultItemStatus.OK);

			if (installed != default && reference != default)
			{
				if (!installed.Checksum.Equals(reference.Checksum, StringComparison.OrdinalIgnoreCase))
				{
					details += $"Checksum '{installed.Checksum}' is not '{reference.Checksum}'! ";
					status = ResultItemStatus.Changed;
				}

				if (installed.OriginalName?.Equals(reference.OriginalName, StringComparison.OrdinalIgnoreCase) == false)
				{
					details += $"Original name '{installed.OriginalName}' is not '{reference.OriginalName}'! ";
					status = ResultItemStatus.Changed;
				}

				if (installed.Size != reference.Size)
				{
					details += $"Size '{installed.Size}' is not '{reference.Size}'! ";
					status = ResultItemStatus.Changed;
				}

				if (installed.Version?.Equals(reference.Version, StringComparison.OrdinalIgnoreCase) == false)
				{
					details += $"Version '{installed.Version}' is not '{reference.Version}'! ";
					status = ResultItemStatus.Changed;
				}
			}

			return new ResultItem
			{
				Path = installed?.Path ?? reference.Path,
				Remarks = BuildRemarks(ResultItemType.File, status, details),
				Status = status,
				Type = ResultItemType.File
			};
		}

		private string GetMainExecutable(string rootPath)
		{
			return Path.Combine(rootPath, "Application", "SafeExamBrowser.exe");
		}

		private string BuildRemarks(ResultItemType type, ResultItemStatus status, string details = default)
		{
			var item = type == ResultItemType.File ? "file" : "folder";

			switch (status)
			{
				case ResultItemStatus.Added:
					return $"This {item} has been added!";
				case ResultItemStatus.Changed:
					return $"This {item} has been changed! {details}";
				case ResultItemStatus.Missing:
					return $"This {item} is missing!";
				default:
					return "";
			}
		}
	}
}
