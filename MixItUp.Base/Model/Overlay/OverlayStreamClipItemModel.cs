﻿using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayStreamClipItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public double Duration { get; set; }

        private string lastClipURL = null;

        public OverlayStreamClipItemModel() : base() { }

        public OverlayStreamClipItemModel(int width, int height, int volume, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.StreamClip, string.Empty, width, height)
        {
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.Effects = new OverlayItemEffectsModel(entranceAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, exitAnimation, 0);
        }

        [DataMember]
        public override string FullLink { get { return this.lastClipURL; } set { } }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } set { } }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override Task LoadTestData()
        {
            return Task.FromResult(0);
        }

        public override async Task Enable()
        {
            await base.Enable();
        }

        public override async Task Disable()
        {
            await base.Disable();
        }

        public override Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, StreamingPlatformTypeEnum platform)
        {
            return null;
        }
    }
}
