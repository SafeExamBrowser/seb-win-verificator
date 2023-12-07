/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Verificator
{
	internal static class Constants
	{
		internal const string CONFIGURATION_FILE_EXTENSION = "seb";
		internal const string REFERENCE_FILE_EXTENSION = "sebref";

		internal static readonly string BUILD = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
		internal static readonly string LOG_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs", $"{DateTime.Today:yyyy-MM-dd}.log");
		internal static readonly string VERSION = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
	}
}
