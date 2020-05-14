﻿// Copyright 2020 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Api.Gax;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[Collection(nameof(PubsubFixture))]
public class AcknowledgeMessageTest
{
    private readonly PubsubFixture _pubsubFixture;
    private readonly GetPublisherAsyncSample _getPublisherAsyncSample;
    private readonly PublishMessagesAsyncSample _publishMessagesAsyncSample;
    private readonly PullMessagesCustomAsyncSample _pullMessagesCustomAsyncSample;
    private readonly PullMessagesAsyncSample _pullMessagesAsyncSample;

    public AcknowledgeMessageTest(PubsubFixture pubsubFixture)
    {
        _pubsubFixture = pubsubFixture;
        _getPublisherAsyncSample = new GetPublisherAsyncSample();
        _publishMessagesAsyncSample = new PublishMessagesAsyncSample();
        _pullMessagesCustomAsyncSample = new PullMessagesCustomAsyncSample();
        _pullMessagesAsyncSample = new PullMessagesAsyncSample();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcknowledgeMessage(bool customFlow)
    {
        string topicId = "testTopicForMessageAck" + _pubsubFixture.RandomName();
        string subscriptionId = "testSubscriptionForMessageAck" + _pubsubFixture.RandomName();
        var message = _pubsubFixture.RandomName();

        _pubsubFixture.CreateTopic(topicId);
        _pubsubFixture.CreateSubscription(topicId, subscriptionId);

        var publisher = Task.Run(() =>
        _getPublisherAsyncSample.GetPublisherAsync(_pubsubFixture.ProjectId, topicId))
            .ResultWithUnwrappedExceptions();

        Task.Run(() =>
        _publishMessagesAsyncSample.PublishMessagesAsync(publisher, new string[] { message }))
            .ResultWithUnwrappedExceptions();

        // Pull and acknowledge the messages
        _pubsubFixture.Eventually(() =>
        {
            var result = HandlePullMessages(customFlow, subscriptionId);
            Assert.Contains(result, c => c.Contains(message));
        });

        _pubsubFixture.Eventually(() =>
        {
            //Pull the Message to confirm it's gone after it's acknowledged
            var result = HandlePullMessages(customFlow, subscriptionId);
            Assert.True(result.Count == 0);
        });
    }

    private List<string> HandlePullMessages(bool customFlow, string subscriptionId)
    {
        if (customFlow)
        {
            return Task.Run(() => _pullMessagesCustomAsyncSample.PullMessagesCustomAsync(
                  _pubsubFixture.ProjectId, subscriptionId, true))
                  .ResultWithUnwrappedExceptions();
        }
        else
        {
            return Task.Run(() => _pullMessagesAsyncSample.PullMessagesAsync(_pubsubFixture.ProjectId,
                     subscriptionId, true)).ResultWithUnwrappedExceptions();
        }
    }
}
