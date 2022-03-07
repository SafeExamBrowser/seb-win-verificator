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
	internal class Folder
	{
		private readonly List<File> files;
		private readonly List<Folder> folders;

		internal IList<File> Files => new List<File>(files);
		internal IList<Folder> Folders => new List<Folder>(folders);
		internal string Path { get; set; }

		internal Folder()
		{
			files = new List<File>();
			folders = new List<Folder>();
		}

		internal void Add(File file)
		{
			files.Add(file);
		}

		internal void Add(Folder folder)
		{
			folders.Add(folder);
		}
	}
}
