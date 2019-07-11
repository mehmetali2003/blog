using Microsoft.Owin.Hosting;
using NUnit.Framework;
using System;

namespace SampleBlog.Tests.Integration
{
    /// <summary>
    /// A base for integration tests which launches a self-hosted HTTP server
    /// </summary>
    public class BaseIntegrationTest
    {
        private IDisposable _webApp;
        private static int _hostPort = 56466;

        protected Uri ApiAction(string actionName)
        {
            return new Uri(string.Format("http://localhost:{0}/api/{1}", _hostPort, actionName));
        }

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.Init();
            _webApp = WebApp.Start<Startup>(string.Format("http://*:{0}/", _hostPort));
        }

        [TearDown]
        public void TearDown()
        {
            _webApp.Dispose();
        }
    }
}
