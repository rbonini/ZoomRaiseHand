using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RaiseHandApp
{
    /// <summary>
    /// Interaction logic for RaiseLowerHand.xaml
    /// </summary>
    public partial class RaiseLowerHand : Window
    {

        public event Action<string> RequestEdit;

        public bool Answering = false;

        string defaultName;
        ParticipantSettings settings;
        uint userid;
        public int CurrentParticipants;
        public RaiseLowerHand(uint userid, string defaultName, ParticipantSettings settings)
        {
            InitializeComponent();

            this.settings = settings;
            this.userid = userid;

            this.CurrentParticipants = settings.Participants.Count;

            this.defaultName = defaultName;

            UpdateButtons();
        }

        public void UpdateCount(int count)
        {
            label_count.Content = $"Current Count: {count}";
        }

        public void UpdateParticipants(string defaultName,ParticipantSettings settings)
        {
            this.settings = settings;

            this.CurrentParticipants = settings.Participants.Count;

            this.defaultName = defaultName;

            this.Title = defaultName;

            UpdateButtons();

        }

        public  void ClearToggles()
        {
            foreach (ToggleButton button in stack1.Children)
            {
                button.IsChecked = false;
            }
            this.Answering = false;
        }


        private void UpdateButtons()
        {
            stack1.Children.Clear();

            if (settings.Participants.Any())
            {
                foreach (var hand in settings.Participants)
                {
                    var newbutton = new ToggleButton()
                    {
                        Content = hand.Name,
                        Width = 120,
                        Height = 50,
                        Margin = new Thickness(10, 5, 0, 10)
                    };

                    newbutton.Click += raise_Click;

                    stack1.Children.Add(newbutton);
                }
            }
            else
            {
                var newbutton = new ToggleButton()
                {
                    Content = defaultName,
                    Width = 120,
                    Height = 50,
                    Margin = new Thickness(10, 5, 0, 10)
                };

                newbutton.Click += raise_Click;

                stack1.Children.Add(newbutton);
            }
        }
        private void raise_Click(object sender, RoutedEventArgs e)
        {
            string text;

            var button = (ToggleButton)sender;
            if (this.CurrentParticipants>1)
            {
                text = $"{button.Content} ({CurrentParticipants})";
            }
            else
            {
                text = button.Content.ToString();
            }

            button.IsChecked = true;

            this.Answering = true;

            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, text , false);
            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().RaiseHand();
        }

        private void lowerHand_Click(object sender, RoutedEventArgs e)
        {
            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().LowerHand(userid);
        }

        private void editHands_Click(object sender, RoutedEventArgs e)
        {
            // Your logic
            RequestEdit?.Invoke("sample parameter");
        }
    }
}
