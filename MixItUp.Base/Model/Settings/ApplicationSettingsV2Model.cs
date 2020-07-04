﻿using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class ApplicationSettingsV2Model
    {
        private const string ApplicationSettingsFileName = "ApplicationSettings.json";

        private const string OldApplicationSettingsFileName = "ApplicationSettings.xml";

        public static async Task<ApplicationSettingsV2Model> Load()
        {
            ApplicationSettingsV2Model settings = new ApplicationSettingsV2Model();
            if (ChannelSession.Services.FileService.FileExists(OldApplicationSettingsFileName))
            {
                try
                {
                    ApplicationSettingsV2Model oldSettings = await FileSerializerHelper.DeserializeFromFile<ApplicationSettingsV2Model>(OldApplicationSettingsFileName);
                    if (oldSettings != null)
                    {
                        await oldSettings.Save();
                    }
                    await ChannelSession.Services.FileService.DeleteFile(OldApplicationSettingsFileName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            if (ChannelSession.Services.FileService.FileExists(ApplicationSettingsFileName))
            {
                try
                {
                    settings = await FileSerializerHelper.DeserializeFromFile<ApplicationSettingsV2Model>(ApplicationSettingsFileName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return settings;
        }

        [JsonIgnore]
        public bool SettingsChangeRestartRequired { get; set; }

        [DataMember]
        public bool DiagnosticLogging { get; set; } = false;

        [DataMember]
        public bool PreviewProgram { get; set; } = false;

        [DataMember]
        public bool TestBuild { get; set; } = false;

        [DataMember]
        public Guid AutoLogInID { get; set; } = Guid.Empty;

        [DataMember]
        public string ColorScheme { get; set; } = "Indigo";

        [DataMember]
        public string BackgroundColor { get; set; } = "Light";

        [DataMember]
        public string FullThemeName { get; set; } = string.Empty;

        [DataMember]
        public double Top { get; set; }

        [DataMember]
        public double Left { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public bool IsMaximized { get; set; }

        [DataMember]
        public LanguageOptions LanguageOption { get; set; }

        [DataMember]
        public string BackupSettingsFilePath { get; set; }
        [DataMember]
        public Guid BackupSettingsToReplace { get; set; }

        [DataMember]
        public Guid SettingsToDelete { get; set; }

        [JsonIgnore]
        public bool IsDarkBackground { get { return this.BackgroundColor.Equals("Dark"); } }

        public ApplicationSettingsV2Model() { }

        public async Task Save()
        {
            try
            {
                await FileSerializerHelper.SerializeToFile(ApplicationSettingsFileName, this);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}