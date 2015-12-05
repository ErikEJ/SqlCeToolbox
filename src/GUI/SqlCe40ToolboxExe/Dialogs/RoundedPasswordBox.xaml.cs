using System.Windows.Controls;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class RoundedPasswordBox : UserControl
    {
        public RoundedPasswordBox()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return TheTextBox.Password; }
            set { TheTextBox.Password = value; }
        }

        public PasswordBox TextBox
        {
            get { return TheTextBox; }
            set { TheTextBox= value; }
        }
    }
}