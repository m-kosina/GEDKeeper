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

using System;
using BSLib;
using BSLib.Calendar;
using GKCommon.GEDCOM;
using GKCore;
using GKCore.Types;
using GKTests;
using GKUI.Providers;
using NUnit.Framework;

namespace GKCommon.GEDCOM
{
    [TestFixture]
    public class GedcomTests
    {
        private BaseContext fContext;

        [TestFixtureSetUp]
        public void SetUp()
        {
            // TempDirtyHack: some functions are references to GlobalOptions (and GfxInit)
            // TODO: replace to mocks
            WFAppHost.ConfigureBootstrap(false);

            fContext = TestUtils.CreateContext();
            TestUtils.FillContext(fContext);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }

        #region True Tests

        [Test]
        public void Test_GEDCOMMathes()
        {
            GDMTree tree = new GDMTree();
            Assert.IsNotNull(tree);

            GDMIndividualRecord ind1, ind2;
            GDMCustomEvent ev1, ev2;
            GDMDateValue dtVal1, dtVal2;

            ind1 = tree.CreateIndividual();
            ind1.Sex = GEDCOMSex.svMale;
            GDMPersonalName pn = ind1.AddPersonalName(new GDMPersonalName(ind1, "", ""));
            pn.SetNameParts("Ivan Ivanov", "Fedoroff", "");

            ind2 = tree.CreateIndividual();
            ind2.Sex = GEDCOMSex.svMale;
            pn = ind2.AddPersonalName(new GDMPersonalName(ind2, "", ""));
            pn.SetNameParts("Ivan Ivanovich", "Fedoroff", "");

            ev1 = new GDMIndividualEvent(ind1, GEDCOMTagType.BIRT, "");
            dtVal1 = ev1.Date;
            ind1.AddEvent(ev1);

            ev2 = new GDMIndividualEvent(ind2, GEDCOMTagType.BIRT, "");
            dtVal2 = ev2.Date;
            ind2.AddEvent(ev2);

            float res;
            MatchParams mParams;
            mParams.NamesIndistinctThreshold = 1.0f;
            mParams.DatesCheck = true;
            mParams.YearsInaccuracy = 0;
            mParams.CheckEventPlaces = false;

            // null
            res = dtVal1.IsMatch(null, mParams);
            Assert.AreEqual(0.0f, res);

            // null
            res = ev1.IsMatch(null, mParams);
            Assert.AreEqual(0.0f, res);

            // dtVal1 -> dtVal2, delta = 0
            dtVal1.SetDateTime(DateTime.Parse("10.10.2013"));
            dtVal2.SetDateTime(DateTime.Parse("10.10.2013"));
            res = dtVal1.IsMatch(dtVal2, mParams);
            Assert.AreEqual(100.0f, res);

            // ev1 -> ev2, delta = 0
            res = ev1.IsMatch(ev2, mParams);
            Assert.AreEqual(100.0f, res);

            // dtVal1 -> dtVal2, delta = 3
            mParams.YearsInaccuracy = 3;

            dtVal2.SetDateTime(DateTime.Parse("10.10.2015"));
            res = dtVal1.IsMatch(dtVal2, mParams);
            Assert.AreEqual(100.0f, res);

            // ev1 -> ev2, delta = 3
            res = ev1.IsMatch(ev2, mParams);
            Assert.AreEqual(100.0f, res);

            dtVal2.SetDateTime(DateTime.Parse("10.10.2009"));
            res = dtVal1.IsMatch(dtVal2, mParams);
            Assert.AreEqual(0.0f, res);

            // ev1 -> ev2, delta = 3
            res = ev1.IsMatch(ev2, mParams);
            Assert.AreEqual(0.0f, res);

            // //

            res = ind1.IsMatch(null, mParams);
            Assert.AreEqual(0.0f, res);

            res = ind1.IsMatch(ind2, mParams);
            Assert.AreEqual(0.0f, res);

            // Ivanov - Ivanov(ich) : 3 chars of difference -> 0.88
            mParams.NamesIndistinctThreshold = 0.85f;
            mParams.YearsInaccuracy = 4;

            res = ind1.IsMatch(ind2, mParams);
            Assert.AreEqual(100.0f, res);
        }

