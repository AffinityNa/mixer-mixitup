﻿using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicEventCommandEditorControl.xaml
    /// </summary>
    public partial class BasicEventCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;

        private EventTypeEnum eventType;

        private EventCommand command;

        private ActionControlBase actionControl;

        public BasicEventCommandEditorControl(CommandWindow window, EventCommand command)
            : this(window, command.EventCommandType, BasicCommandTypeEnum.None)
        {
            this.window = window;
            this.command = command;
            this.eventType = this.command.EventCommandType;

            InitializeComponent();
        }

        public BasicEventCommandEditorControl(CommandWindow window, EventTypeEnum otherEventType, BasicCommandTypeEnum commandType)
        {
            this.window = window;
            this.eventType = otherEventType;
            this.commandType = commandType;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.EventTypeTextBlock.Text = EnumHelper.GetEnumName(this.eventType);
            if (this.command != null)
            {
                if (this.command.Actions.First() is ChatAction)
                {
                    this.actionControl = new ChatActionControl((ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl((SoundAction)this.command.Actions.First());
                }
            }
            else
            {
                if (this.commandType == BasicCommandTypeEnum.Chat)
                {
                    this.actionControl = new ChatActionControl(null);
                }
                else if (this.commandType == BasicCommandTypeEnum.Sound)
                {
                    this.actionControl = new SoundActionControl(null);
                }
            }

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAndClose(false);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAndClose(true);
        }

        private async Task SaveAndClose(bool isBasic)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                ActionBase action = this.actionControl.GetAction();
                if (action == null)
                {
                    if (this.actionControl is ChatActionControl)
                    {
                        await DialogHelper.ShowMessage("The chat message must not be empty");
                    }
                    else if (this.actionControl is SoundActionControl)
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.EmptySoundFilePath);
                    }
                    return;
                }

                EventCommand newCommand = new EventCommand(this.eventType);
                newCommand.IsBasic = isBasic;
                newCommand.Actions.Add(action);

                if (this.command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(this.command);
                }
                ChannelSession.Settings.EventCommands.Add(newCommand);

                await ChannelSession.SaveSettings();

                this.window.Close();

                if (!isBasic)
                {
                    await Task.Delay(250);
                    CommandWindow window = new CommandWindow(new EventCommandDetailsControl(newCommand));
                    window.CommandSaveSuccessfully += (sender, cmd) => this.CommandSavedSuccessfully(cmd);
                    window.Show();
                }
            });
        }
    }
}
