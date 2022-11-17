/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.Core;
using Microsoft.Playwright.Helpers;
using Microsoft.Playwright.Transport.Protocol;

namespace Microsoft.Playwright.Transport.Channels;

internal class BrowserContextChannel : Channel<BrowserContext>
{
    public BrowserContextChannel(string guid, Connection connection, BrowserContext owner) : base(guid, connection, owner)
    {
    }

    internal event EventHandler Close;

    internal event EventHandler<BrowserContextPageEventArgs> Page;

    internal event EventHandler<BrowserContextPageEventArgs> BackgroundPage;

    internal event EventHandler<IWorker> ServiceWorker;

    internal event EventHandler<BindingCall> BindingCall;

    internal event EventHandler<Route> Route;

    internal event EventHandler<BrowserContextChannelRequestEventArgs> Request;

    internal event EventHandler<BrowserContextChannelRequestEventArgs> RequestFinished;

    internal event EventHandler<BrowserContextChannelRequestEventArgs> RequestFailed;

    internal event EventHandler<BrowserContextChannelResponseEventArgs> Response;

    internal override void OnMessage(string method, JsonElement? serverParams)
    {
        switch (method)
        {
            case "close":
                Close?.Invoke(this, EventArgs.Empty);
                break;
            case "bindingCall":
                BindingCall?.Invoke(
                    this,
                    serverParams?.GetProperty("binding").ToObject<BindingCallChannel>(Connection.DefaultJsonSerializerOptions).Object);
                break;
            case "route":
                var route = serverParams?.GetProperty("route").ToObject<RouteChannel>(Connection.DefaultJsonSerializerOptions).Object;
                Route?.Invoke(this, route);
                break;
            case "page":
                Page?.Invoke(
                    this,
                    new() { PageChannel = serverParams?.GetProperty("page").ToObject<PageChannel>(Connection.DefaultJsonSerializerOptions) });
                break;
            case "crBackgroundPage":
                BackgroundPage?.Invoke(
                    this,
                    new() { PageChannel = serverParams?.GetProperty("page").ToObject<PageChannel>(Connection.DefaultJsonSerializerOptions) });
                break;
            case "serviceWorker":
                ServiceWorker?.Invoke(
                    this,
                    serverParams?.GetProperty("worker").ToObject<WorkerChannel>(Connection.DefaultJsonSerializerOptions).Object);
                break;
            case "request":
                Request?.Invoke(this, serverParams?.ToObject<BrowserContextChannelRequestEventArgs>(Connection.DefaultJsonSerializerOptions));
                break;
            case "requestFinished":
                RequestFinished?.Invoke(this, serverParams?.ToObject<BrowserContextChannelRequestEventArgs>(Connection.DefaultJsonSerializerOptions));
                break;
            case "requestFailed":
                RequestFailed?.Invoke(this, serverParams?.ToObject<BrowserContextChannelRequestEventArgs>(Connection.DefaultJsonSerializerOptions));
                break;
            case "response":
                Response?.Invoke(this, serverParams?.ToObject<BrowserContextChannelResponseEventArgs>(Connection.DefaultJsonSerializerOptions));
                break;
        }
    }

    internal Task<PageChannel> NewPageAsync()
        => Object.SendMessageToServerAsync<PageChannel>(
            "newPage",
            null);

    internal Task CloseAsync() => Object.SendMessageToServerAsync("close");

    internal Task PauseAsync()
        => Object.SendMessageToServerAsync("pause");

    internal Task SetDefaultNavigationTimeoutNoReplyAsync(float timeout)
        => Object.SendMessageToServerAsync<PageChannel>(
            "setDefaultNavigationTimeoutNoReply",
            new Dictionary<string, object>
            {
                ["timeout"] = timeout,
            });

    internal Task SetDefaultTimeoutNoReplyAsync(float timeout)
        => Object.SendMessageToServerAsync<PageChannel>(
            "setDefaultTimeoutNoReply",
            new Dictionary<string, object>
            {
                ["timeout"] = timeout,
            });

    internal Task ExposeBindingAsync(string name, bool needsHandle)
        => Object.SendMessageToServerAsync<PageChannel>(
            "exposeBinding",
            new Dictionary<string, object>
            {
                ["name"] = name,
                ["needsHandle"] = needsHandle,
            });

