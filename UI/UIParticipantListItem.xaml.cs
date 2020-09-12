using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaiseHandApp.UI
{
    /// <summary>
    /// Interaction logic for UIParticipantListItem.xaml
    /// </summary>
    public partial class UIParticipantListItem : UserControl
    {
        public string ParticpantName { get; private set; }

        public event EventHandler<ParticpantEventArgs> RemoveClicked;

        public UIParticipantListItem(string name)
        {
            InitializeComponent();

            ParticpantName = name;

            this.particpantLabel.Content = name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveClicked != null)
            {
                RemoveClicked(this,new ParticpantEventArgs(this.ParticpantName));
            }
        }
    }
}
