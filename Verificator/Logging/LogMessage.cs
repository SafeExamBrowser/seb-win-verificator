/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Verificator.Logging
{
	internal class LogMessage : LogContent
	{
		internal DateTime DateTime { get; private set; }
		internal LogLevel Severity { get; private set; }
		internal string Message { get; private set; }
		internal ThreadInfo ThreadInfo { get; private set; }

		internal LogMessage(DateTime dateTime, LogLevel severity, string message, ThreadInfo threadInfo)
		{
			DateTime = dateTime;
			Severity = severity;
			Message = message;
			ThreadInfo = threadInfo ?? throw new ArgumentNullException(nameof(threadInfo));
		}

		public override object Clone()
		{
			return new LogMessage(DateTime, Severity, Message, ThreadInfo.Clone() as ThreadInfo);
		}
	}
}
