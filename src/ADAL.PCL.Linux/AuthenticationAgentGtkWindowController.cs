using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Xwt;
namespace Microsoft.IdentityService.Clients.ActiveDirectory
{
    public class AuthenticationAgentGtkWindowController: Xwt.Dialog
    {
        const int DEFAULT_WINDOW_WIDTH = 420;
        const int DEFAULT_WINDOW_HEIGHT = 650;

        WebView webView;
        ProgressBar progressIndicator;

        Window callerWindow;

        readonly string url;
        readonly string callback;

        readonly ReturnCodeCallback callbackMethod;

        public delegate void ReturnCodeCallback(AuthorizationResult result);

        public AuthenticationAgentGtkWindowController(string url, string callback, ReturnCodeCallback callbackMethod)
        {
            this.url = url;
            this.callback = callback;
            this.callbackMethod = callbackMethod;


            Height = DEFAULT_WINDOW_HEIGHT;
            Width = DEFAULT_WINDOW_WIDTH;
            Resizable = false;
            Padding = 0;
            this.InitialLocation = WindowLocation.CenterParent;

            // UA is:
            // Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/538.15 (KHTML, like Gecko) Version/8.0 Safari/538.15

            webView = new WebView(this.url);
            progressIndicator = new ProgressBar();
            webView.NavigateToUrl += WebView_NavigateToUrl;
            webView.Loaded += (sender, e) => { Application.Invoke(delegate { progressIndicator.Indeterminate = false; progressIndicator.Fraction = 0; }); };
            webView.Loading += (sender, e) => { Application.Invoke(delegate { progressIndicator.Indeterminate = true; }); };
            var scrollHolder = new ScrollView(webView);
            var vbox1 = new VBox();
            vbox1.Margin = 0;
            vbox1.PackStart(scrollHolder, true, true);
            vbox1.PackStart(progressIndicator, false, false);
            Content = vbox1;
        }

        void WebView_NavigateToUrl(object sender, NavigateToUrlEventArgs e)
        {
            /*Console.WriteLine(e.Uri);
            if (e.Uri.ToString().StartsWith("urn:ietf:wg:oauth:2.0:oob", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("MAGICAL SPECIAL HANDLING");
                Application.Invoke(delegate { progressIndicator.Indeterminate = true; progressIndicator.Fraction = 0;});
            }*/

            if (e == null)
            {
                return;
            }

            string requestUrlString = e.Uri.ToString();

            if (requestUrlString.StartsWith(BrokerConstants.BrowserExtPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var result = new AuthorizationResult(AuthorizationStatus.ProtocolError)
                {
                    Error = "Unsupported request",
                    ErrorDescription = "Server is redirecting client to browser. This behavior is not yet defined on Mac OS X."
                };
                callbackMethod(result);
                webView.StopLoading();
                Application.Invoke(delegate { Close(); });
                return;
            }

            if (requestUrlString.ToLower(CultureInfo.InvariantCulture).StartsWith(callback.ToLower(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) ||
                requestUrlString.StartsWith(BrokerConstants.BrowserExtInstallPrefix, StringComparison.OrdinalIgnoreCase))
            {
                callbackMethod(new AuthorizationResult(AuthorizationStatus.Success, requestUrlString));
                webView.StopLoading();
                Application.Invoke(delegate { Close(); });
                return;
            }

            if (requestUrlString.StartsWith(BrokerConstants.DeviceAuthChallengeRedirect, StringComparison.CurrentCultureIgnoreCase))
            {
                var uri = new Uri(requestUrlString);
                string query = uri.Query;
                if (query.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Substring(1);
                }

                Dictionary<string, string> keyPair = EncodingHelper.ParseKeyValueList(query, '&', true, false, null);
                string responseHeader = PlatformPlugin.DeviceAuthHelper.CreateDeviceAuthChallengeResponse(keyPair).Result;

                /*var newRequest = WebRequest.CreateHttp(keyPair["SubmitUrl"]);
                newRequest.Headers[BrokerConstants.ChallengeResponseHeader] = responseHeader;
                var newRequest = (NSMutableUrlRequest)request.MutableCopy();
                newRequest.Url = new NSUrl(keyPair["SubmitUrl"]);
                newRequest[BrokerConstants.ChallengeResponseHeader] = responseHeader;
                webView.MainFrame.LoadRequest(newRequest);
                WebView.DecideIgnore(decisionToken);*/
                return;
            }

            if (!e.Uri.AbsoluteUri.Equals("about:blank", StringComparison.CurrentCultureIgnoreCase) && !e.Uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
            {
                var result = new AuthorizationResult(AuthorizationStatus.ErrorHttp);
                result.Error = AdalError.NonHttpsRedirectNotSupported;
                result.ErrorDescription = AdalErrorMessage.NonHttpsRedirectNotSupported;
                callbackMethod(result);
                webView.StopLoading();
                Application.Invoke(delegate { Close(); });
            }

        }

        void CancelAuthentication()
        {
            callbackMethod(new AuthorizationResult(AuthorizationStatus.UserCancel, null));
        }
    }
}