    internal Task AddInitScriptAsync(string script)
        => Object.SendMessageToServerAsync<PageChannel>(
            "addInitScript",
            new Dictionary<string, object>
            {
                ["source"] = script,
            });

    internal Task SetNetworkInterceptionEnabledAsync(bool enabled)
        => Object.SendMessageToServerAsync<PageChannel>(
            "setNetworkInterceptionEnabled",
            new Dictionary<string, object>
            {
                ["enabled"] = enabled,
            });

    internal Task SetOfflineAsync(bool offline)
        => Object.SendMessageToServerAsync<PageChannel>(
            "setOffline",
            new Dictionary<string, object>
            {
                ["offline"] = offline,
            });

    internal async Task<IReadOnlyList<BrowserContextCookiesResult>> CookiesAsync(IEnumerable<string> urls)
    {
        return (await Object.SendMessageToServerAsync(
            "cookies",
            new Dictionary<string, object>
            {
                ["urls"] = urls?.ToArray() ?? Array.Empty<string>(),
            }).ConfigureAwait(false))?.GetProperty("cookies").ToObject<IReadOnlyList<BrowserContextCookiesResult>>();
    }

    internal Task AddCookiesAsync(IEnumerable<Cookie> cookies)
        => Object.SendMessageToServerAsync<PageChannel>(
            "addCookies",
            new Dictionary<string, object>
            {
                ["cookies"] = cookies,
            });

    internal Task GrantPermissionsAsync(IEnumerable<string> permissions, string origin)
    {
        var args = new Dictionary<string, object>
        {
            ["permissions"] = permissions?.ToArray(),
            ["origin"] = origin,
        };
        return Object.SendMessageToServerAsync<PageChannel>("grantPermissions", args);
    }

    internal Task ClearPermissionsAsync() => Object.SendMessageToServerAsync<PageChannel>("clearPermissions");

    internal Task SetGeolocationAsync(Geolocation geolocation)
        => Object.SendMessageToServerAsync<PageChannel>(
            "setGeolocation",
            new Dictionary<string, object>
            {
                ["geolocation"] = geolocation,
            });

    internal Task ClearCookiesAsync() => Object.SendMessageToServerAsync<PageChannel>("clearCookies");

    internal Task SetExtraHTTPHeadersAsync(IEnumerable<KeyValuePair<string, string>> headers)
        => Object.SendMessageToServerAsync(
            "setExtraHTTPHeaders",
            new Dictionary<string, object>
            {
                ["headers"] = headers.Select(kv => new HeaderEntry { Name = kv.Key, Value = kv.Value }),
            });

    internal Task<StorageState> GetStorageStateAsync()
        => Object.SendMessageToServerAsync<StorageState>("storageState", null);

    internal async Task<Artifact> HarExportAsync(string harId)
    {
        var result = await Object.SendMessageToServerAsync(
        "harExport",
        new Dictionary<string, object>
        {
            ["harId"] = harId,
        }).ConfigureAwait(false);
        return result.GetObject<Artifact>("artifact", Connection);
    }

    internal async Task<string> HarStartAsync(
        Page page,
        string path,
        string recordHarUrlFilter,
        string recordHarUrlFilterString,
        Regex recordHarUrlFilterRegex)
    {
        var args = new Dictionary<string, object>
            {
                { "page", page?.Channel },
                { "options", BrowserChannel.PrepareHarOptions(HarContentPolicy.Attach, HarMode.Minimal, path, null, recordHarUrlFilter, recordHarUrlFilterString, recordHarUrlFilterRegex) },
            };
        var result = await Object.SendMessageToServerAsync("harStart", args).ConfigureAwait(false);
        return result.GetString("harId", false);
    }

    internal async Task<WritableStream> CreateTempFileAsync(string name)
    {
        var args = new Dictionary<string, object>
            {
                { "name", name },
            };
        var result = await Object.SendMessageToServerAsync("createTempFile", args).ConfigureAwait(false);
        return result.GetObject<WritableStream>("writableStream", Connection);
    }
}
