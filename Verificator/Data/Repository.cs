/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;

namespace Verificator.Data
{
	internal class Repository
	{
		internal bool TrySearchInstallationPath(out string path)
		{
			path = default(string);

			// TODO: Admin rights required to search program files?
			// TODO: Use framework with localized program files names!

			return false;
		}

		internal IEnumerable<Reference> SearchReferences()
		{
			return Enumerable.Empty<Reference>();
		}

		internal bool Verify(string installationPath)
		{
			return false;
		}
	}
}
