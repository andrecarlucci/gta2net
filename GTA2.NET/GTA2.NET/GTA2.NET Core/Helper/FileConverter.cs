// GTA2.NET
// 
// File: FileConverter.cs
// Created: 02.04.2013
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Hiale.GTA2NET.Core.Helper.Threading;
using Microsoft.Win32;

namespace Hiale.GTA2NET.Core.Helper
{
    public class FileConverter
    {
        public event AsyncCompletedEventHandler ConversionCompleted;
        public event EventHandler<ProgressMessageChangedEventArgs> ConversionProgressChanged;

        private delegate void ConvertFilesDelegate(string sourcePath, string destinationPath, CancellableContext context, out bool cancelled);
        private CancellableContext _convertFilesContext;
        private bool _isBusy;
        private readonly object _syncCancel = new object();
        private readonly object _syncWaitHandle = new object();
        private readonly List<Style.Style> _runningStyles = new List<Style.Style>();
        private static readonly AutoResetEventValueExchange<bool> WaitHandle = new AutoResetEventValueExchange<bool>(false);


        public static string GetGTA2Directory()
        {
            var registryPath = Globals.GTA2RegistryKey;
            if (Environment.Is64BitProcess)
                registryPath = registryPath.Insert(9, "Wow6432Node\\");
            var registryKey = Registry.LocalMachine.OpenSubKey(registryPath);
            // If the RegistrySubKey doesn't exist -> (null)
            if (registryKey == null)
                return string.Empty;
            try
            {
                return (string)registryKey.GetValue("PATH");
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static bool CheckGTA2Directory(string path)
        {
            if (!File.Exists(path + "\\gta2.exe"))
                return false;

            if (Globals.StyleFiles.Any(styleFile => !File.Exists(path + "\\data\\" + styleFile)))
                return false;

            //if (!File.Exists(dir + "\\gta2.exe"))
            //    return false;

            return true;
        }


        private static bool CreateSubDirectories(string path)
        {
            try
            {
                Directory.CreateDirectory(path + Globals.GraphicsSubDir);
                Directory.CreateDirectory(path + Globals.MapsSubDir);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ConvertFiles(string sourcePath, string destinationPath)
        {
            bool cancelled;
            ConvertFiles(sourcePath, destinationPath, null, out cancelled);
        }

        private void ConvertFiles(string sourcePath, string destinationPath, CancellableContext asyncContext,
                                  out bool cancelled)
        {
            cancelled = false;

            CreateSubDirectories(destinationPath);

            var eArgs = new ProgressMessageChangedEventArgs(0, string.Empty, null);

            //conver style files
            foreach (var styleFile in Globals.StyleFiles)
            {
                if (asyncContext.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                var style = new Style.Style();
                style.ConvertStyleFileProgressChanged += StyleConvertStyleFileProgressChanged;
                style.ConvertStyleFileCompleted += StyleOnConvertStyleFileCompleted;
                style.ReadFromFileAsync(sourcePath + Globals.GTA2DataSubDir + styleFile);
                _runningStyles.Add(style);
            }

            //copy over maps
            var mapFiles = Directory.GetFiles(sourcePath + Globals.GTA2DataSubDir, "*.gmp");
            for (var i = 0; i < mapFiles.Length; i++)
            {
                if (asyncContext.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                File.Copy(mapFiles[i], Globals.MapsSubDir + "\\" + Path.GetFileName(mapFiles[i]));
                //eArgs = new ProgressMessageChangedEventArgs((i / mapFiles.Length * 100), "Test ToDo", null);
                //asyncContext.Async.Post(e => OnConversionProgressChanged((ProgressMessageChangedEventArgs) e), eArgs);
            }


            lock (_syncWaitHandle)
            {
                if (_runningStyles.Count > 0)
                {
                    WaitHandle.WaitOne();
                    cancelled = WaitHandle.Value;
                }
            }

            eArgs = new ProgressMessageChangedEventArgs(100, string.Empty, null);
            asyncContext.Async.Post(e => OnConversionProgressChanged((ProgressMessageChangedEventArgs) e), eArgs);

        }

        private void StyleConvertStyleFileProgressChanged(object sender, ProgressMessageChangedEventArgs e)
        {
            //OnConversionProgressChanged(e); //don't show for now
        }

        private void StyleOnConvertStyleFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            lock (_syncWaitHandle)
            {
                _runningStyles.Remove((Style.Style) sender);
                if (_runningStyles.Count > 0)
                    return;
                WaitHandle.Value = asyncCompletedEventArgs.Cancelled;
                WaitHandle.Set();
            }
        }

        public void ConvertFilesAsync(string sourcePath, string destinationPath)
        {
            var worker = new ConvertFilesDelegate(ConvertFiles);
            var completedCallback = new AsyncCallback(ConversionCompletedCallback);

            lock (_syncCancel)
            {
                if (_isBusy)
                    throw new InvalidOperationException("The control is currently busy.");

                var async = AsyncOperationManager.CreateOperation(null);
                var context = new CancellableContext(async);
                bool cancelled;

                worker.BeginInvoke(sourcePath, destinationPath, context, out cancelled, completedCallback, async);
                
                _isBusy = true;
                _convertFilesContext = context;
            }
        }

        

        private void ConversionCompletedCallback(IAsyncResult ar)
        {
            // get the original worker delegate and the AsyncOperation instance
            var worker = (ConvertFilesDelegate)((AsyncResult)ar).AsyncDelegate;
            var async = (AsyncOperation)ar.AsyncState;
            bool cancelled;

            // finish the asynchronous operation
            worker.EndInvoke(out cancelled, ar);

            // clear the running task flag
            lock (_syncCancel)
            {
                _isBusy = false;
                _convertFilesContext = null;
            }

            // raise the completed event
            var completedArgs = new AsyncCompletedEventArgs(null, cancelled, null);
            async.PostOperationCompleted(e => OnConversionCompleted((AsyncCompletedEventArgs)e), completedArgs);
        }


        protected virtual void OnConversionCompleted(AsyncCompletedEventArgs e)
        {
            if (ConversionCompleted != null)
                ConversionCompleted(this, e);
        }

        protected virtual void OnConversionProgressChanged(ProgressMessageChangedEventArgs e)
        {
            if (ConversionProgressChanged != null)
                ConversionProgressChanged(this, e);
        }

        public void CancelConversion()
        {
            lock (_syncCancel)
            {
                if (_convertFilesContext != null)
                    _convertFilesContext.Cancel();
                foreach (var runningStyle in _runningStyles)
                    runningStyle.CancelConvertStyle();
            }
        }

    }
}
