namespace DoctorWhoToolsWorking
{
    partial class FontEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontEditorForm));
            this.dataGridCoord = new System.Windows.Forms.DataGridView();
            this.id_char = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.symbol_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.x_start_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.x_end_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.y_start_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.y_end_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.width_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.height_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dds_number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.x_offset_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.x_advanced_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.y_offset_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.visible_column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextures = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportTextureddsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTextureddsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importMultitexturesfntToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importOldWorkedMethodfntToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.num_dds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.width_dds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.height_dds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dds_size = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCoord)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTextures)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridCoord
            // 
            this.dataGridCoord.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridCoord.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridCoord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCoord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id_char,
            this.symbol_column,
            this.x_start_column,
            this.x_end_column,
            this.y_start_column,
            this.y_end_column,
            this.width_column,
            this.height_column,
            this.dds_number,
            this.x_offset_column,
            this.x_advanced_column,
            this.y_offset_column,
            this.visible_column});
            this.dataGridCoord.Location = new System.Drawing.Point(22, 286);
            this.dataGridCoord.Name = "dataGridCoord";
            this.dataGridCoord.RowTemplate.Height = 24;
            this.dataGridCoord.Size = new System.Drawing.Size(1149, 371);
            this.dataGridCoord.TabIndex = 3;
            // 
            // id_char
            // 
            this.id_char.HeaderText = "№ char";
            this.id_char.Name = "id_char";
            // 
            // symbol_column
            // 
            this.symbol_column.HeaderText = "Char";
            this.symbol_column.Name = "symbol_column";
            // 
            // x_start_column
            // 
            this.x_start_column.HeaderText = "X start";
            this.x_start_column.Name = "x_start_column";
            // 
            // x_end_column
            // 
            this.x_end_column.HeaderText = "X end";
            this.x_end_column.Name = "x_end_column";
            // 
            // y_start_column
            // 
            this.y_start_column.HeaderText = "Y start";
            this.y_start_column.Name = "y_start_column";
            // 
            // y_end_column
            // 
            this.y_end_column.HeaderText = "Y end";
            this.y_end_column.Name = "y_end_column";
            // 
            // width_column
            // 
            this.width_column.HeaderText = "Width";
            this.width_column.Name = "width_column";
            // 
            // height_column
            // 
            this.height_column.HeaderText = "Height";
            this.height_column.Name = "height_column";
            // 
            // dds_number
            // 
            this.dds_number.HeaderText = "№ DDS";
            this.dds_number.Name = "dds_number";
            // 
            // x_offset_column
            // 
            this.x_offset_column.HeaderText = "X offset";
            this.x_offset_column.Name = "x_offset_column";
            // 
            // x_advanced_column
            // 
            this.x_advanced_column.HeaderText = "X advanced";
            this.x_advanced_column.Name = "x_advanced_column";
            // 
            // y_offset_column
            // 
            this.y_offset_column.HeaderText = "Y offset";
            this.y_offset_column.Name = "y_offset_column";
            // 
            // visible_column
            // 
            this.visible_column.HeaderText = "Visible char (bool)";
            this.visible_column.Name = "visible_column";
            // 
            // dataGridTextures
            // 
            this.dataGridTextures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridTextures.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.num_dds,
            this.width_dds,
            this.height_dds,
            this.dds_size});
            this.dataGridTextures.Location = new System.Drawing.Point(22, 37);
            this.dataGridTextures.Name = "dataGridTextures";
            this.dataGridTextures.ReadOnly = true;
            this.dataGridTextures.RowTemplate.Height = 24;
            this.dataGridTextures.Size = new System.Drawing.Size(531, 220);
            this.dataGridTextures.TabIndex = 4;
            this.dataGridTextures.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridTextures_CellMouseClick);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportTextureddsToolStripMenuItem,
            this.importTextureddsToolStripMenuItem,
            this.importMultitexturesfntToolStripMenuItem,
            this.importOldWorkedMethodfntToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(376, 132);
            // 
            // exportTextureddsToolStripMenuItem
            // 
            this.exportTextureddsToolStripMenuItem.Name = "exportTextureddsToolStripMenuItem";
            this.exportTextureddsToolStripMenuItem.Size = new System.Drawing.Size(375, 32);
            this.exportTextureddsToolStripMenuItem.Text = "Export texture (*.dds)";
            this.exportTextureddsToolStripMenuItem.Click += new System.EventHandler(this.exportTextureddsToolStripMenuItem_Click);
            // 
            // importTextureddsToolStripMenuItem
            // 
            this.importTextureddsToolStripMenuItem.Name = "importTextureddsToolStripMenuItem";
            this.importTextureddsToolStripMenuItem.Size = new System.Drawing.Size(375, 32);
            this.importTextureddsToolStripMenuItem.Text = "Import texture (*.dds)";
            this.importTextureddsToolStripMenuItem.Click += new System.EventHandler(this.importTextureddsToolStripMenuItem_Click);
            // 
            // importMultitexturesfntToolStripMenuItem
            // 
            this.importMultitexturesfntToolStripMenuItem.Name = "importMultitexturesfntToolStripMenuItem";
            this.importMultitexturesfntToolStripMenuItem.Size = new System.Drawing.Size(375, 32);
            this.importMultitexturesfntToolStripMenuItem.Text = "Import multitextures (*.fnt)";
            this.importMultitexturesfntToolStripMenuItem.Click += new System.EventHandler(this.importMultitexturesfntToolStripMenuItem_Click);
            // 
            // importOldWorkedMethodfntToolStripMenuItem
            // 
            this.importOldWorkedMethodfntToolStripMenuItem.Name = "importOldWorkedMethodfntToolStripMenuItem";
            this.importOldWorkedMethodfntToolStripMenuItem.Size = new System.Drawing.Size(375, 32);
            this.importOldWorkedMethodfntToolStripMenuItem.Text = "Import old worked method (*.fnt)";
            this.importOldWorkedMethodfntToolStripMenuItem.Click += new System.EventHandler(this.importOldWorkedMethodfntToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3});
            this.dataGridView1.Location = new System.Drawing.Point(735, 37);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(436, 220);
            this.dataGridView1.TabIndex = 5;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "First char";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column2.HeaderText = "Second char";
            this.Column2.Name = "Column2";
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column3.HeaderText = "Amount";
            this.Column3.Name = "Column3";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(610, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "№ шрифта:";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(598, 77);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(107, 24);
            this.comboBox1.TabIndex = 7;
            this.comboBox1.SelectionChangeCommitted += new System.EventHandler(this.comboBox1_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(556, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "label2";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1191, 36);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 32);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(287, 32);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(287, 32);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(287, 32);
            this.saveAsToolStripMenuItem.Text = "Save As..";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click_1);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(287, 32);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // num_dds
            // 
            this.num_dds.HeaderText = "№ dds";
            this.num_dds.Name = "num_dds";
            this.num_dds.ReadOnly = true;
            this.num_dds.Width = 86;
            // 
            // width_dds
            // 
            this.width_dds.FillWeight = 40.17488F;
            this.width_dds.HeaderText = "Width";
            this.width_dds.Name = "width_dds";
            this.width_dds.ReadOnly = true;
            this.width_dds.Width = 128;
            // 
            // height_dds
            // 
            this.height_dds.FillWeight = 256.4102F;
            this.height_dds.HeaderText = "Height";
            this.height_dds.Name = "height_dds";
            this.height_dds.ReadOnly = true;
            this.height_dds.Width = 128;
            // 
            // dds_size
            // 
            this.dds_size.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dds_size.HeaderText = "Texture size";
            this.dds_size.Name = "dds_size";
            this.dds_size.ReadOnly = true;
            // 
            // FontEditorForm
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1191, 663);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.dataGridTextures);
            this.Controls.Add(this.dataGridCoord);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "FontEditorForm";
            this.Text = "Font Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.FontEditorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCoord)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTextures)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dataGridCoord;
        private System.Windows.Forms.DataGridView dataGridTextures;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem exportTextureddsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTextureddsToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn id_char;
        private System.Windows.Forms.DataGridViewTextBoxColumn symbol_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn x_start_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn x_end_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn y_start_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn y_end_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn width_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn height_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn dds_number;
        private System.Windows.Forms.DataGridViewTextBoxColumn x_offset_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn x_advanced_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn y_offset_column;
        private System.Windows.Forms.DataGridViewTextBoxColumn visible_column;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem importMultitexturesfntToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importOldWorkedMethodfntToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn num_dds;
        private System.Windows.Forms.DataGridViewTextBoxColumn width_dds;
        private System.Windows.Forms.DataGridViewTextBoxColumn height_dds;
        private System.Windows.Forms.DataGridViewTextBoxColumn dds_size;
    }
}