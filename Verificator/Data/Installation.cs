/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Verificator.Data
{
	[Serializable]
	public class Installation
	{
		public string Info { get; set; }
		public Platform Platform { get; set; }
		public Folder Root { get; set; }
		public string Version { get; set; }
	}
}
