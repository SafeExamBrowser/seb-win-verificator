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
	internal class ThreadInfo : ICloneable
	{
		internal int Id { get; private set; }
		internal string Name { get; private set; }

		internal bool HasName
		{
			get { return !String.IsNullOrWhiteSpace(Name); }
		}

		internal ThreadInfo(int id, string name = null)
		{
			Id = id;
			Name = name;
		}

		public object Clone()
		{
			return new ThreadInfo(Id, Name);
		}
	}
}
