﻿using Dropbox.Api;
using HomeBudget.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HomeBudget.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class SettingsPage : Page
    {
        private const string RedirectUri = "https://localhost/authorize/";
        private string appKey = "p6cayskxetnkx1a";
        private string oauth2State;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void ConnectWithDropbox(object sender, RoutedEventArgs e)
        {
            oauth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, new Uri(RedirectUri), state: oauth2State);

            var result = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                authorizeUri,
                new Uri(RedirectUri));

            ProcessResult(result);
        }

        private static void ProcessResult(WebAuthenticationResult result)
        {
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    var response = DropboxOAuth2Helper.ParseTokenFragment(new Uri(result.ResponseData));
                    Helpers.Settings.DropboxAccessToken = response.AccessToken;
                    DropboxManager.Instance.Init();
                    Task.Run(()=> DropboxManager.Instance.DownloadData());
                    break;

                /*case WebAuthenticationStatus.ErrorHttp:
                    throw new OAuthException(result.ResponseErrorDetail);

                case WebAuthenticationStatus.UserCancel:
                default:
                    throw new OAuthUserCancelledException();*/
            }
        }

        private async void WebViewOnNavigating(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (!args.Uri.AbsoluteUri.ToString().StartsWith(RedirectUri.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.  
                return;
            }

            try
            {
                var result = DropboxOAuth2Helper.ParseTokenFragment(args.Uri);

                if (result.State != this.oauth2State)
                {
                    return;
                }

                Helpers.Settings.DropboxAccessToken = result.AccessToken;
                DropboxManager.Instance.Init();
                await DropboxManager.Instance.DownloadData();
            }
            catch (ArgumentException argExc)
            {
                var msg = argExc.Message;
                msg += "error";
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                //args..Cancel = true;
                //await Application.Current.MainPage.Navigation.PopModalAsync();

            }
        }
    }
}
