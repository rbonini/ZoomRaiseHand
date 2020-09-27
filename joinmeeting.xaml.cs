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
using System.Windows.Shapes;
using System.ComponentModel; // CancelEventArgs
using ZOOM_SDK_DOTNET_WRAP;
using System.Data.OleDb;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;
using RaiseHandApp.UI;

namespace RaiseHandApp
{
    /// <summary>
    /// Interaction logic for start_join_meeting.xaml
    /// </summary>
    public partial class joinmeeting : Window
    {
        uint userid;
        string userName;
        int count = 0;
        ParticipantSettings settings;

        List<string> countexcludednames = new List<string> { "Admin Admin", "KHConf" };
        
        RaiseLowerHand raisescreen;
        public joinmeeting()
        {
            InitializeComponent();

            var settingstrings = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Particpants.json");

            this.settings = JsonConvert.DeserializeObject<ParticipantSettings>(settingstrings);

            foreach( var name in this.settings.Participants)
            {
                var control = new UIParticipantListItem(name.Name);

                control.RemoveClicked += Control_RemoveClicked;

                this.participantStack.Children.Add(control);
            }

            feedback.Content = "Settings Loaded";
        }

        private void Control_RemoveClicked(object sender, ParticpantEventArgs e)
        {
            var item = this.settings.Participants.First(g => g.Name == e.Name);

            this.settings.Participants.Remove(item);

            this.participantStack.Children.Remove((sender as UIElement));
        }

        //ZOOM_SDK_DOTNET_WRAP.onMeetingStatusChanged
        public void onMeetingStatusChanged(MeetingStatus status, int iResult)
        {
            switch(status)
            {
                case MeetingStatus.MEETING_STATUS_ENDED:
                case MeetingStatus.MEETING_STATUS_FAILED:
                    {
                        if (raisescreen != null)
                        {
                            raisescreen.Hide();
                            raisescreen = null;
                        }

                        Show();

                        feedback.Content = $"Meeting Ended {textBox_meetingnumber_api.Text}: {status}";
                    }
                    break;

                case MeetingStatus.MEETING_STATUS_INMEETING:
                {
                    var list = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetParticipantsList();

                    foreach (var item in list)
                    {
                        var user = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetUserByUserID(item);

                        var name = user.GetUserNameW();

                        if (name == userName && raisescreen == null)
                        {
                            userid = user.GetUserID();

                            raisescreen = new RaiseLowerHand(userid, settings)
                            {
                                Title = name
                            };

                            raisescreen.Show();

                            Console.WriteLine(userid.ToString());
                        }

                        Console.WriteLine(name);
                    }

                        UpdateCount();

                        raisescreen.UpdateCount(count);

                        break;
                }
                default:
                    feedback.Content = $"waiting to join {textBox_meetingnumber_api.Text}: {status}";
                    //todo
                    break;
            }
        }


        public int ExtractNumber(string original)
        {
            if (original.Any(c => char.IsDigit(c)))
            {
                var result = int.Parse(new string(original.Where(c => char.IsDigit(c)).ToArray()));

                if (result > 10)
                {
                    return 1;
                }

                return result;
            }

            char[] allowed = { ',', '&' };

            int returnValue = 1;

            if (original.Contains(" and "))
            {
                original = original.Replace(" and ", " & ");
            }

            if (original.Any(g=> allowed.Contains(g)))
            {
                returnValue += original.Split(allowed).Length-1;
            }

            return returnValue;
        }

        public void UpdateCount()
        {
            count = 0;
            var list = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetParticipantsList();

            foreach (var item in list)
            {
                var user = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetUserByUserID(item);

                var name = user.GetUserNameW();

                if (!countexcludednames.Contains(name))
                {
                    var participants = ExtractNumber(name);

                    Console.WriteLine($"{name}: ({participants})");

                    count += participants;
                }
            }

            Console.WriteLine($"Current count: {count}");
        }
        public void onUserJoin(Array lstUserID)
        {
            UpdateCount();
            if(raisescreen!= null)
            {
                raisescreen.UpdateCount(count);
            }
        }


