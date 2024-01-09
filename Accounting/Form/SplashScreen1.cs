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
    public partial class SplashScreen1 : SplashScreen
    {
        public SplashScreen1()
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

        private void labelCopyright_Click(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }

        private void SplashScreen1_Load(object sender, EventArgs e)
        {

        }

        private void peImage_Click(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }

        private void SplashScreen1_Click(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }

        private void peLogo_Click(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }
    }
}