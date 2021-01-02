﻿using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameQueueListItemControl.xaml
    /// </summary>
    public partial class OverlayGameQueueListItemControl : OverlayItemControl
    {
        private OverlayGameQueueListItemViewModel viewModel;

        public OverlayGameQueueListItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueListItemViewModel();
        }

        public OverlayGameQueueListItemControl(OverlayGameQueueListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueListItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
