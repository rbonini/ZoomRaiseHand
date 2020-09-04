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
using System.ComponentModel; // CancelEventArgs
using ZOOM_SDK_DOTNET_WRAP;

namespace RaiseHandApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        joinmeeting start_meeting_wnd = new joinmeeting();
        public Login()
        {
            InitializeComponent(); 
        }

        //callback
        public void onAuthenticationReturn(AuthResult ret)
        {
            if (AuthResult.AUTHRET_SUCCESS == ret)
            {
                start_meeting_wnd.Show();
            }
            else//error handle.todo
            {
                Show();
            }
        }
        public void onLoginRet(LOGINSTATUS ret, IAccountInfo pAccountInfo)
        {
            Console.WriteLine($"Return: {ret}");

            feedback.Content = $"Return: {ret}";

            if (LOGINSTATUS.LOGIN_SUCCESS == ret)
            {
                start_meeting_wnd.Init();
                start_meeting_wnd.Show();

                Hide();
            }
            else//error handle.todo
            {
                Show();
            }
        }
        public void onLogout()
        {
            //todo
        }
        private void button_auth_Click(object sender, RoutedEventArgs e)
        {
            //register callback
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLoginRet(onLoginRet);
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLogout(onLogout);
            //

            //param.jwt_token = textBox_apptoken.Text;

            LoginParam4Email login = new LoginParam4Email();

            login.userName = textBox_username.Text;
            login.password = textBox_Password.Password;

            var loginparam = new LoginParam();

            loginparam.loginType = LoginType.LoginType_Email;
            loginparam.emailLogin = login;

            feedback.Content = $"Logging in as {login.userName}";

            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Login(loginparam);
            
        }

        void Wnd_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
