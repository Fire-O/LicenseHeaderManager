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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Properties;

namespace Core
{
  public class LicenseHeaderReplacer
  {
    /// <summary>
    ///   Used to keep track of the user selection when he is trying to insert invalid headers into all files,
    ///   so that the warning is only displayed once per file extension.
    /// </summary>
    private readonly IDictionary<string, bool> _extensionsWithInvalidHeaders = new Dictionary<string, bool>();

    private readonly IEnumerable<string> _keywords;
    private readonly IEnumerable<Language> _languages;

    public LicenseHeaderReplacer (IEnumerable<Language> languages, IEnumerable<string> keywords)
    {
      _languages = languages;
      _keywords = keywords;
    }

    public void ResetExtensionsWithInvalidHeaders ()
    {
      _extensionsWithInvalidHeaders.Clear();
    }

    /// <summary>
    ///   Removes or replaces the header of a given project item.
    /// </summary>
    /// <param name="licenseHeaderInput">The licenseHeaderInput item.</param>
    /// <param name="calledByUser">
    ///   Specifies whether the command was called by the user (as opposed to automatically by a
    ///   linked command or by ItemAdded)
    /// </param>
    /// <param name="nonCommentTextInquiry">
    ///   Determines whether license headers should be inserted even if they contain non-comment text for the respective
    ///   language.
    ///   Is supplied with a <see cref="string" /> argument that represents a specific message describing the issue. If null,
    ///   license headers are inserted.
    /// </param>
    /// <param name="commentDefinitionNotFoundAction">
    ///   Is executed if there there is no license header definition configured for the language of a specific file.
    ///   If null, no action is executed in this case.
    /// </param>
    public Task<string> RemoveOrReplaceHeader (
        LicenseHeaderInput licenseHeaderInput,
        bool calledByUser,
        Func<string, bool> nonCommentTextInquiry = null,
        Action<string> commentDefinitionNotFoundAction = null)
    {
      var message = "";
      try
      {
        var result = TryCreateDocument (licenseHeaderInput.DocumentPath, out var document, licenseHeaderInput.AdditionalProperties, licenseHeaderInput.Headers);

        switch (result)
        {
          case CreateDocumentResult.DocumentCreated:
            if (!document.ValidateHeader())
            {
              message = string.Format (Resources.Warning_InvalidLicenseHeader, Path.GetExtension (licenseHeaderInput.DocumentPath)).Replace (@"\n", "\n");
              var addDespiteNonCommentText = nonCommentTextInquiry?.Invoke (message) ?? true;
              if (addDespiteNonCommentText)
              {
                message = "";
              }
              else
              {
                message = $"Execution of {nameof(RemoveOrReplaceHeader)} was cancelled by caller";
                break;
              }
            }

            try
            {
              document.ReplaceHeaderIfNecessary();
            }
            catch (ParseException)
            {
              message = string.Format (Resources.Error_InvalidLicenseHeader, licenseHeaderInput.DocumentPath).Replace (@"\n", "\n");
            }

            break;
          case CreateDocumentResult.LanguageNotFound:
            if (calledByUser)
            {
              message = string.Format (Resources.Error_LanguageNotFound, Path.GetExtension (licenseHeaderInput.DocumentPath)).Replace (@"\n", "\n");

              if (commentDefinitionNotFoundAction != null)
              {
                commentDefinitionNotFoundAction (message);
                message = "";
              }
            }

            break;
          case CreateDocumentResult.EmptyHeader:
            break;
          case CreateDocumentResult.NoHeaderFound:
            if (calledByUser)
              message = string.Format (Resources.Error_NoHeaderFound).Replace (@"\n", "\n");

            break;
        }
      }
      catch (ArgumentException ex)
      {
        message = $"{ex.Message} {licenseHeaderInput.DocumentPath}";
      }

      return Task.FromResult (message);
    }

    public Task<Dictionary<string, string>> RemoveOrReplaceHeader (IEnumerable<LicenseHeaderInput> licenseHeaders, Func<string, bool> nonCommentTextInquiry = null)
    {
      var errors = new Dictionary<string, string>();

      foreach (var header in licenseHeaders)
      {
        if (TryCreateDocument (header.DocumentPath, out var document, header.AdditionalProperties, header.Headers) != CreateDocumentResult.DocumentCreated)
          continue;

        string message;
        var replace = true;

        if (!document.ValidateHeader())
        {
          var extension = Path.GetExtension (header.DocumentPath);
          if (!_extensionsWithInvalidHeaders.TryGetValue (extension, out replace))
          {
            message = string.Format (Resources.Warning_InvalidLicenseHeader, extension).Replace (@"\n", "\n");
            replace = nonCommentTextInquiry?.Invoke (message) ?? true;
            _extensionsWithInvalidHeaders[extension] = replace;
          }
        }

        if (!replace)
          continue;

        try
        {
          document.ReplaceHeaderIfNecessary();
        }
        catch (ParseException)
        {
          message = string.Format (Resources.Error_InvalidLicenseHeader, header.DocumentPath).Replace (@"\n", "\n");
          errors.Add (header.DocumentPath, message);
        }
      }

      return Task.FromResult (errors);
    }

    public static bool IsLicenseHeader (string documentPath)
    {
      return Path.GetExtension (documentPath) == LicenseHeader.Extension;
    }

    /// <summary>
    ///   Tries to open a given project item as a Document which can be used to add or remove headers.
    /// </summary>
    /// <param name="documentPath">The project item.</param>
    /// <param name="document">The document which was created or null if an error occured (see return value).</param>
    /// <param name="additionalProperties"></param>
    /// <param name="headers">
    ///   A dictionary of headers using the file extension as key and the header as value or null if
    ///   headers should only be removed.
    /// </param>
    /// <returns>A value indicating the result of the operation. Document will be null unless DocumentCreated is returned.</returns>
    public CreateDocumentResult TryCreateDocument (
        string documentPath,
        out Document document,
        IEnumerable<DocumentHeaderProperty> additionalProperties = null,
        IDictionary<string, string[]> headers = null)
    {
      document = null;

      if (IsLicenseHeader (documentPath))
        return CreateDocumentResult.LicenseHeaderDocument;

      var language = _languages
          .FirstOrDefault (x => x.Extensions.Any (y => documentPath.EndsWith (y, StringComparison.OrdinalIgnoreCase)));

      if (language == null)
        return CreateDocumentResult.LanguageNotFound;

      string[] header = null;
      if (headers != null)
      {
        var extension = headers.Keys
            .OrderByDescending (x => x.Length)
            .FirstOrDefault (x => documentPath.EndsWith (x, StringComparison.OrdinalIgnoreCase));

        if (extension == null)
          return CreateDocumentResult.NoHeaderFound;

        header = headers[extension];

        if (header.All (string.IsNullOrEmpty))
          return CreateDocumentResult.EmptyHeader;
      }

      document = new Document (documentPath, language, header, additionalProperties, _keywords);

      return CreateDocumentResult.DocumentCreated;
    }
  }
}