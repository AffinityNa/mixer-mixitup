﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Games;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for WordScrambleGameEditorControl.xaml
    /// </summary>
    public partial class WordScrambleGameEditorControl : GameEditorControlBase
    {
        private WordScrambleGameEditorControlViewModel viewModel;
        private WordScrambleGameCommand existingCommand;

        public WordScrambleGameEditorControl(CurrencyModel currency)
        {
            InitializeComponent();

            this.viewModel = new WordScrambleGameEditorControlViewModel(currency);
        }

        public WordScrambleGameEditorControl(WordScrambleGameCommand command)
        {
            InitializeComponent();

            this.existingCommand = command;
            this.viewModel = new WordScrambleGameEditorControlViewModel(command);
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
                this.CommandDetailsControl.SetDefaultValues("Word Scramble", "scramble", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
            }
            await base.OnLoaded();
        }
    }
}
