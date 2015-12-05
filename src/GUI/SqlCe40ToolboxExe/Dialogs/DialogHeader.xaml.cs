using System.Windows.Controls;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class DialogHeader : UserControl
    {
        public DialogHeader()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
        }
    }
}