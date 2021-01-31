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
        bool nologin = false;
        uint userid;
        string userName;
        int count = 0;
        
        ParticipantSettings settings;
        
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

            RegisterCallBack(); ;
        }

        public void setNoLogin()
        {
            nologin = true;
            this.button_start_api.IsEnabled = false;
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

                        if (user.IsMySelf() && raisescreen == null)
                        {
                            userid = user.GetUserID();

                            raisescreen = new RaiseLowerHand(userid, this.userName, settings)
                            {
                                Title = name
                            };

                                raisescreen.RequestEdit += Raisescreen_RequestEdit;

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

            Console.WriteLine(feedback.Content);
        }

        private void Raisescreen_RequestEdit(string obj)
        {
            textBox_Password.IsEnabled = false;
            textBox_meetingnumber_api.IsEnabled = false;
            button_join_api.IsEnabled = false;
            button_start_api.IsEnabled = false;

            button_Update.Visibility = Visibility.Visible;

            this.Show();
        }

        public Tuple<bool,int> ExtractNumber(string original)
        {
            var loweroriginal = original.ToLowerInvariant();

            if (loweroriginal.Any(c => char.IsDigit(c)))
            {
                var result = int.Parse(new string(loweroriginal.Where(c => char.IsDigit(c)).ToArray()));

                if (result > 10)
                {
                    return  new Tuple<bool, int>(false,1);
                }

                return new Tuple<bool, int>(false, result);
            }

            int returnValue = 1;

            if (settings.ReplaceAnd)
            {
                if (loweroriginal.Contains(" and "))
                {
                    loweroriginal = loweroriginal.Replace(" and ", " & ");
                }
            }

            if (loweroriginal.Any(g=> settings.SplitNamesOn.Contains(g)))
            {
                returnValue += loweroriginal.Split(settings.SplitNamesOn.ToArray()).Length-1;
            }

                return new Tuple<bool, int>(true, returnValue);
        }

        public void UpdateCount( uint overrideid = 0, int overridecount =0)
        {
            count = 0;
            var list = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetParticipantsList();

            foreach (var item in list)
            {
                if(overrideid>0 && item == overrideid)
                {
                    count += overridecount;

                    continue;
                }
                var user = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetUserByUserID(item);

                var name = user.GetUserNameW();

                if (!settings.SkipCountNames.Contains(name))
                {
                    var participants = ExtractNumber(name);

                    Console.WriteLine($"{name}: ({participants})");

                    //if ( participants.Item1 && CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetUserByUserID(this.userid).IsHost())
                    //{
                    //    ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, $"{name} x {count}", false);
                    //}

                    count += participants.Item2;
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
            if (bLow && userid == this.userid)
            {
                CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, userName, false);

                if(raisescreen!= null)
                {
                    raisescreen.ClearToggles();
                }
            }

            //if (CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().GetUserByUserID(this.userid).IsHost())
            //{
            //    CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingAudioController().MuteAudio(userid, false);
            //}
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
            else
            {
                if (raisescreen != null && raisescreen.Answering ==false && this.userName != userName)
                {
                    if (userName.EndsWith($"x {this.settings.Participants.Count}"))
                    {
                        this.userName = userName.Replace($"x {this.settings.Participants.Count}", "");
                    }
                    else
                    {
                        this.userName = userName;
                    }

                    this.textBox_DisplayName.Text = this.userName;

                    if (settings.Participants.Count > 1)
                    {
                        this.userName = $"{this.userName} x {settings.Participants.Count}";
                    }
                    else
                    {
                        this.userName = $"{this.userName}";
                    }

                    raisescreen.UpdateParticipants(this.userName, this.settings);
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

            if (textBox_DisplayName.Text.Length == 0)
            {
                MessageBox.Show("Please fill in a display name",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                return;
            }


            ZOOM_SDK_DOTNET_WRAP.JoinParam param = new ZOOM_SDK_DOTNET_WRAP.JoinParam();
            param.userType = SDKUserType.SDK_UT_WITHOUT_LOGIN;
            ZOOM_SDK_DOTNET_WRAP.JoinParam4WithoutLogin join_api_param = new ZOOM_SDK_DOTNET_WRAP.JoinParam4WithoutLogin();


            if (settings.Participants.Count > 1)
            {
                this.userName = $"{textBox_DisplayName.Text} x {settings.Participants.Count}";
            }
            else
            {
                this.userName = $"{textBox_DisplayName.Text}";
            }
            ZOOM_SDK_DOTNET_WRAP.SDKError err;            

            join_api_param.meetingNumber = ulong.Parse(textBox_meetingnumber_api.Text.Replace("-", "").Replace(" ", ""));
            join_api_param.userName = this.userName;
            join_api_param.psw = textBox_Password.Text;
            param.withoutloginJoin = join_api_param;

            err = ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Join(param);

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

        private void button_start_api_Click(object sender, RoutedEventArgs e)
        {
            ZOOM_SDK_DOTNET_WRAP.StartParam param = new ZOOM_SDK_DOTNET_WRAP.StartParam();
            param.userType = ZOOM_SDK_DOTNET_WRAP.SDKUserType.SDK_UT_NORMALUSER;
            ZOOM_SDK_DOTNET_WRAP.StartParam4NormalUser start_normal_param = new ZOOM_SDK_DOTNET_WRAP.StartParam4NormalUser();

            if (settings.Participants.Count > 1)
            {
                this.userName = $"{textBox_DisplayName.Text} x {settings.Participants.Count}";
            }
            else
            {
                this.userName = $"{textBox_DisplayName.Text}";
            }

            start_normal_param.meetingNumber = UInt64.Parse(textBox_meetingnumber_api.Text.Replace("-", "").Replace(" ", ""));
            start_normal_param.participantId = this.userName;
            start_normal_param.isAudioOff = false;
            start_normal_param.isVideoOff = true;
            start_normal_param.isDirectShareDesktop = false;

            param.normaluserStart = start_normal_param;


            ZOOM_SDK_DOTNET_WRAP.SDKError err = ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Start(param);

            if (SDKError.SDKERR_SUCCESS == err)
            {
                feedback.Content = $"Starting Meeting {textBox_meetingnumber_api.Text}: {err}";
                Hide();
            }
            else//error handle
            {
                feedback.Content = $"Failed to start meeting {textBox_meetingnumber_api.Text}: {err}";
                Console.WriteLine(err);
            }
        }

        void Wnd_Closing(object sender, CancelEventArgs e)
        {
            if (button_Update.Visibility != Visibility.Visible)
            {
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
               
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var settingstrings = JsonConvert.SerializeObject(this.settings);

            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Particpants.json";

            Console.WriteLine($"Saving to {path}");
            File.WriteAllText(path, settingstrings);

            feedback.Content = "Settings saved";
        }

        private void buttonUpdate_click(object sender, RoutedEventArgs e)
        {
            FinishUpdate();
        }

        private void FinishUpdate()
        {
            if (settings.Participants.Count > 1)
            {
                this.userName = $"{textBox_DisplayName.Text} x {settings.Participants.Count}";
            }
            else
            {
                this.userName = $"{textBox_DisplayName.Text}";
            }

            var current = raisescreen.CurrentParticipants;
            raisescreen.UpdateParticipants(this.userName, this.settings);            

            ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().GetMeetingParticipantsController().ChangeUserName(userid, this.userName, false);

            if (this.settings.Participants.Count != current)
            {
                UpdateCount(this.userid, this.settings.Participants.Count);
                raisescreen.UpdateCount(count);
            }

            this.Hide();

            textBox_Password.IsEnabled = true;
            textBox_meetingnumber_api.IsEnabled = true;
            button_join_api.IsEnabled = true;

            if (!nologin)
            {
                button_start_api.IsEnabled = true;
            }

            button_Update.Visibility = Visibility.Hidden;
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
