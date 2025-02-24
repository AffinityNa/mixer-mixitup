﻿using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Settings.Generic;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class ChatSettingsControlViewModel : UIViewModelBase
    {
        private Dictionary<string, int> fontSizes = new Dictionary<string, int>() { { "Normal", 13 }, { "Large", 16 }, { "XLarge", 20 }, { "XXLarge", 24 }, };

        public GenericComboBoxSettingsOptionControlViewModel<string> FontSize { get; set; }
        public GenericToggleSettingsOptionControlViewModel AddSeparatorsBetweenMessages { get; set; }
        public GenericToggleSettingsOptionControlViewModel UseAlternatingBackgroundColors { get; set; }

        public GenericToggleSettingsOptionControlViewModel ShowLatestChatMessagesAtTop { get; set; }
        public GenericToggleSettingsOptionControlViewModel ShowMessageTimestamp { get; set; }
        public GenericToggleSettingsOptionControlViewModel TrackWhisperNumbers { get; set; }

        public GenericToggleSettingsOptionControlViewModel HideViewerChatterCount { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideChatUserList { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideDeletedMessages { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideBotMessages { get; set; }

        public GenericToggleSettingsOptionControlViewModel ShowBetterTTVEmotes { get; set; }
        public GenericToggleSettingsOptionControlViewModel ShowFrankerFaceZEmotes { get; set; }

        public GenericToggleSettingsOptionControlViewModel HideUserAvatar { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserRoleBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserSubscriberBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserSpecialtyBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel UseCustomUsernameColors { get; set; }
        public ObservableCollection<GenericColorComboBoxSettingsOptionControlViewModel> CustomUsernameColorsList { get; set; } = new ObservableCollection<GenericColorComboBoxSettingsOptionControlViewModel>();

        public ChatSettingsControlViewModel()
        {
            this.FontSize = new GenericComboBoxSettingsOptionControlViewModel<string>(MixItUp.Base.Resources.FontSize, this.fontSizes.Keys, this.fontSizes.FirstOrDefault(f => f.Value == ChannelSession.Settings.ChatFontSize).Key,
                (value) =>
                {
                    if (this.fontSizes.ContainsKey(value))
                    {
                        ChannelSession.Settings.ChatFontSize = this.fontSizes[value];
                        GlobalEvents.ChatVisualSettingsChanged();
                    }
                });
            this.AddSeparatorsBetweenMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AddSeparatorsBetweenMessages, ChannelSession.Settings.AddSeparatorsBetweenMessages,
                (value) =>
                {
                    ChannelSession.Settings.AddSeparatorsBetweenMessages = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.UseAlternatingBackgroundColors = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UseAlternatingBackgroundColors, ChannelSession.Settings.UseAlternatingBackgroundColors,
                (value) =>
                {
                    ChannelSession.Settings.UseAlternatingBackgroundColors = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            this.ShowLatestChatMessagesAtTop = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowLatestChatMessagesAtTopInsteadOfBottom, ChannelSession.Settings.LatestChatAtTop,
                (value) => { ChannelSession.Settings.LatestChatAtTop = value; });
            this.ShowMessageTimestamp = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowMessageTimestamp, ChannelSession.Settings.ShowChatMessageTimestamps,
                (value) => { ChannelSession.Settings.ShowChatMessageTimestamps = value; });
            this.TrackWhisperNumbers = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.TrackWhispererNumbers, ChannelSession.Settings.TrackWhispererNumber,
                (value) => { ChannelSession.Settings.TrackWhispererNumber = value; });

            this.HideViewerChatterCount = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideViewerAndChatterNumbers, ChannelSession.Settings.HideViewerAndChatterNumbers,
                (value) => { ChannelSession.Settings.HideViewerAndChatterNumbers = value; });
            this.HideChatUserList = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideChatUserList, ChannelSession.Settings.HideChatUserList,
                (value) => { ChannelSession.Settings.HideChatUserList = value; });
            this.HideDeletedMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideDeletedMessages, ChannelSession.Settings.HideDeletedMessages,
                (value) => { ChannelSession.Settings.HideDeletedMessages = value; });
            this.HideBotMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideBotMessages, ChannelSession.Settings.HideBotMessages,
                (value) => { ChannelSession.Settings.HideBotMessages = value; });

            this.ShowBetterTTVEmotes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowBetterTTVEmotes, ChannelSession.Settings.ShowBetterTTVEmotes,
                (value) => { ChannelSession.Settings.ShowBetterTTVEmotes = value; });
            this.ShowFrankerFaceZEmotes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowFrankerFaceZEmotes, ChannelSession.Settings.ShowFrankerFaceZEmotes,
                (value) => { ChannelSession.Settings.ShowFrankerFaceZEmotes = value; });

            this.HideUserAvatar = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserAvatar, ChannelSession.Settings.HideUserAvatar,
                (value) =>
                {
                    ChannelSession.Settings.HideUserAvatar = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserRoleBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserRoleBadge, ChannelSession.Settings.HideUserRoleBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserRoleBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserSubscriberBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserSubscriberBadge, ChannelSession.Settings.HideUserSubscriberBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserSubscriberBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserSpecialtyBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserSpecialtyBadge, ChannelSession.Settings.HideUserSpecialtyBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserSpecialtyBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            this.UseCustomUsernameColors = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UseCustomUsernameColors, ChannelSession.Settings.UseCustomUsernameColors,
                (value) =>
                {
                    ChannelSession.Settings.UseCustomUsernameColors = value;
                    this.EnableDisableUsernameColors();
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            List<UserRoleEnum> roles = new List<UserRoleEnum>(EnumHelper.GetEnumList<UserRoleEnum>());
            roles.Remove(UserRoleEnum.Banned);
            roles.Remove(UserRoleEnum.Custom);
            foreach (UserRoleEnum role in roles.OrderBy(r => r))
            {
                string name = EnumHelper.GetEnumName(role);
                name = MixItUp.Base.Resources.ResourceManager.GetString(name) ?? name;
                this.CustomUsernameColorsList.Add(new GenericColorComboBoxSettingsOptionControlViewModel(name,
                    ChannelSession.Settings.CustomUsernameColors.ContainsKey(role) ? ChannelSession.Settings.CustomUsernameColors[role] : null,
                    (value) =>
                    {
                        if (!string.IsNullOrEmpty(value) && !value.Equals(GenericColorComboBoxSettingsOptionControlViewModel.NoneOption))
                        {
                            ChannelSession.Settings.CustomUsernameColors[role] = value;
                        }
                        else
                        {
                            ChannelSession.Settings.CustomUsernameColors.Remove(role);
                        }
                        GlobalEvents.ChatVisualSettingsChanged();
                    }));
            }

            foreach (GenericColorComboBoxSettingsOptionControlViewModel colorOption in this.CustomUsernameColorsList)
            {
                colorOption.AddNoneOption();
            }

            this.EnableDisableUsernameColors();
        }

        private void EnableDisableUsernameColors()
        {
            foreach (GenericColorComboBoxSettingsOptionControlViewModel colorOption in this.CustomUsernameColorsList)
            {
                colorOption.Enabled = ChannelSession.Settings.UseCustomUsernameColors;
            }
        }
    }
}
