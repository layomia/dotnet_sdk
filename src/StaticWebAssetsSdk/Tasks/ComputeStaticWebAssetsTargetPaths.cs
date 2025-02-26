// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.AspNetCore.StaticWebAssets.Tasks
{
    public class ComputeStaticWebAssetsTargetPaths : Task
    {
        [Required]
        public ITaskItem[] Assets { get; set; }

        public string PathPrefix { get; set; }

        public bool UseAlternatePathDirectorySeparator { get; set; }

        public bool AdjustPathsForPack { get; set; } = false;

        [Output]
        public ITaskItem[] AssetsWithTargetPath { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.Low, "Using path prefix '{0}'", PathPrefix);
                AssetsWithTargetPath = new TaskItem[Assets.Length];

                for (var i = 0; i < Assets.Length; i++)
                {
                    var staticWebAsset = StaticWebAsset.FromTaskItem(Assets[i]);
                    var result = staticWebAsset.ToTaskItem();
                    var targetPath = staticWebAsset.ComputeTargetPath(
                        PathPrefix,
                        UseAlternatePathDirectorySeparator ? Path.AltDirectorySeparatorChar : Path.DirectorySeparatorChar);

                    if (AdjustPathsForPack && string.IsNullOrEmpty(Path.GetExtension(targetPath)))
                    {
                        targetPath = Path.GetDirectoryName(targetPath);
                    }

                    result.SetMetadata("TargetPath", targetPath);

                    AssetsWithTargetPath[i] = result;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
