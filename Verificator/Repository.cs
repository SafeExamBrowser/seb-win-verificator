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
using Verificator.Data;

namespace Verificator
{
	internal class Repository
	{
		internal static readonly string VERSION = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

		internal string Save(Installation reference)
		{
			// TODO!
			return string.Empty;
		}

		internal IEnumerable<Installation> SearchReferences()
		{
			return Enumerable.Empty<Installation>();
		}

		internal bool TrySearchInstallationPath(out string path)
		{
			path = default;

			var programFilesX64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

			foreach (var directory in Directory.GetDirectories(programFilesX64).Concat(Directory.GetDirectories(programFilesX86)))
			{
				if (Path.GetFileName(directory).Equals("SafeExamBrowser", StringComparison.OrdinalIgnoreCase))
				{
					path = directory;

					break;
				}
			}

			return path != default;
		}
	}
}