        public void Init() {

            userName = CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().GetAccountInfo().GetDisplayName();

            textBox_DisplayName.Text = userName;
        }
        public void onUserLeft(Array lstUserID)
        {
            UpdateCount();

            if (raisescreen != null)
            {
                raisescreen.UpdateCount(count);
            }
        }
        public void onHostChangeNotification(uint userId)
        {
            //todo
        }
        public void onLowOrRaiseHandStatusChanged(bool bLow, uint userid)
        {
            if (bLow)
            {
                CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, userName, false);
            }
        }
        public void onUserNameChanged(uint userId, string userName)
        {
            
            if( userId != this.userid)
            {
                UpdateCount();
                if (raisescreen != null)
                {
                    raisescreen.UpdateCount(count);
                }
            }
        }
        private void RegisterCallBack()
        {
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Add_CB_onMeetingStatusChanged(onMeetingStatusChanged);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().
                GetMeetingParticipantsController().Add_CB_onHostChangeNotification(onHostChangeNotification);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().
                GetMeetingParticipantsController().Add_CB_onLowOrRaiseHandStatusChanged(onLowOrRaiseHandStatusChanged);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().
                GetMeetingParticipantsController().Add_CB_onUserLeft(onUserLeft);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().
                GetMeetingParticipantsController().Add_CB_onUserNameChanged(onUserNameChanged);

            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().
                GetMeetingParticipantsController().Add_CB_onUserJoin(onUserJoin);
        }

        private void button_join_api_Click(object sender, RoutedEventArgs e)
        {
            RegisterCallBack();
            ZOOM_SDK_DOTNET_WRAP.JoinParam param = new ZOOM_SDK_DOTNET_WRAP.JoinParam();
            param.userType = SDKUserType.SDK_UT_NORMALUSER;
            ZOOM_SDK_DOTNET_WRAP.JoinParam4NormalUser join_api_param = new ZOOM_SDK_DOTNET_WRAP.JoinParam4NormalUser();


            if (settings.Participants.Count > 1)
            {
                this.userName = $"{textBox_DisplayName.Text} x {settings.Participants.Count}";
            }
            else
            {
                this.userName = $"{textBox_DisplayName.Text}";
            }

            join_api_param.meetingNumber = ulong.Parse(textBox_meetingnumber_api.Text.Replace("-",""));
            join_api_param.psw = textBox_Password.Text;
            join_api_param.userName = this.userName;            

            param.normaluserJoin = join_api_param;

            ZOOM_SDK_DOTNET_WRAP.SDKError err = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Join(param);
            if (SDKError.SDKERR_SUCCESS == err)
            {
                feedback.Content = $"Joining Meeting {textBox_meetingnumber_api.Text}: {err}";
                Hide();
            }
            else//error handle
            {
                feedback.Content = $"Failed to join meeting {textBox_meetingnumber_api.Text}: {err}";
                Console.WriteLine(err);
            }
        }

        void Wnd_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var settingstrings = JsonConvert.SerializeObject(this.settings);

            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Particpants.json";

            Console.WriteLine($"Saving to {path}");
            File.WriteAllText(path, settingstrings);

            feedback.Content = "Settings saved";
        }

        private void addParticpantButton_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            AddParticipant dlg = new AddParticipant
            {
                // Configure the dialog box
                Owner = this
            };

            // Open the dialog box modally
             var result = dlg.ShowDialog();

            if (result??false)
            {
                var name = dlg.ParticipantName;

                var control = new UIParticipantListItem(name);

                control.RemoveClicked += Control_RemoveClicked;

                this.participantStack.Children.Add(control);

                this.settings.Participants.Add(new RaiseHandParticipant() { Name = name });
            }

            dlg.Close();
        }
    }
}
