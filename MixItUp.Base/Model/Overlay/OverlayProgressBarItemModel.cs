﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayProgressBarItemTypeEnum
    {
        Followers = 0,
        Subscribers = 1,
        Donations = 2,
        [Obsolete]
        Sparks = 3,
        [Obsolete]
        Milestones = 4,
        Custom = 5,
        [Obsolete]
        Embers = 6,
        Bits = 7,
    }

    [DataContract]
    public class OverlayProgressBarItemModel : OverlayHTMLTemplateItemModelBase
    {
        public const string GoalReachedCommandName = "Goal Reached";

        public const string HTMLTemplate =
            @"<div style=""position: absolute; background-color: {BACKGROUND_COLOR}; width: {BAR_WIDTH}px; height: {BAR_HEIGHT}px; transform: translate(-50%, -50%);"">
                <div style=""position: absolute; background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}px; height: {BAR_HEIGHT}px;""></div>
            </div>
            <p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{AMOUNT} ({PERCENTAGE}%)</p>";

        [DataMember]
        public OverlayProgressBarItemTypeEnum ProgressBarType { get; set; }

        [DataMember]
        public double StartAmount { get; set; }
        [DataMember]
        public double CurrentAmount { get; set; }
        [DataMember]
        public double GoalAmount { get; set; }

        [DataMember]
        public string CurrentAmountCustom { get; set; }
        [DataMember]
        public string GoalAmountCustom { get; set; }

        [DataMember]
        public int ResetAfterDays { get; set; }

        [DataMember]
        public string ProgressColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public CustomCommand GoalReachedCommand { get; set; }

        [DataMember]
        private DateTimeOffset LastReset { get; set; }
        [DataMember]
        private bool GoalReached { get; set; }

        public OverlayProgressBarItemModel() : base() { }

        public OverlayProgressBarItemModel(string htmlText, OverlayProgressBarItemTypeEnum progressBarType, double startAmount, double goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.StartAmount = this.CurrentAmount = startAmount;
            this.GoalAmount = goalAmount;
        }

        public OverlayProgressBarItemModel(string htmlText, OverlayProgressBarItemTypeEnum progressBarType, string currentAmountCustom, string goalAmountCustom, int resetAfterDays,
            string progressColor, string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.CurrentAmountCustom = currentAmountCustom;
            this.GoalAmountCustom = goalAmountCustom;
        }

        public OverlayProgressBarItemModel(string htmlText, OverlayProgressBarItemTypeEnum progressBarType, int resetAfterDays, string progressColor, string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : base(OverlayItemModelTypeEnum.ProgressBar, htmlText)
        {
            this.ProgressBarType = progressBarType;
            this.ResetAfterDays = resetAfterDays;
            this.ProgressColor = progressColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.GoalReachedCommand = goalReachedCommand;
            this.LastReset = DateTimeOffset.Now;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return this.ProgressBarType == OverlayProgressBarItemTypeEnum.Custom; } }

        public override async Task Enable()
        {
            if (this.ResetAfterDays > 0 && this.LastReset.TotalDaysFromNow() > this.ResetAfterDays)
            {
                this.CurrentAmount = 0;
                this.LastReset = DateTimeOffset.Now;
            }

            if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Followers)
            {
                this.CurrentAmount = ChannelSession.TwitchChannelV5.followers;

                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            else if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Subscribers)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            else if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            else if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Bits)
            {
                GlobalEvents.OnBitsOccurred += GlobalEvents_OnBitsOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnBitsOccurred -= GlobalEvents_OnBitsOccurred;

            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, StreamingPlatformTypeEnum platform)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["BAR_WIDTH"] = this.Width.ToString();
            replacementSets["BAR_HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = ((3 * this.Height) / 4).ToString();

            double amount = this.CurrentAmount;
            double goal = this.GoalAmount;

            if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Followers)
            {
                if (this.StartAmount > 0)
                {
                    amount = this.CurrentAmount - this.StartAmount;
                }
            }
            else if (this.ProgressBarType == OverlayProgressBarItemTypeEnum.Custom)
            {
                if (!string.IsNullOrEmpty(this.CurrentAmountCustom))
                {
                    string customAmount = await this.ReplaceStringWithSpecialModifiers(this.CurrentAmountCustom, user, arguments, extraSpecialIdentifiers, platform);
                    if (double.TryParse(customAmount, out amount))
                    {
                        if (this.StartAmount <= 0)
                        {
                            this.StartAmount = amount;
                        }
                        this.CurrentAmount = amount;
                    }
                }

                if (!string.IsNullOrEmpty(this.GoalAmountCustom))
                {
                    string customGoal = await this.ReplaceStringWithSpecialModifiers(this.GoalAmountCustom, user, arguments, extraSpecialIdentifiers, platform);
                    if (double.TryParse(customGoal, out goal))
                    {
                        this.GoalAmount = goal;
                    }
                }
            }

            double percentage = 0.0;
            if (goal != 0.0)
            {
                percentage = (amount / goal);
                if (!this.GoalReached && percentage >= 1.0)
                {
                    this.GoalReached = true;
                    if (this.GoalReachedCommand != null)
                    {
                        await this.GoalReachedCommand.Perform();
                    }
                }
            }

            replacementSets["AMOUNT"] = amount.ToString();
            replacementSets["GOAL"] = goal.ToString();
            replacementSets["PERCENTAGE"] = ((int)(percentage * 100)).ToString();
            if (goal > 0)
            {
                int progressWidth = (int)(((double)this.Width) * percentage);
                progressWidth = MathHelper.Clamp(progressWidth, 0, this.Width);
                replacementSets["PROGRESS_WIDTH"] = progressWidth.ToString();
            }

            return replacementSets;
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.AddAmount(1); }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.AddAmount(1); }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.AddAmount(1); }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e) { this.AddAmount(1); }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.AddAmount(donation.Amount); }

        private void GlobalEvents_OnBitsOccurred(object sender, TwitchUserBitsCheeredModel e) { this.AddAmount(e.Amount); }

        private void AddAmount(double amount)
        {
            this.CurrentAmount += amount;
            this.SendUpdateRequired();
        }
    }
}
