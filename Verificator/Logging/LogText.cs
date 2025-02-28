/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Verificator.Logging
{
	internal class LogText : LogContent
	{
		internal string Text { get; private set; }

		internal LogText(string text)
		{
			Text = text;
		}

		public override object Clone()
		{
			return new LogText(Text);
		}
	}
}
