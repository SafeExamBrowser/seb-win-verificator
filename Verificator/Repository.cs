/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Verificator.Data;
using Verificator.Logging;
using File = System.IO.File;

namespace Verificator
{
	internal class Repository
	{
		private readonly Logger logger;

		internal Repository(Logger logger)
		{
			this.logger = logger;
		}

		internal string Save(Installation reference, string path)
		{
			var filePath = Path.Combine(path, $"SEB_{reference.Version}_{reference.Platform}.{Constants.REFERENCE_FILE_EXTENSION}");
			var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
			var serializer = new XmlSerializer(typeof(Installation));

			using (var stream = File.OpenWrite(filePath))
			{
				serializer.Serialize(stream, reference, namespaces);
			}

			logger.Info($"Reference for {reference.Version} ({reference.Platform}) successfully saved as '{path}'.");

			return filePath;
		}

		internal IEnumerable<Installation> SearchReferences()
		{
			var assembly = Assembly.GetAssembly(GetType());
			var serializer = new XmlSerializer(typeof(Installation));

			logger.Debug("Searching references...");

			foreach (var resource in assembly.GetManifestResourceNames())
			{
				if (resource.EndsWith(Constants.REFERENCE_FILE_EXTENSION))
				{
					using (var stream = assembly.GetManifestResourceStream(resource))
					{
						var reference = serializer.Deserialize(stream) as Installation;

						logger.Info($"Found reference for SEB {reference.Version} ({reference.Platform}).");

						yield return reference;
					}
				}
			}
		}

		internal bool TryLoad(string path, out Installation reference)
		{
			var serializer = new XmlSerializer(typeof(Installation));

			logger.Debug("Attempting to load reference...");

			using (var stream = File.OpenRead(path))
			{
				reference = serializer.Deserialize(stream) as Installation;
			}

			if (reference != default)
			{
				logger.Info($"Successfully loaded reference for SEB {reference.Version} ({reference.Platform}).");
			}
			else
			{
				logger.Error($"The selected file '{path}' does not contain a valid Safe Exam Browser reference!");
			}

			return reference != default;
		}

		internal bool TrySearchConfigurations(out ICollection<Configuration> configurations)
		{
			var root = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent;
			var files = root.GetFiles().ToList();

			configurations = new List<Configuration>();

			logger.Debug("Searching configuration files...");

			foreach (var directory in root.EnumerateDirectories("*", SearchOption.AllDirectories))
			{
				files.AddRange(directory.GetFiles());
			}

			foreach (var file in files)
			{
				if (file.Extension.Equals($".{Constants.CONFIGURATION_FILE_EXTENSION}", StringComparison.OrdinalIgnoreCase))
				{
					configurations.Add(new Configuration
					{
						AbsolutePath = file.FullName,
						RelativePath = file.FullName.Replace(root.FullName + Path.DirectorySeparatorChar, "")
					});

					logger.Debug($"Found configuration file '{file.FullName}'.");
				}
			}

			configurations = configurations.OrderBy(c => c.AbsolutePath).ToList();

			if (!configurations.Any())
			{
				logger.Debug("Could not find any configuration files.");
			}

			return configurations.Any();
		}
	}
}
