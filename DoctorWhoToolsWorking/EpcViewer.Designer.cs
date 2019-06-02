namespace DoctorWhoToolsWorking
{
    partial class EpcViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EpcViewer));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEpcFileepcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.NumFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FSizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TexNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tex_num_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MenuStripUpload = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.uploadFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.MenuStripUpload.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.extractToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1200, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEpcFileepcToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openEpcFileepcToolStripMenuItem
            // 
            this.openEpcFileepcToolStripMenuItem.Name = "openEpcFileepcToolStripMenuItem";
            this.openEpcFileepcToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openEpcFileepcToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.openEpcFileepcToolStripMenuItem.Text = "Open Epc file (*.epc)";
            this.openEpcFileepcToolStripMenuItem.Click += new System.EventHandler(this.openEpcFileepcToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allFilesToolStripMenuItem,
            this.texturesOnlyToolStripMenuItem});
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.extractToolStripMenuItem.Text = "Extract";
            // 
            // allFilesToolStripMenuItem
            // 
            this.allFilesToolStripMenuItem.Name = "allFilesToolStripMenuItem";
            this.allFilesToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.allFilesToolStripMenuItem.Text = "All files";
            this.allFilesToolStripMenuItem.Click += new System.EventHandler(this.allFilesToolStripMenuItem_Click);
            // 
            // texturesOnlyToolStripMenuItem
            // 
            this.texturesOnlyToolStripMenuItem.Name = "texturesOnlyToolStripMenuItem";
            this.texturesOnlyToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            this.texturesOnlyToolStripMenuItem.Text = "Textures only";
            this.texturesOnlyToolStripMenuItem.Click += new System.EventHandler(this.texturesOnlyToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NumFileColumn,
            this.FSizeColumn,
            this.FileTypeColumn,
            this.TexNameColumn,
            this.tex_num_column});
            this.dataGridView1.ContextMenuStrip = this.MenuStripUpload;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 28);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(1200, 426);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            // 
            // NumFileColumn
            // 
            this.NumFileColumn.HeaderText = "Filenum";
            this.NumFileColumn.Name = "NumFileColumn";
            // 
            // FSizeColumn
            // 
            this.FSizeColumn.HeaderText = "File Size";
            this.FSizeColumn.Name = "FSizeColumn";
            // 
            // FileTypeColumn
            // 
            this.FileTypeColumn.HeaderText = "Type of file";
            this.FileTypeColumn.Name = "FileTypeColumn";
            // 
            // TexNameColumn
            // 
            this.TexNameColumn.HeaderText = "Texture name";
            this.TexNameColumn.Name = "TexNameColumn";
            // 
            // tex_num_column
            // 
            this.tex_num_column.HeaderText = "Texture code";
            this.tex_num_column.Name = "tex_num_column";
            this.tex_num_column.ReadOnly = true;
            // 
            // MenuStripUpload
            // 
            this.MenuStripUpload.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuStripUpload.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uploadFileToolStripMenuItem,
            this.exportFileToolStripMenuItem});
            this.MenuStripUpload.Name = "MenuStripUpload";
            this.MenuStripUpload.Size = new System.Drawing.Size(150, 56);
            // 
            // uploadFileToolStripMenuItem
            // 
            this.uploadFileToolStripMenuItem.Name = "uploadFileToolStripMenuItem";
            this.uploadFileToolStripMenuItem.Size = new System.Drawing.Size(149, 26);
            this.uploadFileToolStripMenuItem.Text = "Import file";
            this.uploadFileToolStripMenuItem.Click += new System.EventHandler(this.uploadFileToolStripMenuItem_Click);
            // 
            // exportFileToolStripMenuItem
            // 
            this.exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            this.exportFileToolStripMenuItem.Size = new System.Drawing.Size(149, 26);
            this.exportFileToolStripMenuItem.Text = "Export file";
            this.exportFileToolStripMenuItem.Click += new System.EventHandler(this.exportFileToolStripMenuItem_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(231, 12);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(353, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // EpcViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 454);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EpcViewer";
            this.Text = "Epc Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EpcViewer_FormClosing);
            this.Load += new System.EventHandler(this.EpcViewer_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.MenuStripUpload.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openEpcFileepcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesOnlyToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ContextMenuStrip MenuStripUpload;
        private System.Windows.Forms.ToolStripMenuItem uploadFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFileToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumFileColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FSizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TexNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tex_num_column;
    }
}