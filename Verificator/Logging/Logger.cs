/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Verificator.Logging
{
	internal class Logger
	{
		private readonly object @lock = new object();
		private readonly IList<LogContent> log = new List<LogContent>();
		private readonly IList<ILogObserver> observers = new List<ILogObserver>();

		internal LogLevel LogLevel { get; set; }

		internal void Debug(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (LogLevel <= LogLevel.Debug)
			{
				Add(LogLevel.Debug, message);
			}
		}

		internal void Info(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (LogLevel <= LogLevel.Info)
			{
				Add(LogLevel.Info, message);
			}
		}

		internal void Warn(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (LogLevel <= LogLevel.Warning)
			{
				Add(LogLevel.Warning, message);
			}
		}

		internal void Error(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (LogLevel <= LogLevel.Error)
			{
				Add(LogLevel.Error, message);
			}
		}

		internal void Error(string message, Exception exception)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			var details = new StringBuilder();

			details.AppendLine();
			details.AppendLine($"   Exception Message: {exception.Message}");
			details.AppendLine($"   Exception Type: {exception.GetType()}");

			if (exception.StackTrace != null)
			{
				details.AppendLine();
				details.AppendLine(exception.StackTrace);
			}

			for (var inner = exception.InnerException; inner != null; inner = inner.InnerException)
			{
				details.AppendLine();
				details.AppendLine($"   Inner Exception Message: {inner.Message}");
				details.AppendLine($"   Inner Exception Type: {inner.GetType()}");

				if (inner.StackTrace != null)
				{
					details.AppendLine();
					details.AppendLine(inner.StackTrace);
				}
			}

			if (LogLevel <= LogLevel.Error)
			{
				Add(LogLevel.Error, message);
				Add(new LogText(details.ToString()));
			}
		}

		internal void Log(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			Add(new LogText(text));
		}

		internal IList<LogContent> GetLog()
		{
			lock (@lock)
			{
				return log.Select(m => m.Clone() as LogContent).ToList();
			}
		}

		internal void Subscribe(ILogObserver observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			lock (@lock)
			{
				if (!observers.Contains(observer))
				{
					observers.Add(observer);
				}
			}
		}

		internal void Unsubscribe(ILogObserver observer)
		{
			lock (@lock)
			{
				observers.Remove(observer);
			}
		}

		private void Add(LogLevel severity, string message)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var threadName = Thread.CurrentThread.Name;
			var threadInfo = new ThreadInfo(threadId, threadName);

			Add(new LogMessage(DateTime.Now, severity, message, threadInfo));
		}

		private void Add(LogContent content)
		{
			lock (@lock)
			{
				log.Add(content);

				foreach (var observer in observers)
				{
					observer.Notify(content);
				}
			}
		}
	}
}
