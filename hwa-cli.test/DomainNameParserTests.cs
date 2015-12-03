using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using hwa_cli.DomainParser;

namespace hwa_cli.test
{
    [TestClass]
    public class DomainNameParserTests
    {
        [TestMethod]
        public void CanParseBasicUrl()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.com/123.html");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("abc.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseUrlWithSld()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.co.uk/123.html");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.co.uk", domain.HostName);
            Assert.AreEqual("abc.domain.co.uk", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseWildcardProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("*://abc.domain.com/123.html");
            Assert.AreEqual("*://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("abc.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseWildcardSubdomain()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://*.domain.com/123.html");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("*.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);

            domain = DomainNameParser.Parse("http://*.abc.domain.com/123.html");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("*.abc.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);

            domain = DomainNameParser.Parse("http://*.*.domain.com/123.html");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("*.*.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseEmptyProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("abc.domain.com/123.html");
            Assert.AreEqual(string.Empty, domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("abc.domain.com", domain.FullHostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseEmptyPath()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.com");
            Assert.AreEqual("http://", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("abc.domain.com", domain.FullHostName);
            Assert.AreEqual(string.Empty, domain.PathName);
        }

        [TestMethod]
        public void CanParseMsAppxProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("ms-appx:///abc.domain.com");
            Assert.AreEqual("ms-appx:///", domain.Protocol);
            Assert.AreEqual("domain.com", domain.HostName);
            Assert.AreEqual("abc.domain.com", domain.FullHostName);
            Assert.AreEqual(string.Empty, domain.PathName);
        }
    }
}
