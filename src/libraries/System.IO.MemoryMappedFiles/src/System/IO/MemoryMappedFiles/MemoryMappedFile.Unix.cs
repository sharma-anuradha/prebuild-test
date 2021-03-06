// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles
{
    public partial class MemoryMappedFile
    {
        // This will verify file access and return file size. fileSize will return -1 for special devices.
        private static void VerifyMemoryMappedFileAccess(MemoryMappedFileAccess access, long capacity, SafeFileHandle? fileHandle, long fileSize, out bool isRegularFile)
        {
            // if the length has already been fetched and it's more than 0 it's a regular file and there is no need for the FStat sys-call
            isRegularFile = fileHandle is not null && (fileSize > 0 || IsRegularFile(fileHandle));

            if (isRegularFile)
            {
                if (access == MemoryMappedFileAccess.Read && capacity > fileSize)
                {
                    throw new ArgumentException(SR.Argument_ReadAccessWithLargeCapacity);
                }

                // one can always create a small view if they do not want to map an entire file
                if (fileSize > capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(capacity), SR.ArgumentOutOfRange_CapacityGEFileSizeRequired);
                }

                if (access == MemoryMappedFileAccess.Write)
                {
                    throw new ArgumentException(SR.Argument_NewMMFWriteAccessNotAllowed, nameof(access));
                }
            }

            static bool IsRegularFile(SafeFileHandle fileHandle)
            {
                Interop.CheckIo(Interop.Sys.FStat(fileHandle, out Interop.Sys.FileStatus status));
                return (status.Mode & Interop.Sys.FileTypes.S_IFREG) != 0;
            }
        }

        /// <summary>
        /// Used by the 2 Create factory method groups.  A null fileHandle specifies that the
        /// memory mapped file should not be associated with an existing file on disk (i.e. start
        /// out empty).
        /// </summary>
        private static SafeMemoryMappedFileHandle CreateCore(
            SafeFileHandle? fileHandle, string? mapName,
            HandleInheritability inheritability, MemoryMappedFileAccess access,
            MemoryMappedFileOptions options, long capacity, long fileSize)
        {
            VerifyMemoryMappedFileAccess(access, capacity, fileHandle, fileSize, out bool isRegularFile);

            if (mapName != null)
            {
                // Named maps are not supported in our Unix implementation.  We could support named maps on Linux using
                // shared memory segments (shmget/shmat/shmdt/shmctl/etc.), but that doesn't work on OSX by default due
                // to very low default limits on OSX for the size of such objects; it also doesn't support behaviors
                // like copy-on-write or the ability to control handle inheritability, and reliably cleaning them up
                // relies on some non-conforming behaviors around shared memory IDs remaining valid even after they've
                // been marked for deletion (IPC_RMID).  We could also support named maps using the current implementation
                // by not unlinking after creating the backing store, but then the backing stores would remain around
                // and accessible even after process exit, with no good way to appropriately clean them up.
                // (File-backed maps may still be used for cross-process communication.)
                throw CreateNamedMapsNotSupportedException();
            }

            bool ownsFileStream = false;
            if (fileHandle != null)
            {
                if (isRegularFile && fileSize >= 0 && capacity > fileSize)
                {
                    // This map is backed by a file.  Make sure the file's size is increased to be
                    // at least as big as the requested capacity of the map for Write* access.
                    try
                    {
                        Interop.CheckIo(Interop.Sys.FTruncate(fileHandle, capacity));
                    }
                    catch (ArgumentException exc)
                    {
                        // If the capacity is too large, we'll get an ArgumentException from SetLength,
                        // but on Windows this same condition is represented by an IOException.
                        throw new IOException(exc.Message, exc);
                    }
                }
            }
            else
            {
                // This map is backed by memory-only.  With files, multiple views over the same map
                // will end up being able to share data through the same file-based backing store;
                // for anonymous maps, we need a similar backing store, or else multiple views would logically
                // each be their own map and wouldn't share any data.  To achieve this, we create a backing object
                // (either memory or on disk, depending on the system) and use its file descriptor as the file handle.
                // However, we only do this when the permission is more than read-only.  We can't change the size
                // of an object that has read-only permissions, but we also don't need to worry about sharing
                // views over a read-only, anonymous, memory-backed map, because the data will never change, so all views
                // will always see zero and can't change that.  In that case, we just use the built-in anonymous support of
                // the map by leaving fileStream as null.
                Interop.Sys.MemoryMappedProtections protections = MemoryMappedView.GetProtections(access, forVerification: false);
                if ((protections & Interop.Sys.MemoryMappedProtections.PROT_WRITE) != 0 && capacity > 0)
                {
                    ownsFileStream = true;
                    fileHandle = CreateSharedBackingObject(protections, capacity, inheritability);
                }
            }

            return new SafeMemoryMappedFileHandle(fileHandle, ownsFileStream, inheritability, access, options, capacity);
        }

        /// <summary>
        /// Used by the CreateOrOpen factory method groups.
        /// </summary>
        private static SafeMemoryMappedFileHandle CreateOrOpenCore(
            string mapName,
            HandleInheritability inheritability, MemoryMappedFileAccess access,
            MemoryMappedFileOptions options, long capacity)
        {
            // Since we don't support mapName != null, CreateOrOpenCore can't
            // be used to Open an existing map, and thus is identical to CreateCore.
            return CreateCore(null, mapName, inheritability, access, options, capacity, -1);
        }

        /// <summary>
        /// Used by the OpenExisting factory method group and by CreateOrOpen if access is write.
        /// We'll throw an ArgumentException if the file mapping object didn't exist and the
        /// caller used CreateOrOpen since Create isn't valid with Write access
        /// </summary>
        private static SafeMemoryMappedFileHandle OpenCore(
            string mapName, HandleInheritability inheritability, MemoryMappedFileAccess access, bool createOrOpen)
        {
            throw CreateNamedMapsNotSupportedException();
        }

        /// <summary>
        /// Used by the OpenExisting factory method group and by CreateOrOpen if access is write.
        /// We'll throw an ArgumentException if the file mapping object didn't exist and the
        /// caller used CreateOrOpen since Create isn't valid with Write access
        /// </summary>
        private static SafeMemoryMappedFileHandle OpenCore(
            string mapName, HandleInheritability inheritability, MemoryMappedFileRights rights, bool createOrOpen)
        {
            throw CreateNamedMapsNotSupportedException();
        }

        /// <summary>Gets an exception indicating that named maps are not supported on this platform.</summary>
        private static Exception CreateNamedMapsNotSupportedException()
        {
            return new PlatformNotSupportedException(SR.PlatformNotSupported_NamedMaps);
        }

        private static FileAccess TranslateProtectionsToFileAccess(Interop.Sys.MemoryMappedProtections protections)
        {
            return
                (protections & (Interop.Sys.MemoryMappedProtections.PROT_READ | Interop.Sys.MemoryMappedProtections.PROT_WRITE)) != 0 ? FileAccess.ReadWrite :
                (protections & (Interop.Sys.MemoryMappedProtections.PROT_WRITE)) != 0 ? FileAccess.Write :
                FileAccess.Read;
        }

        private static SafeFileHandle CreateSharedBackingObject(Interop.Sys.MemoryMappedProtections protections, long capacity, HandleInheritability inheritability)
        {
            return CreateSharedBackingObjectUsingMemory(protections, capacity, inheritability)
                ?? CreateSharedBackingObjectUsingFile(protections, capacity, inheritability);
        }

        private static SafeFileHandle? CreateSharedBackingObjectUsingMemory(
           Interop.Sys.MemoryMappedProtections protections, long capacity, HandleInheritability inheritability)
        {
            // Determine the flags to use when creating the shared memory object
            Interop.Sys.OpenFlags flags = (protections & Interop.Sys.MemoryMappedProtections.PROT_WRITE) != 0 ?
                Interop.Sys.OpenFlags.O_RDWR :
                Interop.Sys.OpenFlags.O_RDONLY;
            flags |= Interop.Sys.OpenFlags.O_CREAT | Interop.Sys.OpenFlags.O_EXCL; // CreateNew

            // Determine the permissions with which to create the file
            Interop.Sys.Permissions perms = default(Interop.Sys.Permissions);
            if ((protections & Interop.Sys.MemoryMappedProtections.PROT_READ) != 0)
                perms |= Interop.Sys.Permissions.S_IRUSR;
            if ((protections & Interop.Sys.MemoryMappedProtections.PROT_WRITE) != 0)
                perms |= Interop.Sys.Permissions.S_IWUSR;
            if ((protections & Interop.Sys.MemoryMappedProtections.PROT_EXEC) != 0)
                perms |= Interop.Sys.Permissions.S_IXUSR;

            string mapName;
            SafeFileHandle fd;

            do
            {
                mapName = GenerateMapName();
                fd = Interop.Sys.ShmOpen(mapName, flags, (int)perms); // Create the shared memory object.

                if (fd.IsInvalid)
                {
                    Interop.ErrorInfo errorInfo = Interop.Sys.GetLastErrorInfo();
                    fd.Dispose();

                    if (errorInfo.Error == Interop.Error.ENOTSUP)
                    {
                        // If ShmOpen is not supported, fall back to file backing object.
                        // Note that the System.Native shim will force this failure on platforms where
                        // the result of native shm_open does not work well with our subsequent call to mmap.
                        return null;
                    }
                    else if (errorInfo.Error != Interop.Error.EEXIST) // map with same name already existed
                    {
                        throw Interop.GetExceptionForIoErrno(errorInfo);
                    }
                }
            } while (fd.IsInvalid);

            try
            {
                // Unlink the shared memory object immediately so that it'll go away once all handles
                // to it are closed (as with opened then unlinked files, it'll remain usable via
                // the open handles even though it's unlinked and can't be opened anew via its name).
                Interop.CheckIo(Interop.Sys.ShmUnlink(mapName));

                // Give it the right capacity.  We do this directly with ftruncate rather
                // than via FileStream.SetLength after the FileStream is created because, on some systems,
                // lseek fails on shared memory objects, causing the FileStream to think it's unseekable,
                // causing it to preemptively throw from SetLength.
                Interop.CheckIo(Interop.Sys.FTruncate(fd, capacity));

                // shm_open sets CLOEXEC implicitly.  If the inheritability requested is Inheritable, remove CLOEXEC.
                if (inheritability == HandleInheritability.Inheritable &&
                    Interop.Sys.Fcntl.SetFD(fd, 0) == -1)
                {
                    throw Interop.GetExceptionForIoErrno(Interop.Sys.GetLastErrorInfo());
                }

                return fd;
            }
            catch
            {
                fd.Dispose();
                throw;
            }

            static string GenerateMapName()
            {
                const int MaxSharedMemoryObjectNameLength = 32; // SHM_NAME_MAX on OSX ARM64, on other systems it's equal PATH_MAX (250)
                // The POSIX shared memory object name must begin with '/'.  After that we just want something short (32) and unique.
                return string.Create(MaxSharedMemoryObjectNameLength, 0, (span, state) =>
                {
                    Guid.NewGuid().TryFormat(span, out int charsWritten, "N");
                    Debug.Assert(charsWritten == MaxSharedMemoryObjectNameLength);
                    "/dotnet_".CopyTo(span);
                });
            }
        }

        private static SafeFileHandle CreateSharedBackingObjectUsingFile(Interop.Sys.MemoryMappedProtections protections, long capacity, HandleInheritability inheritability)
        {
            // We create a temporary backing file in TMPDIR.  We don't bother putting it into subdirectories as the file exists
            // extremely briefly: it's opened/created and then immediately unlinked.
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            FileShare share = inheritability == HandleInheritability.None ?
                FileShare.ReadWrite :
                FileShare.ReadWrite | FileShare.Inheritable;

            // Create the backing file, then immediately unlink it so that it'll be cleaned up when no longer in use.
            // Then enlarge it to the requested capacity.
            SafeFileHandle fileHandle = File.OpenHandle(path, FileMode.CreateNew, TranslateProtectionsToFileAccess(protections), share);
            try
            {
                Interop.CheckIo(Interop.Sys.Unlink(path));
                Interop.CheckIo(Interop.Sys.FTruncate(fileHandle, capacity), path);
            }
            catch
            {
                fileHandle.Dispose();
                throw;
            }
            return fileHandle;
        }
    }
}
