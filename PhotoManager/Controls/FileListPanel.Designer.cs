namespace PhotoManager.Controls;

partial class FileListPanel
{
    private System.ComponentModel.IContainer components = null;
    internal ToolStrip toolStrip;
    internal ToolStripButton btnSortName;
    internal ToolStripButton btnSortDate;
    internal ListView listView;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        toolStrip = new ToolStrip();
        btnSortName = new ToolStripButton();
        btnSortDate = new ToolStripButton();
        listView = new ListView();

        toolStrip.SuspendLayout();
        SuspendLayout();

        // toolStrip
        toolStrip.Items.AddRange([btnSortName, btnSortDate]);
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;

        // btnSortName
        btnSortName.Text = "Sort by Name ▲";
        btnSortName.CheckOnClick = false;
        btnSortName.Click += OnSortNameClick;

        // btnSortDate
        btnSortDate.Text = "Sort by Date";
        btnSortDate.CheckOnClick = false;
        btnSortDate.Click += OnSortDateClick;

        // listView
        listView.View = View.Details;
        listView.FullRowSelect = true;
        listView.MultiSelect = false;
        listView.HideSelection = false;
        listView.Dock = DockStyle.Fill;
        listView.Columns.AddRange([
            new ColumnHeader { Text = "Name", Width = 300 },
            new ColumnHeader { Text = "Date",  Width = 140 },
            new ColumnHeader { Text = "Size",  Width = 80  }
        ]);
        listView.Click += OnListViewClick;
        listView.DoubleClick += OnListViewDoubleClick;
        listView.ColumnWidthChanged += OnColumnWidthChanged;

        // FileListPanel
        Controls.Add(listView);
        Controls.Add(toolStrip);
        Dock = DockStyle.Fill;

        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
