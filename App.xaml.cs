using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ZOOM_SDK_DOTNET_WRAP;

namespace RaiseHandApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init sdk
            {
                ZOOM_SDK_DOTNET_WRAP.InitParam param = new ZOOM_SDK_DOTNET_WRAP.InitParam();
                param.web_domain = "https://zoom.us";
                ZOOM_SDK_DOTNET_WRAP.SDKError err = CZoomSDKeDotNetWrap.Instance.Initialize(param);
                if (SDKError.SDKERR_SUCCESS == err)
                {
                    Console.WriteLine("SDK loaded");

                    var key = ConfigurationManager.AppSettings["zoomkey"];
                    var secret = ConfigurationManager.AppSettings["zoomsecret"];
                    var authparams = new AuthParam()
                    {
                        appKey = key,
                        appSecret = secret
                    };

                    ZOOM_SDK_DOTNET_WRAP.SDKError autherr = CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().SDKAuth(authparams);

                    if (autherr == SDKError.SDKERR_SUCCESS)
                    {
                        Console.WriteLine("APi auth success");
                    }


                }
                else//error handle.todo
                {
                    Console.WriteLine(err);
                    Console.WriteLine("Sdk loading failed");
                }
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //clean up sdk
            try
            {


                CZoomSDKeDotNetWrap.Instance.CleanUp();
            }
            catch
            {

            }

        }
    }
}
