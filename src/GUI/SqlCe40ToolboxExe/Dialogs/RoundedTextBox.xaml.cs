using System.Windows.Controls;

namespace ErikEJ.SqlCeToolbox.Controls
{
    public partial class RoundedTextBox : UserControl
    {
        public RoundedTextBox()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return TheTextBox.Text; }
            set { TheTextBox.Text = value; }
        }

        public TextBox TextBox
        {
            get { return TheTextBox; }
            set { TheTextBox= value; }
        }
    }
}
