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

// Guids.cs
// MUST match guids.h

using System;

namespace LicenseHeaderManager
{
  internal static class GuidList
  {
    public const string guidLicenseHeadersPkgString = "4c570677-8476-4d33-bd0c-da36c89287c8";
    public const string guidLicenseHeadersCmdSetString = "88ce72ac-651d-4441-be9c-dc74c2dc44b6";
    public const string guidItemTypePhysicalFile = "6bb5f8ee-4483-11d3-8bcf-00c04f8ec28c";
    public const string guidVisualStudioOutputWindow = "0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789";

    public static readonly Guid guidLicenseHeadersCmdSet = new Guid (guidLicenseHeadersCmdSetString);
  }
}