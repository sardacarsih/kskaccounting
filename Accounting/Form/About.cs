using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class About : SplashScreen
    {
        public About()
        {
            InitializeComponent();
            this.labelCopyright.Text = " Copyright © Graha Fajar 2021-" + DateTime.Now.Year.ToString();
            this.lblDev.Text = "Leader Team : Wang Ameng ( Account Director )\nDeveloper : Dadang Haryadi \nSupport by : Hang Tjhiang ( Account Manager )";
            this.lblversion.Text= "Version : "+ Acct.AppVersion;
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
        }

        private void About_Load(object sender, EventArgs e)
        {
            Timer t = new Timer
            {
                Interval = 10000
            };
            t.Tick += new EventHandler(closeabout);
            t.Start();
        }

        private void closeabout(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }

        private void peImage_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}