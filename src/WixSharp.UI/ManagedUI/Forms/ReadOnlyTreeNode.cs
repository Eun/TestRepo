using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace WixSharp.UI.Forms
{
    /// <summary>
    /// Set of extension methods for working with ManagedUI dialogs
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the feature checkbox is checked.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        public static bool IsViewChecked(this FeatureItem feature)
        {
            if (feature.View is TreeNode)
                return (feature.View as TreeNode).Checked;
            return false;
        }

        /// <summary>
        /// Resets the whether the feature checkbox checked state to the initial stat.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public static void ResetViewChecked(this FeatureItem feature)
        {
            if (feature.View is TreeNode)
                (feature.View as TreeNode).Checked = feature.DefaultIsToBeInstalled();
        }

        /// <summary>
        /// Returns default 'is to be installed' state of teh feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        public static bool DefaultIsToBeInstalled(this FeatureItem feature)
        {
            return feature.RequestedState != InstallState.Absent;
        }

        /// <summary>
        /// Returns the FeatireItem bound to the TreeNode.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static FeatureItem FeatureItem(this TreeNode node)
        {
            return node.Tag as FeatureItem;
        }

        /// <summary>
        /// Converts TreeNodeCollection into the TreeNode array.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public static TreeNode[] ToArray(this TreeNodeCollection nodes)
        {
            return nodes.Cast<TreeNode>().ToArray();
        }

        /// <summary>
        /// Aggregates all nodes of the TreeView control.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <returns></returns>
        public static TreeNode[] AllNodes(this TreeView treeView)
        {
            var result = new List<TreeNode>();
            var queue = new Queue<TreeNode>(treeView.Nodes.Cast<TreeNode>());

            while (queue.Any())
            {
                TreeNode node = queue.Dequeue();
                result.Add(node);
                foreach (TreeNode child in node.Nodes)
                    queue.Enqueue(child);
            }
            return result.ToArray();
        }
    }

