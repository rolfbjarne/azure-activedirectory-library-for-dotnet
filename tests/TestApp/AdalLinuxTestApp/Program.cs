using System;
using Xwt;
using Microsoft.IdentityService.Clients.ActiveDirectory;
using TestApp.PCL;

namespace AdalLinuxTestApp
{
    class MainClass
    {
        //
        //NOTE: Replace these with valid values
        //
        const string AUTHORITY = "https://login.windows.net/common";
        const string CLIENTID = "<CLIENTID>";
        const string RESOURCE = "<RESOURCE>";
        const string USER = null;
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        static MobileAppSts sts = new MobileAppSts();

        static TextEntry tokenEntry;
        static Window testerWindow;

        static TokenBroker CreateBrokerWithSts()
        {
            var tokenBroker = new TokenBroker();

            sts.Authority = AUTHORITY;
            sts.ValidClientId = CLIENTID;
            sts.ValidResource = RESOURCE;
            sts.ValidUserName = USER;
            sts.ValidNonExistingRedirectUri = new Uri(REDIRECT_URI);
            tokenBroker.Sts = sts;
            return tokenBroker;
        }

        public static void Main(string[] args)
        {
            Application.Initialize(ToolkitType.Gtk);
            testerWindow = new Window();
            var interactiveButton = new Button("Acquire Interactve");
            interactiveButton.Clicked += AcquireInteractiveClicked;
            var silentButton = new Button("Acquire Silent");
            silentButton.Clicked += AcquireSilentClicked;
            var clearCacheButton = new Button("Clear Cache");
            clearCacheButton.Clicked += ClearCacheClicked;
            var hbox1 = new HBox();
            var label1 = new Label("Token:");
            tokenEntry = new TextEntry();
            tokenEntry.ReadOnly = true;
            hbox1.PackStart(label1);
            hbox1.PackStart(tokenEntry, true);
            var vbox1 = new VBox();
            vbox1.PackStart(hbox1);
            vbox1.PackStart(interactiveButton);
            vbox1.PackStart(silentButton);
            vbox1.PackStart(clearCacheButton);
            testerWindow.Content = vbox1;
            testerWindow.Closed += (sender, e) => { Application.Exit(); };
            testerWindow.Show();
            Application.Run();
        }

        static async void AcquireInteractiveClicked(Object sender, EventArgs e)
        {
            Application.Invoke(delegate { tokenEntry.Text = string.Empty;});
            string token = await CreateBrokerWithSts().GetTokenInteractiveAsync(new PlatformParameters(testerWindow));
            Application.Invoke(delegate{ tokenEntry.Text = token;});
        }

        static async void AcquireSilentClicked(Object sender, EventArgs e)
        {
            Application.Invoke(delegate { tokenEntry.Text = string.Empty;});
            string token = await CreateBrokerWithSts().GetTokenSilentAsync(new PlatformParameters(testerWindow));
            Application.Invoke(delegate { tokenEntry.Text = token;});
        }

        static void ClearCacheClicked(Object sender, EventArgs e)
        {
            var tokenBroker = new TokenBroker();
            tokenBroker.ClearTokenCache();
        }
    }
}
