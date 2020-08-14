﻿/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("E0E8C0E8-0E8E-4251-B885-1ABF58837366")]
  public sealed class DefaultLicenseHeaderPage : VersionedDialogPage, IDefaultLicenseHeaderPage
  {
    public DefaultLicenseHeaderPage ()
    {
      ResetSettings();
    }

    [Browsable (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    protected override IWin32Window Window
    {
      get
      {
        var host = new WpfHost (new WpfDefaultLicenseHeader (this));
        return host;
      }
    }

    public string LicenseHeaderFileText { get; set; }

    public override void ResetSettings ()
    {
      InitializeFromResource();

      base.ResetSettings();
    }

    private void InitializeFromResource ()
    {
      LicenseHeaderFileText = GetDefaultLicenseHeader();
    }

    private string GetDefaultLicenseHeader ()
    {
      using (var resource = Assembly.GetExecutingAssembly()
          .GetManifestResourceStream (typeof (LicenseHeadersPackage), "Resources.default.licenseheader"))
      {
        using (var streamreader = new StreamReader (resource, Encoding.UTF8))
        {
          return streamreader.ReadToEnd();
        }
      }
    }

    #region version updates

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (1, 2, 1), InitializeFromResourceIfRequired);
      yield return new UpdateStep (new Version (3, 0, 1), MigrateStorageLocation_3_0_1);
    }

    private void InitializeFromResourceIfRequired ()
    {
      if (string.IsNullOrEmpty (LicenseHeaderFileText))
      {
        InitializeFromResource();
        MessageBoxHelper.Show (Resources.Update_DefaultLicenseHeader_1_2_1.ReplaceNewLines(), Resources.Update);
      }
    }

    private void MigrateStorageLocation_3_0_1 ()
    {
      if (!System.Version.TryParse (Version, out var version) || version < new Version (3, 0, 0))
      {
        LoadRegistryValuesBefore_3_0_0();
      }
      else
      {
        var migratedDefaultLicenseHeaderPage = new DefaultLicenseHeaderPage();
        LoadRegistryValuesBefore_3_0_0 (migratedDefaultLicenseHeaderPage);

        LicenseHeaderFileText = ThreeWaySelectionForMigration (
            LicenseHeaderFileText,
            migratedDefaultLicenseHeaderPage.LicenseHeaderFileText,
            GetDefaultLicenseHeader());
      }
    }

    #endregion
  }
}