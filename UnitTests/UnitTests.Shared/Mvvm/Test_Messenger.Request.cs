// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Mvvm
{
    public partial class Test_Messenger
    {
        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_Messenger_RequestMessage_Ok()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, NumberRequestMessage m)
            {
                Assert.IsFalse(m.HasReceivedResponse);

                m.Reply(42);

                Assert.IsTrue(m.HasReceivedResponse);
            }

            messenger.Register<NumberRequestMessage>(recipient, Receive);

            int result = messenger.Send<NumberRequestMessage>();

            Assert.AreEqual(result, 42);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_Messenger_RequestMessage_Fail_NoReply()
        {
            var messenger = new Messenger();

            int result = messenger.Send<NumberRequestMessage>();
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_Messenger_RequestMessage_Fail_MultipleReplies()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, NumberRequestMessage m)
            {
                m.Reply(42);
                m.Reply(42);
            }

            messenger.Register<NumberRequestMessage>(recipient, Receive);

            int result = messenger.Send<NumberRequestMessage>();
        }

        public class NumberRequestMessage : RequestMessage<int>
        {
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public async Task Test_Messenger_AsyncRequestMessage_Ok_Sync()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, AsyncNumberRequestMessage m)
            {
                Assert.IsFalse(m.HasReceivedResponse);

                m.Reply(42);

                Assert.IsTrue(m.HasReceivedResponse);
            }

            messenger.Register<AsyncNumberRequestMessage>(recipient, Receive);

            int result = await messenger.Send<AsyncNumberRequestMessage>();

            Assert.AreEqual(result, 42);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public async Task Test_Messenger_AsyncRequestMessage_Ok_Async()
        {
            var messenger = new Messenger();
            var recipient = new object();

            async Task<int> GetNumberAsync()
            {
                await Task.Delay(100);

                return 42;
            }

            void Receive(object recipient, AsyncNumberRequestMessage m)
            {
                Assert.IsFalse(m.HasReceivedResponse);

                m.Reply(GetNumberAsync());

                Assert.IsTrue(m.HasReceivedResponse);
            }

            messenger.Register<AsyncNumberRequestMessage>(recipient, Receive);

            int result = await messenger.Send<AsyncNumberRequestMessage>();

            Assert.AreEqual(result, 42);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Test_Messenger_AsyncRequestMessage_Fail_NoReply()
        {
            var messenger = new Messenger();

            int result = await messenger.Send<AsyncNumberRequestMessage>();
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Test_Messenger_AsyncRequestMessage_Fail_MultipleReplies()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, AsyncNumberRequestMessage m)
            {
                m.Reply(42);
                m.Reply(42);
            }

            messenger.Register<AsyncNumberRequestMessage>(recipient, Receive);

            int result = await messenger.Send<AsyncNumberRequestMessage>();
        }

        public class AsyncNumberRequestMessage : AsyncRequestMessage<int>
        {
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_Messenger_CollectionRequestMessage_Ok_NoReplies()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, NumbersCollectionRequestMessage m)
            {
            }

            messenger.Register<NumbersCollectionRequestMessage>(recipient, Receive);

            var results = messenger.Send<NumbersCollectionRequestMessage>().Responses;

            Assert.AreEqual(results.Count, 0);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_Messenger_CollectionRequestMessage_Ok_MultipleReplies()
        {
            var messenger = new Messenger();
            object
                recipient1 = new object(),
                recipient2 = new object(),
                recipient3 = new object();

            void Receive1(object recipient, NumbersCollectionRequestMessage m) => m.Reply(1);
            void Receive2(object recipient, NumbersCollectionRequestMessage m) => m.Reply(2);
            void Receive3(object recipient, NumbersCollectionRequestMessage m) => m.Reply(3);

            messenger.Register<NumbersCollectionRequestMessage>(recipient1, Receive1);
            messenger.Register<NumbersCollectionRequestMessage>(recipient2, Receive2);
            messenger.Register<NumbersCollectionRequestMessage>(recipient3, Receive3);

            List<int> responses = new List<int>();

            foreach (var response in messenger.Send<NumbersCollectionRequestMessage>())
            {
                responses.Add(response);
            }

            CollectionAssert.AreEquivalent(responses, new[] { 1, 2, 3 });
        }

        public class NumbersCollectionRequestMessage : CollectionRequestMessage<int>
        {
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public async Task Test_Messenger_AsyncCollectionRequestMessage_Ok_NoReplies()
        {
            var messenger = new Messenger();
            var recipient = new object();

            void Receive(object recipient, AsyncNumbersCollectionRequestMessage m)
            {
            }

            messenger.Register<AsyncNumbersCollectionRequestMessage>(recipient, Receive);

            var results = await messenger.Send<AsyncNumbersCollectionRequestMessage>().GetResponsesAsync();

            Assert.AreEqual(results.Count, 0);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public async Task Test_Messenger_AsyncCollectionRequestMessage_Ok_MultipleReplies()
        {
            var messenger = new Messenger();
            object
                recipient1 = new object(),
                recipient2 = new object(),
                recipient3 = new object(),
                recipient4 = new object();

            async Task<int> GetNumberAsync()
            {
                await Task.Delay(100);

                return 3;
            }

            void Receive1(object recipient, AsyncNumbersCollectionRequestMessage m) => m.Reply(1);
            void Receive2(object recipient, AsyncNumbersCollectionRequestMessage m) => m.Reply(Task.FromResult(2));
            void Receive3(object recipient, AsyncNumbersCollectionRequestMessage m) => m.Reply(GetNumberAsync());
            void Receive4(object recipient, AsyncNumbersCollectionRequestMessage m) => m.Reply(_ => GetNumberAsync());

            messenger.Register<AsyncNumbersCollectionRequestMessage>(recipient1, Receive1);
            messenger.Register<AsyncNumbersCollectionRequestMessage>(recipient2, Receive2);
            messenger.Register<AsyncNumbersCollectionRequestMessage>(recipient3, Receive3);
            messenger.Register<AsyncNumbersCollectionRequestMessage>(recipient4, Receive4);

            List<int> responses = new List<int>();

            await foreach (var response in messenger.Send<AsyncNumbersCollectionRequestMessage>())
            {
                responses.Add(response);
            }

            CollectionAssert.AreEquivalent(responses, new[] { 1, 2, 3, 3 });
        }

        public class AsyncNumbersCollectionRequestMessage : AsyncCollectionRequestMessage<int>
        {
        }
    }
}
