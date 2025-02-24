﻿using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Util
{
    public static class GlobalEvents
    {
        public static event EventHandler OnRestartRequested;
        public static void RestartRequested()
        {
            if (GlobalEvents.OnRestartRequested != null)
            {
                GlobalEvents.OnRestartRequested(null, new EventArgs());
            }
        }

        public static event EventHandler<bool> OnMainMenuStateChanged;
        public static void MainMenuStateChained(bool state)
        {
            if (GlobalEvents.OnMainMenuStateChanged != null)
            {
                GlobalEvents.OnMainMenuStateChanged(null, state);
            }
        }

        public static event EventHandler<string> OnShowMessageBox;
        public static void ShowMessageBox(string message)
        {
            if (GlobalEvents.OnShowMessageBox != null)
            {
                GlobalEvents.OnShowMessageBox(null, message);
            }
        }

        public static event EventHandler<string> OnServiceDisconnect;
        public static void ServiceDisconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceDisconnect != null)
            {
                GlobalEvents.OnServiceDisconnect(null, serviceName);
            }
        }

        public static event EventHandler<string> OnServiceReconnect;
        public static void ServiceReconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceReconnect != null)
            {
                GlobalEvents.OnServiceReconnect(null, serviceName);
            }
        }

        public static event EventHandler OnChatVisualSettingsChanged;
        public static void ChatVisualSettingsChanged()
        {
            if (GlobalEvents.OnChatVisualSettingsChanged != null)
            {
                GlobalEvents.OnChatVisualSettingsChanged(null, new EventArgs());
            }
        }

        public static event EventHandler<ChatMessageViewModel> OnChatMessageReceived;
        public static void ChatMessageReceived(ChatMessageViewModel chatMessage)
        {
            if (GlobalEvents.OnChatMessageReceived != null)
            {
                GlobalEvents.OnChatMessageReceived(null, chatMessage);
            }
        }

        public static event EventHandler<Guid> OnChatMessageDeleted;
        public static void ChatMessageDeleted(Guid messageID)
        {
            if (GlobalEvents.OnChatMessageDeleted != null)
            {
                GlobalEvents.OnChatMessageDeleted(null, messageID);
            }
        }

        public static event EventHandler OnGameQueueUpdated;
        public static void GameQueueUpdated()
        {
            if (GlobalEvents.OnGameQueueUpdated != null)
            {
                GlobalEvents.OnGameQueueUpdated(null, new EventArgs());
            }
        }

        public static event EventHandler<UserQuoteViewModel> OnQuoteAdded;
        public static void QuoteAdded(UserQuoteViewModel quote)
        {
            if (GlobalEvents.OnQuoteAdded != null)
            {
                GlobalEvents.OnQuoteAdded(null, quote);
            }
        }

        public static event EventHandler<UserViewModel> OnFollowOccurred;
        public static void FollowOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnFollowOccurred != null)
            {
                GlobalEvents.OnFollowOccurred(null, user);
            }
        }

        public static event EventHandler<UserViewModel> OnSubscribeOccurred;
        public static void SubscribeOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnSubscribeOccurred != null)
            {
                GlobalEvents.OnSubscribeOccurred(null, user);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnResubscribeOccurred;
        public static void ResubscribeOccurred(Tuple<UserViewModel, int> user)
        {
            if (GlobalEvents.OnResubscribeOccurred != null)
            {
                GlobalEvents.OnResubscribeOccurred(null, user);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, UserViewModel>> OnSubscriptionGiftedOccurred;
        public static void SubscriptionGiftedOccurred(UserViewModel gifter, UserViewModel receiver)
        {
            if (GlobalEvents.OnSubscriptionGiftedOccurred != null)
            {
                GlobalEvents.OnSubscriptionGiftedOccurred(null, new Tuple<UserViewModel, UserViewModel>(gifter, receiver));
            }
        }

        public static event EventHandler<UserViewModel> OnHostOccurred;
        public static void HostOccurred(UserViewModel user)
        {
            if (GlobalEvents.OnHostOccurred != null)
            {
                GlobalEvents.OnHostOccurred(null, user);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnRaidOccurred;
        public static void RaidOccurred(UserViewModel user, int viewers)
        {
            if (GlobalEvents.OnRaidOccurred != null)
            {
                GlobalEvents.OnRaidOccurred(null, new Tuple<UserViewModel, int>(user, viewers));
            }
        }

        public static event EventHandler<UserDonationModel> OnDonationOccurred;
        public static void DonationOccurred(UserDonationModel donation)
        {
            if (GlobalEvents.OnDonationOccurred != null)
            {
                GlobalEvents.OnDonationOccurred(null, donation);
            }
        }

        public static event EventHandler<TwitchUserBitsCheeredModel> OnBitsOccurred;
        public static void BitsOccurred(TwitchUserBitsCheeredModel bitsCheer)
        {
            if (GlobalEvents.OnBitsOccurred != null)
            {
                GlobalEvents.OnBitsOccurred(null, bitsCheer);
            }
        }

        public static event EventHandler<Tuple<UserViewModel, int>> OnStreamlootsPurchaseOccurred;
        public static void StreamlootsPurchaseOccurred(Tuple<UserViewModel, int> purchase)
        {
            if (GlobalEvents.OnStreamlootsPurchaseOccurred != null)
            {
                GlobalEvents.OnStreamlootsPurchaseOccurred(null, purchase);
            }
        }

        public static event EventHandler<bool> OnGiveawaysChangedOccurred;
        public static void GiveawaysChangedOccurred(bool usersUpdated = false)
        {
            if (GlobalEvents.OnGiveawaysChangedOccurred != null)
            {
                GlobalEvents.OnGiveawaysChangedOccurred(null, usersUpdated);
            }
        }

        public static event EventHandler OnRedemptionStorePurchasesUpdated;
        public static void RedemptionStorePurchasesUpdated()
        {
            if (GlobalEvents.OnRedemptionStorePurchasesUpdated != null)
            {
                GlobalEvents.OnRedemptionStorePurchasesUpdated(null, new EventArgs());
            }
        }

        public static event EventHandler<Twitch.Base.Models.NewAPI.Clips.ClipModel> OnTwitchClipCreated;
        public static void TwitchClipCreated(Twitch.Base.Models.NewAPI.Clips.ClipModel clip)
        {
            if (GlobalEvents.OnTwitchClipCreated != null)
            {
                GlobalEvents.OnTwitchClipCreated(null, clip);
            }
        }
    }
}
