﻿using System.Net;
using NUnit.Framework;
using Should;

namespace Tests
{
        [TestFixture]
        public class Tests
        {
            private Website _website;

            [TestFixtureSetUp]
            public void Setup()
            {
                _website = Website.Create(@"..\..\..\TestHarness");
                _website.Start();
            }

            [TestFixtureTearDown]
            public void TearDown()
            {
                _website.Stop();
            }

            [Test]
            public void should_serialize_with_bender()
            {
                new WebClient().UploadString(_website.Url, 
                    "{\"value\":\"hai\"}").ShouldEqual("{\"Value\":null}");
            }
        }
}