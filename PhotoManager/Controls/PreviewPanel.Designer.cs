namespace PhotoManager.Controls;

partial class PreviewPanel
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlTop;
    private Label lblFileInfo;
    private Label lblPosition;
    private Panel pnlBottom;
    private Button btnPrev;
    private Button btnNext;
    private Button btnSort;
    private Button btnAction;
    private Panel pnlImage;
    private PictureBox picBox;
    private Label lblError;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        pnlTop = new Panel();
        lblFileInfo = new Label();
        lblPosition = new Label();
        pnlBottom = new Panel();
        btnPrev = new Button();
        btnNext = new Button();
        btnSort = new Button();
        btnAction = new Button();
        pnlImage = new Panel();
        picBox = new PictureBox();
        lblError = new Label();

        pnlTop.SuspendLayout();
        pnlBottom.SuspendLayout();
        pnlImage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picBox).BeginInit();
        SuspendLayout();

        // pnlTop
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 30;
        pnlTop.Padding = new Padding(4, 4, 4, 0);
        pnlTop.Controls.Add(lblFileInfo);
        pnlTop.Controls.Add(lblPosition);

        // lblPosition
        lblPosition.Dock = DockStyle.Right;
        lblPosition.Width = 80;
        lblPosition.TextAlign = ContentAlignment.MiddleRight;
        lblPosition.ForeColor = SystemColors.GrayText;

        // lblFileInfo
        lblFileInfo.Dock = DockStyle.Fill;
        lblFileInfo.TextAlign = ContentAlignment.MiddleLeft;

        // pnlBottom
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 44;
        pnlBottom.Padding = new Padding(4, 4, 4, 4);
        pnlBottom.Controls.Add(btnAction);
        pnlBottom.Controls.Add(btnSort);
        pnlBottom.Controls.Add(btnNext);
        pnlBottom.Controls.Add(btnPrev);

        // btnPrev
        btnPrev.Text = "◀ Prev";
        btnPrev.Width = 80;
        btnPrev.Dock = DockStyle.Left;
        btnPrev.Click += OnPrevClick;

        // btnNext
        btnNext.Text = "Next ▶";
        btnNext.Width = 80;
        btnNext.Dock = DockStyle.Left;
        btnNext.Click += OnNextClick;

        // btnAction
        btnAction.Text = string.Empty;
        btnAction.Width = 130;
        btnAction.Dock = DockStyle.Right;
        btnAction.Click += OnActionClick;

        // btnSort
        btnSort.Text = "Name ↑";
        btnSort.Width = 80;
        btnSort.Dock = DockStyle.Right;
        btnSort.Click += OnSortClick;

        // pnlImage
        pnlImage.Dock = DockStyle.Fill;
        pnlImage.Controls.Add(picBox);
        pnlImage.Controls.Add(lblError);

        // picBox
        picBox.Dock = DockStyle.Fill;
        picBox.SizeMode = PictureBoxSizeMode.Zoom;
        picBox.Visible = false;

        // lblError
        picBox.BackColor = Color.Black;
        lblError.Dock = DockStyle.Fill;
        lblError.TextAlign = ContentAlignment.MiddleCenter;
        lblError.ForeColor = SystemColors.GrayText;
        lblError.Visible = false;

        // PreviewPanel
        Controls.Add(pnlImage);
        Controls.Add(pnlBottom);
        Controls.Add(pnlTop);
        Dock = DockStyle.Fill;

        pnlTop.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        pnlImage.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picBox).EndInit();
        ResumeLayout(false);
    }
}
