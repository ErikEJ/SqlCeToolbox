using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class CheckListItem : INotifyPropertyChanged
    {
        private bool _isChecked;

        public string Label { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public string Tag { get; set; }

        public override string ToString()
        {
            return Label;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
