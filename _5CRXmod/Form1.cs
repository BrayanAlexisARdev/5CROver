using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _5CRXmod;

public class CassetteData
{
	public string Titulo { get; set; } = "";
	public string Imagen { get; set; } = "";
	public string Contenido { get; set; } = "";
	public string Color { get; set; } = "";
	public string PantallaGif { get; set; } = "";
	public string TemaFondo { get; set; } = "";
	public string TemaTV { get; set; } = "";
}

public class FavoriteData
{
	public int CassetteIndex { get; set; }
	public string CassetteTitle { get; set; } = "";
	public string ColorHex { get; set; } = "";
	public string TemaTV { get; set; } = "";
	public string TemaFondo { get; set; } = "";
}

public partial class Form1 : Form
{
	protected override CreateParams CreateParams
	{
		get
		{
			const int CS_DROPSHADOW = 0x20000;
			CreateParams cp = base.CreateParams;
			cp.ClassStyle |= CS_DROPSHADOW;
			return cp;
		}
	}

	private IContainer components;

	private BufferedPanel timerPanel;

	private BufferedLabel lblHours;

	private BufferedLabel lblMinutes;

	private BufferedLabel lblSeconds;

	private BufferedPanel pnlTopButtons;

	private Button btnP;

	private Button btnS;

	private Panel pnlTimerControls;

	private Button btnTheme;

	private Button btnImage;

	private Button btnColor;

	private Button btnStyle;

	private BufferedPanel cassettesHeaderPanel;

	private Label lblCassettes;
	private TextBox txtCassetteNum;
	private Label lblCassetteTotal;
	private Button btnCassetteList;
	private Button btnFavList;
	private Button btnAddFav;
	private Button btnLearn;
	private Button btnInfo;
	private Panel pnlFullListRow;

	private BufferedPanel tasksHeaderPanel;

	private Label lblTasks;

	private BufferedPanel playerFooterPanel;

	private Label lblM3uTitle;

	private Label lblMetadata;

	private Label lblExtraMetadata;

	private PictureBox picPlayer;

	private PictureBox picPlayerNext;

	private BufferedPanel pnlCassetteContainer;

	private BufferedPanel pnlEqualizer;

	private PictureBox picMainDisplay;

	private FlowLayoutPanel tasksListPanel;

	private Label lblTaskInfo;

	private Panel pnlProgressBg;

	private Panel pnlProgressFill;

	private PictureBox picOverlay;

	private Button btnNextM3u;

	private Button btnPrevM3u;

	private Button btnPlayPlayer;

	private Button btnStopPlayer;

	private Panel pnlVolume;

	private Panel pnlVolumeLine;

	private Panel pnlVolumeThumb;

	private Button btnVolLow;
	private Button btnVolMid;
	private Button btnVolMax;

	private BufferedPanel pnlGrip;

	private Button btnCloseApp;
	private PictureBox picCincross;

