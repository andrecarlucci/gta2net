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
        private readonly object _syncStyleFinished = new object();
        private readonly List<Style.Style> _runningStyles = new List<Style.Style>();
        private static readonly AutoResetEventValueExchange<bool> WaitHandle = new AutoResetEventValueExchange<bool>(false);


        public static string GetGTA2Directory()
        {
            var registryPath = Globals.RegistryKey;
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


        public static bool CheckOriginalAssets(string path)
        {
            if (!File.Exists(path + "\\gta2.exe"))
                return false;
            if (Globals.StyleMapFiles.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.DataSubDir + Path.DirectorySeparatorChar + styleFile + Globals.StyleFileExtension)))
                return false;
            if (Globals.StyleMapFiles.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.DataSubDir + Path.DirectorySeparatorChar + styleFile + Globals.MapFileExtension)))
                return false;
            if (Globals.MapFilesMultiplayer.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.DataSubDir + Path.DirectorySeparatorChar + styleFile + Globals.MapFileExtension)))
                return false;
            if (Globals.MiscFiles.Any(miscFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.DataSubDir + Path.DirectorySeparatorChar + miscFile)))
                return false;
            //check more...
            return true;
        }

        public static bool CheckConvertedAssets(string path)
        {
            if (!IndexFile.Vertify())
                return false;
            if (Globals.StyleMapFiles.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.TilesSuffix + Globals.XmlFormat))) //Texture Atlas Tiles
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.SpritesSuffix + Globals.XmlFormat)) //Texture Atlas Sprites
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.SpritesSuffix + Globals.TextureImageFormat)) //Texture Sprites
                return false;
            if (Globals.StyleMapFiles.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.TilesSuffix + Globals.TextureImageFormat))) //Texture Tiles
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.PaletteSuffix + Globals.TextureImageFormat)) //Palettes
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.DeltasSuffix + Globals.XmlFormat)) //Texture Atlas Deltas
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.DeltasSuffix + Globals.TextureImageFormat)) //Delta Texture
                return false;
            if (Globals.StyleMapFiles.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.MapsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.MapFileExtension))) //Main Maps
                return false;
            if (Globals.MapFilesMultiplayer.Any(styleFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.MapsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.MapFileExtension))) //Multiplayer Maps
                return false;
            if (Globals.MiscFiles.Any(miscFile => !File.Exists(path + Path.DirectorySeparatorChar + Globals.MiscSubDir + Path.DirectorySeparatorChar + miscFile))) //Special Files
                return false;
            if (!File.Exists(path + Path.DirectorySeparatorChar + Globals.MiscSubDir + Path.DirectorySeparatorChar + Globals.CarStyleSuffix + Globals.XmlFormat)) //Car Data
                return false;
            return true;
        }

        private static bool CreateSubDirectories(string path)
        {
            try
            {
                Directory.CreateDirectory(path + Globals.GraphicsSubDir);
                Directory.CreateDirectory(path + Globals.MapsSubDir);
                Directory.CreateDirectory(path + Globals.MiscSubDir);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ConvertFiles(string sourcePath, string destinationPath)
        {
            var context = new CancellableContext(null);
            bool cancelled;
            ConvertFiles(sourcePath, destinationPath, context, out cancelled);
        }

        private void ConvertFiles(string sourcePath, string destinationPath, CancellableContext asyncContext, out bool cancelled)
        {
            cancelled = false;

            CreateSubDirectories(destinationPath);

            //convert style files
            for (int i = 0; i < Globals.StyleMapFiles.Length; i++)
            {
                var styleFile = Globals.StyleMapFiles[i];
                if (asyncContext.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                //copy over style file (because some data in it are still needed)
                var style = new Style.Style();
                var sourceFile = sourcePath + Globals.DataSubDir + Path.DirectorySeparatorChar + styleFile + Globals.StyleFileExtension;
                var targetFile = destinationPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.StyleFileExtension;
                if (!File.Exists(targetFile) || !Extensions.FilesAreEqual(sourceFile, targetFile))
                    File.Copy(sourceFile, targetFile, true);
                style.ConvertStyleFileCompleted += StyleOnConvertStyleFileCompleted;
                style.ReadFromFileAsync(targetFile, i == 0);
                _runningStyles.Add(style);
            }


            //copy over maps
            var mapFiles = Globals.StyleMapFiles.Select(mapFile => sourcePath + Globals.DataSubDir + Path.DirectorySeparatorChar + mapFile + Globals.MapFileExtension).ToList();
            mapFiles.AddRange(Globals.MapFilesMultiplayer.Select(multiplayerMapFile => sourcePath + Globals.DataSubDir + Path.DirectorySeparatorChar + multiplayerMapFile + Globals.MapFileExtension));
            for (var i = 0; i < mapFiles.Count; i++)
            {
                if (asyncContext.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                var targetFile = destinationPath + Globals.MapsSubDir + Path.DirectorySeparatorChar + Path.GetFileName(mapFiles[i]);
                if (File.Exists(targetFile))
                {
                    if (Extensions.FilesAreEqual(mapFiles[i], targetFile))
                        continue;
                }
                File.Copy(mapFiles[i], targetFile, true);
            }

            var miscFiles = Globals.MiscFiles.Select(miscFile => sourcePath + Globals.DataSubDir + Path.DirectorySeparatorChar + miscFile).ToList();
            for (var i = 0; i < miscFiles.Count; i++)
            {
                if (asyncContext.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                var targetFile = destinationPath + Globals.MiscSubDir + Path.DirectorySeparatorChar + Path.GetFileName(miscFiles[i]);
                if (File.Exists(targetFile))
                {
                    if (Extensions.FilesAreEqual(miscFiles[i], targetFile))
                        continue;
                }
                File.Copy(miscFiles[i], targetFile, true);
            }

            WaitHandle.WaitOne();
            IndexFile.Save();
            cancelled = WaitHandle.Value;
        }


        private void StyleOnConvertStyleFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            lock (_syncStyleFinished)
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
            sourcePath = Extensions.CheckDirectorySeparator(sourcePath);
            destinationPath = Extensions.CheckDirectorySeparator(destinationPath);
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
