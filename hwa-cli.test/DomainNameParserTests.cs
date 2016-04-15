using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using HwaCli.DomainParser;

namespace HwaCli.test
{
    [TestClass]
    public class DomainNameParserTests
    {
        [TestMethod]
        public void CanParseBasicUrl()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.com/123.html");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("abc.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseUrlWithSld()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.co.uk/123.html");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.co.uk", domain.DomainName);
            Assert.AreEqual("abc.domain.co.uk", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseWildcardProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("*://abc.domain.com/123.html");
            Assert.AreEqual("*", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("abc.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseWildcardSubdomain()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://*.domain.com/123.html");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("*.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);

            domain = DomainNameParser.Parse("http://*.abc.domain.com/123.html");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("*.abc.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);

            domain = DomainNameParser.Parse("http://*.*.domain.com/123.html");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("*.*.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseEmptyProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("abc.domain.com/123.html");
            Assert.AreEqual("*", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("abc.domain.com", domain.HostName);
            Assert.AreEqual("/123.html", domain.PathName);
        }

        [TestMethod]
        public void CanParseEmptyPath()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://abc.domain.com");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("domain.com", domain.DomainName);
            Assert.AreEqual("abc.domain.com", domain.HostName);
            Assert.AreEqual("/", domain.PathName);
        }

        [TestMethod]
        public void CanParseMsAppxProtocol()
        {
            Domain domain;

            domain = DomainNameParser.Parse("ms-appx:///abc.domain.com");
            Assert.AreEqual("ms-appx", domain.Scheme);
            Assert.AreEqual(string.Empty, domain.DomainName);
            Assert.AreEqual(string.Empty, domain.HostName);
            Assert.AreEqual("/abc.domain.com", domain.PathName);
        }

        [TestMethod]
        public void CanParseUnicodeDomain()
        {
            Domain domain;

            domain = DomainNameParser.Parse("http://例え.テスト/");
            Assert.AreEqual("http", domain.Scheme);
            Assert.AreEqual("例え.テスト", domain.DomainName);
            Assert.AreEqual("例え.テスト", domain.HostName);
            Assert.AreEqual("/", domain.PathName);

            domain = DomainNameParser.Parse("チェインクロニクル.gamerch.com/");
            Assert.AreEqual("*", domain.Scheme);
            Assert.AreEqual("gamerch.com", domain.DomainName);
            Assert.AreEqual("チェインクロニクル.gamerch.com", domain.HostName);
            Assert.AreEqual("/", domain.PathName);

            domain = DomainNameParser.Parse("チェインクロニクル.例え.テスト/");
            Assert.AreEqual("*", domain.Scheme);
            Assert.AreEqual("例え.テスト", domain.DomainName);
            Assert.AreEqual("チェインクロニクル.例え.テスト", domain.HostName);
            Assert.AreEqual("/", domain.PathName);

            domain = DomainNameParser.Parse("xn--eckfza0gxcvmna6c.xn--r8jz45g.xn--zckzah/");
            Assert.AreEqual("*", domain.Scheme);
            Assert.AreEqual("xn--r8jz45g.xn--zckzah", domain.DomainName);
            Assert.AreEqual("xn--eckfza0gxcvmna6c.xn--r8jz45g.xn--zckzah", domain.HostName);
            Assert.AreEqual("/", domain.PathName);
        }

        [TestMethod]
        public void CanUseCreateURIMethod()
        {
            string uri = "http://www.domain.com/";
            object obj = null;

            DomainNameParser.CreateUri(uri, CreateUriFlags.Uri_CREATE_ALLOW_IMPLICIT_FILE_SCHEME, new IntPtr(0), out obj);

            IUri iUri = (IUri)obj;
            string domain, host, path;

            iUri.GetDomain(out domain);
            iUri.GetHost(out host);
            iUri.GetPath(out path);

            Assert.AreEqual("domain.com", domain);
            Assert.AreEqual("www.domain.com", host);
            Assert.AreEqual("/", path);
        }

        [TestMethod]
        public void CreateUriParsesWildCardDomain()
        {
            string uri = "http://*.domain.com/";
            object obj = null;

            DomainNameParser.CreateUri(uri, CreateUriFlags.Uri_CREATE_ALLOW_IMPLICIT_FILE_SCHEME, new IntPtr(0), out obj);

            IUri iUri = (IUri)obj;
            string domain, host, path;

            iUri.GetDomain(out domain);
            iUri.GetHost(out host);
            iUri.GetPath(out path);

            Assert.AreEqual("domain.com", domain);
            Assert.AreEqual("*.domain.com", host);
            Assert.AreEqual("/", path);
        }

        [TestMethod]
        public void CreateUriParsesWildCardProtocol()
        {
            string uri = "*://www.domain.com/";
            object obj = null;

            DomainNameParser.CreateUri(uri, CreateUriFlags.Uri_CREATE_ALLOW_IMPLICIT_FILE_SCHEME, new IntPtr(0), out obj);

            IUri iUri = (IUri)obj;
            string domain, host, path;

            iUri.GetDomain(out domain);
            iUri.GetHost(out host);
            iUri.GetPath(out path);

            Assert.AreEqual("domain.com", domain);
            Assert.AreEqual("www.domain.com", host);
            Assert.AreEqual("/", path);
        }

        [TestMethod]
        public void CreateUriParsesSLD()
        {
            string uri = "http://www.domain.co.uk/";
            object obj = null;

            DomainNameParser.CreateUri(uri, CreateUriFlags.Uri_CREATE_ALLOW_IMPLICIT_FILE_SCHEME, new IntPtr(0), out obj);

            IUri iUri = (IUri)obj;
            string domain, host, path;

            iUri.GetDomain(out domain);
            iUri.GetHost(out host);
            iUri.GetPath(out path);

            Assert.AreEqual("domain.co.uk", domain);
            Assert.AreEqual("www.domain.co.uk", host);
            Assert.AreEqual("/", path);
        }
    }
}
