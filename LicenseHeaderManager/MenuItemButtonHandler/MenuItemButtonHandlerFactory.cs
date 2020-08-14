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
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.MenuItemButtonHandler.Util;

namespace LicenseHeaderManager.MenuItemButtonHandler
{
  internal static class MenuItemButtonHandlerFactory
  {
    public static IMenuItemButtonHandler CreateHandler (MenuItemButtonLevel level, MenuItemButtonOperation mode, ILicenseHeaderExtension licenseHeadersPackage)
    {
      return level switch
      {
          MenuItemButtonLevel.Solution => CreateSolutionHandler (licenseHeadersPackage, mode),
          MenuItemButtonLevel.Folder => CreateFolderHandler (licenseHeadersPackage, mode),
          MenuItemButtonLevel.Project => CreateProjectHandler (licenseHeadersPackage, mode),
          _ => throw new ArgumentOutOfRangeException (nameof(level), level, null)
      };
    }

    private static SolutionMenuItemButtonHandler CreateSolutionHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerHelper helper = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInSolutionHelper (licenseHeadersPackage.LicenseHeaderReplacer),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderFromAllFilesInSolutionHelper (licenseHeadersPackage.LicenseHeaderReplacer),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new SolutionMenuItemButtonHandler (licenseHeadersPackage.Dte2, mode, helper);
    }

    private static FolderProjectMenuItemButtonHandler CreateFolderHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerHelper helper = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInFolderProjectHelper (licenseHeadersPackage),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderToAllFilesInFolderProjectHelper (licenseHeadersPackage),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new FolderProjectMenuItemButtonHandler (mode, MenuItemButtonLevel.Folder, helper);
    }

    private static FolderProjectMenuItemButtonHandler CreateProjectHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerHelper helper = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInFolderProjectHelper (licenseHeadersPackage),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderToAllFilesInFolderProjectHelper (licenseHeadersPackage),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new FolderProjectMenuItemButtonHandler (mode, MenuItemButtonLevel.Project, helper);
    }
  }
}