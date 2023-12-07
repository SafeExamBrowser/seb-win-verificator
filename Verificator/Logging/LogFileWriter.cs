/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Text;

namespace Verificator.Logging
{
	internal class LogFileWriter : ILogObserver
	{
		private readonly object @lock = new object();
		private readonly string filePath;
		private readonly LogContentFormatter formatter;

		internal LogFileWriter(LogContentFormatter formatter, string filePath)
		{
			this.filePath = filePath;
			this.formatter = formatter;
		}

		public void Notify(LogContent content)
		{
			lock (@lock)
			{
				var raw = formatter.Format(content);

				using (var stream = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					stream.WriteLine(raw);
				}
			}
		}

		internal void Initialize()
		{
			var directory = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}
	}
}
