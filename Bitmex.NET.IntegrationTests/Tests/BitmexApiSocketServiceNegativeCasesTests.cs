﻿using Bitmex.NET.Dtos;
using Bitmex.NET.Models;
using Bitmex.NET.Models.Socket;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Configuration;
using Unity;

namespace Bitmex.NET.IntegrationTests.Tests
{
    [TestClass]
    [TestCategory("WebSocket")]
    public class BitmexApiSocketServiceNegativeCasesTests : IntegrationTestsClass<IBitmexApiSocketService>
    {
        private IBitmexAuthorization _bitmexAuthorization;

        [TestInitialize]
        public override void TestInitialize()
        {
            _bitmexAuthorization = Substitute.For<IBitmexAuthorization>();
            var container = new UnityContainer();
            container.AddNewExtension<BitmexNetUnityExtension>();
            container.RegisterInstance<IBitmexAuthorization>(_bitmexAuthorization);

            Sut = container.Resolve<IBitmexApiSocketService>();
            _bitmexAuthorization.BitmexEnvironment.Returns(BitmexEnvironment.Test);
            _bitmexAuthorization.Key.Returns(ConfigurationManager.AppSettings["Key1"]);
            _bitmexAuthorization.Secret.Returns(ConfigurationManager.AppSettings["Secret1"]);
        }

        [TestMethod]
        public void should_raise_an_exception_if_authorization_failed_due_to_key()
        {
            try
            {
                var wrongKey = Guid.NewGuid().ToString("N");
                _bitmexAuthorization.Key.Returns(wrongKey);

                // act
                Sut.Connect();

            }
            catch (BitmexWebSocketLimitReachedException)
            {
                Assert.Inconclusive("connection limit reached");
            }
            // assert
            catch (BitmexSocketAuthorizationException)
            {
                return;
            }

            Assert.Fail("BitmexSocketAuthorizationException should be thrown");
        }

        [TestMethod]
        public void should_raise_an_exception_if_authorization_failed_due_to_secret()
        {
            try
            {
                var wrongSign = Guid.NewGuid().ToString("N");
                _bitmexAuthorization.Secret.Returns(wrongSign);

                // act
                Sut.Connect();
            }
            catch (BitmexWebSocketLimitReachedException)
            {
                Assert.Inconclusive("connection limit reached");
            }
            // assert
            catch (BitmexSocketAuthorizationException)
            {
                return;
            }

            Assert.Fail("BitmexSocketAuthorizationException should be thrown");
        }

        [TestMethod]
        public void should_raise_an_exception_if_subscription_is_incorrect()
        {
            try
            {
                // act
                Sut.Connect();
                Action act = () => Sut.Subscribe(new BitmexApiSubscriptionInfo<InstrumentDto>("somethingThatDoesNotExist", dto => { }));

                // assert
                // can be either timeout or API key invalid.
                act.Should().Throw<BitmexSocketSubscriptionException>().Which.Message.Should().Contain("somethingThatDoesNotExist");
            }
            catch (BitmexWebSocketLimitReachedException)
            {
                Assert.Inconclusive("connection limit reached");
            }
        }
    }
}
