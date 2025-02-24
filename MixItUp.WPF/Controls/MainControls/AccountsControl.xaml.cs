﻿using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for AccountsControl.xaml
    /// </summary>
    public partial class AccountsControl : MainControlBase
    {
        private AccountsMainControlViewModel viewModel;

        public AccountsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new AccountsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            return Task.FromResult(0);
        }
    }
}
