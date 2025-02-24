﻿using MixItUp.Base;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : LoadingWindowBase
    {
        private MixItUpUpdateModel currentUpdate;
        private bool updateFound = false;

        private ObservableCollection<SettingsV2Model> streamerSettings = new ObservableCollection<SettingsV2Model>();

        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            GlobalEvents.OnShowMessageBox += GlobalEvents_OnShowMessageBox;

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            this.ExistingStreamerComboBox.ItemsSource = streamerSettings;

            await this.CheckForUpdates();

            foreach (SettingsV2Model setting in (await ChannelSession.Services.Settings.GetAllSettings()).OrderBy(s => s.Name))
            {
                if (setting.IsStreamer)
                {
                    this.streamerSettings.Add(setting);
                }
            }

            if (this.streamerSettings.Count > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                this.streamerSettings.Add(new SettingsV2Model() { ID = Guid.Empty, Name = MixItUp.Base.Resources.NewStreamer });
                if (this.streamerSettings.Count() == 2)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            if (ChannelSession.AppSettings.AutoLogInID != Guid.Empty)
            {
                var allSettings = this.streamerSettings.ToList();
                SettingsV2Model autoLogInSettings = allSettings.FirstOrDefault(s => s.ID == ChannelSession.AppSettings.AutoLogInID);
                if (autoLogInSettings != null)
                {
                    await Task.Delay(5000);

                    if (!updateFound)
                    {
                        if (await this.ExistingSettingLogin(autoLogInSettings))
                        {
                            LoadingWindowBase newWindow = null;
                            if (ChannelSession.Settings.ReRunWizard)
                            {
                                newWindow = new NewUserWizardWindow();
                            }
                            else
                            {
                                newWindow = new MainWindow();
                            }
                            ShowMainWindow(newWindow);
                            this.Hide();
                            this.Close();
                            return;
                        }
                    }
                }
            }

            await base.OnLoaded();
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (this.ExistingStreamerComboBox.Visibility == Visibility.Visible)
                {
                    if (this.ExistingStreamerComboBox.SelectedIndex >= 0)
                    {
                        SettingsV2Model setting = (SettingsV2Model)this.ExistingStreamerComboBox.SelectedItem;
                        if (setting.ID == Guid.Empty)
                        {
                            await this.NewStreamerLogin();
                        }
                        else
                        {
                            if (await this.ExistingSettingLogin(setting))
                            {
                                LoadingWindowBase newWindow = null;
                                if (ChannelSession.Settings.ReRunWizard)
                                {
                                    newWindow = new NewUserWizardWindow();
                                }
                                else
                                {
                                    newWindow = new MainWindow();
                                }
                                ShowMainWindow(newWindow);
                                this.Hide();
                                this.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.LoginErrorNoStreamerAccount);
                    }
                }
                else
                {
                    await this.NewStreamerLogin();
                }
            });
        }

        private async Task CheckForUpdates()
        {
            this.currentUpdate = await ChannelSession.Services.MixItUpService.GetLatestUpdate();
            if (this.currentUpdate != null && this.currentUpdate.SystemVersion > Assembly.GetEntryAssembly().GetName().Version)
            {
                updateFound = true;
                UpdateWindow window = new UpdateWindow(this.currentUpdate);
                window.Show();
            }
        }

        private async Task<bool> ExistingSettingLogin(SettingsV2Model setting)
        {
            Result result = await ChannelSession.ConnectUser(setting);
            if (result.Success)
            {
                if (await ChannelSession.InitializeSession())
                {
                    return true;
                }
            }
            else
            {
                await DialogHelper.ShowMessage(result.Message);
            }
            return false;
        }

        private async Task NewStreamerLogin()
        {
            if (await this.ShowLicenseAgreement())
            {
                ShowMainWindow(new NewUserWizardWindow());
                this.Hide();
                this.Close();
            }
        }

        private async void GlobalEvents_OnShowMessageBox(object sender, string message)
        {
            await this.RunAsyncOperation(async () =>
            {
                await DialogHelper.ShowMessage(message);
            });
        }

        private Task<bool> ShowLicenseAgreement()
        {
            LicenseAgreementWindow window = new LicenseAgreementWindow();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            window.Owner = Application.Current.MainWindow;
            window.Closed += (s, a) => task.SetResult(window.Accepted);
            window.Show();
            window.Focus();
            return task.Task;
        }

        public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}