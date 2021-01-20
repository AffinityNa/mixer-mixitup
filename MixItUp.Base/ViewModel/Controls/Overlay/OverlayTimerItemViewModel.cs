﻿using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayTimerItemViewModel : OverlayHTMLTemplateItemViewModelBase
    {
        public string TotalLengthString
        {
            get { return this.totalLength.ToString(); }
            set
            {
                this.totalLength = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int totalLength;

        public string SizeString
        {
            get { return this.size.ToString(); }
            set
            {
                this.size = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int size;

        public string Font
        {
            get { return this.font; }
            set
            {
                this.font = value;
                this.NotifyPropertyChanged();
            }
        }
        private string font;

        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.NotifyPropertyChanged();
            }
        }
        private string color;
        
        public string Format
        {
        	get {return this.format; }
        	set
        	{
        	this.format = value;
        	this.NotifyPropertyChanged();
        	}
        }
        private string format;

        public CustomCommand TimerCompleteCommand
        {
            get { return this.timerCompleteCommand; }
            set
            {
                this.timerCompleteCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsTimerCompleteCommandSet");
                this.NotifyPropertyChanged("IsTimerCompleteCommandNotSet");
            }
        }
        private CustomCommand timerCompleteCommand;

        public bool IsTimerCompleteCommandSet { get { return this.TimerCompleteCommand != null; } }
        public bool IsTimerCompleteCommandNotSet { get { return !this.IsTimerCompleteCommandSet; } }

        public OverlayTimerItemViewModel()
        {
            this.Font = "Arial";
            this.size = 24;
            this.format = "hh\\:mm\\:ss";
            this.HTML = OverlayTimerItemModel.HTMLTemplate;
        }

        public OverlayTimerItemViewModel(OverlayTimerItemModel item)
            : this()
        {
            this.totalLength = item.TotalLength;
            this.size = item.TextSize;
            this.Font = item.TextFont;
            this.Color = ColorSchemes.GetColorName(item.TextColor);
            this.format = item.TimeFormat;

            this.HTML = item.HTML;

            this.TimerCompleteCommand = item.TimerCompleteCommand;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.Color) && !string.IsNullOrEmpty(this.HTML) 
            && !string.IsNullOrEmpty(this.format) && this.size > 0 && this.totalLength > 0)
            {
                this.Color = ColorSchemes.GetColorCode(this.Color);

                return new OverlayTimerItemModel(this.HTML, this.totalLength, this.Color, this.Font, size, this.format, this.TimerCompleteCommand);
            }
            return null;
        }
    }
}
