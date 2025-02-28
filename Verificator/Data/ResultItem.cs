/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Verificator.Data
{
	internal class ResultItem
	{
		public string Path { get; set; }
		public string Remarks { get; set; }
		public ResultItemStatus Status { get; set; }
		public ResultItemType Type { get; set; }
	}
}