        [Test]
        public void Test_GEDCOMData()
        {
            using (GDMSourceData data = GDMSourceData.Create(null, "", "") as GDMSourceData) {
                Assert.IsNotNull(data);
                
                data.Agency = "test agency";
                Assert.AreEqual("test agency", data.Agency);
                
                GDMTag evenTag = data.Events.Add(new GDMSourceEvent(data, GEDCOMTagType.EVEN, ""));
                Assert.IsNotNull(evenTag);
                
                GDMSourceEvent evt = data.Events[0];
                Assert.AreEqual(evenTag, evt);
                
                data.Clear();
                Assert.IsTrue(data.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMEvent()
        {
            using (GDMSourceEvent evt = GDMSourceEvent.Create(null, "", "") as GDMSourceEvent)
            {
                Assert.IsNotNull(evt);
                
                Assert.IsNotNull(evt.Date);
                
                Assert.IsNotNull(evt.Place);
            }
        }

        [Test]
        public void Test_GEDCOMDateStatus()
        {
            using (GDMDateStatus dateStatus = GDMDateStatus.Create(null, "", "") as GDMDateStatus)
            {
                Assert.IsNotNull(dateStatus);
                Assert.IsNotNull(dateStatus.ChangeDate);
            }
        }

        [Test]
        public void Test_GEDCOMIndividualEvent()
        {
            using (GDMIndividualEvent iEvent = GDMIndividualEvent.Create(null, "", "") as GDMIndividualEvent)
            {
                Assert.IsNotNull(iEvent);
                Assert.IsNotNull(iEvent.Family);
            }
        }

        [Test]
        public void Test_GEDCOMIndividualOrdinance()
        {
            using (GDMIndividualOrdinance iOrd = GDMIndividualOrdinance.Create(null, "", "") as GDMIndividualOrdinance)
            {
                Assert.IsNotNull(iOrd);

                Assert.IsNotNull(iOrd.Date);

                iOrd.TempleCode = "temple code";
                Assert.AreEqual("temple code", iOrd.TempleCode);

                iOrd.Place.StringValue = "test place";
                Assert.AreEqual("test place", iOrd.Place.StringValue);
                
                iOrd.BaptismDateStatus = GEDCOMBaptismDateStatus.bdsCompleted;
                Assert.AreEqual(GEDCOMBaptismDateStatus.bdsCompleted, iOrd.BaptismDateStatus);

                iOrd.EndowmentDateStatus = GEDCOMEndowmentDateStatus.edsExcluded;
                Assert.AreEqual(GEDCOMEndowmentDateStatus.edsExcluded, iOrd.EndowmentDateStatus);
                
                Assert.IsNotNull(iOrd.Family);
                
                iOrd.ChildSealingDateStatus = GEDCOMChildSealingDateStatus.cdsPre1970;
                Assert.AreEqual(GEDCOMChildSealingDateStatus.cdsPre1970, iOrd.ChildSealingDateStatus);
                
                Assert.IsNotNull(iOrd.DateStatus);
            }
        }

        [Test]
        public void Test_GEDCOMSpouseSealing()
        {
            using (GDMSpouseSealing spouseSealing = GDMSpouseSealing.Create(null, "", "") as GDMSpouseSealing)
            {
                Assert.IsNotNull(spouseSealing);

                Assert.IsNotNull(spouseSealing.Date);

                spouseSealing.TempleCode = "temple code";
                Assert.AreEqual("temple code", spouseSealing.TempleCode);

                spouseSealing.Place.StringValue = "test place";
                Assert.AreEqual("test place", spouseSealing.Place.StringValue);

                spouseSealing.SpouseSealingDateStatus = GDMSpouseSealingDateStatus.sdsCanceled;
                Assert.AreEqual(GDMSpouseSealingDateStatus.sdsCanceled, spouseSealing.SpouseSealingDateStatus);

                Assert.IsNotNull(spouseSealing.DateStatus);
            }
        }

        [Test]
        public void Test_XRefReplacer()
        {
            using (XRefReplacer replacer = new XRefReplacer())
            {
                Assert.IsNotNull(replacer);

                GDMIndividualRecord iRec = fContext.CreatePersonEx("ivan", "ivanovich", "ivanov", GEDCOMSex.svMale, false);
                replacer.AddXRef(iRec, "I210", iRec.XRef);

                string newXRef = replacer.FindNewXRef("I210");
                Assert.AreEqual(iRec.XRef, newXRef);

                newXRef = replacer.FindNewXRef("I310");
                Assert.AreEqual("I310", newXRef);

                for (int i = 0; i < replacer.Count; i++) {
                    XRefReplacer.XRefEntry xre = replacer[i];
                    Assert.AreEqual(iRec, xre.Rec);
                }
            }
        }

        [Test]
        public void Test_UDN()
        {
            UDN emptyUDN = UDN.CreateEmpty();
            Assert.IsTrue(emptyUDN.IsEmpty());

            // BIRT: "28 DEC 1990"
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;

            UDN testUDN = iRec.GetUDN(GEDCOMTagType.BIRT);
            Assert.AreEqual("1990/12/28", testUDN.ToString());

            testUDN = GDMDate.GetUDNByFormattedStr("28/12/1990", GEDCOMCalendar.dcGregorian);
            Assert.AreEqual("1990/12/28", testUDN.ToString());

            using (GDMDateValue dateVal = new GDMDateValue(null, "", "")) {
                dateVal.ParseString("28 DEC 1990");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("1990/12/28", testUDN.ToString());

                dateVal.ParseString("ABT 20 JAN 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("~2013/01/20", testUDN.ToString());

                dateVal.ParseString("CAL 20 JAN 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("~2013/01/20", testUDN.ToString());

                dateVal.ParseString("EST 20 DEC 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("~2013/12/20", testUDN.ToString());

                dateVal.ParseString("BET 04 JAN 2013 AND 25 JAN 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("2013/01/14", testUDN.ToString());

                dateVal.ParseString("BEF 20 JAN 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual("<2013/01/20", testUDN.ToString());

                dateVal.ParseString("AFT 20 JAN 2013");
                testUDN = dateVal.GetUDN();
                Assert.AreEqual(">2013/01/20", testUDN.ToString());
            }
        }

        [Test]
        public void Test_GEDCOMTagWithLists()
        {
            // GEDCOMTagWithLists protected class, derived - GEDCOMEventDetail
            using (GDMPlace tag = GDMPlace.Create(null, "", "") as GDMPlace)
            {
                Assert.IsNotNull(tag);

                Assert.IsNotNull(tag.Notes);
                Assert.IsNotNull(tag.SourceCitations);
                Assert.IsNotNull(tag.MultimediaLinks);

                Assert.IsNull(tag.AddNote(null));
                Assert.IsNull(tag.AddSource(null, "page", 1));
                Assert.IsNull(tag.AddMultimedia(null));

                Assert.IsNotNull(tag.AddNote(new GDMNoteRecord(null)));
                Assert.IsNotNull(tag.AddSource(new GDMSourceRecord(null), "page", 1));
                Assert.IsNotNull(tag.AddMultimedia(new GDMMultimediaRecord(null)));
            }
        }

        [Test]
        public void Test_GEDCOMChangeDate()
        {
            using (GDMChangeDate cd = GDMChangeDate.Create(null, GEDCOMTagType.CHAN, "") as GDMChangeDate)
            {
                Assert.IsNotNull(cd);

                Assert.IsNotNull(cd.Notes);

                DateTime dtNow = DateTime.Now;
                dtNow = dtNow.AddTicks(-dtNow.Ticks % 10000000);
                cd.ChangeDateTime = dtNow;

                DateTime dtx = cd.ChangeDateTime;
                Assert.AreEqual(dtNow, dtx);

                GDMTime time = cd.ChangeTime;
                Assert.AreEqual(dtNow.Second, time.Seconds);
                Assert.AreEqual(dtNow.Minute, time.Minutes);
                Assert.AreEqual(dtNow.Hour, time.Hour);
                Assert.AreEqual(dtNow.Millisecond, time.Fraction);

                time.Seconds = 11;
                Assert.AreEqual(11, time.Seconds);
                time.Minutes = 22;
                Assert.AreEqual(22, time.Minutes);
                time.Hour = 12;
                Assert.AreEqual(12, time.Hour);
                
                Assert.AreEqual("12:22:11", time.StringValue);
                
                Assert.AreEqual(DateTime.Now.Date.ToString("yyyy.MM.dd") + " 12:22:11", cd.ToString());

                Assert.IsFalse(time.IsEmpty());
                time.Clear();
                Assert.IsTrue(time.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMTime()
        {
            using (GDMTime time = new GDMTime(null, "", "20:20:20.100"))
            {
                Assert.IsNotNull(time, "time != null");
                Assert.AreEqual(GEDCOMTagType.TIME, time.Name);

                Assert.AreEqual(20, time.Hour);
                Assert.AreEqual(20, time.Minutes);
                Assert.AreEqual(20, time.Seconds);
                Assert.AreEqual(100, time.Fraction);

                time.Fraction = 200;
                Assert.AreEqual(200, time.Fraction);

                Assert.AreEqual("20:20:20.200", time.StringValue);

                time.Hour = 0;
                time.Minutes = 0;
                time.Seconds = 0;
                Assert.AreEqual("", time.StringValue);
            }
        }

        [Test]
        public void Test_GEDCOMDate()
        {
            using (GDMDate dtx1 = new GDMDate(null, "", "20 JAN 2013"))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");

                DateTime dt = TestUtils.ParseDT("20.01.2013");
                Assert.IsTrue(dtx1.Date.Equals(dt), "dtx1.DateTime.Equals(dt)");

                //dtx1.DateCalendar = GEDCOMCalendar.dcFrench;
                Assert.AreEqual(GEDCOMCalendar.dcGregorian, dtx1.DateCalendar);

                dtx1.Day = 21;
                Assert.AreEqual(21, dtx1.Day);

                dtx1.Month = 09;
                Assert.AreEqual(09, dtx1.Month);

                dtx1.Year = 1812;
                Assert.AreEqual(1812, dtx1.Year);

                dtx1.YearBC = true;
                Assert.AreEqual(true, dtx1.YearBC);

                dtx1.YearModifier = "2";
                Assert.AreEqual("2", dtx1.YearModifier);

                //
                dtx1.ParseString("01 FEB 1934/11B.C.");
                Assert.AreEqual(01, dtx1.Day);
                Assert.AreEqual(02, dtx1.Month);
                Assert.AreEqual(1934, dtx1.Year);
                Assert.AreEqual("11", dtx1.YearModifier);
                Assert.AreEqual(true, dtx1.YearBC);
                dtx1.ParseString("01 FEB 1934/11B.C.");
                Assert.AreEqual("01 FEB 1934/11B.C.", dtx1.StringValue);

                // gregorian

                dtx1.SetGregorian(1, 1, 1980);
                Assert.AreEqual(GEDCOMCalendar.dcGregorian, dtx1.DateCalendar);
                Assert.AreEqual("01 JAN 1980", dtx1.StringValue);

                Assert.Throws(typeof(GDMDateException), () => { dtx1.SetGregorian(1, "X", 1980, "", false); });

                // julian

                dtx1.SetJulian(1, "JAN", 1980, false);
                Assert.AreEqual(GEDCOMCalendar.dcJulian, dtx1.DateCalendar);

                dtx1.SetJulian(1, 3, 1980);
                Assert.AreEqual(GEDCOMCalendar.dcJulian, dtx1.DateCalendar);
                Assert.AreEqual("@#DJULIAN@ 01 MAR 1980", dtx1.StringValue);
                dtx1.ParseString("@#DJULIAN@ 01 MAR 1980");
                Assert.AreEqual("@#DJULIAN@ 01 MAR 1980", dtx1.StringValue);

                using (GDMDate dtx2 = new GDMDate(null, "", ""))
                {
                    Assert.IsNotNull(dtx2, "dtx2 != null");

                    Assert.Throws(typeof(ArgumentException), () => { dtx2.Assign(null); });

                    Assert.AreEqual("", dtx2.StringValue);
                    Assert.AreEqual(new DateTime(0), dtx2.GetDateTime());

                    Assert.IsFalse(dtx2.IsValidDate());

                    dtx2.Assign(dtx1);
                    Assert.AreEqual("@#DJULIAN@ 01 MAR 1980", dtx2.StringValue);

                    Assert.IsTrue(dtx2.IsValidDate());
                }

                // hebrew

                dtx1.SetHebrew(1, "TSH", 1980, false);
                Assert.AreEqual(GEDCOMCalendar.dcHebrew, dtx1.DateCalendar);

                dtx1.SetHebrew(1, 2, 1980);
                Assert.AreEqual(GEDCOMCalendar.dcHebrew, dtx1.DateCalendar);
                Assert.AreEqual("@#DHEBREW@ 01 CSH 1980", dtx1.StringValue);
                dtx1.ParseString("@#DHEBREW@ 01 CSH 1980");
                Assert.AreEqual("@#DHEBREW@ 01 CSH 1980", dtx1.StringValue);

                Assert.Throws(typeof(GDMDateException), () => { dtx1.SetHebrew(1, "X", 1980, false); });

                // french

                dtx1.SetFrench(1, "VEND", 1980, false);
                Assert.AreEqual(GEDCOMCalendar.dcFrench, dtx1.DateCalendar);

                dtx1.SetFrench(1, 2, 1980);
                Assert.AreEqual(GEDCOMCalendar.dcFrench, dtx1.DateCalendar);
                Assert.AreEqual("@#DFRENCH R@ 01 BRUM 1980", dtx1.StringValue);
                dtx1.ParseString("@#DFRENCH R@ 01 BRUM 1980");
                Assert.AreEqual("@#DFRENCH R@ 01 BRUM 1980", dtx1.StringValue);

                Assert.Throws(typeof(GDMDateException), () => { dtx1.SetFrench(1, "X", 1980, false); });

                // roman

                dtx1.SetRoman(1, "JAN", 1980, false);
                Assert.AreEqual(GEDCOMCalendar.dcRoman, dtx1.DateCalendar);

                dtx1.SetUnknown(1, "JAN", 1980, false);
                Assert.AreEqual(GEDCOMCalendar.dcUnknown, dtx1.DateCalendar);
            }
        }

        [Test]
        public void Test_GEDCOMDateRange()
        {
            using (var dtx1 = (GDMDateRange)GDMDateRange.Create(null, "", ""))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");
                Assert.AreEqual("", dtx1.StringValue);
                Assert.AreEqual(new DateTime(0), dtx1.GetDateTime());
                Assert.AreEqual("", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true)); // date is empty
                UDN udn = dtx1.GetUDN();
                Assert.IsTrue(udn.IsEmpty());

                dtx1.ParseString("BET 04 JAN 2013 AND 25 JAN 2013");
                Assert.AreEqual("BET 04 JAN 2013 AND 25 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(new DateTime(0), dtx1.Date);
                Assert.AreEqual("2013.01.04 [G] - 2013.01.25 [G]", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                dtx1.ParseString("BEF 20 JAN 2013");
                Assert.AreEqual("BEF 20 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), dtx1.Date);
                Assert.AreEqual("< 2013.01.20 [G]", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                dtx1.ParseString("AFT 20 JAN 2013");
                Assert.AreEqual("AFT 20 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), dtx1.Date);
                Assert.AreEqual("2013.01.20 [G] >", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                Assert.Throws(typeof(NotSupportedException), () => { dtx1.SetDateTime(DateTime.Now); });

                Assert.Throws(typeof(GDMDateException), () => { dtx1.ParseString("BET 04 JAN 2013 X 25 JAN 2013"); });
            }
        }

        [Test]
        public void Test_GEDCOMDatePeriod()
        {
            using (GDMDatePeriod dtx1 = new GDMDatePeriod(null, "", ""))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");
                Assert.AreEqual("", dtx1.StringValue);
                Assert.AreEqual(new DateTime(0), dtx1.GetDateTime());

                Assert.AreEqual("", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true)); // date is empty
                UDN udn = dtx1.GetUDN();
                Assert.IsTrue(udn.IsEmpty());

                dtx1.ParseString("FROM 04 JAN 2013 TO 23 JAN 2013");

                Assert.AreEqual("FROM 04 JAN 2013 TO 23 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(new DateTime(0), dtx1.Date);
                Assert.AreEqual("2013.01.04 [G] - 2013.01.23 [G]", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                dtx1.ParseString("FROM 04 JAN 2013 TO 04 JAN 2013");
                Assert.AreEqual("FROM 04 JAN 2013 TO 04 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(TestUtils.ParseDT("04.01.2013"), dtx1.Date);
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                dtx1.Clear();
                dtx1.ParseString("FROM 04 JAN 2013");
                Assert.AreEqual("FROM 04 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(TestUtils.ParseDT("04.01.2013"), dtx1.Date);
                Assert.AreEqual("2013.01.04 [G] >", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                dtx1.Clear();
                dtx1.ParseString("TO 23 JAN 2013");
                Assert.AreEqual("TO 23 JAN 2013", dtx1.StringValue);
                Assert.AreEqual(TestUtils.ParseDT("23.01.2013"), dtx1.Date);
                Assert.AreEqual("< 2013.01.23 [G]", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
                udn = dtx1.GetUDN();
                Assert.IsFalse(udn.IsEmpty());

                Assert.Throws(typeof(NotSupportedException), () => { dtx1.SetDateTime(DateTime.Now); });
            }
        }

        [Test]
        public void Test_GEDCOMDateValue()
        {
            // check empty dateval match
            using (GDMDateValue dtx1 = new GDMDateValue(null, "", ""))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");

                using (GDMDateValue dtx2 = new GDMDateValue(null, "", ""))
                {
                    Assert.IsNotNull(dtx2, "dtx1 != null");

                    Assert.AreEqual(0.0f, dtx1.IsMatch(dtx2, new MatchParams()));
                }
            }

            using (GDMDateValue dtx1 = new GDMDateValue(null, "", ""))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");
                Assert.AreEqual("", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true)); // value is empty

                dtx1.ParseString("20 JAN 2013");
                Assert.AreEqual("2013.01.20 [G]", dtx1.GetDisplayStringExt(DateFormat.dfYYYY_MM_DD, true, true));
            }

            using (GDMDateValue dtx1 = new GDMDateValue(null, "", "20 JAN 2013"))
            {
                Assert.IsNotNull(dtx1, "dtx1 != null");

                DateTime dt = TestUtils.ParseDT("20.01.2013");
                Assert.IsTrue(dtx1.Date.Equals(dt), "dtx1.DateTime.Equals(dt)");

                dtx1.ParseString("1716/"); // potentially incorrect value
                Assert.AreEqual("1716", dtx1.StringValue);

                dtx1.ParseString("1716/1717");
                Assert.AreEqual("1716/1717", dtx1.StringValue);

                dtx1.ParseString("1716/20");
                Assert.AreEqual("1716/20", dtx1.StringValue);

                dtx1.ParseString("3 MAY 1835/1838");
                Assert.AreEqual("03 MAY 1835/1838", dtx1.StringValue);

                dtx1.ParseString("ABT 1844/1845");
                Assert.AreEqual("ABT 1844/1845", dtx1.StringValue);

                dtx1.ParseString("FEB 1746/1747");
                Assert.AreEqual("FEB 1746/1747", dtx1.StringValue);

                dtx1.ParseString("INT 20 JAN 2013 (today)");
                Assert.IsTrue(dtx1.Date.Equals(dt), "dtx1.DateTime.Equals(dt)");
                Assert.AreEqual("today", (dtx1.Value as GDMDateInterpreted).DatePhrase);

                (dtx1.Value as GDMDateInterpreted).DatePhrase = "now";
                Assert.AreEqual("INT 20 JAN 2013 (now)", dtx1.StringValue);

                (dtx1.Value as GDMDateInterpreted).DatePhrase = "(yesterday)";
                Assert.AreEqual("INT 20 JAN 2013 (yesterday)", dtx1.StringValue);

                dtx1.ParseString("INT 20 JAN 2013 (yesterday)");
                Assert.AreEqual("INT 20 JAN 2013 (yesterday)", dtx1.StringValue);

                string st;

                st = "ABT 20 JAN 2013";
                dtx1.ParseString(st);
                Assert.IsTrue(dtx1.Date.Equals(dt));
                Assert.AreEqual(st, dtx1.StringValue);
                Assert.AreEqual(GEDCOMApproximated.daAbout, ((GDMDate)dtx1.Value).Approximated);
                
                st = "CAL 20 JAN 2013";
                dtx1.ParseString(st);
                Assert.AreEqual(dtx1.Date, dt);
                Assert.AreEqual(st, dtx1.StringValue);
                Assert.AreEqual(GEDCOMApproximated.daCalculated, ((GDMDate)dtx1.Value).Approximated);
                
                st = "EST 20 DEC 2013";
                dtx1.ParseString(st);
                Assert.AreEqual(dtx1.Date, TestUtils.ParseDT("20.12.2013"));
                Assert.AreEqual(st, dtx1.StringValue);
                Assert.AreEqual(GEDCOMApproximated.daEstimated, ((GDMDate)dtx1.Value).Approximated);

                ((GDMDate)dtx1.Value).Approximated = GEDCOMApproximated.daCalculated;
                Assert.AreEqual("CAL 20 DEC 2013", dtx1.StringValue);

                ((GDMDate)dtx1.Value).Approximated = GEDCOMApproximated.daExact;
                Assert.AreEqual("20 DEC 2013", dtx1.StringValue);

                using (GDMDateValue dtx2 = new GDMDateValue(null, "", "19 JAN 2013"))
                {
                    int res = dtx1.CompareTo(dtx2);
                    Assert.AreEqual(1, res);
                }
                
                int res1 = dtx1.CompareTo(null);
                Assert.AreEqual(-1, res1);

                //
                
                dtx1.ParseString("FROM 04 JAN 2013 TO 23 JAN 2013");
                Assert.IsFalse(dtx1.IsEmpty());
                Assert.AreEqual("FROM 04 JAN 2013 TO 23 JAN 2013", dtx1.StringValue);
                Assert.AreEqual("04 JAN 2013", (dtx1.Value as GDMDatePeriod).DateFrom.StringValue);
                Assert.AreEqual("23 JAN 2013", (dtx1.Value as GDMDatePeriod).DateTo.StringValue);
                dtx1.Clear();
                Assert.IsTrue(dtx1.IsEmpty());

                dtx1.ParseString("BEF 20 JAN 2013");
                Assert.IsFalse(dtx1.IsEmpty());
                Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), dtx1.Date);
                Assert.AreEqual("BEF 20 JAN 2013", dtx1.StringValue);

                dtx1.ParseString("AFT 20 JAN 2013");
                Assert.IsFalse(dtx1.IsEmpty());
                Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), dtx1.Date);
                Assert.AreEqual("AFT 20 JAN 2013", dtx1.StringValue);

                dtx1.ParseString("BET 04 JAN 2013 AND 25 JAN 2013");
                Assert.IsFalse(dtx1.IsEmpty());
                Assert.AreEqual("BET 04 JAN 2013 AND 25 JAN 2013", dtx1.StringValue);
                Assert.AreEqual("04 JAN 2013", (dtx1.Value as GDMDateRange).After.StringValue);
                Assert.AreEqual("25 JAN 2013", (dtx1.Value as GDMDateRange).Before.StringValue);
                dtx1.Clear();
                Assert.IsTrue(dtx1.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMAddress()
        {
            using (GDMAddress addr = GDMAddress.Create(null, GEDCOMTagType.ADDR, "") as GDMAddress)
            {
                Assert.IsNotNull(addr, "addr != null");

                addr.SetAddressText("test");
                Assert.AreEqual("test", addr.Address.Text.Trim());

                addr.Address = new StringList("This\r\naddress\r\ntest");
                Assert.AreEqual("This\r\naddress\r\ntest", addr.Address.Text.Trim());
                Assert.AreEqual("This", addr.Address[0]);
                Assert.AreEqual("address", addr.Address[1]);
                Assert.AreEqual("test", addr.Address[2]);

                addr.AddPhoneNumber("8 911 101 99 99");
                Assert.AreEqual("8 911 101 99 99", addr.PhoneNumbers[0].StringValue);

                addr.AddEmailAddress("test@mail.com");
                Assert.AreEqual("test@mail.com", addr.EmailAddresses[0].StringValue);

                addr.AddFaxNumber("abrakadabra");
                Assert.AreEqual("abrakadabra", addr.FaxNumbers[0].StringValue);

                addr.AddWebPage("http://test.com");
                Assert.AreEqual("http://test.com", addr.WebPages[0].StringValue);

                // stream test
                string buf = TestUtils.GetTagStreamText(addr, 0);
                Assert.AreEqual(buf, "0 ADDR This\r\n"+"1 CONT address\r\n"+"1 CONT test\r\n"
                                +"0 PHON 8 911 101 99 99\r\n"
                                +"0 EMAIL test@mail.com\r\n"
                                +"0 FAX abrakadabra\r\n"
                                +"0 WWW http://test.com\r\n");

                addr.AddPhoneNumber("8 911 101 33 33");
                Assert.AreEqual("8 911 101 33 33", addr.PhoneNumbers[1].StringValue);

                addr.AddEmailAddress("test@mail.ru");
                Assert.AreEqual("test@mail.ru", addr.EmailAddresses[1].StringValue);

                addr.AddFaxNumber("abrakadabra");
                Assert.AreEqual("abrakadabra", addr.FaxNumbers[1].StringValue);

                addr.AddWebPage("http://test.ru");
                Assert.AreEqual("http://test.ru", addr.WebPages[1].StringValue);

                //

                addr.AddressLine1 = "test1";
                Assert.AreEqual("test1", addr.AddressLine1);

                addr.AddressLine2 = "test2";
                Assert.AreEqual("test2", addr.AddressLine2);

                addr.AddressLine3 = "test3";
                Assert.AreEqual("test3", addr.AddressLine3);

                addr.AddressCity = "test4";
                Assert.AreEqual("test4", addr.AddressCity);

                addr.AddressState = "test5";
                Assert.AreEqual("test5", addr.AddressState);

                addr.AddressCountry = "test6";
                Assert.AreEqual("test6", addr.AddressCountry);

                addr.AddressPostalCode = "test7";
                Assert.AreEqual("test7", addr.AddressPostalCode);

                using (GDMAddress addr2 = GDMAddress.Create(null, GEDCOMTagType.ADDR, "") as GDMAddress)
                {
                    Assert.Throws(typeof(ArgumentException), () => { addr2.Assign(null); });

                    addr2.Assign(addr);

                    Assert.AreEqual("This\r\naddress\r\ntest", addr2.Address.Text.Trim());
                    Assert.AreEqual("8 911 101 99 99", addr2.PhoneNumbers[0].StringValue);
                    Assert.AreEqual("test@mail.com", addr2.EmailAddresses[0].StringValue);
                    Assert.AreEqual("abrakadabra", addr2.FaxNumbers[0].StringValue);
                    Assert.AreEqual("http://test.com", addr2.WebPages[0].StringValue);
                    Assert.AreEqual("8 911 101 33 33", addr2.PhoneNumbers[1].StringValue);
                    Assert.AreEqual("test@mail.ru", addr2.EmailAddresses[1].StringValue);
                    Assert.AreEqual("abrakadabra", addr2.FaxNumbers[1].StringValue);
                    Assert.AreEqual("http://test.ru", addr2.WebPages[1].StringValue);
                    Assert.AreEqual("test1", addr2.AddressLine1);
                    Assert.AreEqual("test2", addr2.AddressLine2);
                    Assert.AreEqual("test3", addr2.AddressLine3);
                    Assert.AreEqual("test4", addr2.AddressCity);
                    Assert.AreEqual("test5", addr2.AddressState);
                    Assert.AreEqual("test6", addr2.AddressCountry);
                    Assert.AreEqual("test7", addr2.AddressPostalCode);
                }

                addr.SetAddressArray(new string[] {"test11", "test21", "test31"});
                Assert.AreEqual("test11", addr.Address[0]);
                Assert.AreEqual("test21", addr.Address[1]);
                Assert.AreEqual("test31", addr.Address[2]);

                Assert.IsFalse(addr.IsEmpty());
                addr.Clear();
                Assert.IsTrue(addr.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMAlias()
        {
            using (GDMAlias alias = GDMAlias.Create(null, GEDCOMTagType.ALIA, "") as GDMAlias)
            {
                Assert.IsNotNull(alias, "alias != null");
            }
        }

        [Test]
        public void Test_GEDCOMAssociation()
        {
            using (GDMAssociation association = GDMAssociation.Create(null, GEDCOMTagType.ASSO, "") as GDMAssociation) {
                Assert.IsNotNull(association);

                Assert.IsNotNull(association.SourceCitations);
                
                Assert.IsNotNull(association.Notes); // for GEDCOMPointerWithNotes
                
                association.Relation = "This is test relation";
                Assert.AreEqual("This is test relation", association.Relation);

                association.Individual = null;
                Assert.IsNull(association.Individual);

                GDMTag tag = association.SourceCitations.Add(new GDMSourceCitation(association, GEDCOMTagType.SOUR, "xxx"));
                Assert.IsNotNull(tag);
                Assert.IsTrue(tag is GDMSourceCitation);

                Assert.IsFalse(association.IsEmpty());
                association.Clear();
                Assert.IsTrue(association.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMUserRef()
        {
            using (GDMUserReference userRef = GDMUserReference.Create(null, GEDCOMTagType.REFN, "") as GDMUserReference)
            {
                Assert.IsNotNull(userRef);

                userRef.ReferenceType = "test";
                Assert.AreEqual("test", userRef.ReferenceType);
            }
        }

        private void OnTreeChange(object sender, EventArgs e) {}
        private void OnTreeChanging(object sender, EventArgs e) {}
        private void OnTreeProgress(object sender, int progress) {}

        [Test]
        public void Test_GEDCOMTree()
        {
            GDMTree tree = new GDMTree();
            Assert.IsNotNull(tree);


            // Tests of event handlers
            tree.OnChange += OnTreeChange;
            //Assert.AreEqual(OnTreeChange, tree.OnChange);
            tree.OnChange -= OnTreeChange;
            //Assert.AreEqual(null, tree.OnChange);
            tree.OnChanging += OnTreeChanging;
            //Assert.AreEqual(OnTreeChanging, tree.OnChanging);
            tree.OnChanging -= OnTreeChanging;
            //Assert.AreEqual(null, tree.OnChanging);
            tree.OnProgress += OnTreeProgress;
            //Assert.AreEqual(OnTreeProgress, tree.OnProgress);
            tree.OnProgress -= OnTreeProgress;
            //Assert.AreEqual(null, tree.OnProgress);


            //

            Assert.IsNotNull(tree.GetSubmitter());

            GDMRecord rec;

            GDMIndividualRecord iRec = tree.CreateIndividual();
            Assert.IsNotNull(iRec, "CreateIndividual() != null");

            string xref = iRec.XRef;
            rec = tree.XRefIndex_Find(xref);
            Assert.IsNotNull(rec);
            Assert.AreEqual(xref, rec.XRef);

            string uid = iRec.UID;
            rec = tree.FindUID(uid);
            Assert.IsNotNull(rec);
            Assert.AreEqual(uid, rec.UID);
            Assert.IsNull(tree.FindUID(""));

            //
            GDMFamilyRecord famRec = tree.CreateFamily();
            Assert.IsNotNull(famRec, "CreateFamily() != null");
            GEDCOMFamilyRecordTest(famRec, iRec);

            //
            GDMNoteRecord noteRec = tree.CreateNote();
            Assert.IsNotNull(noteRec, "CreateNote() != null");
            GEDCOMNoteRecordTest(noteRec, iRec);

            //
            GDMRepositoryRecord repRec = tree.CreateRepository();
            Assert.IsNotNull(repRec, "CreateRepository() != null");

            //
            GDMSourceRecord srcRec = tree.CreateSource();
            Assert.IsNotNull(srcRec, "CreateSource() != null");
            GEDCOMSourceRecordTest(srcRec, iRec, repRec);

            //
            GDMMultimediaRecord mmRec = tree.CreateMultimedia();
            Assert.IsNotNull(mmRec, "CreateMultimedia() != null");
            GEDCOMMultimediaRecordTest(mmRec, iRec);
            
            //

            GDMRecord sbmrRec = tree.AddRecord(new GDMSubmitterRecord(tree));
            Assert.IsNotNull(sbmrRec, "sbmrRec != null");
            sbmrRec.InitNew();
            string submXRef = sbmrRec.XRef;

            //

            GDMSubmissionRecord submRec = tree.AddRecord(new GDMSubmissionRecord(tree)) as GDMSubmissionRecord;
            Assert.IsNotNull(submRec, "rec1 != null");
            submRec.InitNew();
            GEDCOMSubmissionRecordTest(submRec, submXRef);

            //
            GDMGroupRecord groupRec = tree.CreateGroup();
            Assert.IsNotNull(groupRec, "CreateGroup() != null");

            //
            GDMTaskRecord taskRec = tree.CreateTask();
            Assert.IsNotNull(taskRec, "CreateTask() != null");

            //
            GDMCommunicationRecord commRec = tree.CreateCommunication();
            Assert.IsNotNull(commRec, "CreateCommunication() != null");

            //
            GDMResearchRecord resRec = tree.CreateResearch();
            Assert.IsNotNull(resRec, "CreateResearch() != null");
            GEDCOMResearchRecordTest(resRec, commRec, taskRec, groupRec);

            //
            GDMLocationRecord locRec = tree.CreateLocation();
            Assert.IsNotNull(locRec, "CreateLocation() != null");


            tree.Pack();


            int size = 0;
            var enum1 = tree.GetEnumerator(GEDCOMRecordType.rtNone);
            GDMRecord rec1;
            enum1.Reset();
            while (enum1.MoveNext(out rec1)) {
                size++;
            }
            Assert.AreEqual(14, size);

            for (int i = 0; i < tree.RecordsCount; i++) {
                GDMRecord rec2 = tree[i];
                Assert.IsNotNull(rec2);

                string xref2 = rec2.XRef;
                GDMRecord rec3 = tree.XRefIndex_Find(xref2);
                Assert.IsNotNull(rec3);
                Assert.AreEqual(rec2, rec3);

                /*string uid = rec2.UID;
				GEDCOMRecord rec4 = tree.FindUID(uid);
				Assert.IsNotNull(rec4);
				Assert.AreEqual(rec2, rec4);*/

                int idx = tree.IndexOf(rec2);
                Assert.AreEqual(i, idx);
            }
            
            Assert.IsFalse(tree.IsEmpty);

            Assert.IsFalse(tree.DeleteFamilyRecord(null));
            Assert.IsTrue(tree.DeleteFamilyRecord(famRec));

            Assert.IsFalse(tree.DeleteNoteRecord(null));
            Assert.IsTrue(tree.DeleteNoteRecord(noteRec));

            Assert.IsFalse(tree.DeleteSourceRecord(null));
            Assert.IsTrue(tree.DeleteSourceRecord(srcRec));

            Assert.IsFalse(tree.DeleteGroupRecord(null));
            Assert.IsTrue(tree.DeleteGroupRecord(groupRec));

            Assert.IsFalse(tree.DeleteLocationRecord(null));
            Assert.IsTrue(tree.DeleteLocationRecord(locRec));

            Assert.IsFalse(tree.DeleteResearchRecord(null));
            Assert.IsTrue(tree.DeleteResearchRecord(resRec));

            Assert.IsFalse(tree.DeleteCommunicationRecord(null));
            Assert.IsTrue(tree.DeleteCommunicationRecord(commRec));

            Assert.IsFalse(tree.DeleteTaskRecord(null));
            Assert.IsTrue(tree.DeleteTaskRecord(taskRec));

            Assert.IsFalse(tree.DeleteMediaRecord(null));
            Assert.IsTrue(tree.DeleteMediaRecord(mmRec));

            Assert.IsFalse(tree.DeleteIndividualRecord(null));
            Assert.IsTrue(tree.DeleteIndividualRecord(iRec));

            Assert.IsFalse(tree.DeleteRepositoryRecord(null));
            Assert.IsTrue(tree.DeleteRepositoryRecord(repRec));

            tree.Clear();
            Assert.AreEqual(0, tree.RecordsCount);
            Assert.IsTrue(tree.IsEmpty);

            tree.State = GEDCOMState.osReady;
            Assert.AreEqual(GEDCOMState.osReady, tree.State);


            // Tests of GEDCOMTree.Extract()
            using (GDMTree tree2 = new GDMTree()) {
                GDMIndividualRecord iRec2 = tree.AddRecord(new GDMIndividualRecord(tree2)) as GDMIndividualRecord;
                Assert.IsNotNull(iRec2);
                iRec2.InitNew();

                tree2.AddRecord(iRec2);
                int rIdx = tree2.IndexOf(iRec2);
                Assert.IsTrue(rIdx >= 0);
                GDMRecord extractedRec = tree2.Extract(rIdx);
                Assert.AreEqual(iRec2, extractedRec);
                Assert.IsTrue(tree2.IndexOf(iRec2) < 0);
            }
        }

        [Test]
        public void Test_GEDCOMHeader()
        {
            GEDCOMHeader headRec = fContext.Tree.Header;

            headRec.Notes = new StringList("This notes test");
            Assert.AreEqual("This notes test", headRec.Notes[0]);

            headRec.CharacterSet = GEDCOMCharacterSet.csASCII;
            Assert.AreEqual(GEDCOMCharacterSet.csASCII, headRec.CharacterSet);

            headRec.CharacterSetVersion = "1x";
            Assert.AreEqual("1x", headRec.CharacterSetVersion);

            headRec.Copyright = "copyright";
            Assert.AreEqual("copyright", headRec.Copyright);

            headRec.Source = "GEDKeeper";
            Assert.AreEqual("GEDKeeper", headRec.Source);

            headRec.ReceivingSystemName = "GEDKeeper";
            Assert.AreEqual("GEDKeeper", headRec.ReceivingSystemName);

            headRec.Language.Value = GEDCOMLanguageID.Russian;
            Assert.AreEqual("Russian", headRec.Language.StringValue);

            headRec.GEDCOMVersion = "5.5";
            Assert.AreEqual("5.5", headRec.GEDCOMVersion);

            headRec.GEDCOMForm = "LINEAGE-LINKED";
            Assert.AreEqual("LINEAGE-LINKED", headRec.GEDCOMForm);

            headRec.FileName = "testfile.ged";
            Assert.AreEqual("testfile.ged", headRec.FileName);

            DateTime dtx = DateTime.Now;
            dtx = dtx.AddTicks(-dtx.Ticks % 10000000);
            headRec.TransmissionDateTime = dtx;
            Assert.AreEqual(dtx, headRec.TransmissionDateTime);

            headRec.FileRevision = 113;
            Assert.AreEqual(113, headRec.FileRevision);

            headRec.PlaceHierarchy = "test11";
            Assert.AreEqual("test11", headRec.PlaceHierarchy);

            Assert.IsNotNull(headRec.SourceBusinessAddress);

            headRec.SourceBusinessName = "test23";
            Assert.AreEqual("test23", headRec.SourceBusinessName);

            headRec.SourceProductName = "test33";
            Assert.AreEqual("test33", headRec.SourceProductName);

            headRec.SourceVersion = "test44";
            Assert.AreEqual("test44", headRec.SourceVersion);

            Assert.IsNotNull(headRec.Submission);

            Assert.IsFalse(headRec.IsEmpty());
            headRec.Clear();
            Assert.IsTrue(headRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMMap()
        {
            using (GDMMap map = GDMMap.Create(null, "", "") as GDMMap) {
                map.Lati = 5.11111;
                Assert.AreEqual(5.11111, map.Lati);

                map.Long = 7.99999;
                Assert.AreEqual(7.99999, map.Long);
            }
        }

        [Test]
        public void Test_GEDCOMAux()
        {
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;
            GDMCustomEvent evt, evtd;

            evt = iRec.FindEvent(GEDCOMTagType.BIRT);
            Assert.IsNotNull(evt);

            evtd = iRec.FindEvent(GEDCOMTagType.DEAT);
            Assert.IsNotNull(evtd);

            GEDCOMCustomEventTest(evt, "28.12.1990");
            Assert.IsNotNull(evt.Address);
        }

        [Test]
        public void Test_GEDCOMIndividualRecord()
        {
            GDMIndividualRecord ind3 = fContext.Tree.XRefIndex_Find("I3") as GDMIndividualRecord;
            Assert.IsNotNull(ind3.GetParentsFamily());

            GDMIndividualRecord ind2 = fContext.Tree.XRefIndex_Find("I2") as GDMIndividualRecord;
            Assert.IsNotNull(ind2.GetMarriageFamily());

            //
            GDMIndividualRecord indiRec = fContext.Tree.XRefIndex_Find("I4") as GDMIndividualRecord;
            Assert.IsNull(indiRec.GetMarriageFamily());
            Assert.IsNotNull(indiRec.GetMarriageFamily(true));

            GEDCOMRecordTest(indiRec);

            Assert.IsNotNull(indiRec.Aliases);
            Assert.IsNotNull(indiRec.Associations);
            Assert.IsNotNull(indiRec.Submittors);
            Assert.IsNotNull(indiRec.UserReferences); // for GEDCOMRecord

            Assert.Throws(typeof(ArgumentException), () => { indiRec.AddEvent(GDMFamilyEvent.Create(null, "", "") as GDMCustomEvent); });

            GDMIndividualRecord father, mother;
            GDMFamilyRecord fam = indiRec.GetParentsFamily();
            if (fam == null) {
                father = null;
                mother = null;
            } else {
                father = fam.GetHusband();
                mother = fam.GetWife();
            }

            Assert.IsNull(father);
            Assert.IsNull(mother);

            indiRec.Sex = GEDCOMSex.svMale;
            Assert.AreEqual(GEDCOMSex.svMale, indiRec.Sex);

            indiRec.Restriction = GEDCOMRestriction.rnLocked;
            Assert.AreEqual(GEDCOMRestriction.rnLocked, indiRec.Restriction);

            indiRec.Patriarch = true;
            Assert.AreEqual(true, indiRec.Patriarch);
            indiRec.Patriarch = false;
            Assert.AreEqual(false, indiRec.Patriarch);

            indiRec.Bookmark = true;
            Assert.AreEqual(true, indiRec.Bookmark);
            indiRec.Bookmark = false;
            Assert.AreEqual(false, indiRec.Bookmark);

            indiRec.AncestralFileNumber = "test11";
            Assert.AreEqual("test11", indiRec.AncestralFileNumber);

            indiRec.PermanentRecordFileNumber = "test22";
            Assert.AreEqual("test22", indiRec.PermanentRecordFileNumber);

            Assert.Throws(typeof(ArgumentException), () => { indiRec.MoveTo(null, false); });

            using (GDMIndividualRecord copyIndi = new GDMIndividualRecord(null)) {
                Assert.IsNotNull(copyIndi);

                Assert.Throws(typeof(ArgumentException), () => { copyIndi.Assign(null); });

                copyIndi.Assign(indiRec);
                Assert.AreEqual(100.0f, indiRec.IsMatch(copyIndi, new MatchParams()));
            }


            Assert.IsFalse(indiRec.IsEmpty());
            indiRec.Clear();
            Assert.IsTrue(indiRec.IsEmpty());

            float ca = indiRec.GetCertaintyAssessment();
            Assert.AreEqual(0.0f, ca);


            Assert.IsNull(indiRec.GetPrimaryMultimediaLink());
            GDMMultimediaLink mmLink = indiRec.SetPrimaryMultimediaLink(null);
            Assert.IsNull(mmLink);
            GDMMultimediaRecord mmRec = fContext.Tree.CreateMultimedia();
            mmLink = indiRec.SetPrimaryMultimediaLink(mmRec);
            Assert.IsNotNull(mmLink);
            mmLink = indiRec.GetPrimaryMultimediaLink();
            Assert.AreEqual(mmRec, mmLink.Value);


            Assert.AreEqual(-1, indiRec.IndexOfGroup(null));
            Assert.AreEqual(-1, indiRec.IndexOfSpouse(null));


            GDMIndividualRecord indi2 = fContext.Tree.XRefIndex_Find("I2") as GDMIndividualRecord;
            GDMAssociation asso = indiRec.AddAssociation("test", indi2);
            Assert.IsNotNull(asso);

            using (GDMIndividualRecord indi = new GDMIndividualRecord(fContext.Tree)) {
                Assert.IsNotNull(indi);

                var parts = GKUtils.GetNameParts(indi); // test with empty PersonalNames
                Assert.AreEqual("", parts.Surname);
                Assert.AreEqual("", parts.Name);
                Assert.AreEqual("", parts.Patronymic);

                indi.AddPersonalName(new GDMPersonalName(indi, "", "")); // test with empty Name
                parts = GKUtils.GetNameParts(indi);
                Assert.AreEqual("", parts.Surname);
                Assert.AreEqual("", parts.Name);
                Assert.AreEqual("", parts.Patronymic);
                indi.PersonalNames.Clear();

                string st;
                Assert.AreEqual("", GKUtils.GetNameString(indi, true, false));
                Assert.AreEqual("", GKUtils.GetNickString(indi));

                GDMPersonalName pName = new GDMPersonalName(indi);
                indi.AddPersonalName(pName);
                pName.Pieces.Nickname = "BigHead";
                pName.SetNameParts("Ivan", "Petrov", "");

                st = GKUtils.GetNameString(indi, true, true);
                Assert.AreEqual("Petrov Ivan [BigHead]", st);
                st = GKUtils.GetNameString(indi, false, true);
                Assert.AreEqual("Ivan Petrov [BigHead]", st);
                Assert.AreEqual("BigHead", GKUtils.GetNickString(indi));

                Assert.IsNull(indi.GetParentsFamily());
                Assert.IsNotNull(indi.GetParentsFamily(true));

                // MoveTo test
                GDMIndividualRecord ind = fContext.Tree.XRefIndex_Find("I2") as GDMIndividualRecord;

                indi.AddAssociation("test", ind);
                indi.Aliases.Add(new GDMAlias(indi, "", ""));
                indi.Submittors.Add(new GDMPointer(indi, "", ""));

                using (GDMIndividualRecord indi3 = new GDMIndividualRecord(fContext.Tree)) {
                    indi.MoveTo(indi3, false);

                    st = GKUtils.GetNameString(indi3, true, true);
                    Assert.AreEqual("Petrov Ivan [BigHead]", st);
                }

                indi.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, indi.GetTree());
            }
        }

        [Test]
        public void Test_GEDCOMPersonalName()
        {
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;

            GDMPersonalName pName = iRec.PersonalNames[0];
            Assert.AreEqual("Ivanov", pName.Surname);
            Assert.AreEqual("Ivan Ivanovich", pName.FirstPart);

            pName.SetNameParts("Ivan Ivanovich", "Ivanov", "testLastPart");
            Assert.AreEqual("Ivanov", pName.Surname);
            Assert.AreEqual("Ivan Ivanovich", pName.FirstPart);
            Assert.AreEqual("testLastPart", pName.LastPart);

//			GEDCOMPersonalNamePieces pieces = pName.Pieces;
//			Assert.AreEqual(pieces.Surname, "surname");
//			Assert.AreEqual(pieces.Name, "name");
//			Assert.AreEqual(pieces.PatronymicName, "patr");

            var parts = GKUtils.GetNameParts(iRec);
            Assert.AreEqual("Ivanov", parts.Surname);
            Assert.AreEqual("Ivan", parts.Name);
            Assert.AreEqual("Ivanovich", parts.Patronymic);


            GDMPersonalName persName = GDMPersonalName.Create(iRec, "", "") as GDMPersonalName;
            iRec.AddPersonalName(persName);

            persName = iRec.PersonalNames[0];
            persName.NameType = GEDCOMNameType.ntBirth;
            Assert.AreEqual(GEDCOMNameType.ntBirth, persName.NameType);

            //

            persName.SetNameParts("Petr", "Ivanov", "Fedoroff");

            //persName.Surname = "Ivanov";
            Assert.AreEqual("Petr", persName.FirstPart);
            Assert.AreEqual("Ivanov", persName.Surname);
            Assert.AreEqual("Fedoroff", persName.LastPart);

            Assert.AreEqual("Petr Ivanov Fedoroff", persName.FullName);

            persName.FirstPart = "Petr";
            Assert.AreEqual("Petr", persName.FirstPart);

            persName.Surname = "Test";
            Assert.AreEqual("Test", persName.Surname);

            persName.LastPart = "Fedoroff";
            Assert.AreEqual("Fedoroff", persName.LastPart);

            //

            GDMPersonalNamePieces pnPieces = persName.Pieces;
            
            pnPieces.Prefix = "Prefix";
            Assert.AreEqual("Prefix", pnPieces.Prefix);

            pnPieces.Given = "Given";
            Assert.AreEqual("Given", pnPieces.Given);

            pnPieces.Nickname = "Nickname";
            Assert.AreEqual("Nickname", pnPieces.Nickname);

            pnPieces.SurnamePrefix = "SurnamePrefix";
            Assert.AreEqual("SurnamePrefix", pnPieces.SurnamePrefix);

            pnPieces.Surname = "Surname";
            Assert.AreEqual("Surname", pnPieces.Surname);

            pnPieces.Suffix = "Suffix";
            Assert.AreEqual("Suffix", pnPieces.Suffix);

            pnPieces.PatronymicName = "PatronymicName";
            Assert.AreEqual("PatronymicName", pnPieces.PatronymicName);

            pnPieces.MarriedName = "MarriedName";
            Assert.AreEqual("MarriedName", pnPieces.MarriedName);

            pnPieces.ReligiousName = "ReligiousName";
            Assert.AreEqual("ReligiousName", pnPieces.ReligiousName);

            pnPieces.CensusName = "CensusName";
            Assert.AreEqual("CensusName", pnPieces.CensusName);

            persName.Pack();

            //

            Assert.AreEqual(GEDCOMLanguageID.Unknown, persName.Language.Value);
            persName.Language.Value = GEDCOMLanguageID.English;
            Assert.AreEqual(GEDCOMLanguageID.English, persName.Language.Value);
            persName.Language.Value = GEDCOMLanguageID.Unknown;
            Assert.AreEqual(GEDCOMLanguageID.Unknown, persName.Language.Value);
            persName.Language.Value = GEDCOMLanguageID.Polish;
            Assert.AreEqual(GEDCOMLanguageID.Polish, persName.Language.Value);

            //

            string buf = TestUtils.GetTagStreamText(persName, 1);
            Assert.AreEqual("1 NAME Petr /Test/ Fedoroff\r\n"+
                            "2 TYPE birth\r\n"+
                            "2 _LANG Polish\r\n"+ // extension
                            "2 SURN Surname\r\n"+
                            "2 GIVN Given\r\n"+
                            "2 _PATN PatronymicName\r\n"+
                            "2 NPFX Prefix\r\n"+
                            "2 NICK Nickname\r\n"+
                            "2 SPFX SurnamePrefix\r\n"+
                            "2 NSFX Suffix\r\n"+
                            "2 _MARN MarriedName\r\n"+
                            "2 _RELN ReligiousName\r\n"+
                            "2 _CENN CensusName\r\n", buf);

            persName.Language.Value = GEDCOMLanguageID.Unknown;
            persName.Pack();

            using (GDMPersonalName nameCopy = new GDMPersonalName(iRec, "", "")) {
                Assert.Throws(typeof(ArgumentException), () => { nameCopy.Assign(null); });

                iRec.AddPersonalName(nameCopy);
                nameCopy.Assign(persName);

                string buf2 = TestUtils.GetTagStreamText(nameCopy, 1);
                Assert.AreEqual("1 NAME Petr /Test/ Fedoroff\r\n"+
                                "2 TYPE birth\r\n"+
                                "2 SURN Surname\r\n"+
                                "2 GIVN Given\r\n"+
                                "2 _PATN PatronymicName\r\n"+
                                "2 NPFX Prefix\r\n"+
                                "2 NICK Nickname\r\n"+
                                "2 SPFX SurnamePrefix\r\n"+
                                "2 NSFX Suffix\r\n"+
                                "2 _MARN MarriedName\r\n"+
                                "2 _RELN ReligiousName\r\n"+
                                "2 _CENN CensusName\r\n", buf2);

                iRec.PersonalNames.Delete(nameCopy);
            }

            using (GDMPersonalName name1 = new GDMPersonalName(null, "", "")) {
                Assert.AreEqual("", name1.FirstPart);
                Assert.AreEqual("", name1.Surname);

                Assert.AreEqual(0.0f, name1.IsMatch(null, false));

                using (GDMPersonalName name2 = new GDMPersonalName(null, "", "")) {
                    Assert.AreEqual(0.0f, name1.IsMatch(name2, false));

                    name1.SetNameParts("Ivan", "Dub", "");
                    name2.SetNameParts("Ivan", "Dub", "");
                    Assert.AreEqual(100.0f, name1.IsMatch(name2, false));

                    name1.SetNameParts("Ivan", "Dub", "");
                    name2.SetNameParts("Ivan", "Dub2", "");
                    Assert.AreEqual(12.5f, name1.IsMatch(name2, false));

                    name1.SetNameParts("Ivan", "Dub", "");
                    name2.SetNameParts("Ivan2", "Dub", "");
                    Assert.AreEqual(50.0f, name1.IsMatch(name2, false));
                }
            }

            persName.ResetOwner(fContext.Tree);
            Assert.AreEqual(fContext.Tree, persName.GetTree());

            persName.Clear();
            Assert.IsTrue(persName.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMFileReference()
        {
            using (GDMFileReference fileRef = new GDMFileReference(null, "", "")) {
                fileRef.MediaType = GEDCOMMediaType.mtAudio;
                Assert.AreEqual(GEDCOMMediaType.mtAudio, fileRef.MediaType);
            }

            Assert.AreEqual(GEDCOMMultimediaFormat.mfUnknown, GDMFileReference.RecognizeFormat(""));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfUnknown, GDMFileReference.RecognizeFormat("sample.xxx"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfBMP, GDMFileReference.RecognizeFormat("sample.BMP"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfGIF, GDMFileReference.RecognizeFormat("sample.Gif"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfJPG, GDMFileReference.RecognizeFormat("sample.jpg"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfJPG, GDMFileReference.RecognizeFormat("sample.Jpeg"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfOLE, GDMFileReference.RecognizeFormat("sample.ole"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfPCX, GDMFileReference.RecognizeFormat("sample.pCx"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfTIF, GDMFileReference.RecognizeFormat("sample.TiF"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfTIF, GDMFileReference.RecognizeFormat("sample.tiff"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfWAV, GDMFileReference.RecognizeFormat("sample.wav"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfTXT, GDMFileReference.RecognizeFormat("sample.txt"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfRTF, GDMFileReference.RecognizeFormat("sample.rtf"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfAVI, GDMFileReference.RecognizeFormat("sample.AvI"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfTGA, GDMFileReference.RecognizeFormat("sample.TGA"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfPNG, GDMFileReference.RecognizeFormat("sample.png"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfMPG, GDMFileReference.RecognizeFormat("sample.mpg"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfMPG, GDMFileReference.RecognizeFormat("sample.mpeg"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfHTM, GDMFileReference.RecognizeFormat("sample.htm"));
            Assert.AreEqual(GEDCOMMultimediaFormat.mfHTM, GDMFileReference.RecognizeFormat("sample.html"));
        }

        [Test]
        public void Test_GEDCOMLanguage()
        {
            using (GDMLanguage langTag = GDMLanguage.Create(null, "", "") as GDMLanguage) {
                Assert.IsTrue(langTag.IsEmpty());

                langTag.Value = GEDCOMLanguageID.AngloSaxon;
                Assert.AreEqual(GEDCOMLanguageID.AngloSaxon, langTag.Value);

                langTag.ParseString("Spanish");
                Assert.AreEqual("Spanish", langTag.StringValue);

                using (GDMLanguage langTag2 = GDMLanguage.Create(null, "", "") as GDMLanguage) {
                    Assert.IsTrue(langTag2.IsEmpty());

                    Assert.Throws(typeof(ArgumentException), () => { langTag2.Assign(null); });

                    langTag2.Assign(langTag);
                    Assert.AreEqual("Spanish", langTag2.StringValue);
                }

                langTag.Clear();
                Assert.IsTrue(langTag.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMGroupRecord()
        {
            using (GDMGroupRecord grpRec = new GDMGroupRecord(fContext.Tree))
            {
                Assert.IsNotNull(grpRec);

                grpRec.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, grpRec.GetTree());
            }

            using (GDMGroupRecord groupRec = fContext.Tree.CreateGroup()) {
                GDMIndividualRecord member = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;

                groupRec.GroupName = "Test Group";
                Assert.AreEqual("Test Group", groupRec.GroupName);

                groupRec.UID = string.Empty;
                groupRec.DeleteTag(GEDCOMTagType.CHAN);
                string buf = TestUtils.GetTagStreamText(groupRec, 0);
                Assert.AreEqual("0 @G2@ _GROUP\r\n1 NAME Test Group\r\n", buf);

                bool res = groupRec.AddMember(null);
                Assert.IsFalse(res);

                res = groupRec.RemoveMember(null);
                Assert.IsFalse(res);

                Assert.AreEqual(-1, groupRec.IndexOfMember(null));

                groupRec.AddMember(member);
                Assert.AreEqual(0, groupRec.IndexOfMember(member));

                groupRec.RemoveMember(member);
                Assert.AreEqual(-1, groupRec.IndexOfMember(member));

                Assert.IsFalse(groupRec.IsEmpty());
                groupRec.Clear();
                Assert.IsTrue(groupRec.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMList()
        {
            GDMTag obj1 = new GDMTag(null);
            GDMTag obj2 = new GDMTag(null);

            using (GDMList<GDMTag> list = new GDMList<GDMTag>(null)) {
                // internal list is null (all routines instant returned)
                list.Delete(null);
                list.Exchange(0, 1);
                Assert.IsNull(list.Extract(0));
                Assert.IsNull(list.Extract(null));

                // normal checks
                list.Add(obj1);
                list.Add(obj2);
                Assert.AreEqual(0, list.IndexOf(obj1));
                Assert.AreEqual(1, list.IndexOf(obj2));

                list.Delete(obj1);
                Assert.AreEqual(-1, list.IndexOf(obj1));
                Assert.AreEqual(0, list.IndexOf(obj2));

                list.Add(obj1);
                Assert.AreEqual(1, list.IndexOf(obj1));
                Assert.AreEqual(0, list.IndexOf(obj2));
                list.Exchange(0, 1);
                Assert.AreEqual(0, list.IndexOf(obj1));
                Assert.AreEqual(1, list.IndexOf(obj2));

                Assert.AreEqual(null, list.Extract(null));
                list.Add(obj1);
                Assert.AreEqual(obj1, list.Extract(obj1));

                foreach (GDMObject obj in list) {
                }
            }
        }

        #endregion

        #region Partial Tests

        [Test]
        public void Test_GEDCOMCustomEvent()
        {
            using (GDMIndividualAttribute customEvent = GDMIndividualAttribute.Create(null, "", "") as GDMIndividualAttribute)
            {
                Assert.IsNotNull(customEvent);

                StringList strs = new StringList("test");
                customEvent.PhysicalDescription = strs;
                Assert.AreEqual(strs.Text, customEvent.PhysicalDescription.Text);

                customEvent.Address.AddEmailAddress("email");
                Assert.AreEqual("email", customEvent.Address.EmailAddresses[0].StringValue);

                customEvent.Pack();
            }

            using (GDMIndividualEvent customEvent = GDMIndividualEvent.Create(null, "", "") as GDMIndividualEvent)
            {
                Assert.IsNotNull(customEvent);

                // stream test
                customEvent.SetName(GEDCOMTagType.BIRT);
                customEvent.Date.ParseString("20 SEP 1970");
                customEvent.Place.StringValue = "test place";
                string buf = TestUtils.GetTagStreamText(customEvent, 0);
                Assert.AreEqual("0 BIRT\r\n"+
                                "1 DATE 20 SEP 1970\r\n"+
                                "1 PLAC test place\r\n", buf);

                using (GDMIndividualEvent copyEvent = GDMIndividualEvent.Create(null, "", "") as GDMIndividualEvent)
                {
                    Assert.IsNotNull(copyEvent);
                    copyEvent.Assign(customEvent);

                    string buf1 = TestUtils.GetTagStreamText(copyEvent, 0);
                    Assert.AreEqual("0 BIRT\r\n"+
                                    "1 DATE 20 SEP 1970\r\n"+
                                    "1 PLAC test place\r\n", buf1);
                }

                customEvent.Address.AddEmailAddress("email");
                Assert.AreEqual("email", customEvent.Address.EmailAddresses[0].StringValue);

                customEvent.Pack();
            }

            using (GDMFamilyEvent customEvent = GDMFamilyEvent.Create(null, "", "") as GDMFamilyEvent)
            {
                Assert.IsNotNull(customEvent);

                customEvent.Address.AddEmailAddress("email");
                Assert.AreEqual("email", customEvent.Address.EmailAddresses[0].StringValue);

                customEvent.Pack();
            }
        }

        public static void GEDCOMCustomEventTest(GDMCustomEvent evt, string dateTest)
        {
            GEDCOMEventDetailTest(evt, dateTest);

            Assert.AreEqual(evt.Date.GetDateTime(), TestUtils.ParseDT(dateTest));
        }

        [Test]
        public void Test_GEDCOMPlace()
        {
            using (GDMPlace place = GDMPlace.Create(null, "", "") as GDMPlace) {
                place.Form = "abrakadabra";
                Assert.AreEqual("abrakadabra", place.Form);

                Assert.IsNotNull(place.Map);
                Assert.IsNotNull(place.Location);
            }
        }

        private static void GEDCOMEventDetailTest(GDMCustomEvent detail, string dateTest)
        {
            Assert.AreEqual(TestUtils.ParseDT(dateTest), detail.Date.Date);
            Assert.AreEqual("Ivanovo", detail.Place.StringValue);

            Assert.IsNotNull(detail.Place);

            detail.Agency = "test agency";
            Assert.AreEqual("test agency", detail.Agency);

            detail.Classification = "test type";
            Assert.AreEqual("test type", detail.Classification);

            detail.Cause = "test cause";
            Assert.AreEqual("test cause", detail.Cause);

            detail.ReligiousAffilation = "test aff";
            Assert.AreEqual("test aff", detail.ReligiousAffilation);

            detail.Restriction = GEDCOMRestriction.rnLocked;
            Assert.AreEqual(GEDCOMRestriction.rnLocked, detail.Restriction);
        }

        [Test]
        public void Test_GEDCOMTag()
        {
            using (GDMTag tag = GDMTag.Create(null, "", "")) {
                Assert.AreEqual(-1, tag.IndexOfTag(null));
            }
        }

        private static void GEDCOMRecordTest(GDMRecord rec)
        {
            Assert.Throws(typeof(ArgumentException), () => { rec.Assign(null); });

            rec.AutomatedRecordID = "test11";
            Assert.AreEqual("test11", rec.AutomatedRecordID);

            Assert.AreEqual(GEDCOMRecordType.rtIndividual, rec.RecordType);

            Assert.AreEqual(4, rec.GetId());
            Assert.AreEqual("4", rec.GetXRefNum());

            Assert.AreEqual(-1, rec.IndexOfSource(null));

            rec.AddUserRef("test userref");
            Assert.AreEqual("test userref", rec.UserReferences[0].StringValue);
        }

        [Test]
        public void Test_GEDCOMFamilyRecord()
        {
            using (GDMFamilyRecord famRec = new GDMFamilyRecord(fContext.Tree))
            {
                Assert.IsNotNull(famRec);

                GDMIndividualRecord unkInd = new GDMIndividualRecord(null);
                unkInd.Sex = GEDCOMSex.svUndetermined;
                Assert.IsFalse(famRec.AddSpouse(unkInd));

                GDMIndividualRecord child1 = fContext.Tree.CreateIndividual(); // for pointer need a proper object
                Assert.IsTrue(famRec.AddChild(child1));

                GDMIndividualRecord child2 = fContext.Tree.CreateIndividual(); // for pointer need a proper object
                Assert.IsTrue(famRec.AddChild(child2));
                Assert.AreEqual(1, famRec.IndexOfChild(child2));

                famRec.DeleteChild(child1);
                Assert.AreEqual(-1, famRec.IndexOfChild(child1));

                string str = GKUtils.GetFamilyString(famRec, null, null);
                Assert.AreEqual("? - ?", str);

                str = GKUtils.GetFamilyString(famRec, "x", "x");
                Assert.AreEqual("x - x", str);

                Assert.AreEqual(0.0f, famRec.IsMatch(null, new MatchParams()));
                Assert.AreEqual(100.0f, famRec.IsMatch(famRec, new MatchParams()));

                // MoveTo test
                Assert.Throws(typeof(ArgumentException), () => { famRec.MoveTo(null, false); });

                GDMCustomEvent evt = famRec.AddEvent(new GDMFamilyEvent(famRec, GEDCOMTagType.MARR, "01 SEP 1981"));
                Assert.AreEqual(1, famRec.Events.Count);
                Assert.AreEqual(evt, famRec.FindEvent(GEDCOMTagType.MARR));

                using (GDMFamilyRecord famRec2 = new GDMFamilyRecord(fContext.Tree))
                {
                    Assert.AreEqual(0, famRec2.Events.Count);
                    Assert.AreEqual(null, famRec2.FindEvent(GEDCOMTagType.MARR));

                    famRec.MoveTo(famRec2, false);

                    Assert.AreEqual(1, famRec2.Events.Count);
                    Assert.AreEqual(evt, famRec2.FindEvent(GEDCOMTagType.MARR));
                }

                famRec.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, famRec.GetTree());
            }
        }

        private static void GEDCOMFamilyRecordTest(GDMFamilyRecord famRec, GDMIndividualRecord indiv)
        {
            Assert.IsNotNull(famRec.Submittors);

            famRec.Restriction = GEDCOMRestriction.rnLocked;
            Assert.AreEqual(GEDCOMRestriction.rnLocked, famRec.Restriction);

            famRec.AddChild(indiv);
            Assert.AreEqual(0, famRec.IndexOfChild(indiv));

            // stream test
            famRec.UID = string.Empty;
            famRec.DeleteTag(GEDCOMTagType.CHAN);
            string buf = TestUtils.GetTagStreamText(famRec, 0);
            Assert.AreEqual("0 @F1@ FAM\r\n"+
                            "1 RESN locked\r\n"+
                            "1 CHIL @I1@\r\n", buf);

            // Integrity test
            GDMChildToFamilyLink childLink = indiv.ChildToFamilyLinks[0];
            Assert.IsNotNull(childLink.Family);

            famRec.RemoveChild(indiv);
            Assert.AreEqual(-1, famRec.IndexOfChild(indiv));

            //

            Assert.Throws(typeof(ArgumentException), () => { famRec.AddEvent(GDMIndividualEvent.Create(null, "", "") as GDMCustomEvent); });

            //

            famRec.Husband.Value = indiv;
            Assert.AreEqual(indiv, famRec.GetHusband());
            famRec.Husband.Value = null;

            //

            famRec.Wife.Value = indiv;
            Assert.AreEqual(indiv, famRec.GetWife());
            famRec.Wife.Value = null;

            //

            indiv.Sex = GEDCOMSex.svMale;
            famRec.AddSpouse(indiv);
            Assert.AreEqual(0, indiv.IndexOfSpouse(famRec));
            GEDCOMSpouseToFamilyLinkTest(indiv.SpouseToFamilyLinks[0]);
            Assert.IsNull(famRec.GetSpouseBy(indiv));
            famRec.RemoveSpouse(indiv);

            indiv.Sex = GEDCOMSex.svFemale;
            famRec.AddSpouse(indiv);
            Assert.AreEqual(0, indiv.IndexOfSpouse(famRec));
            GEDCOMSpouseToFamilyLinkTest(indiv.SpouseToFamilyLinks[0]);
            Assert.IsNull(famRec.GetSpouseBy(indiv));
            famRec.RemoveSpouse(indiv);

            //

            famRec.SortChilds();

            //

            famRec.AddChild(null);
            famRec.RemoveChild(null);
            famRec.AddSpouse(null);
            famRec.RemoveSpouse(null);

            Assert.IsFalse(famRec.IsEmpty());
            famRec.Clear();
            Assert.IsTrue(famRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMChildToFamilyLink()
        {
            using (GDMChildToFamilyLink childLink = GDMChildToFamilyLink.Create(null, "", "") as GDMChildToFamilyLink)
            {
                Assert.IsNotNull(childLink);

                childLink.ChildLinkageStatus = GDMChildLinkageStatus.clChallenged;
                Assert.AreEqual(GDMChildLinkageStatus.clChallenged, childLink.ChildLinkageStatus);

                childLink.PedigreeLinkageType = GDMPedigreeLinkageType.plFoster;
                Assert.AreEqual(GDMPedigreeLinkageType.plFoster, childLink.PedigreeLinkageType);
            }
        }

        private static void GEDCOMSpouseToFamilyLinkTest(GDMSpouseToFamilyLink spouseLink)
        {
            Assert.IsNotNull(spouseLink.Family);
            
            using (spouseLink = GDMSpouseToFamilyLink.Create(null, "", "") as GDMSpouseToFamilyLink)
            {
                Assert.IsNotNull(spouseLink);
            }
        }

        [Test]
        public void Test_GEDCOMSourceRecord()
        {
            GDMTree tree = new GDMTree();

            // check match
            using (GDMSourceRecord src1 = new GDMSourceRecord(tree))
            {
                Assert.IsNotNull(src1, "src1 != null");

                Assert.Throws(typeof(ArgumentNullException), () => { src1.RemoveRepository(null); });

                using (GDMSourceRecord src2 = new GDMSourceRecord(tree))
                {
                    Assert.IsNotNull(src2, "src2 != null");

                    Assert.AreEqual(0.0f, src1.IsMatch(null, new MatchParams()));

                    // empty records
                    Assert.AreEqual(100.0f, src1.IsMatch(src2, new MatchParams()));

                    // filled records
                    src1.ShortTitle = "test source";
                    src2.ShortTitle = "test source";
                    Assert.AreEqual(100.0f, src1.IsMatch(src2, new MatchParams()));
                }

                src1.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, src1.GetTree());
            }

            // check move
            using (GDMSourceRecord src1 = new GDMSourceRecord(tree))
            {
                Assert.Throws(typeof(ArgumentException), () => { src1.MoveTo(null, false); });

                // fill the record
                src1.ShortTitle = "test source";
                src1.Title = new StringList("test title");
                src1.Originator = new StringList("test author");
                src1.Publication = new StringList("test publ");
                src1.Text = new StringList("test text");

                Assert.AreEqual("test source", src1.ShortTitle);
                Assert.AreEqual("test title", src1.Title.Text);
                Assert.AreEqual("test author", src1.Originator.Text);
                Assert.AreEqual("test publ", src1.Publication.Text);
                Assert.AreEqual("test text", src1.Text.Text);

                GDMRepositoryRecord repRec = tree.CreateRepository();
                repRec.RepositoryName = "test repository";
                src1.AddRepository(repRec);
                Assert.AreEqual(1, src1.RepositoryCitations.Count);

                using (GDMSourceRecord src2 = new GDMSourceRecord(tree))
                {
                    src2.ShortTitle = "test source 2"; // title isn't replaced

                    Assert.AreEqual(0, src2.RepositoryCitations.Count);

                    src1.MoveTo(src2, false);

                    Assert.AreEqual("test source 2", src2.ShortTitle);

                    Assert.AreEqual("test title", src2.Title.Text);
                    Assert.AreEqual("test author", src2.Originator.Text);
                    Assert.AreEqual("test publ", src2.Publication.Text);
                    Assert.AreEqual("test text", src2.Text.Text);

                    Assert.AreEqual(1, src2.RepositoryCitations.Count);
                }
            }
        }

        private static void GEDCOMSourceRecordTest(GDMSourceRecord sourRec, GDMIndividualRecord indiv, GDMRepositoryRecord repRec)
        {
            Assert.IsNotNull(sourRec.Data);
            
            sourRec.ShortTitle = "This is test source";
            Assert.AreEqual("This is test source", sourRec.ShortTitle);

            //
            sourRec.Originator = new StringList("author");
            Assert.AreEqual("author", sourRec.Originator.Text.Trim());
            
            sourRec.Title = new StringList("title");
            Assert.AreEqual("title", sourRec.Title.Text.Trim());
            
            sourRec.Publication = new StringList("publication");
            Assert.AreEqual("publication", sourRec.Publication.Text.Trim());
            
            sourRec.Text = new StringList("sample");
            Assert.AreEqual("sample", sourRec.Text.Text.Trim());

            //
            sourRec.SetOriginatorArray(new string[] {"author"});
            Assert.AreEqual("author", sourRec.Originator.Text.Trim());
            
            sourRec.SetTitleArray(new string[] {"title"});
            Assert.AreEqual("title", sourRec.Title.Text.Trim());
            
            sourRec.SetPublicationArray(new string[] {"publication"});
            Assert.AreEqual("publication", sourRec.Publication.Text.Trim());
            
            sourRec.SetTextArray(new string[] {"sample"});
            Assert.AreEqual("sample", sourRec.Text.Text.Trim());
            
            //
            GEDCOMSourceCitationTest(sourRec, indiv);
            GEDCOMRepositoryCitationTest(sourRec, repRec);

            sourRec.UID = string.Empty;
            sourRec.DeleteTag(GEDCOMTagType.CHAN);
            string buf = TestUtils.GetTagStreamText(sourRec, 0);
            Assert.AreEqual("0 @S1@ SOUR\r\n"+
                            "1 DATA\r\n"+
                            "1 ABBR This is test source\r\n"+
                            "1 AUTH author\r\n"+
                            "1 TITL title\r\n"+
                            "1 PUBL publication\r\n"+
                            "1 TEXT sample\r\n"+
                            "1 REPO @R1@\r\n", buf);

            //
            Assert.IsFalse(sourRec.IsEmpty());
            sourRec.Clear();
            Assert.IsTrue(sourRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMSourceCitation()
        {
            using (GDMSourceCitation srcCit = GDMSourceCitation.Create(null, "", "") as GDMSourceCitation) {
                Assert.IsNotNull(srcCit);
            }
        }

        private static void GEDCOMSourceCitationTest(GDMSourceRecord sourRec, GDMIndividualRecord indiv)
        {
            GDMSourceCitation srcCit = indiv.AddSource(sourRec, "p2", 3);

            int idx = indiv.IndexOfSource(sourRec);
            Assert.AreEqual(0, idx);

            Assert.AreEqual("p2", srcCit.Page);
            Assert.AreEqual(3, srcCit.CertaintyAssessment);

            Assert.IsTrue(srcCit.IsPointer, "srcCit.IsPointer");

            Assert.IsFalse(srcCit.IsEmpty(), "srcCit.IsEmpty()"); // its pointer

            srcCit.Clear();
            srcCit.Value = null;

            Assert.IsTrue(srcCit.IsEmpty(), "srcCit.IsEmpty()"); // its pointer
            
            StringList strs = new StringList("test");
            srcCit.Description = strs;
            
            strs = srcCit.Description;
            Assert.AreEqual("test", strs.Text);
        }

        private static void GEDCOMRepositoryCitationTest(GDMSourceRecord sourRec, GDMRepositoryRecord repRec)
        {
            GDMRepositoryCitation repCit = sourRec.AddRepository(repRec);

            Assert.IsFalse(repCit.IsEmpty(), "repCit.IsEmpty()"); // its pointer
        }

        [Test]
        public void Test_GEDCOMResearchRecord()
        {
            using (GDMResearchRecord resRec = new GDMResearchRecord(fContext.Tree)) {
                resRec.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, resRec.GetTree());
            }
        }

        private static void GEDCOMResearchRecordTest(GDMResearchRecord resRec, GDMCommunicationRecord commRec, GDMTaskRecord taskRec, GDMGroupRecord groupRec)
        {
            Assert.IsNotNull(resRec.Communications);
            Assert.IsNotNull(resRec.Groups);
            Assert.IsNotNull(resRec.Tasks);
            
            resRec.ResearchName = "Test Research";
            Assert.AreEqual("Test Research", resRec.ResearchName);
            
            resRec.Priority = GDMResearchPriority.rpNormal;
            Assert.AreEqual(GDMResearchPriority.rpNormal, resRec.Priority);
            
            resRec.Status = GDMResearchStatus.rsOnHold;
            Assert.AreEqual(GDMResearchStatus.rsOnHold, resRec.Status);
            
            resRec.StartDate.Date = TestUtils.ParseDT("20.01.2013");
            Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), resRec.StartDate.Date);
            
            resRec.StopDate.Date = TestUtils.ParseDT("21.01.2013");
            Assert.AreEqual(TestUtils.ParseDT("21.01.2013"), resRec.StopDate.Date);
            
            resRec.Percent = 33;
            Assert.AreEqual(33, resRec.Percent);

            resRec.UID = string.Empty;
            resRec.DeleteTag(GEDCOMTagType.CHAN);
            string buf = TestUtils.GetTagStreamText(resRec, 0);
            Assert.AreEqual("0 @RS1@ _RESEARCH\r\n"+
                            "1 NAME Test Research\r\n"+
                            "1 _PRIORITY normal\r\n"+
                            "1 _STATUS onhold\r\n"+
                            "1 _STARTDATE 20 JAN 2013\r\n"+
                            "1 _STOPDATE 21 JAN 2013\r\n"+
                            "1 _PERCENT 33\r\n", buf);

            Assert.AreEqual(-1, resRec.IndexOfCommunication(null));
            resRec.AddCommunication(commRec);
            resRec.RemoveCommunication(commRec);
            resRec.RemoveCommunication(null);

            Assert.AreEqual(-1, resRec.IndexOfTask(null));
            resRec.AddTask(taskRec);
            resRec.RemoveTask(taskRec);
            resRec.RemoveTask(null);

            Assert.AreEqual(-1, resRec.IndexOfGroup(null));
            resRec.AddGroup(groupRec);
            resRec.RemoveGroup(groupRec);
            resRec.RemoveGroup(null);

            Assert.IsFalse(resRec.IsEmpty());
            resRec.Clear();
            Assert.IsTrue(resRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMRepositoryRecord()
        {
            using (GDMRepositoryRecord repoRec = new GDMRepositoryRecord(fContext.Tree))
            {
                Assert.IsNotNull(repoRec);

                repoRec.InitNew();
                repoRec.RepositoryName = "Test Repository";
                Assert.AreEqual("Test Repository", repoRec.RepositoryName);

                Assert.IsNotNull(repoRec.Address);

                repoRec.UID = string.Empty;
                repoRec.DeleteTag(GEDCOMTagType.CHAN);
                string buf = TestUtils.GetTagStreamText(repoRec, 0);
                Assert.AreEqual("0 @R2@ REPO\r\n"+
                                "1 NAME Test Repository\r\n"+
                                "1 ADDR\r\n", buf);

                Assert.IsFalse(repoRec.IsEmpty());
                repoRec.Clear();
                Assert.IsTrue(repoRec.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMMultimediaRecord()
        {
            using (var mmRec = new GDMMultimediaRecord(fContext.Tree))
            {
                Assert.IsNotNull(mmRec);

                mmRec.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, mmRec.GetTree());
            }
        }

        private static void GEDCOMMultimediaRecordTest(GDMMultimediaRecord mediaRec, GDMIndividualRecord indiv)
        {
            Assert.AreEqual("", mediaRec.GetFileTitle());

            mediaRec.FileReferences.Add(new GDMFileReferenceWithTitle(mediaRec, GEDCOMTagType.FILE, ""));
            GDMFileReferenceWithTitle fileRef = mediaRec.FileReferences[0];
            Assert.IsNotNull(fileRef);

            fileRef.Title = "File Title 2";
            Assert.AreEqual("File Title 2", fileRef.Title);

            fileRef.LinkFile("sample.png");
            fileRef.MediaType = GEDCOMMediaType.mtManuscript;
            Assert.AreEqual("sample.png", fileRef.StringValue);
            Assert.AreEqual(GEDCOMMultimediaFormat.mfPNG, fileRef.MultimediaFormat);
            Assert.AreEqual(GEDCOMMediaType.mtManuscript, fileRef.MediaType);

            string title = mediaRec.GetFileTitle();
            Assert.AreEqual("File Title 2", title);

            mediaRec.UID = string.Empty;
            mediaRec.DeleteTag(GEDCOMTagType.CHAN);
            string buf = TestUtils.GetTagStreamText(mediaRec, 0);
            Assert.AreEqual("0 @O1@ OBJE\r\n"+
                            "1 FILE sample.png\r\n"+
                            "2 TITL File Title 2\r\n"+
                            "2 FORM png\r\n"+
                            "3 TYPE manuscript\r\n", buf);
            
            GEDCOMMultimediaLinkTest(mediaRec, indiv);
            
            Assert.IsFalse(mediaRec.IsEmpty());
            mediaRec.Clear();
            Assert.IsTrue(mediaRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMMultimediaLink()
        {
            var iRec = new GDMIndividualRecord(fContext.Tree);
            using (GDMMultimediaLink mmLink = GDMMultimediaLink.Create(iRec, "", "") as GDMMultimediaLink) {
                Assert.IsNotNull(mmLink);
                Assert.IsTrue(mmLink.IsEmpty());

                // extensions
                Assert.IsFalse(mmLink.IsPrimaryCutout);
                mmLink.IsPrimaryCutout = true;
                Assert.IsTrue(mmLink.IsPrimaryCutout);

                mmLink.CutoutPosition.Value = ExtRect.Create(10, 15, 500, 600);
                ExtRect rt = mmLink.CutoutPosition.Value;
                Assert.AreEqual(10, rt.Left);
                Assert.AreEqual(15, rt.Top);
                Assert.AreEqual(500, rt.Right);
                Assert.AreEqual(600, rt.Bottom);

                Assert.AreEqual(10, mmLink.CutoutPosition.X1);
                Assert.AreEqual(15, mmLink.CutoutPosition.Y1);
                Assert.AreEqual(500, mmLink.CutoutPosition.X2);
                Assert.AreEqual(600, mmLink.CutoutPosition.Y2);

                mmLink.CutoutPosition.X1 = 10;
                mmLink.CutoutPosition.Y1 = 10;
                mmLink.CutoutPosition.X2 = 300;
                mmLink.CutoutPosition.Y2 = 400;
                Assert.AreEqual(10, mmLink.CutoutPosition.X1);
                Assert.AreEqual(10, mmLink.CutoutPosition.Y1);
                Assert.AreEqual(300, mmLink.CutoutPosition.X2);
                Assert.AreEqual(400, mmLink.CutoutPosition.Y2);

                mmLink.CutoutPosition.ParseString("11 15 576 611");
                Assert.IsFalse(mmLink.CutoutPosition.IsEmpty());
                Assert.AreEqual("11 15 576 611", mmLink.CutoutPosition.StringValue);

                using (var mmRec = new GDMMultimediaRecord(fContext.Tree)) {
                    Assert.IsNull(mmLink.GetUID());

                    mmLink.Value = mmRec;

                    Assert.IsNotNull(mmLink.GetUID());
                }

                mmLink.CutoutPosition.Clear();
                Assert.IsTrue(mmLink.CutoutPosition.IsEmpty());
                Assert.AreEqual("", mmLink.CutoutPosition.StringValue);
            }
        }

        private static void GEDCOMMultimediaLinkTest(GDMMultimediaRecord mediaRec, GDMIndividualRecord indiv)
        {
            GDMMultimediaLink mmLink = indiv.AddMultimedia(mediaRec);

            Assert.IsNotNull(mmLink.FileReferences);

            mmLink.Title = "Title1";
            Assert.AreEqual("Title1", mmLink.Title);

            string buf = TestUtils.GetTagStreamText(mmLink, 1);
            Assert.AreEqual("1 OBJE @O1@\r\n"+
                            "2 TITL Title1\r\n", buf);

            Assert.IsTrue(mmLink.IsPointer, "mmLink.IsPointer");

            mmLink.IsPrimary = true;
            Assert.IsTrue(mmLink.IsPrimary, "mmLink.IsPrimary");

            Assert.IsFalse(mmLink.IsEmpty(), "mmLink.IsEmpty()"); // its pointer

            mmLink.Clear();
        }

        private static void GEDCOMSubmissionRecordTest(GDMSubmissionRecord submRec, string submitterXRef)
        {
            submRec.FamilyFileName = "FamilyFileName";
            Assert.AreEqual("FamilyFileName", submRec.FamilyFileName);

            submRec.TempleCode = "TempleCode";
            Assert.AreEqual("TempleCode", submRec.TempleCode);

            submRec.GenerationsOfAncestors = 11;
            Assert.AreEqual(11, submRec.GenerationsOfAncestors);

            submRec.GenerationsOfDescendants = 77;
            Assert.AreEqual(77, submRec.GenerationsOfDescendants);

            submRec.OrdinanceProcessFlag = GEDCOMOrdinanceProcessFlag.opYes;
            Assert.AreEqual(GEDCOMOrdinanceProcessFlag.opYes, submRec.OrdinanceProcessFlag);
            
            submRec.Submitter.XRef = submitterXRef;
            GDMSubmitterRecord subr = submRec.Submitter.Value as GDMSubmitterRecord;
            Assert.IsNotNull(subr);
            
            
            Assert.IsFalse(submRec.IsEmpty());
            submRec.Clear();
            Assert.IsTrue(submRec.IsEmpty());
        }

        [Test]
        public void Test_GEDCOMSubmitterRecord()
        {
            using (GDMSubmitterRecord subrRec = new GDMSubmitterRecord(fContext.Tree)) {
                subrRec.Name.StringValue = "Test Submitter";
                Assert.AreEqual("Test Submitter", subrRec.Name.StringValue);

                subrRec.RegisteredReference = "regref";
                Assert.AreEqual("regref", subrRec.RegisteredReference);

                subrRec.Languages.Add(new GDMLanguage(subrRec, "", "Russian"));
                Assert.AreEqual("Russian", subrRec.Languages[0].StringValue);

                subrRec.SetLanguage(0, "nothing"); // return without exceptions

                subrRec.SetLanguage(1, "English");
                Assert.AreEqual("English", subrRec.Languages[1].StringValue);

                Assert.IsNotNull(subrRec.Address);


                Assert.IsFalse(subrRec.IsEmpty());
                subrRec.Clear();
                Assert.IsTrue(subrRec.IsEmpty());


                subrRec.ResetOwner(fContext.Tree);
                Assert.AreEqual(fContext.Tree, subrRec.GetTree());
            }
        }

        [Test]
        public void Test_GEDCOMCommunicationRecord()
        {
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;
            Assert.IsNotNull(iRec);

            using (GDMCommunicationRecord comRec = new GDMCommunicationRecord(fContext.Tree)) {
                comRec.CommName = "Test Communication";
                Assert.AreEqual("Test Communication", comRec.CommName);

                comRec.CommunicationType = GDMCommunicationType.ctFax;
                Assert.AreEqual(GDMCommunicationType.ctFax, comRec.CommunicationType);

                comRec.Date.Date = TestUtils.ParseDT("23.01.2013");
                Assert.AreEqual(TestUtils.ParseDT("23.01.2013"), comRec.Date.Date);

                comRec.SetCorresponder(GDMCommunicationDir.cdFrom, iRec);

                var corr = comRec.GetCorresponder();
                Assert.AreEqual(GDMCommunicationDir.cdFrom, corr.CommDir);
                Assert.AreEqual(iRec, corr.Corresponder);

                comRec.SetCorresponder(GDMCommunicationDir.cdTo, iRec);
                corr = comRec.GetCorresponder();
                Assert.AreEqual(GDMCommunicationDir.cdTo, corr.CommDir);
                Assert.AreEqual(iRec, corr.Corresponder);

                Assert.IsFalse(comRec.IsEmpty());
                comRec.Clear();
                Assert.IsTrue(comRec.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMLocationRecord()
        {
            using (GDMLocationRecord locRec = new GDMLocationRecord(null)) {
                locRec.LocationName = "Test Location";
                Assert.AreEqual("Test Location", locRec.LocationName);

                Assert.IsNotNull(locRec.Map);

                Assert.IsFalse(locRec.IsEmpty());
                locRec.Clear();
                Assert.IsTrue(locRec.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMTaskRecord()
        {
            GDMIndividualRecord iRec = fContext.Tree.XRefIndex_Find("I1") as GDMIndividualRecord;
            Assert.IsNotNull(iRec);

            GDMFamilyRecord famRec = fContext.Tree.XRefIndex_Find("F1") as GDMFamilyRecord;
            Assert.IsNotNull(famRec);

            GDMSourceRecord srcRec = fContext.Tree.XRefIndex_Find("S1") as GDMSourceRecord;
            Assert.IsNotNull(srcRec);

            using (GDMTaskRecord taskRec = new GDMTaskRecord(fContext.Tree))
            {
                Assert.IsNotNull(taskRec);

                taskRec.Priority = GDMResearchPriority.rpNormal;
                Assert.AreEqual(GDMResearchPriority.rpNormal, taskRec.Priority);

                taskRec.StartDate.Date = TestUtils.ParseDT("20.01.2013");
                Assert.AreEqual(TestUtils.ParseDT("20.01.2013"), taskRec.StartDate.Date);

                taskRec.StopDate.Date = TestUtils.ParseDT("21.01.2013");
                Assert.AreEqual(TestUtils.ParseDT("21.01.2013"), taskRec.StopDate.Date);

                taskRec.Goal = "Test Goal";
                Assert.AreEqual("Test Goal", taskRec.Goal);
                var goal = taskRec.GetTaskGoal();
                Assert.AreEqual(GDMGoalType.gtOther, goal.GoalType);
                Assert.AreEqual(null, goal.GoalRec);

                taskRec.Goal = iRec.XRef;
                goal = taskRec.GetTaskGoal();
                Assert.AreEqual(GDMGoalType.gtIndividual, goal.GoalType);
                Assert.AreEqual(iRec, goal.GoalRec);

                taskRec.Goal = famRec.XRef;
                goal = taskRec.GetTaskGoal();
                Assert.AreEqual(GDMGoalType.gtFamily, goal.GoalType);
                Assert.AreEqual(famRec, goal.GoalRec);

                taskRec.Goal = srcRec.XRef;
                goal = taskRec.GetTaskGoal();
                Assert.AreEqual(GDMGoalType.gtSource, goal.GoalType);
                Assert.AreEqual(srcRec, goal.GoalRec);

                Assert.IsFalse(taskRec.IsEmpty());
                taskRec.Clear();
                Assert.IsTrue(taskRec.IsEmpty());
            }
        }

        [Test]
        public void Test_GEDCOMNotes()
        {
            using (GDMNotes notes = GDMNotes.Create(null, "", "") as GDMNotes) {
                Assert.IsTrue(notes.IsEmpty());
                notes.Notes = new StringList("Test note");
                Assert.IsFalse(notes.IsEmpty());
                Assert.AreEqual("Test note", notes.Notes.Text);
            }
        }

        [Test]
        public void Test_GEDCOMNoteRecord()
        {
            using (GDMNoteRecord noteRec = new GDMNoteRecord(null)) {
                noteRec.AddNoteText("text");
                Assert.AreEqual("text", noteRec.Note.Text.Trim());

                Assert.Throws(typeof(ArgumentNullException), () => { noteRec.SetNoteText(null); });

                noteRec.SetNoteText("Test text");
                Assert.AreEqual("Test text", noteRec.Note.Text.Trim());

                using (GDMNoteRecord noteRec2 = new GDMNoteRecord(null)) {
                    noteRec2.SetNoteText("Test text");
                    Assert.AreEqual("Test text", noteRec2.Note.Text.Trim());

                    Assert.AreEqual(100.0f, noteRec.IsMatch(noteRec2, new MatchParams()));

                    Assert.IsFalse(noteRec2.IsEmpty());
                    noteRec2.Clear();
                    Assert.IsTrue(noteRec2.IsEmpty());

                    Assert.AreEqual(0.0f, noteRec.IsMatch(noteRec2, new MatchParams()));

                    Assert.AreEqual(0.0f, noteRec.IsMatch(null, new MatchParams()));
                }

                Assert.Throws(typeof(ArgumentException), () => { noteRec.MoveTo(null, false); });

                using (GDMNoteRecord noteRec3 = new GDMNoteRecord(null)) {
                    noteRec3.SetNoteText("Test text 3");
                    Assert.AreEqual("Test text 3", noteRec3.Note.Text.Trim());

                    noteRec.MoveTo(noteRec3, false);

                    Assert.AreEqual("Test text 3", noteRec3.Note.Text.Trim());
                }
            }
        }

        private static void GEDCOMNoteRecordTest(GDMNoteRecord noteRec, GDMIndividualRecord indiv)
        {
            noteRec.SetNotesArray(new string[] { "This", "notes", "test" });
            
            string ctx = GKUtils.MergeStrings(noteRec.Note);
            Assert.AreEqual("This notes test", ctx);

            noteRec.Note = new StringList("This\r\nnotes2\r\ntest2");
            Assert.AreEqual("This", noteRec.Note[0]);
            Assert.AreEqual("notes2", noteRec.Note[1]);
            Assert.AreEqual("test2", noteRec.Note[2]);
            
            Assert.Throws(typeof(ArgumentNullException), () => { GKUtils.MergeStrings(null); });
            
            ctx = GKUtils.MergeStrings(noteRec.Note);
            Assert.AreEqual("This notes2 test2", ctx);
            
            noteRec.Clear();
            noteRec.AddNoteText("Test text");
            Assert.AreEqual("Test text", noteRec.Note.Text.Trim());
            
            GEDCOMNotesTest(noteRec, indiv);

            Assert.IsFalse(noteRec.IsEmpty());
            noteRec.Clear();
            Assert.IsTrue(noteRec.IsEmpty());
        }

        private static void GEDCOMNotesTest(GDMNoteRecord noteRec, GDMIndividualRecord indiv)
        {
            GDMNotes notes = indiv.AddNote(noteRec);
            
            Assert.AreEqual(notes.Notes.Text, noteRec.Note.Text);
            
            Assert.IsTrue(notes.IsPointer, "notes.IsPointer");
            
            Assert.IsFalse(notes.IsEmpty()); // its pointer
            
            notes.Clear();
        }
        
        #endregion
    }
}
