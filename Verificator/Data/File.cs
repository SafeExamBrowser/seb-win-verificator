/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Verificator.Data
{
	[Serializable]
	public class File
	{
		public string Checksum { get; set; }
		public string OriginalName { get; set; }
		public string Path { get; set; }
		public string Signature { get; set; }
		public long Size { get; set; }
		public string Version { get; set; }
	}
}
