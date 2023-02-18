using System;
using System.ComponentModel;
using System.Windows;

namespace RaspCommander
{
    public partial class Progress : Window, INotifyPropertyChanged
    {
        public uint Items { get; } = 1;

        public uint ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = Math.Min(value, Items);
                OnPropertyChanged(nameof(ProgressValue));
                if (Items == ProgressValue) Close();
            }
        }
        private uint progressValue = 0;

        public string ProgressText
        {
            get => progressText;
            set
            {
                progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }
        private string progressText = Properties.Resources.PROGRESS_DEFAULT_TEXT;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Progress(uint? items)
        {
            if (items != null)
                Items = items.Value;
            else
                Bar.IsIndeterminate = true;

            InitializeComponent();
        }
    }
}
