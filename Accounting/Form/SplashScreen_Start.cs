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
    public partial class SplashScreen_Start : SplashScreen
    {
        public SplashScreen_Start()
        {
            InitializeComponent();
            this.labelCopyright.Text = " Copyright © Graha Fajar 2021-" + DateTime.Now.Year.ToString();
            this.lblDev.Text = "Leader Team : Wang Ameng ( Account Director )\nDeveloper : Dadang Haryadi \nSupport by : Hang Tjhiang ( Account Manager )";
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

    }
}