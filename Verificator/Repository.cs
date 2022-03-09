/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Verificator.Data;
using File = System.IO.File;

namespace Verificator
{
	internal class Repository
	{
		internal const string REFERENCE_FILE_EXTENSION = "sebref";
		internal static readonly string VERSION = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

		internal bool IsValidInstallationPath(string path)
		{
			return Directory.Exists(path) && Path.GetFileName(path).Equals("SafeExamBrowser", StringComparison.OrdinalIgnoreCase);
		}

		internal string Save(Installation reference, string path)
		{
			var filePath = Path.Combine(path, $"SEB_{reference.Version}.{REFERENCE_FILE_EXTENSION}");
			var serializer = new XmlSerializer(typeof(Installation));

			using (var stream = File.OpenWrite(filePath))
			{
				serializer.Serialize(stream, reference);
			}

			return filePath;
		}

		internal IEnumerable<Installation> SearchReferences()
		{
			// TODO
			return Enumerable.Empty<Installation>();
		}

		internal bool TryLoadReference(string path, out Installation reference)
		{
			var serializer = new XmlSerializer(typeof(Installation));

			using (var stream = File.OpenRead(path))
			{
				reference = serializer.Deserialize(stream) as Installation;
			}

			return reference != default;
		}

		internal bool TrySearchInstallationPath(out string path)
		{
			path = default;

			var programFilesX64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

			foreach (var directory in Directory.GetDirectories(programFilesX64).Concat(Directory.GetDirectories(programFilesX86)))
			{
				if (IsValidInstallationPath(directory))
				{
					path = directory;

					break;
				}
			}

			return path != default;
		}
	}
}
