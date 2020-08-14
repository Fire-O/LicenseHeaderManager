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
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using LicenseHeaderManager.SolutionUpdateViewModels;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager
{
  /// <summary>
  ///   Interaction logic for TestDialog.xaml
  /// </summary>
  public partial class SolutionUpdateDialog : DialogWindow
  {
    public SolutionUpdateDialog (SolutionUpdateViewModel solutionUpdateViewModel)
    {
      InitializeComponent();
      DataContext = solutionUpdateViewModel;
      ((SolutionUpdateViewModel) DataContext).PropertyChanged += OnPropertyChanged1;
    }

    private void OnPropertyChanged1 (object sender, PropertyChangedEventArgs e)
    {
      UpdateTextBlockAsync (e).FireAndForget();
    }

    private async Task UpdateTextBlockAsync(PropertyChangedEventArgs args)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var context = (SolutionUpdateViewModel) DataContext;
      switch (args.PropertyName)
      {
        case nameof(context.ProcessedProjectCount):
          BindingOperations.GetMultiBindingExpression (ProjectsDoneTextBlock, TextBlock.TextProperty)?.UpdateTarget();
          BindingOperations.GetBindingExpression(ProjectsDoneProgressBar, ProgressBar.ValueProperty)?.UpdateTarget();
          break;
        case nameof(context.CurrentProject):
          BindingOperations.GetBindingExpression(CurrentProjectTextBlock, TextBlock.TextProperty)?.UpdateTarget();
          break;
        case nameof(context.ProjectCount):
          BindingOperations.GetBindingExpression(ProjectsDoneProgressBar, ProgressBar.MaximumProperty)?.UpdateTarget();
          break;
      }
    }
  }
}