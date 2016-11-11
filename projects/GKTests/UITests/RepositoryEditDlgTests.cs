﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2016 by Serg V. Zhdanovskih (aka Alchemist, aka Norseman).
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

#if !__MonoCS__

using System;
using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKTests.Mocks;
using GKUI.Dialogs;
using NUnit.Extensions.Forms;
using NUnit.Framework;

namespace GKTests.UITests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class RepositoryEditDlgTests : NUnitFormTest
    {
        public RepositoryEditDlgTests()
        {
        }

        private IBaseContext fContext;
        private GEDCOMRepositoryRecord fRepositoryRecord;
        private IBaseWindow fBase;

        private RepositoryEditDlg _frm;

        public override void Setup()
        {
            base.Setup();

            fBase = new BaseWindowMock();
            fContext = fBase.Context;
            fRepositoryRecord = new GEDCOMRepositoryRecord(fContext.Tree, fContext.Tree, "", "");

            //ExpectModal("RepositoryEditDlg", "DlgHandler");
            _frm = new RepositoryEditDlg(fBase);
            _frm.Repository = fRepositoryRecord;
            //_frm.ShowDialog();
            //_frm.Show();
        }

        [Test]
        public void Test_Misc()
        {
            Assert.AreEqual(fBase, _frm.Base);
            Assert.AreEqual(fRepositoryRecord, _frm.Repository);
        }

        public void DlgHandler()
        {
            //var btnCancel = new ButtonTester("btnCancel", "NoteEditDlg");
            //btnCancel.Click();
        }
    }
}

#endif