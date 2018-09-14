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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using GKCore.Controllers;
using GKCore.Interfaces;
using GKCore.UIContracts;

namespace GKUI.Forms
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonForm : Form, IView
    {
        private System.ComponentModel.IContainer fComponents;
        private ToolTip fToolTip;

        protected readonly ControlsManager fControlsManager;

        #region View Interface

        public string Caption
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #endregion

        public CommonForm()
        {
            fComponents = new System.ComponentModel.Container();
            fToolTip = new ToolTip(this.fComponents);

            fControlsManager = new ControlsManager();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (fComponents != null) fComponents.Dispose();
            }
            base.Dispose(disposing);
        }

        public void SetToolTip(Component component, string toolTip)
        {
            if (component != null && !string.IsNullOrEmpty(toolTip)) {
                if (component is Control) {
                    fToolTip.SetToolTip((Control)component, toolTip);
                }
                else
                if (component is ToolStripItem) {
                    ((ToolStripItem)component).ToolTipText = toolTip;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CommonDialog : CommonForm, ICommonDialog
    {
        public virtual bool ShowModalX(object owner)
        {
            return (ShowDialog() == DialogResult.OK);
        }
    }
}
