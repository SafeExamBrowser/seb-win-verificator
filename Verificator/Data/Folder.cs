/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Verificator.Data
{
	[Serializable]
	public class Folder
	{
		public List<File> Files { get; set; }
		public List<Folder> Folders { get; set; }

		[XmlAttribute]
		public string Path { get; set; }

		public Folder()
		{
			Files = new List<File>();
			Folders = new List<Folder>();
		}
	}
}
