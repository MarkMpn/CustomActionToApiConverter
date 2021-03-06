
namespace MarkMpn.CustomActionToApiConverter
{
    partial class PluginControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.solutionComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.solutionPanel = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.customActionListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.convertButton = new System.Windows.Forms.Button();
            this.selectedActionLabel = new System.Windows.Forms.Label();
            this.solutionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // solutionComboBox
            // 
            this.solutionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.solutionComboBox.FormattingEnabled = true;
            this.solutionComboBox.Location = new System.Drawing.Point(57, 3);
            this.solutionComboBox.Name = "solutionComboBox";
            this.solutionComboBox.Size = new System.Drawing.Size(345, 21);
            this.solutionComboBox.TabIndex = 0;
            this.solutionComboBox.SelectedIndexChanged += new System.EventHandler(this.solutionComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Solution:";
            // 
            // solutionPanel
            // 
            this.solutionPanel.Controls.Add(this.label1);
            this.solutionPanel.Controls.Add(this.solutionComboBox);
            this.solutionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.solutionPanel.Location = new System.Drawing.Point(0, 0);
            this.solutionPanel.Name = "solutionPanel";
            this.solutionPanel.Size = new System.Drawing.Size(853, 28);
            this.solutionPanel.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.customActionListView);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(853, 615);
            this.splitContainer1.SplitterDistance = 525;
            this.splitContainer1.TabIndex = 3;
            // 
            // customActionListView
            // 
            this.customActionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.customActionListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customActionListView.FullRowSelect = true;
            this.customActionListView.HideSelection = false;
            this.customActionListView.Location = new System.Drawing.Point(0, 34);
            this.customActionListView.Name = "customActionListView";
            this.customActionListView.Size = new System.Drawing.Size(525, 581);
            this.customActionListView.TabIndex = 2;
            this.customActionListView.UseCompatibleStateImageBehavior = false;
            this.customActionListView.View = System.Windows.Forms.View.Details;
            this.customActionListView.SelectedIndexChanged += new System.EventHandler(this.customActionListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Custom Action Name";
            this.columnHeader1.Width = 159;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Message Name";
            this.columnHeader2.Width = 162;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(525, 34);
            this.panel1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(517, 26);
            this.label2.TabIndex = 0;
            this.label2.Text = "Custom Actions in Solution";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 34);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(324, 581);
            this.propertyGrid.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.convertButton);
            this.panel2.Controls.Add(this.selectedActionLabel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(4);
            this.panel2.Size = new System.Drawing.Size(324, 34);
            this.panel2.TabIndex = 2;
            // 
            // convertButton
            // 
            this.convertButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.convertButton.Enabled = false;
            this.convertButton.Location = new System.Drawing.Point(245, 4);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(75, 26);
            this.convertButton.TabIndex = 1;
            this.convertButton.Text = "Convert";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
            // 
            // selectedActionLabel
            // 
            this.selectedActionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedActionLabel.Location = new System.Drawing.Point(4, 4);
            this.selectedActionLabel.Name = "selectedActionLabel";
            this.selectedActionLabel.Size = new System.Drawing.Size(316, 26);
            this.selectedActionLabel.TabIndex = 0;
            this.selectedActionLabel.Text = "Select a Custom Action to convert";
            this.selectedActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.solutionPanel);
            this.Name = "PluginControl";
            this.Size = new System.Drawing.Size(853, 643);
            this.solutionPanel.ResumeLayout(false);
            this.solutionPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox solutionComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel solutionPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView customActionListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.Label selectedActionLabel;
    }
}
