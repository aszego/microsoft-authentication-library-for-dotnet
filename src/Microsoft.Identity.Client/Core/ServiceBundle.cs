﻿// ------------------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.
// All rights reserved.
// 
// This code is licensed under the MIT License.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// ------------------------------------------------------------------------------

using System;
using Microsoft.Identity.Client.Http;
using Microsoft.Identity.Client.Instance;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Mats;
using Microsoft.Identity.Client.PlatformsCommon.Factories;
using Microsoft.Identity.Client.PlatformsCommon.Interfaces;
using Microsoft.Identity.Client.TelemetryCore;
using Microsoft.Identity.Client.WsTrust;

namespace Microsoft.Identity.Client.Core
{
    internal class ServiceBundle : IServiceBundle
    {
        internal ServiceBundle(
            ApplicationConfiguration config,
            bool shouldClearCaches = false)
        {
            Config = config;

            DefaultLogger = new MsalLogger(
                Guid.Empty,
                config.ClientName,
                config.ClientVersion,
                config.LogLevel,
                config.EnablePiiLogging,
                config.IsDefaultPlatformLoggingEnabled,
                config.LoggingCallback);

            PlatformProxy = PlatformProxyFactory.CreatePlatformProxy(DefaultLogger);
            HttpManager = config.HttpManager ?? new HttpManager(config.HttpClientFactory);

            if (config.MatsConfig != null)
            {
                // This can return null if the device isn't sampled in.  There's no need for processing MATS events if we're not going to send them.
                Mats = MatsTelemetryClient.CreateMats(config, PlatformProxy, config.MatsConfig);
                TelemetryManager = Mats?.TelemetryManager ?? new TelemetryManager(config, PlatformProxy, config.TelemetryCallback);
            }
            else
            {
                TelemetryManager = new TelemetryManager(config, PlatformProxy, config.TelemetryCallback);
            }

            AadInstanceDiscovery = new AadInstanceDiscovery(DefaultLogger, HttpManager, TelemetryManager, shouldClearCaches);
            WsTrustWebRequestManager = new WsTrustWebRequestManager(HttpManager);
            AuthorityEndpointResolutionManager = new AuthorityEndpointResolutionManager(this, shouldClearCaches);
        }

        public ICoreLogger DefaultLogger { get; }

        /// <inheritdoc />
        public IHttpManager HttpManager { get; }

        /// <inheritdoc />
        public ITelemetryManager TelemetryManager { get; }

        /// <inheritdoc />
        public IAadInstanceDiscovery AadInstanceDiscovery { get; }

        /// <inheritdoc />
        public IWsTrustWebRequestManager WsTrustWebRequestManager { get; }

        /// <inheritdoc />
        public IAuthorityEndpointResolutionManager AuthorityEndpointResolutionManager { get; }

        /// <inheritdoc />
        public IPlatformProxy PlatformProxy { get; }

        /// <inheritdoc />
        public IApplicationConfiguration Config { get; }

        /// <inheritdoc />
        public IMatsTelemetryClient Mats { get; }

        public static ServiceBundle Create(ApplicationConfiguration config)
        {
            return new ServiceBundle(config);
        }
    }
}
