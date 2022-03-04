/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Verificator.Data
{
	internal class InstallationFolder
	{
		private readonly List<InstallationFile> files;
		private readonly List<InstallationFolder> folders;

		internal IList<InstallationFile> Files => new List<InstallationFile>(files);
		internal IList<InstallationFolder> Folders => new List<InstallationFolder>(folders);
		internal string Path { get; set; }

		internal InstallationFolder()
		{
			files = new List<InstallationFile>();
			folders = new List<InstallationFolder>();
		}

		internal void Add(InstallationFile file)
		{
			files.Add(file);
		}

		internal void Add(InstallationFolder folder)
		{
			folders.Add(folder);
		}
	}
}
