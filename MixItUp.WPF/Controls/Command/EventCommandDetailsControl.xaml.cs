﻿using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using StreamingClient.Base.Util;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for EventCommandDetailsControl.xaml
    /// </summary>
    public partial class EventCommandDetailsControl : CommandDetailsControlBase
    {
        public ConstellationEventTypeEnum EventType { get; private set; }
        public OtherEventTypeEnum OtherEventType { get; private set; }

        private EventCommand command;

        public EventCommandDetailsControl(EventCommand command)
        {
            this.command = command;
            if (this.command.IsOtherEventType)
            {
                this.OtherEventType = this.command.OtherEventType;
            }
            else
            {
                this.EventType = this.command.EventType;
            }

            InitializeComponent();
        }

        public EventCommandDetailsControl(ConstellationEventTypeEnum eventType)
        {
            this.EventType = eventType;

            InitializeComponent();
        }

        public EventCommandDetailsControl(OtherEventTypeEnum otherEventType)
        {
            this.OtherEventType = otherEventType;

            InitializeComponent();
        }

        public EventCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            if (this.OtherEventType != OtherEventTypeEnum.None)
            {
                this.OtherEventTypeTextBox.Text = EnumHelper.GetEnumName(this.OtherEventType);
                this.OtherEventTypeTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                this.EventTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConstellationEventTypeEnum>();
                this.EventTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.EventType);
                this.EventTypeComboBox.Visibility = Visibility.Visible;
            }

            if (this.EventType.ToString().Contains("channel") || this.EventType.ToString().Contains("progression"))
            {
                this.EventIDTextBox.Text = ChannelSession.MixerUser.channel.id.ToString();
            }
            else
            {
                this.EventIDTextBox.Text = ChannelSession.MixerUser.id.ToString();
            }

            if (this.command != null)
            {
                this.UnlockedControl.Unlocked = this.command.Unlocked;
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (this.OtherEventTypeTextBox.Visibility == Visibility.Visible && string.IsNullOrEmpty(this.OtherEventTypeTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("An event type must be specified");
                return false;
            }

            if (this.EventTypeComboBox.Visibility == Visibility.Visible && this.EventTypeComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("An event type must be selected");
                return false;
            }

            if (this.EventIDTextBox.IsEnabled && string.IsNullOrEmpty(this.EventIDTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A name must be specified for this event type");
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                if (this.command == null)
                {
                    if (this.OtherEventTypeTextBox.Visibility == Visibility.Visible)
                    {
                        this.command = new EventCommand(EnumHelper.GetEnumValueFromString<OtherEventTypeEnum>(this.OtherEventTypeTextBox.Text), ChannelSession.MixerChannel.id.ToString());
                    }
                    else if (this.EventTypeComboBox.Visibility == Visibility.Visible)
                    {
                        ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);

                        ChannelAdvancedModel channel = null;
                        UserModel user = null;

                        if (eventType.ToString().Contains("channel") || eventType.ToString().Contains("progression"))
                        {
                            channel = await ChannelSession.MixerUserConnection.GetChannel(uint.Parse(this.EventIDTextBox.Text));
                            if (channel == null)
                            {
                                await MessageBoxHelper.ShowMessageDialog("Unable to find the channel for the specified username");
                                return null;
                            }
                        }
                        else if (eventType.ToString().Contains("user"))
                        {
                            user = await ChannelSession.MixerUserConnection.GetUser(uint.Parse(this.EventIDTextBox.Text));
                            if (user == null)
                            {
                                await MessageBoxHelper.ShowMessageDialog("Unable to find a user for the specified username");
                                return null;
                            }
                        }

                        if (channel != null)
                        {
                            this.command = new EventCommand(eventType, channel);
                        }
                        else if (user != null)
                        {
                            this.command = new EventCommand(eventType, user);
                        }
                        else
                        {
                            this.command = new EventCommand(eventType);
                        }
                    }

                    if (ChannelSession.Settings.EventCommands.Any(se => se.UniqueEventID.Equals(this.command.UniqueEventID)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("This event already exists");
                        return null;
                    }

                    ChannelSession.Settings.EventCommands.Add(this.command);
                }
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                return this.command;
            }
            return null;
        }

        private void EventTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.EventTypeComboBox.SelectedIndex >= 0)
            {
                ConstellationEventTypeEnum eventType = EnumHelper.GetEnumValueFromString<ConstellationEventTypeEnum>((string)this.EventTypeComboBox.SelectedItem);
            }
        }
    }
}
