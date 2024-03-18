/*
 * Copyright (c) 2023 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Verificator.Logging
{
	internal class LogContentFormatter
	{
		internal string Format(LogContent content)
		{
			if (content is LogText text)
			{
				return text.Text;
			}

			if (content is LogMessage message)
			{
				return FormatLogMessage(message);
			}

			throw new NotImplementedException($"The default formatter is not yet implemented for log content of type {content.GetType()}!");
		}

		private string FormatLogMessage(LogMessage message)
		{
			var date = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();
			var threadId = message.ThreadInfo.Id < 10 ? $"0{message.ThreadInfo.Id}" : message.ThreadInfo.Id.ToString();
			var threadName = message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty;
			var threadInfo = $"[{threadId}{threadName}]";

			return $"{date} {threadInfo} - {severity}: {message.Message}";
		}
	}
}
