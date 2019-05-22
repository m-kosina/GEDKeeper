﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2019 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace GKCommon.GEDCOM
{
    public sealed class GDMSourceData : GDMTagWithLists
    {
        private GDMList<GDMSourceEvent> fEvents;


        public GDMList<GDMSourceEvent> Events
        {
            get { return fEvents; }
        }

        public string Agency
        {
            get { return GetTagStringValue(GEDCOMTagType.AGNC); }
            set { SetTagStringValue(GEDCOMTagType.AGNC, value); }
        }


        public new static GDMTag Create(GDMObject owner, string tagName, string tagValue)
        {
            return new GDMSourceData(owner, tagName, tagValue);
        }

        public GDMSourceData(GDMObject owner) : base(owner)
        {
            SetName(GEDCOMTagType.DATA);

            fEvents = new GDMList<GDMSourceEvent>(this);
        }

        public GDMSourceData(GDMObject owner, string tagName, string tagValue) : this(owner)
        {
            SetNameValue(tagName, tagValue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fEvents.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Clear()
        {
            base.Clear();
            fEvents.Clear();
        }

        public override bool IsEmpty()
        {
            return base.IsEmpty() && (fEvents.Count == 0);
        }
    }
}
