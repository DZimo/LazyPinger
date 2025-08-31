using LazyPinger.Base.IServices;
using LazyPinger.Core.Services;

namespace LazyPinger.Tests.Services
{
    [TestClass]
    public sealed class NetworkServiceTest
    {
        [TestMethod]
        public void InitNetworkSettingsTest()
        {
            var networkService = new NetworkService(new TextParserService());
            var res = networkService.InitNetworkSettings();
            Assert.IsNotNull(res);
        }
    }
}
