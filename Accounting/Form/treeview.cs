using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class TreeviewForm : DevExpress.XtraEditors.XtraForm
    {
        public TreeviewForm()
        {
            InitializeComponent();
        }

        private void treeview_Load(object sender, EventArgs e)
        {
            //List<Comment> categories = new List<Comment>()
            //                              {
            //                                  new Comment () { Id = 1, Text = "Item 1", ParentId = 0},
            //                                  new Comment() { Id = 2, Text = "Item 1.1", ParentId = 1 },
            //                                  new Comment() { Id = 3, Text = "Item 1.1.1", ParentId = 2 },
            //                                  new Comment() { Id = 4, Text = "Item 1.2", ParentId = 1 },
            //                                  new Comment() { Id = 5, Text = "Item 1.1.1.1", ParentId = 3 },
            //                                  new Comment() { Id = 6, Text = "Item 1.2.1", ParentId = 4 },
            //                                  new Comment() { Id = 7, Text = "Item 1.1.2", ParentId = 2 },
            //                                  new Comment () { Id = 8, Text = "Item 2", ParentId = 0},
            //                                  new Comment() { Id = 9, Text = "Item 1.1", ParentId = 8 },
            //                                  new Comment() { Id = 10, Text = "Item 1.1.1", ParentId = 9 },
            //                                  new Comment() { Id = 11, Text = "Item 1.2", ParentId = 10 },
            //                                  new Comment() { Id = 12, Text = "Item 1.1.1.1", ParentId = 9 },
            //                                  new Comment() { Id = 13, Text = "Item 1.2.1", ParentId = 11 },
            //                                  new Comment() { Id = 14, Text = "Item 1.1.2", ParentId = 11 }
            //                              };
            IOverlaySplashScreenHandle handle = null;
            handle = SplashScreenManager.ShowOverlayForm(this);
            var x= AccountServices.GetPerkiraanSaldo_TreeView("KSKPUSAT", 2022, 1);
            List<coaHIA> categories = x.ToList();

            List<coaHIA> hierarchy = new List<coaHIA>();
            hierarchy = categories
                            .Where(c => c.INDUK == null)
                            .Select(c => new coaHIA()
                            {
                                KODEACC = c.KODEACC,
                                NAMAACC = c.NAMAACC,
                                INDUK = c.INDUK,
                                GD=c.GD,
                                GRP=c.GRP,
                                LVL=c.LVL,  
                                hierarchy = "-" + c.KODEACC,
                                Children = GetChildren(categories, c.KODEACC)
                            })
                            .ToList();
            TreeNode rootNode = treeView1.Nodes.Add("PT. KALIMANTAN SAWIT KUSUMA");
            HieararchyWalk(hierarchy, rootNode);
            SplashScreenManager.CloseOverlayForm(handle);
        }

        private void HieararchyWalk(List<coaHIA> hierarchy, TreeNode treeNode)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    TreeNode newTreeNode = treeNode.Nodes.Add(item.KODEACC + " " + item.NAMAACC) ;//+ " Group: " + item.GRP) + " Hierarchy-" + item.hierarchy);
                    if (item.Children.Count != 0)
                    {
                        HieararchyWalk(item.Children, newTreeNode);
                    }
                }
            }
        }

        private List<coaHIA> GetChildren(List<coaHIA> comments, string parentId)
        {
            return comments
                    .Where(c => c.INDUK == parentId)
                    .Select(c => new coaHIA
                    {
                        KODEACC = c.KODEACC,
                        NAMAACC = c.NAMAACC,
                        INDUK = c.INDUK,
                        hierarchy = GetHiera(comments, c, parentId),
                        Children = GetChildren(comments, c.KODEACC)
                    })
                    .ToList();
        }
        private string GetHiera(List<coaHIA> comments, coaHIA comment, string parentId)
        {
            string hierarchy = "-" + comment.KODEACC + " ";
            coaHIA parentComm = comments.Where(a => a.KODEACC == parentId).FirstOrDefault();
            if (parentComm.INDUK != null)
            {
                hierarchy = GetHiera(comments, parentComm, parentComm.INDUK) + hierarchy;
            }
            else
            {
                hierarchy = "-" + parentId + " " + hierarchy + " ";
            }
            return hierarchy;
        }

    }

}
