/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Management;

namespace Verificator.Data
{
	internal class SystemInfo
	{
		public string Manufacturer { get; private set; }
		public string Model { get; private set; }
		public string Name { get; private set; }
		public OperatingSystem OperatingSystem { get; private set; }
		public string OperatingSystemInfo => $"{OperatingSystemName()}, {Environment.OSVersion.VersionString} ({Architecture()})";
		public string[] PlugAndPlayDeviceIds { get; private set; }

		public SystemInfo()
		{
			InitializeMachineInfo();
			InitializeOperatingSystem();
		}

		private string Architecture()
		{
			return Environment.Is64BitOperatingSystem ? "x64" : "x86";
		}

		private void InitializeMachineInfo()
		{
			var model = default(string);
			var systemFamily = default(string);

			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
				using (var results = searcher.Get())
				using (var system = results.Cast<ManagementObject>().First())
				{
					foreach (var property in system.Properties)
					{
						if (property.Name.Equals("Manufacturer"))
						{
							Manufacturer = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("Model"))
						{
							model = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("Name"))
						{
							Name = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("SystemFamily"))
						{
							systemFamily = Convert.ToString(property.Value);
						}
					}
				}

				Model = $"{systemFamily} {model}";
			}
			catch (Exception)
			{
				Manufacturer = "";
				Model = "";
				Name = "";
			}
		}

		private void InitializeOperatingSystem()
		{
			// IMPORTANT:
			// In order to be able to retrieve the correct operating system version via System.Environment.OSVersion,
			// the executing assembly needs to define an application manifest specifying all supported Windows versions!
			var major = Environment.OSVersion.Version.Major;
			var minor = Environment.OSVersion.Version.Minor;
			var build = Environment.OSVersion.Version.Build;

			// See https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions for mapping source...
			if (major == 6)
			{
				if (minor == 1)
				{
					OperatingSystem = OperatingSystem.Windows7;
				}
				else if (minor == 2)
				{
					OperatingSystem = OperatingSystem.Windows8;
				}
				else if (minor == 3)
				{
					OperatingSystem = OperatingSystem.Windows8_1;
				}
			}
			else if (major == 10)
			{
				if (build < 22000)
				{
					OperatingSystem = OperatingSystem.Windows10;
				}
				else
				{
					OperatingSystem = OperatingSystem.Windows11;
				}
			}
		}

		private string OperatingSystemName()
		{
			switch (OperatingSystem)
			{
				case OperatingSystem.Windows7:
					return "Windows 7";
				case OperatingSystem.Windows8:
					return "Windows 8";
				case OperatingSystem.Windows8_1:
					return "Windows 8.1";
				case OperatingSystem.Windows10:
					return "Windows 10";
				case OperatingSystem.Windows11:
					return "Windows 11";
				default:
					return "Unknown Windows Version";
			}
		}
	}
}