#pragma warning disable 1591
    public class ReadOnlyTreeNode : TreeNode
    {
        public bool IsReadOnly { get; set; }

        public class Behavior
        {
            public static void AttachTo(TreeView treeView)
            {
                treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
                treeView.DrawNode += treeView_DrawNode;
                treeView.BeforeCheck += treeView_BeforeCheck;
            }

            static void treeView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
            {
                if (IsReadOnly(e.Node))
                {
                    e.Cancel = true;
                }
            }

            static Pen dotPen = new Pen(Color.FromArgb(128, 128, 128)) { DashStyle = DashStyle.Dot };
            static Brush selectionModeBrush = new SolidBrush(Color.FromArgb(51, 153, 255));

            static bool IsReadOnly(TreeNode node)
            {
                return (node is ReadOnlyTreeNode) && (node as ReadOnlyTreeNode).IsReadOnly;
            }

            static int cIndentBy = -1;
            static int cMargin = -1;

            static void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
            {
                //Loosely based on Jason Williams solution (http://stackoverflow.com/questions/1003459/c-treeview-owner-drawing-with-ownerdrawtext-and-the-weird-black-highlighting-w)
                if (e.Bounds.Height < 1 || e.Bounds.Width < 1)
                    return;

                if (cIndentBy == -1)
                {
                    cIndentBy = e.Bounds.Height;
                    cMargin = e.Bounds.Height/2;
                }

                var treeView = (TreeView)sender;

                Rectangle itemRect = e.Bounds;
                e.Graphics.FillRectangle(Brushes.White, itemRect);
                //e.Graphics.FillRectangle(Brushes.WhiteSmoke, itemRect);

                int cTwoMargins = cMargin * 2;

                int midY = (itemRect.Top + itemRect.Bottom) / 2;

                int iconWidth = itemRect.Height + 2;
                int checkboxWidth = itemRect.Height + 2;

                int indent = (e.Node.Level * cIndentBy) + cMargin;
                int iconLeft = indent;						                    // lines left position
                int checkboxLeft = iconLeft + iconWidth;                        // +/- icon left position


                int textLeft = checkboxLeft + checkboxWidth;	               // text left position
                if (!treeView.CheckBoxes)
                    textLeft = checkboxLeft;

                // Draw parentage lines
                if (treeView.ShowLines)
                {
                    int x = cMargin * 2;

                    if (e.Node.Level == 0 && e.Node.PrevNode == null)
                    {
                        // The very first node in the tree has a half-height line
                        e.Graphics.DrawLine(dotPen, x, midY, x, itemRect.Bottom);
                    }
                    else
                    {
                        TreeNode testNode = e.Node;			// Used to only draw lines to nodes with Next Siblings, as in normal TreeViews
                        for (int iLine = e.Node.Level; iLine >= 0; iLine--)
                        {
                            if (testNode.NextNode != null)
                            {
                                x = (iLine * cIndentBy) + (cMargin * 2);
                                e.Graphics.DrawLine(dotPen, x, itemRect.Top, x, itemRect.Bottom);
                            }

                            testNode = testNode.Parent;
                        }

                        x = (e.Node.Level * cIndentBy) + (cMargin * 2);
                        e.Graphics.DrawLine(dotPen, x, itemRect.Top, x, midY);
                    }

                    e.Graphics.DrawLine(dotPen, iconLeft + cMargin, midY, iconLeft + cMargin + 10, midY);
                }

                // Draw (plus/minus) icon if required
                if (e.Node.Nodes.Count > 0)
                {

                    var element = e.Node.IsExpanded ? VisualStyleElement.TreeView.Glyph.Opened : VisualStyleElement.TreeView.Glyph.Closed;
                    var renderer = new VisualStyleRenderer(element);
                    var iconTrueSize = renderer.GetPartSize(e.Graphics, ThemeSizeType.True);

                    var bounds = new Rectangle(itemRect.Left + iconLeft, itemRect.Top, iconWidth, iconWidth);

                    //e.Graphics.FillRectangle(Brushes.Salmon, bounds);

                    //deflate (resize and center) icon within bounds 
                    var dif = (iconWidth - iconTrueSize.Height)/2;
                    bounds.Inflate(-dif, -dif);
                    renderer.DrawBackground(e.Graphics, bounds);
                }

                //Checkbox
                if (treeView.CheckBoxes)
                {
                    var element = e.Node.Checked ? VisualStyleElement.Button.CheckBox.CheckedNormal : VisualStyleElement.Button.CheckBox.UncheckedNormal;
                    if (IsReadOnly(e.Node))
                        element = e.Node.Checked ? VisualStyleElement.Button.CheckBox.CheckedDisabled : VisualStyleElement.Button.CheckBox.UncheckedDisabled;

                    var renderer = new VisualStyleRenderer(element);
                    var bounds = new Rectangle(itemRect.Left + checkboxLeft, itemRect.Top, checkboxWidth, itemRect.Height);
                    //e.Graphics.FillRectangle(Brushes.Bisque, bounds);
                    renderer.DrawBackground(e.Graphics, bounds);
                }

                //Text
                if (!string.IsNullOrEmpty(e.Node.Text))
                {
                    SizeF textSize = e.Graphics.MeasureString(e.Node.Text, treeView.Font);

                    var drawFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags = StringFormatFlags.NoWrap,
                    };

                    var bounds = new Rectangle(itemRect.Left + textLeft, itemRect.Top, (int)(textSize.Width + 2), itemRect.Height);

                    if (e.Node.IsSelected)
                    {
                        e.Graphics.FillRectangle(selectionModeBrush, bounds);
                        e.Graphics.DrawString(e.Node.Text, treeView.Font, Brushes.White, bounds, drawFormat);
                    }
                    else
                    {
                        //e.Graphics.FillRectangle(Brushes.Pink, bounds);
                        e.Graphics.DrawString(e.Node.Text, treeView.Font, Brushes.Black, bounds, drawFormat);
                    }

                }

                // Focus rectangle around the text
                if (e.State == TreeNodeStates.Focused)
                {
                    var r = itemRect;
                    r.Width -= 2;
                    r.Height -= 2;
                    r.Offset(indent, 0);
                    r.Width -= indent;
                    e.Graphics.DrawRectangle(dotPen, r);
                }
            }
        }
    }
#pragma warning restore 1591
}