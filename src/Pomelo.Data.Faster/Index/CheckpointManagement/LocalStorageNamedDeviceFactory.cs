// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pomelo.Data.Faster
{
    /// <summary>
    /// Local storage device factory
    /// </summary>
    public class LocalStorageNamedDeviceFactory : INamedDeviceFactory
    {
        private string baseName;
        private readonly bool deleteOnClose;
        private readonly int? throttleLimit;
        private readonly bool preallocateFile;

        /// <summary>
        /// Create instance of factory
        /// </summary>
        /// <param name="preallocateFile">Whether files should be preallocated</param>
        /// <param name="deleteOnClose">Whether file should be deleted on close</param>
        /// <param name="throttleLimit">Throttle limit (max number of pending I/Os) for this device instance</param>
        public LocalStorageNamedDeviceFactory(bool preallocateFile = false, bool deleteOnClose = false, int? throttleLimit = null)
        {
            this.preallocateFile = preallocateFile;
            this.deleteOnClose = deleteOnClose;
            this.throttleLimit = throttleLimit;
        }

        /// <inheritdoc />
        public void Initialize(string baseName)
        {
            this.baseName = baseName;
        }

        /// <inheritdoc />
        public IDevice Get(FileDescriptor fileInfo)
        {
            var device = Devices.CreateLogDevice(Path.Combine(baseName, fileInfo.directoryName, fileInfo.fileName), preallocateFile: preallocateFile, deleteOnClose: deleteOnClose);
            if (this.throttleLimit.HasValue)
            {
                device.ThrottleLimit = this.throttleLimit.Value;
            }
            return device;
        }


        /// <inheritdoc />
        public IEnumerable<FileDescriptor> ListContents(string path)
        {
            var pathInfo = new DirectoryInfo(Path.Combine(baseName, path));

            if (pathInfo.Exists)
            {
                foreach (var folder in pathInfo.GetDirectories().OrderByDescending(f => f.LastWriteTime))
                {
                    yield return new FileDescriptor(folder.Name, "");
                }

                foreach (var file in pathInfo.GetFiles().OrderByDescending(f => f.LastWriteTime))
                {
                    yield return new FileDescriptor("", file.Name);
                }
            }
        }

        /// <inheritdoc />
        public void Delete(FileDescriptor fileInfo)
        {
            if (fileInfo.fileName != null)
            {
                var file = new FileInfo(Path.Combine(baseName, fileInfo.directoryName, fileInfo.fileName + ".0"));
                if (file.Exists)
                    file.Delete();
            }
            else
            {
                try
                {
                    var dir = new DirectoryInfo(Path.Combine(baseName, fileInfo.directoryName));
                    dir.Delete(true);
                }
                catch { }
            }
        }
    }
}