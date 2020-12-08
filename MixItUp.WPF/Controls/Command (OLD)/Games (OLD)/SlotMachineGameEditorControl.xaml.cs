﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for SlotMachineGameEditorControl.xaml
    /// </summary>
    public partial class SlotMachineGameEditorControl : GameEditorControlBase
    {
        private SlotMachineGameCommandEditorWindowViewModel viewModel;
        private SlotMachineGameCommand existingCommand;

        public SlotMachineGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new SlotMachineGameCommandEditorWindowViewModel(currency);
        }

        public SlotMachineGameEditorControl(SlotMachineGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new SlotMachineGameCommandEditorWindowViewModel(command);
        }

        public override async Task<bool> Validate()
        {
            if (!await this.CommandDetailsControl.Validate())
            {
                return false;
            }
            return await this.viewModel.Validate();
        }

        public override void SaveGameCommand()
        {
            this.viewModel.SaveGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers, this.CommandDetailsControl.GetRequirements());
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Slot Machine", "slots", CurrencyRequirementTypeEnum.MinimumOnly, 10);
            }
            await base.OnLoaded();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.viewModel.DeleteOutcomeCommand.Execute(button.DataContext);
        }

        private void AddOutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            RequirementViewModel requirements = this.CommandDetailsControl.GetRequirements();
            if (requirements.Currency != null)
            {
                CurrencyModel currency = requirements.Currency.GetCurrency();
                this.viewModel.AddOutcomeCommand.Execute(currency);
            }
        }
    }
}
