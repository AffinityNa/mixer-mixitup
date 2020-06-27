﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Clients;
using Twitch.Base.Models.Clients.PubSub;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public interface ITwitchEventService
    {
        Task<Result> Connect();
        Task Disconnect();
    }

    public class TwitchEventService : StreamingPlatformServiceBase, ITwitchEventService
    {
        private static readonly List<PubSubTopicsEnum> topicTypes = new List<PubSubTopicsEnum>()
        {
            PubSubTopicsEnum.ChannelBitsEventsV2,
            PubSubTopicsEnum.ChannelBitsBadgeUnlocks,
            PubSubTopicsEnum.ChannelSubscriptionsV1,
            PubSubTopicsEnum.UserWhispers,
            PubSubTopicsEnum.ChannelPointsRedeemed
        };

        private PubSubClient pubSub;

        private CancellationTokenSource cancellationTokenSource;

        private HashSet<string> follows = new HashSet<string>();

        public override string Name { get { return "Twitch Events"; } }

        public TwitchEventService() { }

        public async Task<Result> Connect()
        {
            if (ChannelSession.TwitchUserConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    try
                    {
                        this.pubSub = new PubSubClient(ChannelSession.TwitchUserConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.pubSub.OnSentOccurred += PubSub_OnSentOccurred;
                            this.pubSub.OnTextReceivedOccurred += PubSub_OnTextReceivedOccurred;
                            this.pubSub.OnMessageReceived += PubSub_OnMessageReceived;
                        }
                        this.pubSub.OnReconnectReceived += PubSub_OnReconnectReceived;
                        this.pubSub.OnDisconnectOccurred += PubSub_OnDisconnectOccurred;
                        this.pubSub.OnPongReceived += PubSub_OnPongReceived;
                        this.pubSub.OnResponseReceived += PubSub_OnResponseReceived;

                        this.pubSub.OnWhisperReceived += PubSub_OnWhisperReceived;
                        this.pubSub.OnBitsV2Received += PubSub_OnBitsV2Received;
                        this.pubSub.OnBitsBadgeReceived += PubSub_OnBitsBadgeReceived;
                        this.pubSub.OnSubscribedReceived += PubSub_OnSubscribedReceived;
                        this.pubSub.OnSubscriptionsGiftedReceived += PubSub_OnSubscriptionsGiftedReceived;
                        this.pubSub.OnChannelPointsRedeemed += PubSub_OnChannelPointsRedeemed;

                        await this.pubSub.Connect();

                        await Task.Delay(1000);

                        List<PubSubListenTopicModel> topics = new List<PubSubListenTopicModel>();
                        foreach (PubSubTopicsEnum topic in TwitchEventService.topicTypes)
                        {
                            topics.Add(new PubSubListenTopicModel(topic, ChannelSession.TwitchUserNewAPI.id));
                        }

                        await this.pubSub.Listen(topics);

                        await Task.Delay(1000);

                        await this.pubSub.Ping();

                        this.cancellationTokenSource = new CancellationTokenSource();

                        follows.Clear();
                        IEnumerable<UserFollowModel> followers = await ChannelSession.TwitchUserConnection.GetNewAPIFollowers(ChannelSession.TwitchChannelNewAPI, maxResult: 100);
                        foreach (UserFollowModel follow in followers)
                        {
                            follows.Add(follow.from_id);
                        }

                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 60000, this.FollowerBackground);

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                });
            }
            return new Result("Twitch connection has not been established");
        }

        public async Task Disconnect()
        {
            try
            {
                if (this.pubSub != null)
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        this.pubSub.OnSentOccurred -= PubSub_OnSentOccurred;
                        this.pubSub.OnTextReceivedOccurred -= PubSub_OnTextReceivedOccurred;
                        this.pubSub.OnMessageReceived -= PubSub_OnMessageReceived;
                    }
                    this.pubSub.OnReconnectReceived -= PubSub_OnReconnectReceived;
                    this.pubSub.OnDisconnectOccurred -= PubSub_OnDisconnectOccurred;
                    this.pubSub.OnPongReceived -= PubSub_OnPongReceived;
                    this.pubSub.OnResponseReceived -= PubSub_OnResponseReceived;

                    this.pubSub.OnWhisperReceived -= PubSub_OnWhisperReceived;
                    this.pubSub.OnBitsV2Received -= PubSub_OnBitsV2Received;
                    this.pubSub.OnBitsBadgeReceived -= PubSub_OnBitsBadgeReceived;
                    this.pubSub.OnSubscribedReceived -= PubSub_OnSubscribedReceived;
                    this.pubSub.OnSubscriptionsGiftedReceived -= PubSub_OnSubscriptionsGiftedReceived;
                    this.pubSub.OnChannelPointsRedeemed -= PubSub_OnChannelPointsRedeemed;

                    await this.pubSub.Disconnect();
                }

                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.pubSub = null;
        }

        private async Task FollowerBackground(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                foreach (UserFollowModel follow in await ChannelSession.TwitchUserConnection.GetNewAPIFollowers(ChannelSession.TwitchChannelNewAPI, maxResult: 100))
                {
                    if (!follows.Contains(follow.from_id))
                    {
                        follows.Add(follow.from_id);

                        UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(follow.from_id);
                        if (user == null)
                        {
                            user = new UserViewModel(follow);
                        }

                        EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelFollowed, user);
                        if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                        {
                            user.Data.TwitchFollowDate = DateTimeOffset.Now;

                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestFollowerUserData] = user.ID;

                            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                            {
                                currency.AddAmount(user.Data, currency.OnFollowBonus);
                            }

                            await ChannelSession.Services.Events.PerformEvent(trigger);

                            GlobalEvents.FollowOccurred(user);
                            await this.AddAlertChatMessage(user, string.Format("{0} Followed", user.Username));
                        }
                    }
                }
            }
        }

        private async void PubSub_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Twitch PubSub");

            Result result;
            await this.Disconnect();
            do
            {
                await Task.Delay(2500);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Twitch PubSub");
        }

        private void PubSub_OnReconnectReceived(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Twitch PubSub");
        }

        private void PubSub_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, "PUB SUB SEND: " + packet);
        }

        private void PubSub_OnTextReceivedOccurred(object sender, string text)
        {
            Logger.Log(LogLevel.Debug, "PUB SUB TEXT: " + text);
        }

        private void PubSub_OnMessageReceived(object sender, PubSubMessagePacketModel packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("PUB SUB MESSAGE: {0} {1} ", packet.type, packet.message));

            Logger.Log(LogLevel.Debug, JSONSerializerHelper.SerializeToString(packet));
        }

        private void PubSub_OnResponseReceived(object sender, PubSubResponsePacketModel packet)
        {
            Logger.Log("PUB SUB RESPONSE: " + packet.error);
        }

        private async void PubSub_OnBitsV2Received(object sender, PubSubBitsEventV2Model packet)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (user == null)
            {
                user = new UserViewModel(packet);
            }

            foreach (CurrencyModel emberCurrency in ChannelSession.Settings.Currency.Values.Where(c => c.SpecialTracking == CurrencySpecialTrackingEnum.Bits))
            {
                emberCurrency.AddAmount(user.Data, packet.bits_used);
            }

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsUsageUserData] = user.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsUsageAmountData] = packet.bits_used;

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelBitsCheered, user);
            trigger.SpecialIdentifiers["bitsamount"] = packet.bits_used.ToString();
            await ChannelSession.Services.Events.PerformEvent(trigger);
        }

        private void PubSub_OnBitsBadgeReceived(object sender, PubSubBitBadgeEventModel packet)
        {

        }

        private async void PubSub_OnSubscribedReceived(object sender, PubSubSubscriptionsEventModel packet)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (user == null)
            {
                user = new UserViewModel(packet);
            }

            if (packet.IsSubscription || packet.cumulative_months == 1)
            {
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscribed, user);
                if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                {
                    trigger.SpecialIdentifiers["message"] = (packet.sub_message.ContainsKey("message")) ? packet.sub_message["message"].ToString() : string.Empty;
                    trigger.SpecialIdentifiers["usersubplan"] = packet.sub_plan_name;

                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                    user.Data.TwitchSubscribeDate = DateTimeOffset.Now;
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                    }
                    user.Data.TotalMonthsSubbed++;

                    await ChannelSession.Services.Events.PerformEvent(trigger);
                }

                GlobalEvents.SubscribeOccurred(user);

                await this.AddAlertChatMessage(user, string.Format("{0} Subscribed", user.Username));
            }
            else
            {
                int months = Math.Max(packet.streak_months, packet.cumulative_months);
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelResubscribed, user);
                if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                {
                    trigger.SpecialIdentifiers["message"] = (packet.sub_message.ContainsKey("message")) ? packet.sub_message["message"].ToString() : string.Empty;
                    trigger.SpecialIdentifiers["usersubplan"] = packet.sub_plan_name;
                    trigger.SpecialIdentifiers["usersubmonths"] = months.ToString();

                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                    user.Data.TwitchSubscribeDate = DateTimeOffset.Now.SubtractMonths(months - 1);
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                    }
                    user.Data.TotalMonthsSubbed++;

                    await ChannelSession.Services.Events.PerformEvent(trigger);
                }

                GlobalEvents.ResubscribeOccurred(new Tuple<UserViewModel, int>(user, months));

                await this.AddAlertChatMessage(user, string.Format("{0} Re-Subscribed For {1} Months", user.Username, months));
            }
        }

        private async void PubSub_OnSubscriptionsGiftedReceived(object sender, PubSubSubscriptionsGiftEventModel packet)
        {
            UserViewModel gifter = ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (gifter == null)
            {
                gifter = new UserViewModel(packet);
            }

            UserViewModel receiver = ChannelSession.Services.User.GetUserByTwitchID(packet.recipient_id);
            if (receiver == null)
            {
                receiver = new UserViewModel(new UserModel()
                {
                    id = packet.recipient_id,
                    login = packet.recipient_user_name,
                    display_name = packet.recipient_display_name
                });
            }

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscriptionGifted, gifter);
            trigger.SpecialIdentifiers["usersubplan"] = packet.sub_plan_name;

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = receiver.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

            receiver.Data.TwitchSubscribeDate = DateTimeOffset.Now;
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                currency.AddAmount(gifter.Data, currency.OnSubscribeBonus);
            }
            gifter.Data.TotalSubsGifted++;
            receiver.Data.TotalSubsReceived++;
            receiver.Data.TotalMonthsSubbed++;

            trigger.Arguments.Add(receiver.Username);
            await ChannelSession.Services.Events.PerformEvent(trigger);

            await this.AddAlertChatMessage(gifter, string.Format("{0} Gifted A Subscription To {1}", gifter.Username, receiver.Username));

            GlobalEvents.SubscriptionGiftedOccurred(gifter, receiver);
        }

        private async void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointsRedemptionEventModel packet)
        {
            PubSubChannelPointsRedeemedEventModel redemption = packet.redemption;

            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(redemption.user.id);
            if (user == null)
            {
                user = new UserViewModel(redemption.user);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers["rewardname"] = redemption.reward.title;
            specialIdentifiers["rewardcost"] = redemption.reward.cost.ToString();
            specialIdentifiers["message"] = redemption.user_input;

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelPointsRedeemed, user);
            foreach (var kvp in specialIdentifiers)
            {
                trigger.SpecialIdentifiers[kvp.Key] = kvp.Value;
            }

            await ChannelSession.Services.Events.PerformEvent(trigger);

            TwitchChannelPointsCommand command = ChannelSession.Settings.TwitchChannelPointsCommands.FirstOrDefault(c => string.Equals(c.Name, redemption.reward.title, StringComparison.CurrentCultureIgnoreCase));
            if (command != null)
            {
                await command.Perform(user, extraSpecialIdentifiers: specialIdentifiers);
            }
        }

        private async void PubSub_OnWhisperReceived(object sender, PubSubWhisperEventModel packet)
        {
            if (!string.IsNullOrEmpty(packet.body))
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.from_id.ToString());
                await ChannelSession.Services.Chat.AddMessage(new TwitchChatMessageViewModel(packet, user));
            }
        }

        private async Task AddAlertChatMessage(UserViewModel user, string message)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, message, ChannelSession.Settings.ChatEventAlertsColorScheme));
            }
        }

        private void PubSub_OnPongReceived(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Twitch Pong Received");
            Task.Run(async () =>
            {
                await Task.Delay(1000 * 60 * 3);
                await this.pubSub.Ping();
            });
        }
    }
}
