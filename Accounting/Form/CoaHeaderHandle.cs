using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    /// <summary>
    /// Owns the runtime sizing of the Chart Of Account header built by <see cref="CoaHeaderLayout"/>.
    /// Sizes are only meaningful once the skin and the DPI-scaled fonts are settled, so measuring is
    /// deferred to <see cref="Relayout"/> rather than done while the form is still constructing.
    /// </summary>
    internal sealed class CoaHeaderHandle : IDisposable
    {
        private const int LogicalMinButtonWidth = 96;
        private const int LogicalMinControlHeight = 30;
        private const int LogicalMinFormWidth = 1120;
        private const int LogicalMinFormHeight = 680;
        private const int LogicalIconPadding = 42;
        private const int LogicalTextPadding = 24;

        private readonly XtraForm form;
        private readonly SidePanel host;
        private readonly FlowLayoutPanel toolbarFlow;
        private readonly GroupControl[] groups;
        private readonly IReadOnlyList<SimpleButton> buttons;
        private readonly IReadOnlyList<KeyValuePair<Control, int>> editors;

        private bool isApplyingHostHeight;
        private bool isDisposed;

        internal CoaHeaderHandle(
            XtraForm form,
            SidePanel host,
            FlowLayoutPanel toolbarFlow,
            GroupControl[] groups,
            IReadOnlyList<SimpleButton> buttons,
            IReadOnlyList<KeyValuePair<Control, int>> editors)
        {
            this.form = form;
            this.host = host;
            this.toolbarFlow = toolbarFlow;
            this.groups = groups;
            this.buttons = buttons;
            this.editors = editors;

            form.Resize += OnHostResized;
            host.SizeChanged += OnHostResized;
        }

        /// <summary>
        /// Re-measures every header control against the form's current DPI and font, then resizes the
        /// group boxes and the host panel to fit. Safe to call repeatedly.
        /// </summary>
        public void Relayout()
        {
            if (isDisposed || host.IsDisposed || form.IsDisposed)
            {
                return;
            }

            float scale = CoaHeaderLayout.GetScale(form);
            using Font headerFont = CoaHeaderLayout.CreateHeaderFont();
            int controlHeight = MeasureControlHeight(headerFont, scale);

            form.SuspendLayout();
            host.SuspendLayout();
            try
            {
                form.MinimumSize = ClampToScreen(new Size(
                    CoaHeaderLayout.Scale(LogicalMinFormWidth, scale),
                    CoaHeaderLayout.Scale(LogicalMinFormHeight, scale)));

                foreach (KeyValuePair<Control, int> editor in editors)
                {
                    editor.Key.Margin = CoaHeaderLayout.ScalePadding(4, 3, 4, 3, scale);
                    editor.Key.Size = new Size(CoaHeaderLayout.Scale(editor.Value, scale), controlHeight);
                }

                foreach (SimpleButton button in buttons)
                {
                    button.Margin = CoaHeaderLayout.ScalePadding(2, 3, 2, 3, scale);
                    button.Size = new Size(MeasureButtonWidth(button, headerFont, scale), controlHeight);
                }

                foreach (GroupControl group in groups)
                {
                    ResizeGroup(group);
                }

                int tallest = 0;
                foreach (GroupControl group in groups)
                {
                    tallest = Math.Max(tallest, group.Height);
                }
                foreach (GroupControl group in groups)
                {
                    group.Height = tallest;
                }
            }
            finally
            {
                host.ResumeLayout(true);
                form.ResumeLayout(true);
            }

            ApplyHostHeight();
        }

        /// <summary>
        /// The scaled minimum can exceed a small screen at 150% DPI, which would leave the MDI child
        /// unable to fit its parent. Never demand more than the monitor actually offers.
        /// </summary>
        private Size ClampToScreen(Size minimum)
        {
            Rectangle workingArea = Screen.FromControl(form).WorkingArea;
            return new Size(
                Math.Min(minimum.Width, workingArea.Width),
                Math.Min(minimum.Height, workingArea.Height));
        }

        private static int MeasureControlHeight(Font headerFont, float scale)
        {
            int textHeight = TextRenderer.MeasureText("Ag", headerFont).Height;
            return Math.Max(CoaHeaderLayout.Scale(LogicalMinControlHeight, scale), textHeight + CoaHeaderLayout.Scale(10, scale));
        }

        private static int MeasureButtonWidth(SimpleButton button, Font headerFont, float scale)
        {
            int text = TextRenderer.MeasureText(button.Text ?? string.Empty, headerFont).Width;
            bool hasIcon = button.ImageOptions?.Image != null;
            int padding = CoaHeaderLayout.Scale(hasIcon ? LogicalIconPadding : LogicalTextPadding, scale);
            return Math.Max(CoaHeaderLayout.Scale(LogicalMinButtonWidth, scale), text + padding);
        }

        /// <summary>
        /// Sizes a group box around its table. The caption height and border thickness change with DPI
        /// and skin, so they are read back from <see cref="Control.DisplayRectangle"/> instead of being
        /// hard-coded.
        /// </summary>
        private static void ResizeGroup(GroupControl group)
        {
            if (group.Controls.Count == 0 || group.Controls[0] is not TableLayoutPanel table)
            {
                return;
            }

            Size insets = group.Size - group.DisplayRectangle.Size;
            Size content = table.GetPreferredSize(Size.Empty);
            group.Size = new Size(content.Width + insets.Width, content.Height + insets.Height);
        }

        private void OnHostResized(object sender, EventArgs e)
        {
            ApplyHostHeight();
        }

        /// <summary>
        /// Grows the header panel to whatever height the flow panel needs after wrapping at the
        /// current width. Assigning <see cref="Control.Height"/> raises SizeChanged, hence the guard.
        /// </summary>
        private void ApplyHostHeight()
        {
            if (isDisposed || host.IsDisposed || isApplyingHostHeight)
            {
                return;
            }

            float scale = CoaHeaderLayout.GetScale(form);
            int minWidth = CoaHeaderLayout.Scale(640, scale);
            int targetWidth = Math.Max(minWidth, host.ClientSize.Width - CoaHeaderLayout.Scale(16, scale));

            isApplyingHostHeight = true;
            try
            {
                Size preferred = toolbarFlow.GetPreferredSize(new Size(targetWidth, 0));
                host.Height = preferred.Height + CoaHeaderLayout.Scale(8, scale);
            }
            finally
            {
                isApplyingHostHeight = false;
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            form.Resize -= OnHostResized;
            host.SizeChanged -= OnHostResized;
        }
    }
}
