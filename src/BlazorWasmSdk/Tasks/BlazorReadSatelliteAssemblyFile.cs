// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.NET.Sdk.BlazorWebAssembly
{
    public class BlazorReadSatelliteAssemblyFile : Task
    {
        [Output]
        public ITaskItem[] SatelliteAssembly { get; set; }

        [Required]
        public ITaskItem ReadFile { get; set; }

        public override bool Execute()
        {
            var document = XDocument.Load(ReadFile.ItemSpec);
            SatelliteAssembly = document.Root
                .Elements()
                .Select(e =>
                {
                    // <Assembly Name="..." Culture="..." DestinationSubDirectory="..." />

                    var taskItem = new TaskItem(e.Attribute("Name").Value);
                    taskItem.SetMetadata("Culture", e.Attribute("Culture").Value);
                    taskItem.SetMetadata("DestinationSubDirectory", e.Attribute("DestinationSubDirectory").Value);

                    return taskItem;
                }).ToArray();

            return true;
        }
    }
}
