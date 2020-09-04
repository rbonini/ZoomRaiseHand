using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace RaiseHandApp
{
    /// <summary>
    /// Interaction logic for RaiseLowerHand.xaml
    /// </summary>
    public partial class RaiseLowerHand : Window
    {

        ParticipantSettings settings;
        uint userid;

        int participants;
        public RaiseLowerHand(uint userid, ParticipantSettings settings)
        {
            InitializeComponent();

            this.settings = settings;
            this.userid = userid;

            this.participants = settings.Participants.Count;

            foreach (var hand in settings.Participants)
            {
                var newbutton = new Button()
                {
                    Content = hand.Name,
                    Width = 120,
                    Height = 50,
                    Margin = new Thickness(10,5,0,10)
                };

                newbutton.Click += raise_Click;

                stack1.Children.Add(newbutton);
            }

        }

        public void UpdateCount(int count)
        {
            label_count.Content = $"Current Count: {count}";
        }

        private void raise_Click(object sender, RoutedEventArgs e)
        {
            string text;

            var button = (Button)sender;
            if (this.participants>1)
            {
                text = $"{button.Content} ({participants})";
            }
            else
            {
                text = button.Content.ToString();
            }
            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, text , false);
            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().RaiseHand();
        }

        private void lowerHand_Click(object sender, RoutedEventArgs e)
        {
            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().LowerHand(userid);
        }
    }
}
