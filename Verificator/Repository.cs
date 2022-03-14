/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Verificator.Data;
using File = System.IO.File;

namespace Verificator
{
	internal class Repository
	{
		internal string Save(Installation reference, string path)
		{
			var filePath = Path.Combine(path, $"SEB_{reference.Version}_{reference.Platform}.{Constants.REFERENCE_FILE_EXTENSION}");
			var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
			var serializer = new XmlSerializer(typeof(Installation));

			using (var stream = File.OpenWrite(filePath))
			{
				serializer.Serialize(stream, reference, namespaces);
			}

			return filePath;
		}

		internal IEnumerable<Installation> SearchReferences()
		{
			var assembly = Assembly.GetAssembly(GetType());
			var serializer = new XmlSerializer(typeof(Installation));

			foreach (var resource in assembly.GetManifestResourceNames())
			{
				if (resource.EndsWith(Constants.REFERENCE_FILE_EXTENSION))
				{
					using (var stream = assembly.GetManifestResourceStream(resource))
					{
						yield return serializer.Deserialize(stream) as Installation;
					}
				}
			}
		}

		internal bool TryLoad(string path, out Installation reference)
		{
			var serializer = new XmlSerializer(typeof(Installation));

			using (var stream = File.OpenRead(path))
			{
				reference = serializer.Deserialize(stream) as Installation;
			}

			return reference != default;
		}
	}
}
