using Accounting._1.Interface;
using DevExpress.XtraEditors;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Accounting.UC.Jurnal
{
    public abstract class UcJurnalTabHostBase : XtraUserControl
    {
        private readonly PanelControl contentPanel;

        protected UcJurnalTabHostBase(IJurnalTabUseCase useCase)
        {
            UseCase = useCase;
            Dock = DockStyle.Fill;

            contentPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            Controls.Add(contentPanel);
        }

        protected IJurnalTabUseCase UseCase { get; }

        public void AttachExistingControls(IReadOnlyList<Control> controls)
        {
            contentPanel.SuspendLayout();
            contentPanel.Controls.Clear();

            for (int index = 0; index < controls.Count; index++)
            {
                Control control = controls[index];
                contentPanel.Controls.Add(control);
            }

            contentPanel.ResumeLayout();
        }
    }
}
