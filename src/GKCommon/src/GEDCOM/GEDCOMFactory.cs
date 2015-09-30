﻿using System.Collections.Generic;

namespace GKCommon.GEDCOM
{
	public sealed class GEDCOMFactory
	{
		private static GEDCOMFactory fInstance = null;
		private readonly Dictionary<string, TagConstructor> fConstructors;

		public static GEDCOMFactory GetInstance()
		{
			if (fInstance == null) fInstance = new GEDCOMFactory();
			return fInstance;
		}

        public GEDCOMFactory()
        {
            this.fConstructors = new Dictionary<string, TagConstructor>();
        }

		public void RegisterTag(string key, TagConstructor constructor)
		{
			if (fConstructors.ContainsKey(key))
				fConstructors[key] = constructor;
			else
				fConstructors.Add(key, constructor);
		}

        public GEDCOMTag CreateTag(GEDCOMTree owner, GEDCOMObject parent, string tagName, string tagValue)
		{
			TagConstructor constructor;

			if (fConstructors.TryGetValue(tagName, out constructor)) {
				return constructor(owner, parent, tagName, tagValue);
			} else {
				return null;
			}
		}
	}
}
