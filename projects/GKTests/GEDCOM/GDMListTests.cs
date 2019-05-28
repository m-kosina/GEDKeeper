﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2018 by Sergey V. Zhdanovskih.
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

using GDModel;
using GKCore;
using GKTests;
using GKUI.Providers;
using NUnit.Framework;

namespace GDModel
{
    [TestFixture]
    public class GDMListTests
    {
        private BaseContext fContext;

        [TestFixtureSetUp]
        public void SetUp()
        {
            WFAppHost.ConfigureBootstrap(false);

            LangMan.DefInit();

            fContext = TestUtils.CreateContext();
            TestUtils.FillContext(fContext);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test_Common()
        {
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;
            Assert.IsNotNull(iRec);

            for (int k = 0; k < REP_COUNT; k++) {
                GEDCOMListTest11(iRec);
                GEDCOMListTest12(iRec);
                GEDCOMListTest21(iRec);
                GEDCOMListTest22(iRec);
                GEDCOMListTest23(iRec);
            }
        }

        private const int REP_COUNT = 1000; // 1000000; // for profile tests

        private static void GEDCOMListTest11(GDMIndividualRecord iRec)
        {
            int hash;
            foreach (GDMCustomEvent evt1 in iRec.Events) {
                hash = evt1.GetHashCode();
            }
        }

        private static void GEDCOMListTest12(GDMIndividualRecord iRec)
        {
            int hash;
            IGEDCOMListEnumerator<GDMCustomEvent> enumer = iRec.Events.GetEnumerator();
            enumer.Reset();
            while (enumer.MoveNext()) {
                GDMCustomEvent evt1 = enumer.Current;
                hash = evt1.GetHashCode();
            }
        }

        private static void GEDCOMListTest21(GDMIndividualRecord iRec)
        {
            int hash;
            for (int i = 0; i < iRec.Events.Count; i++) {
                GDMCustomEvent evt1 = iRec.Events[i];
                hash = evt1.GetHashCode();
            }
        }

        private static void GEDCOMListTest22(GDMIndividualRecord iRec)
        {
            int hash;
            for (int i = 0, num = iRec.Events.Count; i < num; i++) {
                GDMCustomEvent evt1 = iRec.Events[i];
                hash = evt1.GetHashCode();
            }
        }

        private static void GEDCOMListTest23(GDMIndividualRecord iRec)
        {
            int hash;
            GDMList<GDMCustomEvent> events = iRec.Events;
            for (int i = 0, num = events.Count; i < num; i++) {
                GDMCustomEvent evt1 = events[i];
                hash = evt1.GetHashCode();
            }
        }
    }
}