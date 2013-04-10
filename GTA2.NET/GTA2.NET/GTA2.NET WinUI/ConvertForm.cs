// GTA2.NET
// 
// File: ConvertForm.cs
// Created: 05.04.2013
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Helper.Threading;

namespace Hiale.GTA2NET.WinUI
{
    public partial class ConvertForm : Form
    {
        private const string GameDownload = "http://www.rockstargames.com/classics/";
        private const string ProjectWebsite = "http://code.google.com/p/gta2net/";

        private readonly FileConverter _converter;
        private readonly ProgressForm _progressForm;

        public ConvertForm()
        {
            InitializeComponent();
            var assembly = Assembly.GetExecutingAssembly();
            var fileInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            lblCopyright.Text = fileInfo.LegalCopyright + ", Version " + assembly.GetName().Version;

            if (!CheckAccess())
            {
                var msgBoxResult = MessageBox.Show(
                    "There is a problem writing in the current directory. It is necessary to have rights to write files the first time you start GTA2.NET." +
                    Environment.NewLine +
                    "Do you want to restart this application with elevated privileges?", Text, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (msgBoxResult == DialogResult.Yes)
                {
                    RestartAsAdmin();
                    Close();
                    return;
                }
                if (msgBoxResult == DialogResult.No)
                {
                    Close();
                    return;
                }
            }

            _converter = new FileConverter();
            _converter.ConversionProgressChanged += ConverterConversionProgressChanged;
            _converter.ConversionCompleted += ConverterConversionCompleted;

            _progressForm = new ProgressForm();
            _progressForm.Canceled += ProgressFormCanceled;

            var path = FileConverter.GetGTA2Directory();
            if (FileConverter.CheckOriginalAssets(path))
                txtPath.Text = path;
        }

        private static bool CheckAccess()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var currentDir = Path.GetDirectoryName(assembly.Location);
            currentDir = Extensions.CheckDirectorySeparator(currentDir);
            var testFile = currentDir + "test.bin";
            try
            {
                File.Create(testFile).Dispose();
                File.Delete(testFile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void RestartAsAdmin()
        {
            var processStartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };
            try
            {
                Process.Start(processStartInfo);
            }
            catch
            {
                // The user refused the elevation.
                // Do nothing and return directly ...
                return;
            }
            Close();
        }

        private void ProgressFormCanceled(object sender, EventArgs e)
        {
            _converter.CancelConversion();
        }

        private void BtnOKClick(object sender, EventArgs e)
        {
            var sourcePath = txtPath.Text;
            if (FileConverter.CheckOriginalAssets(sourcePath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var currentDir = Path.GetDirectoryName(assembly.Location);

                btnOK.Enabled = false;

                _progressForm.ProgressValue = 0;
                _converter.ConvertFilesAsync(sourcePath, currentDir);
                _progressForm.ShowDialog();
            }
            else
            {
                MessageBox.Show(txtPath.Text + " seems to be a wrong path.", Text, MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                txtPath.Focus();
                return;
            }
        }

        private void ConverterConversionProgressChanged(object sender, ProgressMessageChangedEventArgs e)
        {
            if (_progressForm != null)
                _progressForm.ProgressValue = e.ProgressPercentage;
        }

        private void ConverterConversionCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            btnOK.Enabled = true;
            if (e.Cancelled)
            {
                _progressForm.Hide();
            }
            else
            {
                _progressForm.Close();
                MessageBox.Show(
                    "The files have been converted." + Environment.NewLine +
                    "Please note: This game is an an early development stage and the file format will change in the future. Then you will need to convert the files again.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                DialogResult = DialogResult.OK;
            }
        }

        #region WinForm Stuff

        private void BtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ConvertFormFormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (Directory.Exists(txtPath.Text))
                dialog.SelectedPath = txtPath.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                txtPath.Text = dialog.SelectedPath;
        }

        private void LblIntroLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(GameDownload);
        }

        private void LblCopyrightLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(ProjectWebsite);
        }

        private void TxtPathTextChanged(object sender, EventArgs e)
        {
            if (FileConverter.CheckOriginalAssets(txtPath.Text))
                txtPath.BackColor = Color.LightGreen;
            else
            {
                txtPath.BackColor = SystemColors.Window;
            }

        }

        #endregion
    }
}
