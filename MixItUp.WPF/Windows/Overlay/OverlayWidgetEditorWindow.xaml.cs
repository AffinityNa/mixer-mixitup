﻿using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.Base.ViewModel.Window.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetEditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetEditorWindow : LoadingWindowBase
    {
        private OverlayWidgetEditorWindowViewModel viewModel;

        private Dictionary<OverlayItemModelTypeEnum, OverlayItemControl> overlayTypeEditors = new Dictionary<OverlayItemModelTypeEnum, OverlayItemControl>();
        private OverlayItemControl overlayTypeEditor;

        public OverlayWidgetEditorWindow(OverlayWidgetModel widget)
            : this()
        {
            this.viewModel = new OverlayWidgetEditorWindowViewModel(widget);
        }

        public OverlayWidgetEditorWindow()
        {
            InitializeComponent();

            this.viewModel = new OverlayWidgetEditorWindowViewModel();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            this.viewModel.OverlayTypeSelected += ViewModel_OverlayTypeSelected;
            await this.viewModel.OnLoaded();

            if (this.viewModel.OverlayWidget != null)
            {
                this.ItemPosition.SetPosition(this.viewModel.OverlayWidget.Item.Position);

                if (this.viewModel.OverlayWidget.Item is OverlayHTMLItemModel) { this.SetOverlayWidgetEditorControl(new OverlayHTMLItemControl((OverlayHTMLItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayChatMessagesListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayChatMessagesListItemControl((OverlayChatMessagesListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayClipPlaybackItemModel) { this.SetOverlayWidgetEditorControl(new OverlayClipPlaybackItemControl((OverlayClipPlaybackItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEventListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayEventListItemControl((OverlayEventListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayGameQueueListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayGameQueueListItemControl((OverlayGameQueueListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayProgressBarItemModel) { this.SetOverlayWidgetEditorControl(new OverlayProgressBarItemControl((OverlayProgressBarItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayImageItemModel) { this.SetOverlayWidgetEditorControl(new OverlayImageItemControl((OverlayImageItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayLeaderboardListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayLeaderboardListItemControl((OverlayLeaderboardListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayStreamBossItemModel) { this.SetOverlayWidgetEditorControl(new OverlayStreamBossItemControl((OverlayStreamBossItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTextItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTextItemControl((OverlayTextItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTickerTapeListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTickerTapeListItemControl((OverlayTickerTapeListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTimerItemControl((OverlayTimerItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerTrainItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTimerTrainItemControl((OverlayTimerTrainItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayVideoItemModel) { this.SetOverlayWidgetEditorControl(new OverlayVideoItemControl((OverlayVideoItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayWebPageItemModel) { this.SetOverlayWidgetEditorControl(new OverlayWebPageItemControl((OverlayWebPageItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayYouTubeItemModel) { this.SetOverlayWidgetEditorControl(new OverlayYouTubeItemControl((OverlayYouTubeItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEndCreditsItemModel) { this.SetOverlayWidgetEditorControl(new OverlayEndCreditsItemControl((OverlayEndCreditsItemModel)this.viewModel.OverlayWidget.Item)); }
            }
            else
            {
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ChatMessages, new OverlayChatMessagesListItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ClipPlayback, new OverlayClipPlaybackItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.EventList, new OverlayEventListItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.GameQueue, new OverlayGameQueueListItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ProgressBar, new OverlayProgressBarItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.HTML, new OverlayHTMLItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Image, new OverlayImageItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Leaderboard, new OverlayLeaderboardListItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.StreamBoss, new OverlayStreamBossItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Text, new OverlayTextItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.TickerTape, new OverlayTickerTapeListItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Timer, new OverlayTimerItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.TimerTrain, new OverlayTimerTrainItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Video, new OverlayVideoItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.WebPage, new OverlayWebPageItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.YouTube, new OverlayYouTubeItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.EndCredits, new OverlayEndCreditsItemControl());
            }
        }

        private void ViewModel_OverlayTypeSelected(object sender, OverlayTypeListing overlayType)
        {
            this.SetOverlayWidgetEditorControl(this.overlayTypeEditors[this.viewModel.SelectedOverlayType.Type]);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation((System.Func<Task>)(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    OverlayItemPositionModel position = this.ItemPosition.GetPosition();
                    if (position == null)
                    {
                        await DialogHelper.ShowMessage("A valid position for this overlay widget must be selected");
                        return;
                    }

                    OverlayItemModelBase overlayItem = overlayTypeEditor.GetItem();
                    if (overlayItem == null)
                    {
                        await DialogHelper.ShowMessage("There are missing details for the overlay item");
                        return;
                    }

                    overlayItem.Position = position;

                    OverlayWidgetModel widget = new OverlayWidgetModel(this.viewModel.Name, this.viewModel.SelectedOverlayEndpoint, overlayItem, (int)this.viewModel.RefreshTime);
                    if (this.viewModel.OverlayWidget != null)
                    {
                        await this.viewModel.OverlayWidget.Disable();
                        ChannelSession.Settings.OverlayWidgets.Remove(this.viewModel.OverlayWidget);
                        overlayItem.ID = this.viewModel.OverlayWidget.Item.ID;
                    }

                    if (widget.Item is OverlayEndCreditsItemModel)
                    {
                        widget.IsEnabled = false;
                    }

                    ChannelSession.Settings.OverlayWidgets.Add(widget);

                    this.Close();
                }
            }));
        }

        private void SetOverlayWidgetEditorControl(OverlayItemControl overlayTypeEditor)
        {
            this.MainContentControl.Content = this.overlayTypeEditor = overlayTypeEditor;

            OverlayItemViewModelBase itemViewModel = this.overlayTypeEditor.GetViewModel();
            this.viewModel.SupportsRefreshUpdating = itemViewModel.SupportsRefreshUpdating;
        }
    }
}
