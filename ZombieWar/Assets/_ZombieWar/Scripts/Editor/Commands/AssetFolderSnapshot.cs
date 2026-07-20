using System;
using System.Collections.Generic;
using System.IO;

namespace ZombieWar.Editor
{
    internal sealed class AssetFolderSnapshot
    {
        #region State

        private readonly string _absoluteRoot;
        private readonly string _rootMetaPath;
        private readonly bool _rootExisted;
        private readonly bool _rootMetaExisted;
        private readonly byte[] _rootMetaContents;
        private readonly Dictionary<string, byte[]> _files;
        private readonly HashSet<string> _directories;

        #endregion

        #region Lifecycle

        private AssetFolderSnapshot(string assetRelativeRoot)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Unity project root could not be resolved.");
            }

            projectRoot = Path.GetFullPath(projectRoot);
            string assetsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Assets"));
            _absoluteRoot = Path.GetFullPath(Path.Combine(projectRoot, assetRelativeRoot));
            if (!_absoluteRoot.StartsWith(assetsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Snapshot path must stay inside Assets: {assetRelativeRoot}");
            }

            _rootMetaPath = _absoluteRoot + ".meta";
            _rootExisted = Directory.Exists(_absoluteRoot);
            _rootMetaExisted = File.Exists(_rootMetaPath);
            _rootMetaContents = _rootMetaExisted ? File.ReadAllBytes(_rootMetaPath) : null;
            _files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            _directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CaptureContents();
        }

        #endregion

        #region API

        public static AssetFolderSnapshot Capture(string assetRelativeRoot)
        {
            return new AssetFolderSnapshot(assetRelativeRoot);
        }

        public void Restore()
        {
            RemoveGeneratedFiles();
            RestoreOriginalFiles();
            RemoveGeneratedDirectories();
            RestoreRootState();
        }

        #endregion

        #region Internal

        private void CaptureContents()
        {
            if (!_rootExisted)
            {
                return;
            }

            string[] directories = Directory.GetDirectories(_absoluteRoot, "*", SearchOption.AllDirectories);
            for (int i = 0; i < directories.Length; i++)
            {
                _directories.Add(GetRelativePath(directories[i]));
            }

            string[] files = Directory.GetFiles(_absoluteRoot, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                _files[GetRelativePath(files[i])] = File.ReadAllBytes(files[i]);
            }
        }

        private void RemoveGeneratedFiles()
        {
            if (!Directory.Exists(_absoluteRoot))
            {
                return;
            }

            string[] currentFiles = Directory.GetFiles(_absoluteRoot, "*", SearchOption.AllDirectories);
            for (int i = 0; i < currentFiles.Length; i++)
            {
                if (!_files.ContainsKey(GetRelativePath(currentFiles[i])))
                {
                    File.Delete(currentFiles[i]);
                }
            }
        }

        private void RestoreOriginalFiles()
        {
            foreach (KeyValuePair<string, byte[]> file in _files)
            {
                string absolutePath = Path.Combine(_absoluteRoot, file.Key);
                string directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(absolutePath, file.Value);
            }
        }

        private void RemoveGeneratedDirectories()
        {
            if (!Directory.Exists(_absoluteRoot))
            {
                return;
            }

            string[] currentDirectories = Directory.GetDirectories(_absoluteRoot, "*", SearchOption.AllDirectories);
            Array.Sort(currentDirectories, CompareDeepestFirst);
            for (int i = 0; i < currentDirectories.Length; i++)
            {
                string directory = currentDirectories[i];
                if (!_directories.Contains(GetRelativePath(directory))
                    && Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
        }

        private void RestoreRootState()
        {
            if (!_rootExisted && Directory.Exists(_absoluteRoot)
                && Directory.GetFileSystemEntries(_absoluteRoot).Length == 0)
            {
                Directory.Delete(_absoluteRoot);
            }

            if (_rootMetaExisted)
            {
                File.WriteAllBytes(_rootMetaPath, _rootMetaContents);
            }
            else if (File.Exists(_rootMetaPath))
            {
                File.Delete(_rootMetaPath);
            }
        }

        private string GetRelativePath(string absolutePath)
        {
            return absolutePath.Substring(_absoluteRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static int CompareDeepestFirst(string left, string right)
        {
            return right.Length.CompareTo(left.Length);
        }

        #endregion
    }
}
