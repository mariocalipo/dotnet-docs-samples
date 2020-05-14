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

using System.Linq;
using Xunit;

[Collection(nameof(PubsubFixture))]
public class ListSubscriptionsTest
{
    private readonly PubsubFixture _pubsubFixture;
    private readonly ListSubscriptionsSample listSubscriptionsSample;
    public ListSubscriptionsTest(PubsubFixture pubsubFixture)
    {
        _pubsubFixture = pubsubFixture;
        listSubscriptionsSample = new ListSubscriptionsSample();
    }

    [Fact]
    public void TestListSubscriptions()
    {
        string topicId = "testTopicForListingSubscriptions" + _pubsubFixture.RandomName();
        string subscriptionId = "testSubscriptionForListingSubscriptions" + _pubsubFixture.RandomName();

        _pubsubFixture.CreateTopic(topicId);
        _pubsubFixture.CreateSubscription(topicId, subscriptionId);

        _pubsubFixture.Eventually(() =>
        {
            var subscriptions = listSubscriptionsSample.ListSubscriptions(_pubsubFixture.ProjectId);

            Assert.Contains(subscriptions.Select(s => s.SubscriptionName.SubscriptionId), c => c.Contains(subscriptionId));
        });
    }
}
