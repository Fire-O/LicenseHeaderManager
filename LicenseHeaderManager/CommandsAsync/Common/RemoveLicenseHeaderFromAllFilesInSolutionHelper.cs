﻿#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager.CommandsAsync.Common
{
  public class RemoveLicenseHeaderFromAllFilesInSolutionHelper : ISolutionLevelCommand
  {
    private const string c_commandName = "Remove LicenseHeader from all Projects";

    private readonly IVsStatusbar _statusBar;
    private readonly LicenseHeaderReplacer _licenseReplacer;

    public RemoveLicenseHeaderFromAllFilesInSolutionHelper (IVsStatusbar statusBar, LicenseHeaderReplacer licenseReplacer)
    {
      _statusBar = statusBar;
      _licenseReplacer = licenseReplacer;
    }

    public string GetCommandName ()
    {
      return c_commandName;
    }

    public void Execute (Solution solution)
    {
      if (solution == null) return;

      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var projectsInSolution = allSolutionProjectsSearcher.GetAllProjects (solution);

      var progressCount = 1;
      var projectCount = projectsInSolution.Count;
      var removeAllLicenseHeadersCommand = new RemoveLicenseHeaderFromAllFilesInProjectHelper (_licenseReplacer);

      foreach (var project in projectsInSolution)
      {
        _statusBar.SetText (string.Format (Resources.UpdateSolution, progressCount, projectCount));
        removeAllLicenseHeadersCommand.Execute (project);
        progressCount++;
      }

      _statusBar.SetText (string.Empty);
    }
  }
}