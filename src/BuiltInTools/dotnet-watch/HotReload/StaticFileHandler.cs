﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Tools.Internal;

namespace Microsoft.DotNet.Watcher.Tools
{
    internal sealed class StaticFileHandler
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly IReporter _reporter;

        public StaticFileHandler(IReporter reporter)
        {
            _reporter = reporter;
        }

        public async ValueTask<bool> TryHandleFileChange(BrowserRefreshServer? server, FileItem file, CancellationToken cancellationToken)
        {
            HotReloadEventSource.Log.HotReloadStart(HotReloadEventSource.StartType.StaticHandler);
            if (!file.IsStaticFile || server is null)
            {
                HotReloadEventSource.Log.HotReloadEnd(HotReloadEventSource.StartType.StaticHandler);
                return false;
            }
            _reporter.Verbose($"Handling file change event for static content {file.FilePath}.");
            await HandleBrowserRefresh(server, file, cancellationToken);
            HotReloadEventSource.Log.HotReloadEnd(HotReloadEventSource.StartType.StaticHandler);
            _reporter.Output("Hot reload of static file succeeded.", emoji: "🔥");
            return true;
        }

        private static async Task HandleBrowserRefresh(BrowserRefreshServer browserRefreshServer, FileItem fileItem, CancellationToken cancellationToken)
        {
            var message = JsonSerializer.SerializeToUtf8Bytes(new UpdateStaticFileMessage { Path = fileItem.StaticWebAssetPath }, JsonSerializerOptions);
            await browserRefreshServer.SendMessage(message, cancellationToken);
        }

        private readonly struct UpdateStaticFileMessage
        {
            public string Type => "UpdateStaticFile";

            public string Path { get; init; }
        }
    }
}