	[DllImport("Gdi32.dll")]
	private static extern nint CreateRoundRectRgn(int nLeftRect, int nTopRect, int RightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

	public Form1()
	{
		InitializeComponent();
		DoubleBuffered = true;
		base.FormBorderStyle = FormBorderStyle.None;
		base.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, base.Width, base.Height, 12, 12));
		playerFooterPanel.Paint += delegate(object? s, PaintEventArgs e)
		{
			if (_appColor != Color.Transparent)
			{
				Color overlay = Color.FromArgb(102, _appColor);
				using Brush brush = new SolidBrush(overlay);
				e.Graphics.FillRectangle(brush, 0, 0, playerFooterPanel.Width, playerFooterPanel.Height);
			}
		};
		LoadCustomFont();
		base.Load += Form1_Load;
		btnS.Click += btnS_Click;
		btnP.Click += btnP_Click;
		btnLearn.Click += btnLearn_Click;
		btnColor.Click += delegate
		{
			ShowColorMenu();
		};
		btnImage.Click += delegate
		{
			CycleAni();
		};
		btnTheme.Click += delegate
		{
			CycleThemeImage();
		};
		btnPrevM3u.Click += delegate
		{
			ChangeCassette(-1);
		};
		btnNextM3u.Click += delegate
		{
			ChangeCassette(1);
		};
		txtCassetteNum.KeyDown += txtCassetteNum_KeyDown;
		txtCassetteNum.Leave += txtCassetteNum_Leave;
		btnPlayPlayer.Click += async delegate
		{
			if (_isHlsStream && _hlsPlayer != null)
			{
				if (!_hlsPlayer.IsPlaying && _lastHlsUrl != null)
					await _hlsPlayer.PlayAsync(_lastHlsUrl);
			}
			else
			{
				_wmp?.controls.play();
			}
			_isPlaying = true;
		};
		btnStopPlayer.Click += delegate
		{
			StopM3u();
			_isPlaying = false;
		};
		pnlVolume.MouseDown += delegate(object? s, MouseEventArgs e)
		{
			_isDraggingVolume = true;
			UpdateVolumeFromMouse(e.X);
		};
		pnlVolume.MouseMove += delegate(object? s, MouseEventArgs e)
		{
			if (_isDraggingVolume)
			{
				UpdateVolumeFromMouse(e.X);
			}
		};
		pnlVolume.MouseUp += delegate
		{
			_isDraggingVolume = false;
		};
		pnlVolumeThumb.MouseDown += delegate
		{
			_isDraggingVolume = true;
		};
		pnlVolumeThumb.MouseMove += delegate(object? s, MouseEventArgs e)
		{
			if (_isDraggingVolume)
			{
				UpdateVolumeFromMouse(pnlVolumeThumb.Left + e.X + pnlVolumeLine.Left);
			}
		};
		pnlVolumeThumb.MouseUp += delegate
		{
			_isDraggingVolume = false;
		};
		pnlVolumeThumb.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Brush brush = new SolidBrush(Color.White);
			int w = pnlVolumeThumb.Width;
			int h = pnlVolumeThumb.Height;
			Point[] triangle = new Point[3]
			{
				new Point(w / 2, 0),
				new Point(0, h),
				new Point(w, h)
			};
			e.Graphics.FillPolygon(brush, triangle);
		};
		btnVolLow.Click += delegate { SetVolumePreset(3); };
		btnVolMid.Click += delegate { SetVolumePreset(9); };
		btnVolMax.Click += delegate { SetVolumePreset(15); };
		_slideTimer = new Timer
		{
			Interval = 125
		};
		_slideTimer.Tick += _fadeTimer_Tick;
		_eqTimer = new Timer
		{
			Interval = 50
		};
		_eqTimer.Tick += delegate
		{
			_eqAnimationOffset += 0.15f;
			pnlEqualizer.Invalidate();
			pnlProgressFill?.Invalidate();
		};
		pnlEqualizer.Paint += pnlEqualizer_Paint;
		btnColor.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnImage.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnTheme.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnP.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnS.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnPrevM3u.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnNextM3u.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnVolLow.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnVolMid.BackColor = Color.FromArgb(0, 0, 0, 0);
		btnVolMax.BackColor = Color.FromArgb(0, 0, 0, 0);
		picPlayer.BackColor = Color.Transparent;
		picPlayerNext.BackColor = Color.Transparent;
		lblHours.BackColor = Color.Transparent;
		lblMinutes.BackColor = Color.Transparent;
		lblSeconds.BackColor = Color.Transparent;
		picPlayer.SizeMode = PictureBoxSizeMode.Zoom;
		picPlayer.Size = new Size(140, 88);
		picPlayer.Left = (pnlCassetteContainer.Width - 140) / 2;
		picPlayer.Top = 1;
		picPlayerNext.SizeMode = PictureBoxSizeMode.Zoom;
		picPlayerNext.Size = new Size(140, 88);
		lblTasks.Click += delegate(object? s, EventArgs e)
		{
			btnAddTask_Click(s, e);
		};
		lblHours.Click += delegate(object? s, EventArgs e)
		{
			btnAddTime_Click(s, e);
		};
		lblMinutes.Click += delegate(object? s, EventArgs e)
		{
			btnAddTime_Click(s, e);
		};
		lblSeconds.Click += delegate(object? s, EventArgs e)
		{
			btnAddTime_Click(s, e);
		};
		lblHours.Paint += delegate(object? s, PaintEventArgs e)
		{
			DrawGlow(s, e);
		};
		lblMinutes.Paint += delegate(object? s, PaintEventArgs e)
		{
			DrawGlow(s, e);
		};
		lblSeconds.Paint += delegate(object? s, PaintEventArgs e)
		{
			DrawGlow(s, e);
		};
		pnlGrip.MouseDown += delegate(object? s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_isDraggingForm = true;
				_dragStartOffset = e.Location;
			}
		};
		pnlGrip.MouseMove += delegate(object? s, MouseEventArgs e)
		{
			pnlGrip.Cursor = Cursors.SizeAll;
			if (_isDraggingForm)
			{
				Point point = PointToScreen(e.Location);
				base.Location = new Point(point.X - _dragStartOffset.X, point.Y - _dragStartOffset.Y);
			}
		};
		pnlGrip.MouseUp += delegate
		{
			_isDraggingForm = false;
		};
		lblM3uTitle.Visible = true;
		lblMetadata.Visible = true;
		lblExtraMetadata.Visible = true;
		base.Paint += delegate(object? s, PaintEventArgs e)
		{
			if (_currentThemeImage != null)
			{
				e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
				e.Graphics.SmoothingMode = SmoothingMode.None;
				int x = 0;
				int y = base.Height - _currentThemeImage.Height - 60;
				e.Graphics.DrawImage(_currentThemeImage, x, y, _currentThemeImage.Width, _currentThemeImage.Height);
			}
		};
		pnlGrip.Height = 20;
		timerPanel.Paint += delegate(object? s, PaintEventArgs e)
		{
			if (_appColor != Color.Transparent)
			{
				Color overlay = Color.FromArgb(102, _appColor);
				using Brush brush = new SolidBrush(overlay);
				e.Graphics.FillRectangle(brush, 0, 0, timerPanel.Width, timerPanel.Height);
			}
		};
		UpdateTimerDisplay();
	}

	private void pnlGrip_Paint(object? sender, PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int lineHeight = 2;
		int lineSpacing = 4;
		int totalHeight = 3 * lineHeight + 2 * lineSpacing;
		int startY = (pnlGrip.Height - totalHeight) / 2;
		using Pen linePen = new Pen(Color.FromArgb(120, 120, 120), lineHeight);
		linePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
		linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
		int margin = 80;
		for (int i = 0; i < 3; i++)
		{
			int y = startY + i * (lineHeight + lineSpacing);
			e.Graphics.DrawLine(linePen, margin, y, pnlGrip.Width - margin, y);
		}
	}

	private void btnCloseApp_Click(object? sender, EventArgs e)
	{
		Application.Exit();
	}

	private void ShowColorMenu()
	{
		ContextMenuStrip menu = new ContextMenuStrip();
		menu.BackColor = Color.FromArgb(40, 40, 40);
		menu.ForeColor = Color.White;

		ToolStripMenuItem blancoItem = new ToolStripMenuItem("BLANCO");
		blancoItem.Click += delegate { ApplyAppColor(Color.White); };
		menu.Items.Add(blancoItem);

		ToolStripMenuItem negroItem = new ToolStripMenuItem("NEGRO");
		negroItem.Click += delegate { ApplyAppColor(Color.Black); };
		menu.Items.Add(negroItem);

		menu.Items.Add(new ToolStripSeparator());

		ToolStripMenuItem primarios = new ToolStripMenuItem("PRIMARIOS");
		ToolStripMenuItem rojo = new ToolStripMenuItem("ROJO", null, delegate { ApplyAppColor(Color.Red); });
		ToolStripMenuItem azul = new ToolStripMenuItem("AZUL", null, delegate { ApplyAppColor(Color.Blue); });
		ToolStripMenuItem amarillo = new ToolStripMenuItem("AMARILLO", null, delegate { ApplyAppColor(Color.Yellow); });
		primarios.DropDownItems.Add(rojo);
		primarios.DropDownItems.Add(azul);
		primarios.DropDownItems.Add(amarillo);
		menu.Items.Add(primarios);

		ToolStripMenuItem secundarios = new ToolStripMenuItem("SECUNDARIOS");
		ToolStripMenuItem verde = new ToolStripMenuItem("VERDE", null, delegate { ApplyAppColor(Color.Lime); });
		ToolStripMenuItem naranja = new ToolStripMenuItem("NARANJA", null, delegate { ApplyAppColor(Color.Orange); });
		ToolStripMenuItem violeta = new ToolStripMenuItem("VIOLETA", null, delegate { ApplyAppColor(Color.Violet); });
		secundarios.DropDownItems.Add(verde);
		secundarios.DropDownItems.Add(naranja);
		secundarios.DropDownItems.Add(violeta);
		menu.Items.Add(secundarios);

		menu.Items.Add(new ToolStripSeparator());

		ToolStripMenuItem cyan = new ToolStripMenuItem("CYAN", null, delegate { ApplyAppColor(Color.Cyan); });
		ToolStripMenuItem magenta = new ToolStripMenuItem("MAGENTA", null, delegate { ApplyAppColor(Color.Magenta); });
		ToolStripMenuItem coral = new ToolStripMenuItem("CORAL", null, delegate { ApplyAppColor(Color.FromArgb(255, 127, 80)); });
		ToolStripMenuItem teal = new ToolStripMenuItem("TEAL", null, delegate { ApplyAppColor(Color.Teal); });
		ToolStripMenuItem rosa = new ToolStripMenuItem("ROSA", null, delegate { ApplyAppColor(Color.HotPink); });
		ToolStripMenuItem dorado = new ToolStripMenuItem("DORADO", null, delegate { ApplyAppColor(Color.FromArgb(255, 215, 0)); });
		menu.Items.Add(cyan);
		menu.Items.Add(magenta);
		menu.Items.Add(coral);
		menu.Items.Add(teal);
		menu.Items.Add(rosa);
		menu.Items.Add(dorado);

		menu.Show(btnColor, new Point(0, btnColor.Height));
	}

	private void Form1_Load(object? sender, EventArgs e)
	{
		base.StartPosition = FormStartPosition.Manual;
		Screen screen = Screen.PrimaryScreen;
		int screenWidth = screen?.WorkingArea.Width ?? 1920;
		base.Location = new Point(screenWidth - base.Width, 150);
		lblCassettes.Text = "CASSETTES";
		txtCassetteNum.Text = "0";
		lblCassetteTotal.Text = "/0";
		_eqTimer?.Start();
		this.BeginInvoke(new Action(() => LoadAllData()));
	}

	private void LoadAllData()
	{
		Screen screen = Screen.PrimaryScreen;
		LoadCassetteMaster();
		lblCassettes.Text = "CASSETTES";
		txtCassetteNum.Text = "0";
		lblCassetteTotal.Text = $"/{_cassettes.Count}";
		InitPlayer();
		LayoutCassetteHeader();
		try
		{
			string imgDir = PathHelper.GetImgDir();
			if (Directory.Exists(imgDir))
			{
				for (int j = 1; j <= 5; j++)
				{
					string pPath = Path.Combine(imgDir, $"cass00{j}.png");
					if (File.Exists(pPath))
					{
						try
						{
							_cassetteImages.Add(PathHelper.LoadImage(pPath));
						}
						catch (Exception ex)
						{
							Logger.Error("Form1.LoadAllData.CassetteImage", ex);
						}
					}
				}
				string[] files = Directory.GetFiles(imgDir, "*_ANI.gif");
				foreach (string file in files)
				{
					if (file.EndsWith("noise_ANI.gif", StringComparison.OrdinalIgnoreCase) || file.EndsWith("noise_ANIgif.gif", StringComparison.OrdinalIgnoreCase))
					{
						_noiseFile = file;
					}
					else
					{
						_aniFiles.Add(file);
					}
				}
				if (_aniFiles.Count > 0)
				{
					int defaultIndex = _aniFiles.FindIndex((string f) => f.EndsWith("cat_ANI.gif", StringComparison.OrdinalIgnoreCase));
					if (defaultIndex != -1)
					{
						_currentAniIndex = defaultIndex;
					}
					picMainDisplay.ImageLocation = _aniFiles[_currentAniIndex];
					picMainDisplay.SizeMode = PictureBoxSizeMode.Zoom;
					picMainDisplay.BackColor = Color.Transparent;
				}
				files = Directory.GetFiles(imgDir, "*_TH.*");
				foreach (string file2 in files)
				{
					try
					{
					Image img = PathHelper.LoadImage(file2);
					_themeImages.Add(img);
						_themeFilePaths.Add(file2);
						if (file2.EndsWith("05_TH.gif", StringComparison.OrdinalIgnoreCase))
						{
							_currentThemeImageIndex = _themeImages.Count - 1;
							_currentThemeImage = img;
							BackgroundImage = null;
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Form1.LoadAllData.ThemeImage", ex);
					}
				}
				if (_currentThemeImage != null)
				{
					UpdateTVFrame(_themeFilePaths[_currentThemeImageIndex]);
				}
				string spritePath = Path.Combine(imgDir, "sprite_s.png");
				if (File.Exists(spritePath))
				{
					LoadSpriteSheet(spritePath, 160, 160, 15);
					picOverlay.Parent = picMainDisplay;
					picOverlay.Location = new Point(0, 0);
					picOverlay.Size = picMainDisplay.Size;
					picOverlay.SizeMode = PictureBoxSizeMode.Zoom;
					picOverlay.BackColor = Color.Transparent;
					_spriteTimer = new Timer
					{
						Interval = 100
					};
					_spriteTimer.Tick += delegate
					{
						if (_spriteFrames.Count != 0)
						{
							_currentSpriteFrame = (_currentSpriteFrame + 1) % _spriteFrames.Count;
							picOverlay.Image = _spriteFrames[_currentSpriteFrame];
						}
					};
					_spriteTimer.Start();
				}
				string tvPath = Path.Combine(imgDir, "old_TV.gif");
				if (File.Exists(tvPath))
				{
				Image tvImg = PathHelper.LoadImage(tvPath);
				base.Width = tvImg.Width;
				timerPanel.Height = tvImg.Height - 4;
				timerPanel.BackgroundImage = tvImg;
				timerPanel.BackgroundImageLayout = ImageLayout.None;
				timerPanel.BackColor = Color.Transparent;
				pnlTopButtons.BackColor = Color.Transparent;
				pnlTimerControls.BackColor = Color.Transparent;
				picMainDisplay.BackColor = Color.Transparent;
					pnlTimerControls.BackColor = Color.Transparent;
					btnP.BackColor = Color.Transparent;
					btnS.BackColor = Color.Transparent;
					base.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, base.Width, base.Height, 12, 12));
					if (screen != null)
					{
						base.Location = new Point(screen.WorkingArea.Width - base.Width, 150);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Form1.LoadAllData", ex);
		}
		Panel pnlTaskInfo = new Panel
		{
			Width = base.Width,
			Height = 60,
			BackColor = Color.FromArgb(40, 40, 40),
			Margin = new Padding(0),
			Name = "pnlTaskInfo"
		};
		lblTaskInfo = new Label
		{
			Text = "NO TASK",
			TextAlign = ContentAlignment.MiddleCenter,
			Dock = DockStyle.Fill,
			ForeColor = Color.Gray,
			Font = new Font("Segoe UI", 8f, FontStyle.Bold)
		};
		pnlTaskInfo.Controls.Add(lblTaskInfo);
		pnlProgressBg = new Panel
		{
			Height = 14,
			BackColor = Color.FromArgb(25, 25, 25),
			Name = "pnlProgressBg"
		};
		pnlProgressFill = new Panel
		{
			Height = 14,
			Width = 0,
			Dock = DockStyle.Left,
			BackColor = Color.Transparent,
			Name = "pnlProgressFill"
		};
		int dotCount = 20;
		pnlProgressBg.Paint += (s, pe) =>
		{
			pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			int pw = pnlProgressBg.Width;
			int ph = pnlProgressBg.Height;
			int spacing = pw > 12 ? (pw - 12) / (dotCount - 1) : 10;
			int cy = ph / 2;
			int r = 3;
			for (int i = 0; i < dotCount; i++)
			{
				int cx = 6 + i * spacing;
				pe.Graphics.FillEllipse(Brushes.Black, cx - r, cy - r, r * 2, r * 2);
				pe.Graphics.DrawEllipse(new Pen(Color.FromArgb(40, 40, 40), 1f), cx - r, cy - r, r * 2, r * 2);
			}
		};
		pnlProgressFill.Paint += (s, pe) =>
		{
			pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			int pw = pnlProgressBg.Width;
			int ph = pnlProgressFill.Height;
			int fw = pnlProgressFill.Width;
			int spacing = pw > 12 ? (pw - 12) / (dotCount - 1) : 10;
			int cy = ph / 2;
			int r = 3;
			Color c = _appColor == Color.Transparent || _appColor == Color.Empty ? Color.White : _appColor;
			float lum = 0.299f * c.R + 0.587f * c.G + 0.114f * c.B;
			Color neonColor = lum < 80 ? Color.White : c;

			for (int i = 0; i < dotCount; i++)
			{
				int cx = 6 + i * spacing;
				int dotLeft = cx - r;
				int dotRight = cx + r;
				float flicker = 0.7f + 0.3f * MathF.Sin(_eqAnimationOffset + i * 1.7f);

				if (dotRight <= fw)
				{
					int glowAlpha = (int)(140 * flicker);
					using var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, neonColor));
					pe.Graphics.FillEllipse(glowBrush, cx - r - 4, cy - r - 4, r * 2 + 8, r * 2 + 8);
					using var dotBrush = new SolidBrush(neonColor);
					pe.Graphics.FillEllipse(dotBrush, cx - r, cy - r, r * 2, r * 2);
				}
				else if (dotLeft < fw)
				{
					float overlap = (float)(fw - dotLeft) / (dotRight - dotLeft);
					int alpha = (int)((80 + 175f * overlap) * flicker);
					using var glowBrush = new SolidBrush(Color.FromArgb(alpha / 2, neonColor));
					pe.Graphics.FillEllipse(glowBrush, cx - r - 4, cy - r - 4, r * 2 + 8, r * 2 + 8);
					using var dotBrush = new SolidBrush(Color.FromArgb(alpha, neonColor));
					pe.Graphics.FillEllipse(dotBrush, cx - r, cy - r, r * 2, r * 2);
				}
				else break;
			}
		};
		pnlProgressBg.Dock = DockStyle.Bottom;
		pnlProgressBg.Controls.Add(pnlProgressFill);
		pnlTaskInfo.Controls.Add(pnlProgressBg);
		tasksListPanel.Controls.Add(pnlTaskInfo);
		AddTaskToPanel("CODE", TimeSpan.FromMinutes(30L), "", "code_TSK.png", isFixed: true);
		AddTaskToPanel("EXER", TimeSpan.FromMinutes(30L), "", "exerc_TSK.png", isFixed: true);
		AddTaskToPanel("WORK", TimeSpan.FromMinutes(30L), "", "progress_TSK.png", isFixed: true);
		AddTaskToPanel("RLAX", TimeSpan.FromMinutes(30L), "", "music_TSK.png", isFixed: true);
		var loaded = LearningData.Load();
		if (loaded != null)
			_learningData = loaded;
		ApplyCurrentTheme();
		LoadThemesFile();
		if (_cassettes.Count > 0)
		{
			ApplyCassette(0);
		}
		else if (_m3uFiles.Count > 0)
		{
			LoadCurrentM3u();
		}
		SetVolumePreset(3);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_learningData?.Save();
			_hlsPlayer?.Dispose();
			_slideTimer?.Dispose();
			_eqTimer?.Dispose();
			_spriteTimer?.Dispose();
			_metaTimer?.Dispose();
			_m3u8WatchTimer?.Dispose();
			_pfc?.Dispose();
			_currentThemeImage?.Dispose();
			foreach (var img in _themeImages) img.Dispose();
			_themeImages.Clear();
			foreach (var img in _cassetteImages) img.Dispose();
			_cassetteImages.Clear();
			foreach (var bmp in _spriteFrames) bmp.Dispose();
			_spriteFrames.Clear();
			if (_wmp != null)
			{
				try { System.Runtime.InteropServices.Marshal.ReleaseComObject(_wmp); }
				catch (Exception ex) { Logger.Error("Form1.Dispose.Wmp", ex); }
				_wmp = null;
			}
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.timerPanel = new BufferedPanel();
		this.picOverlay = new System.Windows.Forms.PictureBox();
		this.picMainDisplay = new System.Windows.Forms.PictureBox();
		this.pnlTimerControls = new System.Windows.Forms.Panel();
		this.btnTheme = new System.Windows.Forms.Button();
		this.btnImage = new System.Windows.Forms.Button();
		this.btnStyle = new System.Windows.Forms.Button();
		this.btnColor = new System.Windows.Forms.Button();
		this.pnlTopButtons = new BufferedPanel();
		this.lblHours = new BufferedLabel();
		this.lblMinutes = new BufferedLabel();
		this.lblSeconds = new BufferedLabel();
		this.btnP = new System.Windows.Forms.Button();
		this.btnS = new System.Windows.Forms.Button();
		this.countdownTimer = new System.Windows.Forms.Timer(this.components);
		this.tasksHeaderPanel = new BufferedPanel();
		this.lblTasks = new System.Windows.Forms.Label();
		this.cassettesHeaderPanel = new BufferedPanel();
		this.lblCassettes = new System.Windows.Forms.Label();
		this.playerFooterPanel = new BufferedPanel();
		this.pnlEqualizer = new BufferedPanel();
		this.pnlVolume = new System.Windows.Forms.Panel();
		this.btnStopPlayer = new System.Windows.Forms.Button();
		this.btnPlayPlayer = new System.Windows.Forms.Button();
		this.pnlVolumeLine = new System.Windows.Forms.Panel();
		this.pnlVolumeThumb = new System.Windows.Forms.Panel();
		this.btnVolLow = new System.Windows.Forms.Button();
		this.btnVolMid = new System.Windows.Forms.Button();
		this.btnVolMax = new System.Windows.Forms.Button();
		this.btnNextM3u = new System.Windows.Forms.Button();
		this.btnPrevM3u = new System.Windows.Forms.Button();
		this.pnlCassetteContainer = new BufferedPanel();
		this.picPlayer = new System.Windows.Forms.PictureBox();
		this.picPlayerNext = new System.Windows.Forms.PictureBox();
		this.lblM3uTitle = new System.Windows.Forms.Label();
		this.lblMetadata = new System.Windows.Forms.Label();
		this.lblExtraMetadata = new System.Windows.Forms.Label();
		this.tasksListPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.pnlGrip = new BufferedPanel();
		this.btnCloseApp = new System.Windows.Forms.Button();
		this.picCincross = new System.Windows.Forms.PictureBox();
		this.timerPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picPlayer).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picPlayerNext).BeginInit();
		base.SuspendLayout();
		this.pnlGrip.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
		this.pnlGrip.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlGrip.Height = 20;
		this.pnlGrip.Name = "pnlGrip";
		this.pnlGrip.TabIndex = 6;
		this.pnlGrip.Paint += new System.Windows.Forms.PaintEventHandler(pnlGrip_Paint);
		this.btnCloseApp.BackColor = System.Drawing.Color.Black;
		this.btnCloseApp.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.btnCloseApp.FlatAppearance.BorderSize = 0;
		this.btnCloseApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCloseApp.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnCloseApp.ForeColor = System.Drawing.Color.White;
		this.btnCloseApp.Height = 30;
		this.btnCloseApp.Name = "btnCloseApp";
		this.btnCloseApp.TabIndex = 7;
		this.btnCloseApp.Text = "X CLOSE";
		this.btnCloseApp.UseVisualStyleBackColor = false;
		this.btnCloseApp.Click += new System.EventHandler(btnCloseApp_Click);
		string cincrossPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "files", "img", "cincross.png");
		if (System.IO.File.Exists(cincrossPath))
		{
			this.picCincross.Image = PathHelper.LoadImage(cincrossPath);
		}
		this.picCincross.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.picCincross.Height = 60;
		this.picCincross.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.picCincross.Name = "picCincross";
		this.picCincross.TabIndex = 8;
		this.picCincross.TabStop = false;
		this.timerPanel.BackColor = System.Drawing.Color.Black;
		this.timerPanel.Controls.Add(this.picOverlay);
		this.timerPanel.Controls.Add(this.picMainDisplay);
		this.timerPanel.Controls.Add(this.pnlTimerControls);
		this.timerPanel.Controls.Add(this.pnlTopButtons);
		this.timerPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.timerPanel.Location = new System.Drawing.Point(0, 0);
		this.timerPanel.Name = "timerPanel";
		this.timerPanel.Size = new System.Drawing.Size(260, 200);
		this.timerPanel.TabIndex = 0;
		this.picOverlay.BackColor = System.Drawing.Color.Transparent;
		this.picOverlay.Location = new System.Drawing.Point(10, 40);
		this.picOverlay.Name = "picOverlay";
		this.picOverlay.Size = new System.Drawing.Size(160, 160);
		this.picOverlay.TabIndex = 6;
		this.picOverlay.TabStop = false;
		this.picMainDisplay.BackColor = System.Drawing.Color.Black;
		this.picMainDisplay.Location = new System.Drawing.Point(10, 40);
		this.picMainDisplay.Name = "picMainDisplay";
		this.picMainDisplay.Size = new System.Drawing.Size(160, 160);
		this.picMainDisplay.TabIndex = 5;
		this.picMainDisplay.TabStop = false;
		this.pnlTimerControls.Controls.Add(this.btnTheme);
		this.pnlTimerControls.Controls.Add(this.btnImage);
		this.pnlTimerControls.Controls.Add(this.btnColor);
		this.pnlTimerControls.Dock = System.Windows.Forms.DockStyle.Right;
		this.pnlTimerControls.Location = new System.Drawing.Point(180, 40);
		this.pnlTimerControls.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
		this.pnlTimerControls.Name = "pnlTimerControls";
		this.pnlTimerControls.Size = new System.Drawing.Size(80, 160);
		this.pnlTimerControls.TabIndex = 4;
		this.btnTheme.FlatAppearance.BorderSize = 0;
		this.btnTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnTheme.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnTheme.ForeColor = System.Drawing.Color.White;
		this.btnTheme.Location = new System.Drawing.Point(0, 50);
		this.btnTheme.Name = "btnTheme";
		this.btnTheme.Size = new System.Drawing.Size(80, 25);
		this.btnTheme.TabIndex = 3;
		this.btnTheme.Text = "THEME";
		this.btnTheme.UseVisualStyleBackColor = true;
		this.btnAddFav = new Button();
		this.btnAddFav.FlatAppearance.BorderSize = 0;
		this.btnAddFav.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnAddFav.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnAddFav.ForeColor = System.Drawing.Color.White;
		this.btnAddFav.Location = new System.Drawing.Point(0, 75);
		this.btnAddFav.Name = "btnAddFav";
		this.btnAddFav.Size = new System.Drawing.Size(80, 25);
		this.btnAddFav.TabIndex = 4;
		this.btnAddFav.Text = "ADD FAV";
		this.btnAddFav.UseVisualStyleBackColor = true;
		this.btnAddFav.Click += new EventHandler(btnAddFav_Click);
		this.pnlTimerControls.Controls.Add(this.btnAddFav);
		this.btnLearn = new Button();
		this.btnLearn.FlatAppearance.BorderSize = 0;
		this.btnLearn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnLearn.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnLearn.ForeColor = System.Drawing.Color.White;
		this.btnLearn.Location = new System.Drawing.Point(0, 100);
		this.btnLearn.Name = "btnLearn";
		this.btnLearn.Size = new System.Drawing.Size(80, 25);
		this.btnLearn.TabIndex = 6;
		this.btnLearn.Text = "LEARN";
		this.btnLearn.UseVisualStyleBackColor = true;
		this.pnlTimerControls.Controls.Add(this.btnLearn);
		this.btnInfo = new Button();
		this.btnInfo.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
		this.btnInfo.FlatAppearance.BorderSize = 0;
		this.btnInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnInfo.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnInfo.ForeColor = System.Drawing.Color.White;
		this.btnInfo.Location = new System.Drawing.Point(0, 125);
		this.btnInfo.Name = "btnInfo";
		this.btnInfo.Size = new System.Drawing.Size(80, 25);
		this.btnInfo.TabIndex = 7;
		this.btnInfo.Text = "+ INFO";
		this.btnInfo.UseVisualStyleBackColor = false;
		this.btnInfo.Click += new EventHandler(btnInfo_Click);
		this.pnlTimerControls.Controls.Add(this.btnInfo);
		this.btnImage.FlatAppearance.BorderSize = 0;
		this.btnImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnImage.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnImage.ForeColor = System.Drawing.Color.White;
		this.btnImage.Location = new System.Drawing.Point(0, 25);
		this.btnImage.Name = "btnImage";
		this.btnImage.Size = new System.Drawing.Size(80, 25);
		this.btnImage.TabIndex = 2;
		this.btnImage.Text = "IMAGE";
		this.btnImage.UseVisualStyleBackColor = true;
		this.btnColor.FlatAppearance.BorderSize = 0;
		this.btnColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnColor.Font = new System.Drawing.Font("Segoe UI", 6f);
		this.btnColor.ForeColor = System.Drawing.Color.White;
		this.btnColor.Location = new System.Drawing.Point(0, 0);
		this.btnColor.Name = "btnColor";
		this.btnColor.Size = new System.Drawing.Size(80, 25);
		this.btnColor.TabIndex = 0;
		this.btnColor.Text = "COLOR";
		this.btnColor.UseVisualStyleBackColor = true;
		this.pnlTopButtons.BackColor = System.Drawing.Color.Black;
		this.pnlTopButtons.Controls.Add(this.lblHours);
		this.pnlTopButtons.Controls.Add(this.lblMinutes);
		this.pnlTopButtons.Controls.Add(this.lblSeconds);
		this.pnlTopButtons.Controls.Add(this.btnP);
		this.pnlTopButtons.Controls.Add(this.btnS);
		this.pnlTopButtons.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlTopButtons.Location = new System.Drawing.Point(0, 0);
		this.pnlTopButtons.Name = "pnlTopButtons";
		this.pnlTopButtons.Size = new System.Drawing.Size(260, 40);
		this.pnlTopButtons.TabIndex = 3;
		this.lblHours.AutoSize = true;
		this.lblHours.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.lblHours.ForeColor = System.Drawing.Color.White;
		this.lblHours.Location = new System.Drawing.Point(14, 5);
		this.lblHours.Name = "lblHours";
		this.lblHours.Size = new System.Drawing.Size(42, 32);
		this.lblHours.TabIndex = 2;
		this.lblHours.Text = "00";
		this.lblMinutes.AutoSize = true;
		this.lblMinutes.Font = new System.Drawing.Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
		this.lblMinutes.ForeColor = System.Drawing.Color.White;
		this.lblMinutes.Location = new System.Drawing.Point(64, 2);
		this.lblMinutes.Name = "lblMinutes";
		this.lblMinutes.Size = new System.Drawing.Size(56, 45);
		this.lblMinutes.TabIndex = 3;
		this.lblMinutes.Text = "00";
		this.lblSeconds.AutoSize = true;
		this.lblSeconds.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.lblSeconds.ForeColor = System.Drawing.Color.White;
		this.lblSeconds.Location = new System.Drawing.Point(124, 5);
		this.lblSeconds.Name = "lblSeconds";
		this.lblSeconds.Size = new System.Drawing.Size(42, 32);
		this.lblSeconds.TabIndex = 4;
		this.lblSeconds.Text = "00";
		this.btnP.BackColor = System.Drawing.Color.Black;
		this.btnP.Dock = System.Windows.Forms.DockStyle.Right;
		this.btnP.FlatAppearance.BorderSize = 0;
		this.btnP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnP.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.btnP.ForeColor = System.Drawing.Color.White;
		this.btnP.Location = new System.Drawing.Point(180, 0);
		this.btnP.Name = "btnP";
		this.btnP.Size = new System.Drawing.Size(40, 40);
		this.btnP.TabIndex = 1;
		this.btnP.Text = "▶";
		this.btnP.UseVisualStyleBackColor = false;
		this.btnS.BackColor = System.Drawing.Color.Black;
		this.btnS.Dock = System.Windows.Forms.DockStyle.Right;
		this.btnS.FlatAppearance.BorderSize = 0;
		this.btnS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnS.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.btnS.ForeColor = System.Drawing.Color.White;
		this.btnS.Location = new System.Drawing.Point(220, 0);
		this.btnS.Name = "btnS";
		this.btnS.Size = new System.Drawing.Size(40, 40);
		this.btnS.TabIndex = 0;
		this.btnS.Text = "⏹";
		this.btnS.UseVisualStyleBackColor = false;
		this.countdownTimer.Interval = 1000;
		this.countdownTimer.Tick += new System.EventHandler(countdownTimer_Tick);
		this.tasksHeaderPanel.BackColor = System.Drawing.Color.LightGray;
		this.tasksHeaderPanel.Controls.Add(this.lblTasks);
		this.tasksHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.tasksHeaderPanel.Location = new System.Drawing.Point(0, 385);
		this.tasksHeaderPanel.Name = "tasksHeaderPanel";
		this.tasksHeaderPanel.Size = new System.Drawing.Size(260, 25);
		this.tasksHeaderPanel.TabIndex = 2;
		this.lblTasks.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblTasks.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.lblTasks.Location = new System.Drawing.Point(0, 0);
		this.lblTasks.Name = "lblTasks";
		this.lblTasks.Size = new System.Drawing.Size(260, 25);
		this.lblTasks.TabIndex = 1;
		this.lblTasks.Text = "TASKS";
		this.lblTasks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cassettesHeaderPanel.BackColor = System.Drawing.Color.LightGray;
		this.cassettesHeaderPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.cassettesHeaderPanel.Location = new System.Drawing.Point(0, 200);
		this.cassettesHeaderPanel.Name = "cassettesHeaderPanel";
		this.cassettesHeaderPanel.Size = new System.Drawing.Size(260, 28);
		this.cassettesHeaderPanel.TabIndex = 4;

		this.lblCassettes.AutoSize = true;
		this.lblCassettes.Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
		this.lblCassettes.Location = new System.Drawing.Point(5, 4);
		this.lblCassettes.Name = "lblCassettes";
		this.lblCassettes.Text = "CASSETTES";
		this.lblCassettes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

		this.txtCassetteNum = new TextBox();
		this.txtCassetteNum.BackColor = System.Drawing.Color.Black;
		this.txtCassetteNum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCassetteNum.Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
		this.txtCassetteNum.ForeColor = System.Drawing.Color.White;
		this.txtCassetteNum.Location = new System.Drawing.Point(110, 2);
		this.txtCassetteNum.Name = "txtCassetteNum";
		this.txtCassetteNum.Size = new System.Drawing.Size(28, 23);
		this.txtCassetteNum.TabIndex = 1;
		this.txtCassetteNum.Text = "1";
		this.txtCassetteNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

		this.lblCassetteTotal = new Label();
		this.lblCassetteTotal.AutoSize = true;
		this.lblCassetteTotal.Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
		this.lblCassetteTotal.Location = new System.Drawing.Point(148, 4);
		this.lblCassetteTotal.Name = "lblCassetteTotal";
		this.lblCassetteTotal.Text = "/0";
		this.lblCassetteTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

		this.btnCassetteList = new Button();
		this.btnCassetteList.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
		this.btnCassetteList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCassetteList.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnCassetteList.ForeColor = System.Drawing.Color.White;
		this.btnCassetteList.Location = new System.Drawing.Point(145, 0);
		this.btnCassetteList.Name = "btnCassetteList";
		this.btnCassetteList.Size = new System.Drawing.Size(110, 28);
		this.btnCassetteList.TabIndex = 2;
		this.btnCassetteList.Text = "FULL LIST";
		this.btnCassetteList.UseVisualStyleBackColor = false;
		this.btnCassetteList.Click += new EventHandler(this.btnCassetteList_Click);

		this.btnFavList = new Button();
		this.btnFavList.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
		this.btnFavList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnFavList.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnFavList.ForeColor = System.Drawing.Color.White;
		this.btnFavList.Location = new System.Drawing.Point(30, 0);
		this.btnFavList.Name = "btnFavList";
		this.btnFavList.Size = new System.Drawing.Size(110, 28);
		this.btnFavList.TabIndex = 3;
		this.btnFavList.Text = "FAV LIST";
		this.btnFavList.UseVisualStyleBackColor = false;
		this.btnFavList.Click += new EventHandler(this.btnFavList_Click);

		this.cassettesHeaderPanel.Controls.Add(this.lblCassetteTotal);
		this.cassettesHeaderPanel.Controls.Add(this.txtCassetteNum);
		this.cassettesHeaderPanel.Controls.Add(this.lblCassettes);

		this.pnlFullListRow = new Panel();
		this.pnlFullListRow.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
		this.pnlFullListRow.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlFullListRow.Height = 28;
		this.pnlFullListRow.Name = "pnlFullListRow";
		this.pnlFullListRow.Controls.Add(this.btnFavList);
		this.pnlFullListRow.Controls.Add(this.btnCassetteList);

		this.playerFooterPanel.BackColor = System.Drawing.Color.White;
		this.playerFooterPanel.Controls.Add(this.pnlEqualizer);
		this.playerFooterPanel.Controls.Add(this.pnlVolume);
		this.playerFooterPanel.Controls.Add(this.btnNextM3u);
		this.playerFooterPanel.Controls.Add(this.btnPrevM3u);
		this.playerFooterPanel.Controls.Add(this.pnlCassetteContainer);
		this.playerFooterPanel.Controls.Add(this.lblM3uTitle);
		this.playerFooterPanel.Controls.Add(this.lblMetadata);
		this.playerFooterPanel.Controls.Add(this.lblExtraMetadata);
		this.playerFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.playerFooterPanel.Location = new System.Drawing.Point(0, 200);
		this.playerFooterPanel.Name = "playerFooterPanel";
		this.playerFooterPanel.Size = new System.Drawing.Size(260, 250);
		this.playerFooterPanel.TabIndex = 3;
		this.pnlEqualizer.BackColor = System.Drawing.Color.Transparent;
		this.pnlEqualizer.Location = new System.Drawing.Point(28, 93);
		this.pnlEqualizer.Name = "pnlEqualizer";
		this.pnlEqualizer.Size = new System.Drawing.Size(204, 26);
		this.pnlEqualizer.TabIndex = 6;
		this.pnlVolume.Controls.Add(this.btnVolLow);
		this.pnlVolume.Controls.Add(this.btnVolMid);
		this.pnlVolume.Controls.Add(this.btnVolMax);
		this.pnlVolume.Controls.Add(this.btnPlayPlayer);
		this.pnlVolume.Controls.Add(this.btnStopPlayer);
		this.pnlVolume.Controls.Add(this.pnlVolumeLine);
		this.pnlVolume.Controls.Add(this.pnlVolumeThumb);
		this.pnlVolume.Location = new System.Drawing.Point(5, 180);
		this.pnlVolume.Name = "pnlVolume";
		this.pnlVolume.Size = new System.Drawing.Size(250, 60);
		this.pnlVolume.TabIndex = 5;
		this.btnVolLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolLow.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Bold);
		this.btnVolLow.ForeColor = System.Drawing.Color.White;
		this.btnVolLow.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolLow.Location = new System.Drawing.Point(5, 3);
		this.btnVolLow.Name = "btnVolLow";
		this.btnVolLow.Size = new System.Drawing.Size(28, 22);
		this.btnVolLow.Text = "LOW";
		this.btnVolMid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolMid.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnVolMid.ForeColor = System.Drawing.Color.White;
		this.btnVolMid.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolMid.Location = new System.Drawing.Point(35, 3);
		this.btnVolMid.Name = "btnVolMid";
		this.btnVolMid.Size = new System.Drawing.Size(28, 22);
		this.btnVolMid.Text = "MID";
		this.btnVolMax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolMax.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnVolMax.ForeColor = System.Drawing.Color.White;
		this.btnVolMax.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolMax.Location = new System.Drawing.Point(65, 3);
		this.btnVolMax.Name = "btnVolMax";
		this.btnVolMax.Size = new System.Drawing.Size(28, 22);
		this.btnVolMax.Text = "MAX";
		this.btnPlayPlayer.FlatAppearance.BorderSize = 0;
		this.btnPlayPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPlayPlayer.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnPlayPlayer.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnPlayPlayer.ForeColor = System.Drawing.Color.White;
		this.btnPlayPlayer.Location = new System.Drawing.Point(5, 32);
		this.btnPlayPlayer.Name = "btnPlayPlayer";
		this.btnPlayPlayer.Size = new System.Drawing.Size(48, 20);
		this.btnPlayPlayer.TabIndex = 2;
		this.btnPlayPlayer.Text = "PLAY";
		this.btnPlayPlayer.UseVisualStyleBackColor = false;
		this.btnPlayPlayer.Padding = new System.Windows.Forms.Padding(0);
		this.btnStopPlayer.FlatAppearance.BorderSize = 0;
		this.btnStopPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStopPlayer.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnStopPlayer.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnStopPlayer.ForeColor = System.Drawing.Color.White;
		this.btnStopPlayer.Location = new System.Drawing.Point(53, 32);
		this.btnStopPlayer.Name = "btnStopPlayer";
		this.btnStopPlayer.Size = new System.Drawing.Size(48, 20);
		this.btnStopPlayer.TabIndex = 3;
		this.btnStopPlayer.Text = "STOP";
		this.btnStopPlayer.UseVisualStyleBackColor = false;
		this.btnStopPlayer.Padding = new System.Windows.Forms.Padding(0);
		this.pnlVolumeLine.BackColor = System.Drawing.Color.Gray;
		this.pnlVolumeLine.Controls.Add(this.pnlVolumeThumb);
		this.pnlVolumeLine.Location = new System.Drawing.Point(105, 38);
		this.pnlVolumeLine.Name = "pnlVolumeLine";
		this.pnlVolumeLine.Size = new System.Drawing.Size(140, 4);
		this.pnlVolumeLine.TabIndex = 0;
		this.pnlVolumeThumb.BackColor = System.Drawing.Color.White;
		this.pnlVolumeThumb.Location = new System.Drawing.Point(0, -4);
		this.pnlVolumeThumb.Name = "pnlVolumeThumb";
		this.pnlVolumeThumb.Size = new System.Drawing.Size(12, 12);
		this.pnlVolumeThumb.TabIndex = 0;
		this.btnNextM3u.FlatAppearance.BorderSize = 0;
		this.btnNextM3u.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnNextM3u.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.btnNextM3u.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnNextM3u.Location = new System.Drawing.Point(220, 20);
		this.btnNextM3u.Name = "btnNextM3u";
		this.btnNextM3u.Size = new System.Drawing.Size(30, 40);
		this.btnNextM3u.TabIndex = 3;
		this.btnNextM3u.Text = "\u25B6";
		this.btnNextM3u.UseVisualStyleBackColor = true;
		this.btnPrevM3u.FlatAppearance.BorderSize = 0;
		this.btnPrevM3u.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPrevM3u.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
		this.btnPrevM3u.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnPrevM3u.Location = new System.Drawing.Point(10, 20);
		this.btnPrevM3u.Name = "btnPrevM3u";
		this.btnPrevM3u.Size = new System.Drawing.Size(30, 40);
		this.btnPrevM3u.TabIndex = 2;
		this.btnPrevM3u.Text = "\u25C0";
		this.btnPrevM3u.UseVisualStyleBackColor = true;
		this.pnlCassetteContainer.BackColor = System.Drawing.Color.Transparent;
		this.pnlCassetteContainer.Controls.Add(this.picPlayer);
		this.pnlCassetteContainer.Controls.Add(this.picPlayerNext);
		this.pnlCassetteContainer.Location = new System.Drawing.Point(60, 0);
		this.pnlCassetteContainer.Name = "pnlCassetteContainer";
		this.pnlCassetteContainer.Size = new System.Drawing.Size(140, 90);
		this.pnlCassetteContainer.TabIndex = 1;
		this.picPlayer.BackColor = System.Drawing.Color.Transparent;
		this.picPlayer.Location = new System.Drawing.Point(0, 0);
		this.picPlayer.Name = "picPlayer";
		this.picPlayer.Size = new System.Drawing.Size(140, 90);
		this.picPlayer.TabIndex = 0;
		this.picPlayer.TabStop = false;
		this.picPlayerNext.BackColor = System.Drawing.Color.Transparent;
		this.picPlayerNext.Location = new System.Drawing.Point(140, 0);
		this.picPlayerNext.Name = "picPlayerNext";
		this.picPlayerNext.Size = new System.Drawing.Size(140, 88);
		this.picPlayerNext.TabIndex = 1;
		this.picPlayerNext.TabStop = false;
		this.lblM3uTitle.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.lblM3uTitle.Location = new System.Drawing.Point(10, 124);
		this.lblM3uTitle.Name = "lblM3uTitle";
		this.lblM3uTitle.Size = new System.Drawing.Size(240, 16);
		this.lblM3uTitle.TabIndex = 0;
		this.lblM3uTitle.Text = "M3U TITLE";
		this.lblM3uTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblMetadata.Font = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold);
		this.lblMetadata.Location = new System.Drawing.Point(10, 141);
		this.lblMetadata.Name = "lblMetadata";
		this.lblMetadata.Size = new System.Drawing.Size(240, 15);
		this.lblMetadata.TabIndex = 6;
		this.lblMetadata.Text = "SONG TITLE";
		this.lblMetadata.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblExtraMetadata.Font = new System.Drawing.Font("Segoe UI", 6.5f);
		this.lblExtraMetadata.Location = new System.Drawing.Point(10, 157);
		this.lblExtraMetadata.Name = "lblExtraMetadata";
		this.lblExtraMetadata.Size = new System.Drawing.Size(240, 15);
		this.lblExtraMetadata.TabIndex = 7;
		this.lblExtraMetadata.Text = "ARTIST - ALBUM";
		this.lblExtraMetadata.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.tasksListPanel.AutoScroll = true;
		this.tasksListPanel.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.tasksListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tasksListPanel.Location = new System.Drawing.Point(0, 410);
		this.tasksListPanel.Name = "tasksListPanel";
		this.tasksListPanel.Size = new System.Drawing.Size(260, 840);
		this.tasksListPanel.TabIndex = 5;

		base.AutoScaleDimensions = new System.Drawing.SizeF(10f, 25f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(260, 825);
		base.Controls.Add(this.cassettesHeaderPanel);
		base.Controls.Add(this.pnlFullListRow);
		base.Controls.Add(this.playerFooterPanel);
		base.Controls.Add(this.btnCloseApp);
		base.Controls.Add(this.picCincross);
		base.Controls.Add(this.tasksListPanel);
		base.Controls.Add(this.tasksHeaderPanel);
		base.Controls.Add(this.pnlGrip);
		base.Controls.Add(this.timerPanel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.Name = "Form1";
		this.Text = "5CRXmod";
		base.TopMost = true;
		this.timerPanel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picOverlay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picMainDisplay).EndInit();
		this.pnlTimerControls.ResumeLayout(false);
		this.pnlTopButtons.ResumeLayout(false);
		this.pnlTopButtons.PerformLayout();
		this.tasksHeaderPanel.ResumeLayout(false);
		this.playerFooterPanel.ResumeLayout(false);
		this.pnlVolume.ResumeLayout(false);
		this.pnlVolume.PerformLayout();
		this.pnlVolumeLine.ResumeLayout(false);
		this.pnlCassetteContainer.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picPlayer).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picPlayerNext).EndInit();
		base.ResumeLayout(false);
	}
}
