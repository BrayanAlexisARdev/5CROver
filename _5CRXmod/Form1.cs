using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
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

public class Form1 : Form
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

	private TimeSpan _timeRemaining;

	private bool _timerRunning;

	private PrivateFontCollection _pfc = new PrivateFontCollection();

	private Panel? _activeTaskPanel;

	private double _activeTaskTotalSeconds;

	private dynamic? _wmp;

	private Timer? _metaTimer;

	private string _currentM3uName = "";
	private string _currentCassetteTitle = "";

	private string _lastTitle = "";

	private List<string> _aniFiles = new List<string>();

	private int _currentAniIndex;

	private string? _noiseFile;

	private List<Image> _themeImages = new List<Image>();

	private List<string> _themeFilePaths = new List<string>();

	private int _currentThemeImageIndex;

	private Image? _currentThemeImage;

	private bool _isDraggingForm;

	private Point _dragStartOffset;

	private List<Bitmap> _spriteFrames = new List<Bitmap>();

	private int _currentSpriteFrame;

	private Timer? _spriteTimer;

	private int _currentThemeIndex;

	private List<string> _m3uFiles = new List<string>();

	private int _currentM3uIndex;

	private List<Image> _cassetteImages = new List<Image>();

	private List<CassetteData> _cassettes = new List<CassetteData>();

	private int _currentCassetteIndex;

	private Color _cassetteColor = Color.FromArgb(40, 40, 40);

	private Color _appColor = Color.Transparent;

	private bool _isDraggingVolume;

	private Timer? _slideTimer;
	private Timer? _m3u8WatchTimer;

	private int _slideStep = 6;

	private int _slideDirection;

	private const int PictureBoxWidth = 140;

	private const int PictureBoxHeight = 88;

	private Timer? _eqTimer;

	private int[] _eqBars = new int[25];

	private Random _rnd = new Random();

	private readonly (Color timer, Color header, Color list, Color footer)[] _themes = new(Color, Color, Color, Color)[8]
	{
		(Color.FromArgb(60, 60, 60), Color.FromArgb(80, 80, 80), Color.FromArgb(40, 40, 40), Color.FromArgb(50, 50, 50)),
		(Color.RoyalBlue, Color.LightGray, Color.FromArgb(64, 64, 64), Color.White),
		(Color.Black, Color.FromArgb(45, 45, 45), Color.Black, Color.Black),
		(Color.White, Color.FromArgb(240, 240, 240), Color.White, Color.White),
		(Color.Crimson, Color.LightPink, Color.Maroon, Color.White),
		(Color.ForestGreen, Color.LightGreen, Color.DarkGreen, Color.White),
		(Color.Gold, Color.LightYellow, Color.DarkGoldenrod, Color.White),
		(Color.Purple, Color.Plum, Color.Indigo, Color.White)
	};

	private IContainer components;

	private Panel timerPanel;

	private Label lblHours;

	private Label lblMinutes;

	private Label lblSeconds;

	private Panel pnlTopButtons;

	private Button btnP;

	private Button btnS;

	private Panel pnlTimerControls;

	private Button btnTheme;

	private Button btnImage;

	private Button btnStyle;

	private Button btnColor;

	private Timer countdownTimer;

	private Panel cassettesHeaderPanel;

	private Label lblCassettes;

	private Panel tasksHeaderPanel;

	private Label lblTasks;

	private Panel playerFooterPanel;

	private Label lblM3uTitle;

	private Label lblMetadata;

	private Label lblExtraMetadata;

	private PictureBox picPlayer;

	private PictureBox picPlayerNext;

	private Panel pnlCassetteContainer;

	private Panel pnlEqualizer;

	private PictureBox picMainDisplay;

	private FlowLayoutPanel tasksListPanel;

	private PictureBox picOverlay;

	private Button btnNextM3u;

	private Button btnPrevM3u;

	private Button btnPlayPlayer;

	private Button btnStopPlayer;

	private Panel pnlVolume;

	private Panel pnlVolumeLine;

	private Panel pnlVolumeThumb;

	private Panel pnlVolButtons;
	private Button btnVolLow;
	private Button btnVolMid;
	private Button btnVolMax;

	private Label lblVolumeLabel;

	private Panel pnlGrip;

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
		EnableDoubleBuffered(pnlCassetteContainer);
		EnableDoubleBuffered(pnlEqualizer);
		EnableDoubleBuffered(playerFooterPanel);
		playerFooterPanel.Paint += delegate(object? s, PaintEventArgs e)
		{
			if (_appColor != Color.Transparent)
			{
				Color overlay = Color.FromArgb(102, _appColor);
				using Brush brush = new SolidBrush(overlay);
				e.Graphics.FillRectangle(brush, 0, 0, playerFooterPanel.Width, playerFooterPanel.Height);
			}
		};
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(pnlCassetteContainer, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(playerFooterPanel, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		EnableDoubleBuffered(timerPanel);
		EnableDoubleBuffered(pnlTopButtons);
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(timerPanel, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(pnlTopButtons, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(lblHours, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(lblMinutes, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(lblSeconds, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		Control[] array = new Control[6] { timerPanel, pnlGrip, cassettesHeaderPanel, playerFooterPanel, tasksHeaderPanel, pnlTopButtons };
		foreach (Control p in array)
		{
			typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(p, new object[2]
			{
				ControlStyles.SupportsTransparentBackColor,
				true
			});
		}
		LoadCustomFont();
		InitPlayer();
		base.Load += Form1_Load;
		btnS.Click += btnS_Click;
		btnP.Click += btnP_Click;
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
		btnPlayPlayer.Click += delegate
		{
			_wmp?.controls.play();
			_eqTimer?.Start();
		};
		btnStopPlayer.Click += delegate
		{
			StopM3u();
			_eqTimer?.Stop();
			ResetEq();
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
			Interval = 8
		};
		_slideTimer.Tick += _slideTimer_Tick;
		_eqTimer = new Timer
		{
			Interval = 50
		};
		_eqTimer.Tick += delegate
		{
			for (int j = 0; j < _eqBars.Length; j++)
			{
				_eqBars[j] = _rnd.Next(2, pnlEqualizer.Height);
			}
			pnlEqualizer.Invalidate();
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
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(timerPanel, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(pnlTopButtons, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
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

	private void ResetEq()
	{
		for (int i = 0; i < _eqBars.Length; i++)
		{
			_eqBars[i] = 0;
		}
		pnlEqualizer.Invalidate();
	}

	private void pnlEqualizer_Paint(object? sender, PaintEventArgs e)
	{
		if (_eqBars == null)
		{
			return;
		}
		using Brush brush = new SolidBrush(GetContrastColor(playerFooterPanel.BackColor));
		float barWidth = (float)pnlEqualizer.Width / (float)_eqBars.Length;
		for (int i = 0; i < _eqBars.Length; i++)
		{
			float x = (float)i * barWidth;
			float h = _eqBars[i];
			float y = (float)pnlEqualizer.Height - h;
			e.Graphics.FillRectangle(brush, x + 1f, y, barWidth - 2f, h);
		}
	}

	private void EnableDoubleBuffered(Control control)
	{
		typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(control, true, null);
	}

	private void _slideTimer_Tick(object? sender, EventArgs e)
	{
		if (_slideDirection == 1)
		{
			picPlayer.Left += _slideStep;
			picPlayerNext.Left += _slideStep;
			if (picPlayerNext.Left >= 0)
			{
				picPlayerNext.Left = 0;
				picPlayerNext.Width = 140;
				picPlayerNext.Height = 88;
				PictureBox tempPicBox = picPlayer;
				picPlayer = picPlayerNext;
				picPlayerNext = tempPicBox;
				picPlayer.Left = (pnlCassetteContainer.Width - 140) / 2;
				picPlayer.Width = 140;
				picPlayer.Height = 88;
				picPlayerNext.Left = 140;
				picPlayerNext.Width = 140;
				picPlayerNext.Height = 88;
				_slideTimer?.Stop();
			}
		}
		else if (_slideDirection == -1)
		{
			picPlayer.Left -= _slideStep;
			picPlayerNext.Left -= _slideStep;
			if (picPlayerNext.Left <= 0)
			{
				picPlayerNext.Left = 0;
				picPlayerNext.Width = 140;
				picPlayerNext.Height = 88;
				PictureBox tempPicBox2 = picPlayer;
				picPlayer = picPlayerNext;
				picPlayerNext = tempPicBox2;
				picPlayer.Left = (pnlCassetteContainer.Width - 140) / 2;
				picPlayer.Width = 140;
				picPlayer.Height = 88;
				picPlayerNext.Left = -140;
				picPlayerNext.Width = 140;
				picPlayerNext.Height = 88;
				_slideTimer?.Stop();
			}
		}
	}

	private void SetVolumePreset(int percent)
	{
		if (_wmp == null) return;
		_wmp.settings.volume = percent;
		UpdateVolumeVisual(percent);
		btnVolLow.Font = new Font("Segoe UI", 5.5f, percent == 3 ? FontStyle.Bold | FontStyle.Underline : FontStyle.Bold);
		btnVolMid.Font = new Font("Segoe UI", 5.5f, percent == 9 ? FontStyle.Bold | FontStyle.Underline : FontStyle.Bold);
		btnVolMax.Font = new Font("Segoe UI", 5.5f, percent == 15 ? FontStyle.Bold | FontStyle.Underline : FontStyle.Bold);
	}

	private void UpdateVolumeFromMouse(int mouseX)
	{
		int x = Math.Max(0, Math.Min(mouseX - pnlVolumeLine.Left, pnlVolumeLine.Width));
		int thumbCenter = pnlVolumeThumb.Width / 2;
		pnlVolumeThumb.Left = x - thumbCenter;
		if (_wmp != null)
		{
			double raw = (double)x / (double)pnlVolumeLine.Width;
			int volume = (int)(Math.Max(0.03, Math.Min(0.15, raw)) * 100.0);
			_wmp.settings.volume = volume;
		}
	}

	private Image? GetCassetteImageFromM3u(string m3uPath, int indexFallback)
	{
		try
		{
			if (File.Exists(m3uPath))
			{
				string[] array = File.ReadAllLines(m3uPath);
				foreach (string line in array)
				{
					if (line.StartsWith("#CASSETTE:", StringComparison.OrdinalIgnoreCase))
					{
						string imgName = line.Substring("#CASSETTE:".Length).Trim();
						string fullPath = Path.Combine(Path.GetDirectoryName(m3uPath) ?? "", imgName);
						if (File.Exists(fullPath))
						{
							return Image.FromFile(fullPath);
						}
					}
				}
			}
		}
		catch
		{
		}
		if (_cassetteImages.Count > 0)
		{
			return _cassetteImages[indexFallback % _cassetteImages.Count];
		}
		return null;
	}

	private void ChangeM3u(int direction)
	{
		if (_m3uFiles.Count == 0)
		{
			return;
		}
		Timer? slideTimer = _slideTimer;
		if (slideTimer == null || !slideTimer.Enabled)
		{
			_currentM3uIndex = (_currentM3uIndex + direction + _m3uFiles.Count) % _m3uFiles.Count;
			_slideDirection = -direction;
			ResetCassetteTitle();
			string path = _m3uFiles[_currentM3uIndex];
			PlayM3u(path);
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			Image nextImg = GetCassetteImageFromM3u(path, _currentM3uIndex);
			if (nextImg != null)
			{
				picPlayerNext.Image = nextImg;
				picPlayerNext.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayerNext.Size = new Size(140, 88);
				picPlayerNext.Left = ((_slideDirection == 1) ? (-140) : 140);
				_slideTimer?.Start();
			}
		}
	}

	private void LoadCurrentM3u()
	{
		if (_m3uFiles.Count != 0)
		{
			string path = _m3uFiles[_currentM3uIndex];
			PlayM3u(path);
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			Image currentImg = GetCassetteImageFromM3u(path, _currentM3uIndex);
			if (currentImg != null)
			{
				picPlayer.Image = currentImg;
				picPlayer.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayer.Size = new Size(140, 88);
				picPlayer.Left = (pnlCassetteContainer.Width - 140) / 2;
				picPlayer.Top = 1;
				picPlayer.Width = 140;
			}
		}
	}

	private void SetupButtonGlow(Button btn)
	{
		btn.MouseEnter += delegate
		{
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = Color.FromArgb(50, Color.White);
		};
		btn.MouseLeave += delegate
		{
			btn.BackColor = Color.FromArgb(20, 20, 20);
		};
	}

	private void DrawGlow(object? sender, PaintEventArgs e)
	{
		if (!(sender is Label lbl))
		{
			return;
		}
		e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		string text = lbl.Text;
		Font font = lbl.Font;
		Color glowColor = (_appColor != Color.Transparent) ? _appColor : Color.Cyan;
		for (int i = 1; i <= 3; i++)
		{
			using Brush glowBrush = new SolidBrush(Color.FromArgb(50 / i, glowColor));
			int[] array = new int[2]
			{
				-i,
				i
			};
			foreach (int dx in array)
			{
				int[] array2 = new int[2]
				{
					-i,
					i
				};
				foreach (int dy in array2)
				{
					e.Graphics.DrawString(text, font, glowBrush, new PointF(dx, dy));
				}
			}
		}
		using Brush textBrush = new SolidBrush(lbl.ForeColor);
		e.Graphics.DrawString(text, font, textBrush, new PointF(0f, 0f));
	}

	private async void CycleAni()
	{
		if (_aniFiles.Count != 0)
		{
			if (!string.IsNullOrEmpty(_noiseFile))
			{
				picMainDisplay.ImageLocation = _noiseFile;
				picMainDisplay.Refresh();
				await Task.Delay(800);
			}
			_currentAniIndex = (_currentAniIndex + 1) % _aniFiles.Count;
			picMainDisplay.ImageLocation = _aniFiles[_currentAniIndex];
			picMainDisplay.Refresh();
		}
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

	private void ApplyAppColor(Color baseColor)
	{
		_appColor = baseColor;
		Color transparent = Color.FromArgb(102, baseColor);
		Color headerColor = Color.FromArgb(133, baseColor);
		Color listColor = Color.FromArgb(100, ControlPaint.Dark(baseColor));
		Color darkColor = Color.FromArgb(120, ControlPaint.Dark(baseColor));

		try { BackColor = transparent; } catch { }
		timerPanel.BackColor = Color.Transparent;
		pnlTopButtons.BackColor = Color.Transparent;
		pnlTimerControls.BackColor = Color.Transparent;
		tasksHeaderPanel.BackColor = headerColor;
		cassettesHeaderPanel.BackColor = headerColor;
		playerFooterPanel.BackColor = Color.Transparent;
		playerFooterPanel.Invalidate();
		tasksListPanel.BackColor = listColor;
		pnlCassetteContainer.BackColor = Color.Transparent;
		pnlEqualizer.BackColor = darkColor;
		pnlVolume.BackColor = darkColor;

		lblHours.ForeColor = Color.White;
		lblMinutes.ForeColor = Color.White;
		lblSeconds.ForeColor = Color.White;

		UpdateControlContrast(tasksHeaderPanel, headerColor);
		UpdateControlContrast(cassettesHeaderPanel, headerColor);
		UpdateControlContrast(playerFooterPanel, transparent);

		foreach (Control c in tasksListPanel.Controls)
		{
			if (c is Panel p && p.Tag is TaskData)
			{
				p.BackColor = listColor;
				UpdateControlContrast(p, listColor);
			}
			else if (c.Name == "pnlAddSlot")
			{
				c.BackColor = Color.FromArgb(100, ControlPaint.Dark(baseColor));
				foreach (Control child in c.Controls)
				{
					if (child is Button b) b.BackColor = Color.FromArgb(67, ControlPaint.Dark(baseColor));
				}
			}
		}

		timerPanel.Invalidate();
		lblHours.Invalidate();
		lblMinutes.Invalidate();
		lblSeconds.Invalidate();
	}

	private void CycleTheme()
	{
		_currentThemeIndex = (_currentThemeIndex + 1) % _themes.Length;
		ApplyCurrentTheme();
	}

	private List<(string TvPath, string ThPath)> _themesList = new();
	private int _currentCustomThemeIndex;

	private void LoadThemesFile()
	{
		string imgDir = Path.Combine(Application.StartupPath, "files", "img");
		if (!Directory.Exists(imgDir)) imgDir = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", "..")), "files", "img");
		string themeFile = Path.Combine(imgDir, "THEME.txt");
		if (!File.Exists(themeFile)) return;

		_themesList.Clear();
		foreach (string line in File.ReadAllLines(themeFile))
		{
			string t = line.Trim();
			if (t.StartsWith(";") || string.IsNullOrEmpty(t)) continue;
			if (t.StartsWith("[TEMA"))
			{
				int close = t.IndexOf(']');
				string parts = t.Substring(close + 1).Trim();
				string[] arr = parts.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (arr.Length >= 2)
				{
					string tvPath = Path.Combine(imgDir, arr[0]);
					string thPath = Path.Combine(imgDir, arr[1]);
					if (File.Exists(tvPath) && File.Exists(thPath))
						_themesList.Add((tvPath, thPath));
				}
			}
		}
	}

	private void CycleThemeImage()
	{
		if (_themesList.Count == 0) return;
		_currentCustomThemeIndex = (_currentCustomThemeIndex + 1) % _themesList.Count;
		var theme = _themesList[_currentCustomThemeIndex];

		Image tvImg = Image.FromFile(theme.TvPath);
		timerPanel.BackgroundImage = tvImg;
		timerPanel.Height = tvImg.Height - 4;
		timerPanel.BackColor = Color.Transparent;

		string thDir = Path.GetDirectoryName(theme.ThPath) ?? "";
		if (File.Exists(theme.ThPath))
		{
			_currentThemeImage = Image.FromFile(theme.ThPath);
			BackgroundImage = null;
		}

		timerPanel.Invalidate();
		Invalidate();
	}

	private void ResetCassetteTitle()
	{
		_currentCassetteTitle = "";
	}

	private void UpdateTVFrame(string themePath)
	{
		try
		{
			string? path = Path.GetDirectoryName(themePath) ?? "";
			string tvName = "old_TV.gif";
			if (themePath.EndsWith("06_TH.gif", StringComparison.OrdinalIgnoreCase))
			{
				tvName = "steel_TV.gif";
			}
			else if (themePath.EndsWith("03_TH.gif", StringComparison.OrdinalIgnoreCase))
			{
				tvName = "gcard_TV.gif";
			}
			else if (themePath.EndsWith("01_TH.gif", StringComparison.OrdinalIgnoreCase))
			{
				tvName = "bluesk_TV.gif";
			}
			else if (themePath.EndsWith("04_TH.gif", StringComparison.OrdinalIgnoreCase))
			{
				tvName = "flowbk_TV.gif";
			}
			string tvPath = Path.Combine(path, tvName);
			if (File.Exists(tvPath))
			{
				Image img = Image.FromFile(tvPath);
				timerPanel.BackgroundImage = img;
				timerPanel.Height = img.Height - 4;
			}
		}
		catch
		{
		}
	}

	private void ApplyCurrentTheme()
	{
		_appColor = Color.Transparent;
		(Color, Color, Color, Color) theme = _themes[_currentThemeIndex];
		Color darkSolid = Color.FromArgb(20, 20, 20);
		Color bgColor = Color.FromArgb(240, 20, 20, 20);
		Color panelColor = Color.FromArgb(180, 40, 40, 40);
		Color listColor = Color.FromArgb(150, 30, 30, 30);
		try
		{
			BackColor = bgColor;
		}
		catch
		{
		}
		pnlGrip.BackColor = Color.FromArgb(40, 40, 40);
		pnlGrip.Height = 20;
		pnlGrip.BorderStyle = BorderStyle.None;
		Color headerColor = Color.FromArgb(200, 60, 60, 60);
		tasksHeaderPanel.BackColor = headerColor;
		cassettesHeaderPanel.BackColor = headerColor;
		tasksListPanel.BackColor = listColor;
		playerFooterPanel.BackColor = Color.Transparent;
		if (_currentThemeImage != null)
		{
			timerPanel.BackColor = Color.Transparent;
		}
		else
		{
			timerPanel.BackColor = theme.Item1;
		}
		UpdateControlContrast(tasksHeaderPanel, headerColor);
		UpdateControlContrast(cassettesHeaderPanel, headerColor);
		UpdateControlContrast(playerFooterPanel, panelColor);
		UpdateControlContrast(pnlGrip, pnlGrip.BackColor);
		SetSafeBackColor(pnlCassetteContainer, darkSolid);
		SetSafeBackColor(pnlEqualizer, darkSolid);
		SetSafeBackColor(pnlVolume, darkSolid);
	}

	private void SetSafeBackColor(Control c, Color bg)
	{
		try
		{
			c.BackColor = bg;
		}
		catch
		{
		}
	}

	private void UpdateControlContrast(Control parent, Color bg)
	{
		Color fg = (parent.ForeColor = Color.White);
		foreach (Control c in parent.Controls)
		{
			if (c is Label || c is Button || c is TextBox)
			{
				c.ForeColor = fg;
				if (c is Label && c.BackColor == Color.Transparent)
				{
				}
				else
				{
					SetSafeBackColor(c, bg);
				}
				if (c is Button b)
				{
					b.FlatAppearance.BorderColor = fg;
					b.ForeColor = fg;
				}
			}
			if (c.HasChildren)
			{
				UpdateControlContrast(c, bg);
			}
		}
	}

	private Color GetContrastColor(Color bg)
	{
		return Color.White;
	}

	private void InitPlayer()
	{
		try
		{
			Type type = Type.GetTypeFromProgID("WMPlayer.OCX.7");
			if (type != null)
			{
				_wmp = Activator.CreateInstance(type);
				_wmp.settings.autoStart = true;
				_m3u8WatchTimer = new Timer
				{
					Interval = 3000
				};
				_m3u8WatchTimer.Tick += delegate
				{
					if (_wmp == null) return;
					try
					{
						int state = (int)_wmp.playState;
						string url = _wmp.URL ?? "";
						if (state == 1 || state == 8)
						{
							if (url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
							{
								string saved = url;
								_wmp.URL = saved;
								_wmp.controls.play();
							}
						}
					}
					catch { }
				};
				_metaTimer = new Timer
				{
					Interval = 2000
				};
				_metaTimer.Tick += delegate
				{
					UpdateMetadata();
				};
			}
		}
		catch
		{
		}
	}

	private void UpdateMetadata()
	{
		if (_wmp == null)
		{
			return;
		}
		try
		{
			dynamic media = _wmp.currentMedia;
			if (!((media != null) ? true : false))
			{
				return;
			}
			string title = media.getItemInfo("Title");
			string artist = media.getItemInfo("Author");
			string album = media.getItemInfo("Album");
			string genre = media.getItemInfo("Genre");
			string src = media.sourceURL;
			if (string.IsNullOrEmpty(title))
			{
				if (!string.IsNullOrEmpty(src))
				{
					try
					{
						title = Path.GetFileNameWithoutExtension(src);
					}
					catch
					{
						title = src;
					}
				}
				if (string.IsNullOrEmpty(title))
				{
					title = media.name;
				}
			}
			if (!string.IsNullOrEmpty(title) && title != _lastTitle)
			{
				_lastTitle = title;
				lblM3uTitle.Text = _currentCassetteTitle;
				lblMetadata.Text = title.ToUpper();
				string line2 = ((!string.IsNullOrEmpty(artist)) ? artist : genre);
				if (!string.IsNullOrEmpty(album))
				{
					line2 = ((!string.IsNullOrEmpty(line2)) ? (line2 + " - " + album) : album);
				}
				lblExtraMetadata.Text = line2.ToUpper();
			}
		}
		catch
		{
		}
	}

	private void LoadCustomFont()
	{
		string dcPath = Path.Combine(Application.StartupPath, "files", "typo", "dc.ttf");
		if (File.Exists(dcPath))
		{
			_pfc.AddFontFile(dcPath);
			lblHours.Font = new Font(_pfc.Families[0], 11f, FontStyle.Bold);
			lblMinutes.Font = new Font(_pfc.Families[0], 15f, FontStyle.Bold);
			lblSeconds.Font = new Font(_pfc.Families[0], 11f, FontStyle.Bold);
		}
		string[] excludes = new string[13]
		{
			"lblHours", "lblMinutes", "lblSeconds", "btnP", "btnS", "btnColor", "btnStyle", "btnImage", "btnTheme", "btnPrevM3u",
			"btnNextM3u", "btnPlayPlayer", "btnStopPlayer"
		};
		FontHelper.ApplyFont(this, 10f, FontStyle.Regular, excludes);
		if (FontHelper.CustomFontFamily != null)
		{
			lblTasks.Font = new Font(FontHelper.CustomFontFamily, 9f, FontStyle.Bold);
			lblCassettes.Font = new Font(FontHelper.CustomFontFamily, 9f, FontStyle.Bold);
			lblM3uTitle.Font = new Font(FontHelper.CustomFontFamily, 8f, FontStyle.Bold);
			lblMetadata.Font = new Font(FontHelper.CustomFontFamily, 6.5f, FontStyle.Regular);
			lblExtraMetadata.Font = new Font(FontHelper.CustomFontFamily, 6f, FontStyle.Regular);
			lblVolumeLabel.Font = new Font(FontHelper.CustomFontFamily, 6f, FontStyle.Bold);
			Font miniFont = new Font(FontHelper.CustomFontFamily, 6f, FontStyle.Regular);
			btnColor.Font = miniFont;
			btnStyle.Font = miniFont;
			btnImage.Font = miniFont;
			btnTheme.Font = miniFont;
			btnPlayPlayer.Font = miniFont;
			btnStopPlayer.Font = miniFont;
		}
	}

	private void btnAddTask_Click(object? sender, EventArgs e)
	{
		using AddTaskForm addTaskForm = new AddTaskForm();
		if (addTaskForm.ShowDialog() == DialogResult.OK)
		{
			AddTaskToPanel(addTaskForm.TaskName, addTaskForm.TaskTime, addTaskForm.M3uPath, addTaskForm.SelectedIcon);
		}
	}

	private void AddTaskToPanel(string taskName, TimeSpan taskTime, string m3uPath, string iconName = "tasks_TSK.png", bool isFixed = false)
	{
		(Color, Color, Color, Color) theme = _themes[_currentThemeIndex];
		Panel taskPanel = new Panel
		{
			Height = 60,
			Width = base.Width / 2,
			BackColor = (isFixed ? Color.FromArgb(216, theme.Item2) : theme.Item2),
			Margin = new Padding(0),
			BorderStyle = BorderStyle.None,
			Tag = new TaskData
			{
				Time = taskTime,
				M3uPath = m3uPath,
				IsFixed = isFixed
			}
		};
		Panel pnlProgressBg = new Panel
		{
			Height = 4,
			Dock = DockStyle.Bottom,
			BackColor = Color.Black,
			Name = "pnlProgressBg"
		};
		Panel pnlProgressFill = new Panel
		{
			Height = 4,
			Width = 0,
			Dock = DockStyle.Left,
			BackColor = (isFixed ? Color.Cyan : Color.Magenta),
			Name = "pnlProgressFill"
		};
		pnlProgressBg.Controls.Add(pnlProgressFill);
		PictureBox picTask = new PictureBox
		{
			Size = new Size(32, 32),
			SizeMode = PictureBoxSizeMode.Zoom,
			BackColor = Color.Transparent,
			Location = new Point(5, 12)
		};
		try
		{
			string baseDir = Application.StartupPath;
			for (int i = 0; i < 5; i++)
			{
				string testPath = Path.Combine(baseDir, "files", "img", iconName);
				if (File.Exists(testPath))
				{
					picTask.Image = Image.FromFile(testPath);
					break;
				}
				baseDir = Directory.GetParent(baseDir)?.FullName ?? baseDir;
			}
		}
		catch
		{
		}
		Label lblTaskName = new Label
		{
			Text = taskName.ToUpper(),
			Location = new Point(42, 8),
			AutoSize = false,
			Size = new Size(taskPanel.Width - 45, 20),
			BackColor = Color.Transparent,
			Font = ((FontHelper.CustomFontFamily != null) ? new Font(FontHelper.CustomFontFamily, 8f, FontStyle.Bold) : new Font("Segoe UI", 8f, FontStyle.Bold))
		};
		string timeText = "";
		timeText = ((!(taskTime.TotalHours >= 1.0)) ? $"{(int)taskTime.TotalMinutes} MIN" : $"{(int)taskTime.TotalHours}H {taskTime.Minutes}M");
		Label lblTaskTime = new Label
		{
			Text = timeText,
			Location = new Point(42, 28),
			AutoSize = true,
			BackColor = Color.Transparent,
			Font = ((FontHelper.CustomFontFamily != null) ? new Font(FontHelper.CustomFontFamily, 7f, FontStyle.Regular) : new Font("Segoe UI", 7f, FontStyle.Regular))
		};
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(lblTaskName, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(lblTaskTime, new object[2]
		{
			ControlStyles.SupportsTransparentBackColor,
			true
		});
		taskPanel.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		lblTaskName.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		lblTaskTime.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		picTask.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		ContextMenuStrip menu = new ContextMenuStrip();
		Action<int> setTime = delegate(int m)
		{
			TimeSpan timeSpan = TimeSpan.FromMinutes(m);
			((TaskData)taskPanel.Tag).Time = timeSpan;
			lblTaskTime.Text = $"{m} MIN";
			if (_activeTaskPanel == taskPanel)
			{
				_timeRemaining = timeSpan;
				_activeTaskTotalSeconds = timeSpan.TotalSeconds;
				UpdateTimerDisplay();
			}
		};
		menu.Items.Add("15 MIN", null, delegate
		{
			setTime(15);
		});
		menu.Items.Add("30 MIN", null, delegate
		{
			setTime(30);
		});
		menu.Items.Add("1 HORA", null, delegate
		{
			setTime(60);
		});
		menu.Items.Add("2 HORAS", null, delegate
		{
			setTime(120);
		});
		ToolStripMenuItem itemMas = new ToolStripMenuItem("MAS...");
		itemMas.Click += delegate
		{
			using SetTimeForm setTimeForm = new SetTimeForm();
			if (setTimeForm.ShowDialog() == DialogResult.OK)
			{
				TimeSpan time = new TimeSpan(setTimeForm.SelectedHours, setTimeForm.SelectedMinutes, 0);
				((TaskData)taskPanel.Tag).Time = time;
				lblTaskTime.Text = $"{(int)time.TotalMinutes} MIN";
			}
		};
		menu.Items.Add(itemMas);
		menu.Items.Add(new ToolStripSeparator());
		ToolStripMenuItem itemEdit = new ToolStripMenuItem("EDITAR");
		itemEdit.Click += delegate
		{
			using AddTaskForm addTaskForm = new AddTaskForm();
			if (addTaskForm.ShowDialog() == DialogResult.OK)
			{
				lblTaskName.Text = addTaskForm.TaskName.Substring(0, Math.Min(4, addTaskForm.TaskName.Length)).ToUpper();
				((TaskData)taskPanel.Tag).Time = addTaskForm.TaskTime;
				((TaskData)taskPanel.Tag).M3uPath = addTaskForm.M3uPath;
				lblTaskTime.Text = $"{(int)addTaskForm.TaskTime.TotalMinutes} MIN";
				try
				{
					string text = Application.StartupPath;
					string text2 = "";
					for (int j = 0; j < 5; j++)
					{
						string text3 = Path.Combine(text, "files", "img", addTaskForm.SelectedIcon);
						if (File.Exists(text3))
						{
							text2 = text3;
							break;
						}
						text = Directory.GetParent(text)?.FullName ?? text;
					}
					if (File.Exists(text2))
					{
						picTask.Image = Image.FromFile(text2);
					}
					return;
				}
				catch
				{
					return;
				}
			}
		};
		menu.Items.Add(itemEdit);
		taskPanel.ContextMenuStrip = menu;
		lblTaskName.ContextMenuStrip = menu;
		lblTaskTime.ContextMenuStrip = menu;
		picTask.ContextMenuStrip = menu;
		taskPanel.Controls.Add(lblTaskName);
		taskPanel.Controls.Add(lblTaskTime);
		taskPanel.Controls.Add(picTask);
		taskPanel.Controls.Add(pnlProgressBg);
		int index = tasksListPanel.Controls.Count;
		for (int i2 = 0; i2 < tasksListPanel.Controls.Count; i2++)
		{
			if (tasksListPanel.Controls[i2].Name == "pnlAddSlot")
			{
				index = i2;
				break;
			}
		}
		if (tasksListPanel.Controls.Count >= 8 && index < tasksListPanel.Controls.Count)
		{
			Control slot = tasksListPanel.Controls[index];
			tasksListPanel.Controls.Remove(slot);
			tasksListPanel.Controls.Add(taskPanel);
			tasksListPanel.Controls.SetChildIndex(taskPanel, index);
			slot.Dispose();
		}
		else
		{
			tasksListPanel.Controls.Add(taskPanel);
		}
		UpdateControlContrast(taskPanel, theme.Item2);
	}

	private void ToggleTask(Panel taskPanel, TaskData data)
	{
		if (_activeTaskPanel == taskPanel)
		{
			StopTimer();
			StopM3u();
			_activeTaskPanel = null;
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
			ResetTaskProgress(taskPanel);
			_eqTimer?.Stop();
			ResetEq();
		}
		else if (!_timerRunning)
		{
			_activeTaskPanel = taskPanel;
			_activeTaskTotalSeconds = data.Time.TotalSeconds;
			_timeRemaining = data.Time;
			UpdateTimerDisplay();
			StartTimer();
			if (!string.IsNullOrEmpty(data.M3uPath))
			{
				PlayM3u(data.M3uPath);
				_eqTimer?.Start();
			}
		}
	}

	private void PlayM3u(string path)
	{
		if (_wmp == null)
		{
			return;
		}
		try
		{
			_metaTimer?.Stop();
			_m3u8WatchTimer?.Stop();
			_wmp.URL = path;
			_wmp.controls.play();
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			_lastTitle = "";
			if (string.IsNullOrEmpty(_currentCassetteTitle))
				lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			lblExtraMetadata.Text = "";
			lblM3uTitle.Visible = true;
			lblMetadata.Visible = true;
			lblExtraMetadata.Visible = true;
			_metaTimer?.Start();
			_eqTimer?.Start();
			SetVolumePreset(3);
			if (path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
			{
				_m3u8WatchTimer?.Start();
			}
		}
		catch
		{
			lblMetadata.Text = "";
			lblExtraMetadata.Text = "";
		}
	}

	private void LoadCassetteMaster()
	{
		string m3uDir = Path.Combine(Application.StartupPath, "files", "m3u");
		if (!Directory.Exists(m3uDir))
		{
			m3uDir = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", "..")), "files", "m3u");
		}
		string masterPath = Path.Combine(m3uDir, "CASS_master.txt");
		if (!File.Exists(masterPath)) return;

		_cassettes.Clear();
		CassetteData? current = null;

		foreach (string line in File.ReadAllLines(masterPath))
		{
			string trimmed = line.Trim();
			if (trimmed.StartsWith(";") || string.IsNullOrEmpty(trimmed)) continue;

			if (trimmed.StartsWith("[CASSETTE"))
			{
				if (current != null) _cassettes.Add(current);
				current = new CassetteData();
				continue;
			}

			if (current != null && trimmed.Contains(":"))
			{
				int colonIdx = trimmed.IndexOf(':');
				string key = trimmed.Substring(0, colonIdx).Trim().ToUpper();
				string value = trimmed.Substring(colonIdx + 1).Trim();

				switch (key)
				{
					case "TITULO": current.Titulo = value; break;
					case "IMAGEN": current.Imagen = value; break;
					case "CONTENIDO": current.Contenido = value; break;
					case "COLOR": current.Color = value; break;
					case "PANTALLA_GIF": current.PantallaGif = value; break;
					case "TEMA_FONDO": current.TemaFondo = value; break;
					case "TEMA_TV": current.TemaTV = value; break;
				}
			}
		}
		if (current != null) _cassettes.Add(current);
	}

	private string ResolveImgPath(string fileName)
	{
		string imgDir = Path.Combine(Application.StartupPath, "files", "img");
		if (!Directory.Exists(imgDir))
		{
			imgDir = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", "..")), "files", "img");
		}
		return Path.Combine(imgDir, fileName);
	}

	private void ApplyCassette(int index)
	{
		if (index < 0 || index >= _cassettes.Count) return;
		CassetteData cass = _cassettes[index];
		_currentCassetteIndex = index;

		lblM3uTitle.Text = cass.Titulo.ToUpper();
		_currentCassetteTitle = cass.Titulo.ToUpper();
		lblMetadata.Text = "";
		lblExtraMetadata.Text = "";

		if (!string.IsNullOrEmpty(cass.Color))
		{
			try
			{
				Color baseColor = ColorTranslator.FromHtml(cass.Color);
				ApplyAppColor(baseColor);
			}
			catch { }
		}

		if (!string.IsNullOrEmpty(cass.Imagen))
		{
			string imgPath = ResolveImgPath(cass.Imagen);
			if (File.Exists(imgPath))
			{
				picPlayer.Image = Image.FromFile(imgPath);
				picPlayer.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayer.Size = new Size(140, 88);
				picPlayer.Left = (pnlCassetteContainer.Width - 140) / 2;
				picPlayer.Top = 1;
			}
		}

		if (!string.IsNullOrEmpty(cass.PantallaGif))
		{
			string gifPath = ResolveImgPath(cass.PantallaGif);
			if (File.Exists(gifPath))
			{
				picMainDisplay.ImageLocation = gifPath;
				picMainDisplay.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}

		if (!string.IsNullOrEmpty(cass.TemaTV))
		{
			string tvPath = ResolveImgPath(cass.TemaTV);
			if (File.Exists(tvPath))
			{
				Image tvImg = Image.FromFile(tvPath);
				timerPanel.BackgroundImage = tvImg;
				timerPanel.BackgroundImageLayout = ImageLayout.None;
				timerPanel.Height = tvImg.Height - 4;
			}
		}

		if (!string.IsNullOrEmpty(cass.TemaFondo))
		{
			string fondoPath = ResolveImgPath(cass.TemaFondo);
			if (File.Exists(fondoPath))
			{
				_currentThemeImage = Image.FromFile(fondoPath);
				BackgroundImage = null;
				Invalidate();
			}
		}

		if (!string.IsNullOrEmpty(cass.Contenido))
		{
			PlayM3u(cass.Contenido);
			_eqTimer?.Start();
		}
	}

	private void ChangeCassette(int direction)
	{
		if (_cassettes.Count == 0) return;
		Timer? slideTimer = _slideTimer;
		if (slideTimer == null || !slideTimer.Enabled)
		{
			int newIndex = (_currentCassetteIndex + direction + _cassettes.Count) % _cassettes.Count;
			_slideDirection = -direction;
			CassetteData nextCass = _cassettes[newIndex];

			Image? nextImg = null;
			if (!string.IsNullOrEmpty(nextCass.Imagen))
			{
				string imgPath = ResolveImgPath(nextCass.Imagen);
				if (File.Exists(imgPath)) nextImg = Image.FromFile(imgPath);
			}

			if (nextImg != null)
			{
				picPlayerNext.Image = nextImg;
				picPlayerNext.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayerNext.Size = new Size(140, 88);
				picPlayerNext.Left = ((_slideDirection == 1) ? (-140) : 140);
				_slideTimer?.Start();
			}

			ApplyCassette(newIndex);
		}
	}

	private void UpdateVolumeVisual(int volumePercent)
	{
		try
		{
			double normalized = (double)(volumePercent - 3) / 12.0;
			normalized = Math.Max(0, Math.Min(1, normalized));
			int x = (int)(normalized * (double)pnlVolumeLine.Width);
			int thumbCenter = pnlVolumeThumb.Width / 2;
			pnlVolumeThumb.Left = x - thumbCenter;
		}
		catch
		{
		}
	}

	private void StopM3u()
	{
		if (_wmp == null)
		{
			return;
		}
		try
		{
			_metaTimer?.Stop();
			_m3u8WatchTimer?.Stop();
			_wmp.controls.stop();
			_lastTitle = "";
			lblMetadata.Text = "";
			lblExtraMetadata.Text = "";
			_eqTimer?.Stop();
			ResetEq();
		}
		catch
		{
		}
	}

	private void ResetTaskProgress(Panel taskPanel)
	{
		Control[] foundBg = taskPanel.Controls.Find("pnlProgressBg", searchAllChildren: true);
		if (foundBg.Length != 0)
		{
			Control[] foundFill = foundBg[0].Controls.Find("pnlProgressFill", searchAllChildren: true);
			if (foundFill.Length != 0)
			{
				foundFill[0].Width = 0;
			}
		}
	}

	private void Form1_Load(object? sender, EventArgs e)
	{
		base.StartPosition = FormStartPosition.Manual;
		Screen screen = Screen.PrimaryScreen;
		int screenWidth = screen?.WorkingArea.Width ?? 1920;
		base.Location = new Point(screenWidth - base.Width, 150);
		LoadCassetteMaster();
		//AddAutoButtons();
		LoadCustomFont();
		try
		{
			string baseDir = Application.StartupPath;
			string imgDir = "";
			for (int i = 0; i < 5; i++)
			{
				string testPath = Path.Combine(baseDir, "files", "img");
				if (Directory.Exists(testPath))
				{
					imgDir = testPath;
					break;
				}
				baseDir = Directory.GetParent(baseDir)?.FullName ?? baseDir;
			}
			if (!string.IsNullOrEmpty(imgDir))
			{
				if (Directory.Exists(imgDir))
				{
					for (int j = 1; j <= 5; j++)
					{
						string pPath = Path.Combine(imgDir, $"cass00{j}.png");
						if (File.Exists(pPath))
						{
							try
							{
								_cassetteImages.Add(Image.FromFile(pPath));
							}
							catch
							{
							}
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
						Image img = Image.FromFile(file2);
						_themeImages.Add(img);
						_themeFilePaths.Add(file2);
						if (file2.EndsWith("05_TH.gif", StringComparison.OrdinalIgnoreCase))
						{
							_currentThemeImageIndex = _themeImages.Count - 1;
							_currentThemeImage = img;
							BackgroundImage = null;
						}
					}
					catch
					{
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
				Image tvImg = Image.FromFile(tvPath);
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
		catch
		{
		}
		AddTaskToPanel("CODE", TimeSpan.FromMinutes(30L), "", "code_TSK.png", isFixed: true);
		AddTaskToPanel("EXER", TimeSpan.FromMinutes(30L), "", "exerc_TSK.png", isFixed: true);
		AddTaskToPanel("WORK", TimeSpan.FromMinutes(30L), "", "progress_TSK.png", isFixed: true);
		AddTaskToPanel("RLAX", TimeSpan.FromMinutes(30L), "", "music_TSK.png", isFixed: true);
		for (int i2 = 0; i2 < 2; i2++)
		{
			AddEmptySlot();
		}
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

	private void LoadSpriteSheet(string path, int frameWidth, int frameHeight, int frameCount)
	{
		try
		{
			using Bitmap fullSheet = new Bitmap(path);
			_spriteFrames.Clear();
			for (int i = 0; i < frameCount; i++)
			{
				Rectangle section = new Rectangle(i * frameWidth, 0, frameWidth, frameHeight);
				if (section.Right > fullSheet.Width)
				{
					section = new Rectangle(0, i * frameHeight, frameWidth, frameHeight);
				}
				Bitmap frame = fullSheet.Clone(section, fullSheet.PixelFormat);
				_spriteFrames.Add(frame);
			}
		}
		catch
		{
		}
	}

	private void btnAddTime_Click(object? sender, EventArgs e)
	{
		using SetTimeForm setTimeForm = new SetTimeForm();
		setTimeForm.StartPosition = FormStartPosition.Manual;
		setTimeForm.Location = new Point(base.Location.X - setTimeForm.Width, base.Location.Y);
		if (setTimeForm.ShowDialog() == DialogResult.OK)
		{
			_timeRemaining = new TimeSpan(setTimeForm.SelectedHours, setTimeForm.SelectedMinutes, 0);
			UpdateTimerDisplay();
			if (setTimeForm.StartImmediately && _timeRemaining.TotalSeconds > 0.0)
			{
				StartTimer();
			}
			else if (_timeRemaining.TotalSeconds > 0.0)
			{
				_timerRunning = false;
				countdownTimer.Stop();
				btnP.Text = "▶";
				btnS.Text = "⏹";
			}
			else
			{
				StopTimer();
			}
		}
	}

	private void btnP_Click(object? sender, EventArgs e)
	{
		if (_timerRunning)
		{
			PauseTimer();
		}
		else if (_timeRemaining.TotalSeconds > 0.0)
		{
			StartTimer();
		}
	}

	private void btnS_Click(object? sender, EventArgs e)
	{
		if (_activeTaskPanel != null)
		{
			StopTimer();
			StopM3u();
			_activeTaskPanel = null;
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
		}
		else
		{
			StopTimer();
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
		}
	}

	private void StartTimer()
	{
		countdownTimer.Start();
		_timerRunning = true;
		btnP.Text = "⏸";
		btnS.Text = "⏹";
	}

	private void StopTimer()
	{
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
	}

	private void PauseTimer()
	{
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
	}

	private async void countdownTimer_Tick(object? sender, EventArgs e)
	{
		if (_timeRemaining.TotalSeconds > 0.0)
		{
			_timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1L));
			UpdateTimerDisplay();
			if (_activeTaskPanel != null)
			{
				UpdateTaskProgress();
			}
			return;
		}
		countdownTimer.Stop();
		StopM3u();
		PlayDoneSound();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
		_timeRemaining = TimeSpan.Zero;
		UpdateTimerDisplay();
		if (_activeTaskPanel != null)
		{
			UpdateTaskProgress();
			Panel panelToDelete = _activeTaskPanel;
			bool isFixed = (panelToDelete.Tag as TaskData)?.IsFixed ?? false;
			_activeTaskPanel = null;
			await Task.Delay(3000);
			if (!isFixed)
			{
				tasksListPanel.Controls.Remove(panelToDelete);
				AddEmptySlot();
				panelToDelete.Dispose();
			}
			else
			{
				ResetTaskProgress(panelToDelete);
			}
		}
	}

	private void PlayDoneSound()
	{
		if (_wmp == null)
		{
			return;
		}
		string baseDir = Application.StartupPath;
		string donePath = "";
		for (int i = 0; i < 5; i++)
		{
			string testPath = Path.Combine(baseDir, "files", "mp3", "DONE.mp3");
			if (File.Exists(testPath))
			{
				donePath = testPath;
				break;
			}
			baseDir = Directory.GetParent(baseDir)?.FullName ?? baseDir;
		}
		if (File.Exists(donePath))
		{
			_wmp.URL = donePath;
			_wmp.controls.play();
			lblMetadata.Text = "";
		}
	}

	private void UpdateTaskProgress()
	{
		if (_activeTaskPanel == null)
		{
			return;
		}
		Control[] foundBg = _activeTaskPanel.Controls.Find("pnlProgressBg", searchAllChildren: true);
		if (foundBg.Length != 0)
		{
			Control[] foundFill = foundBg[0].Controls.Find("pnlProgressFill", searchAllChildren: true);
			if (foundFill.Length != 0)
			{
				double percent = (_activeTaskTotalSeconds - _timeRemaining.TotalSeconds) / _activeTaskTotalSeconds;
				foundFill[0].Width = (int)((double)foundBg[0].Width * percent);
			}
		}
	}

	private void UpdateTimerDisplay()
	{
		lblHours.Text = _timeRemaining.Hours.ToString("00");
		lblMinutes.Text = _timeRemaining.Minutes.ToString("00");
		lblSeconds.Text = _timeRemaining.Seconds.ToString("00");
	}

	private void AddAutoButtons()
	{
		Panel pnlAuto = new Panel
		{
			Dock = DockStyle.Right,
			Width = 80,
			BackColor = Color.FromArgb(20, 20, 20)
		};
		tasksHeaderPanel.Controls.Add(pnlAuto);
		for (int i = 0; i < 2; i++)
		{
			Button btnAuto = new Button
			{
				Text = "AUTO",
				Width = 35,
				Height = 20,
				Top = 2,
				Left = 5 + i * 40,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 5f, FontStyle.Bold),
				BackColor = Color.FromArgb(50, Color.Gray),
				ForeColor = Color.White
			};
			btnAuto.FlatAppearance.BorderSize = 0;
			pnlAuto.Controls.Add(btnAuto);
		}
		lblTasks.SendToBack();
	}

	private void AddEmptySlot()
	{
		Panel pnlAdd = new Panel
		{
			Height = 60,
			Width = base.Width / 2,
			BackColor = Color.FromArgb(40, Color.Black),
			BorderStyle = BorderStyle.None,
			Margin = new Padding(0),
			Name = "pnlAddSlot"
		};
		Button btnPlus = new Button
		{
			Text = "+",
			Dock = DockStyle.Fill,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 20f, FontStyle.Bold),
			ForeColor = Color.White,
			BackColor = Color.FromArgb(20, 20, 20)
		};
		btnPlus.FlatAppearance.BorderSize = 0;
		btnPlus.Click += delegate(object? s, EventArgs e)
		{
			btnAddTask_Click(s, e);
		};
		pnlAdd.Controls.Add(btnPlus);
		tasksListPanel.Controls.Add(pnlAdd);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.timerPanel = new System.Windows.Forms.Panel();
		this.picOverlay = new System.Windows.Forms.PictureBox();
		this.picMainDisplay = new System.Windows.Forms.PictureBox();
		this.pnlTimerControls = new System.Windows.Forms.Panel();
		this.btnTheme = new System.Windows.Forms.Button();
		this.btnImage = new System.Windows.Forms.Button();
		this.btnStyle = new System.Windows.Forms.Button();
		this.btnColor = new System.Windows.Forms.Button();
		this.pnlTopButtons = new System.Windows.Forms.Panel();
		this.lblHours = new System.Windows.Forms.Label();
		this.lblMinutes = new System.Windows.Forms.Label();
		this.lblSeconds = new System.Windows.Forms.Label();
		this.btnP = new System.Windows.Forms.Button();
		this.btnS = new System.Windows.Forms.Button();
		this.countdownTimer = new System.Windows.Forms.Timer(this.components);
		this.tasksHeaderPanel = new System.Windows.Forms.Panel();
		this.lblTasks = new System.Windows.Forms.Label();
		this.cassettesHeaderPanel = new System.Windows.Forms.Panel();
		this.lblCassettes = new System.Windows.Forms.Label();
		this.playerFooterPanel = new System.Windows.Forms.Panel();
		this.pnlEqualizer = new System.Windows.Forms.Panel();
		this.pnlVolume = new System.Windows.Forms.Panel();
		this.btnStopPlayer = new System.Windows.Forms.Button();
		this.btnPlayPlayer = new System.Windows.Forms.Button();
		this.lblVolumeLabel = new System.Windows.Forms.Label();
		this.pnlVolumeLine = new System.Windows.Forms.Panel();
		this.pnlVolumeThumb = new System.Windows.Forms.Panel();
		this.pnlVolButtons = new System.Windows.Forms.Panel();
		this.btnVolLow = new System.Windows.Forms.Button();
		this.btnVolMid = new System.Windows.Forms.Button();
		this.btnVolMax = new System.Windows.Forms.Button();
		this.btnNextM3u = new System.Windows.Forms.Button();
		this.btnPrevM3u = new System.Windows.Forms.Button();
		this.pnlCassetteContainer = new System.Windows.Forms.Panel();
		this.picPlayer = new System.Windows.Forms.PictureBox();
		this.picPlayerNext = new System.Windows.Forms.PictureBox();
		this.lblM3uTitle = new System.Windows.Forms.Label();
		this.lblMetadata = new System.Windows.Forms.Label();
		this.lblExtraMetadata = new System.Windows.Forms.Label();
		this.tasksListPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.pnlGrip = new System.Windows.Forms.Panel();
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
			this.picCincross.Image = System.Drawing.Image.FromFile(cincrossPath);
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
		this.lblTasks.Text = "TAREAS";
		this.lblTasks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cassettesHeaderPanel.BackColor = System.Drawing.Color.LightGray;
		this.cassettesHeaderPanel.Controls.Add(this.lblCassettes);
		this.cassettesHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.cassettesHeaderPanel.Location = new System.Drawing.Point(0, 200);
		this.cassettesHeaderPanel.Name = "cassettesHeaderPanel";
		this.cassettesHeaderPanel.Size = new System.Drawing.Size(260, 25);
		this.cassettesHeaderPanel.TabIndex = 4;
		this.lblCassettes.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblCassettes.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.lblCassettes.Location = new System.Drawing.Point(0, 0);
		this.lblCassettes.Name = "lblCassettes";
		this.lblCassettes.Size = new System.Drawing.Size(260, 25);
		this.lblCassettes.TabIndex = 1;
		this.lblCassettes.Text = "CASSETTES";
		this.lblCassettes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.playerFooterPanel.BackColor = System.Drawing.Color.White;
		this.playerFooterPanel.Controls.Add(this.pnlEqualizer);
		this.playerFooterPanel.Controls.Add(this.pnlVolume);
		this.playerFooterPanel.Controls.Add(this.btnNextM3u);
		this.playerFooterPanel.Controls.Add(this.btnPrevM3u);
		this.playerFooterPanel.Controls.Add(this.pnlCassetteContainer);
		this.playerFooterPanel.Controls.Add(this.lblM3uTitle);
		this.playerFooterPanel.Controls.Add(this.lblMetadata);
		this.playerFooterPanel.Controls.Add(this.lblExtraMetadata);
		this.playerFooterPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.playerFooterPanel.Location = new System.Drawing.Point(0, 200);
		this.playerFooterPanel.Name = "playerFooterPanel";
		this.playerFooterPanel.Size = new System.Drawing.Size(260, 230);
		this.playerFooterPanel.TabIndex = 3;
		this.pnlEqualizer.BackColor = System.Drawing.Color.Transparent;
		this.pnlEqualizer.Location = new System.Drawing.Point(28, 95);
		this.pnlEqualizer.Name = "pnlEqualizer";
		this.pnlEqualizer.Size = new System.Drawing.Size(204, 10);
		this.pnlEqualizer.TabIndex = 6;
		this.pnlVolume.Controls.Add(this.btnStopPlayer);
		this.pnlVolume.Controls.Add(this.btnPlayPlayer);
		this.pnlVolume.Controls.Add(this.lblVolumeLabel);
		this.pnlVolume.Controls.Add(this.pnlVolumeLine);
		this.pnlVolume.Controls.Add(this.pnlVolButtons);
		this.pnlVolume.Controls.Add(this.pnlVolumeThumb);
		this.pnlVolume.Location = new System.Drawing.Point(5, 155);
		this.pnlVolume.Name = "pnlVolume";
		this.pnlVolume.Size = new System.Drawing.Size(250, 75);
		this.pnlVolume.TabIndex = 5;
		this.btnPlayPlayer.FlatAppearance.BorderSize = 0;
		this.btnPlayPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPlayPlayer.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnPlayPlayer.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnPlayPlayer.ForeColor = System.Drawing.Color.White;
		this.btnPlayPlayer.Location = new System.Drawing.Point(5, 22);
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
		this.btnStopPlayer.Location = new System.Drawing.Point(53, 22);
		this.btnStopPlayer.Name = "btnStopPlayer";
		this.btnStopPlayer.Size = new System.Drawing.Size(48, 20);
		this.btnStopPlayer.TabIndex = 3;
		this.btnStopPlayer.Text = "STOP";
		this.btnStopPlayer.UseVisualStyleBackColor = false;
		this.btnStopPlayer.Padding = new System.Windows.Forms.Padding(0);
		this.lblVolumeLabel.AutoSize = true;
		this.lblVolumeLabel.BackColor = System.Drawing.Color.Transparent;
		this.lblVolumeLabel.Font = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold);
		this.lblVolumeLabel.ForeColor = System.Drawing.Color.White;
		this.lblVolumeLabel.Location = new System.Drawing.Point(108, 5);
		this.lblVolumeLabel.Name = "lblVolumeLabel";
		this.lblVolumeLabel.Size = new System.Drawing.Size(45, 15);
		this.lblVolumeLabel.TabIndex = 1;
		this.lblVolumeLabel.Text = "VOLUMEN";
		this.pnlVolumeLine.BackColor = System.Drawing.Color.Gray;
		this.pnlVolumeLine.Controls.Add(this.pnlVolumeThumb);
		this.pnlVolumeLine.Location = new System.Drawing.Point(105, 30);
		this.pnlVolumeLine.Name = "pnlVolumeLine";
		this.pnlVolumeLine.Size = new System.Drawing.Size(140, 4);
		this.pnlVolumeLine.TabIndex = 0;
		this.pnlVolumeThumb.BackColor = System.Drawing.Color.White;
		this.pnlVolumeThumb.Location = new System.Drawing.Point(0, -4);
		this.pnlVolumeThumb.Name = "pnlVolumeThumb";
		this.pnlVolumeThumb.Size = new System.Drawing.Size(12, 12);
		this.pnlVolumeThumb.TabIndex = 0;
		this.pnlVolButtons.Location = new System.Drawing.Point(105, 46);
		this.pnlVolButtons.Name = "pnlVolButtons";
		this.pnlVolButtons.Size = new System.Drawing.Size(140, 28);
		this.pnlVolButtons.BackColor = System.Drawing.Color.Transparent;
		this.btnVolLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolLow.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Bold);
		this.btnVolLow.ForeColor = System.Drawing.Color.White;
		this.btnVolLow.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolLow.Location = new System.Drawing.Point(0, 0);
		this.btnVolLow.Size = new System.Drawing.Size(42, 28);
		this.btnVolLow.Text = "LOW";
		this.btnVolMid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolMid.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnVolMid.ForeColor = System.Drawing.Color.White;
		this.btnVolMid.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolMid.Location = new System.Drawing.Point(44, 0);
		this.btnVolMid.Size = new System.Drawing.Size(42, 28);
		this.btnVolMid.Text = "MID";
		this.btnVolMax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVolMax.Font = new System.Drawing.Font("Segoe UI", 5.5f, System.Drawing.FontStyle.Bold);
		this.btnVolMax.ForeColor = System.Drawing.Color.White;
		this.btnVolMax.BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
		this.btnVolMax.Location = new System.Drawing.Point(88, 0);
		this.btnVolMax.Size = new System.Drawing.Size(45, 28);
		this.btnVolMax.Text = "MAX";
		this.pnlVolButtons.Controls.Add(this.btnVolLow);
		this.pnlVolButtons.Controls.Add(this.btnVolMid);
		this.pnlVolButtons.Controls.Add(this.btnVolMax);
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
		this.lblM3uTitle.Location = new System.Drawing.Point(10, 107);
		this.lblM3uTitle.Name = "lblM3uTitle";
		this.lblM3uTitle.Size = new System.Drawing.Size(240, 16);
		this.lblM3uTitle.TabIndex = 0;
		this.lblM3uTitle.Text = "M3U TITLE";
		this.lblM3uTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblMetadata.Font = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Bold);
		this.lblMetadata.Location = new System.Drawing.Point(10, 124);
		this.lblMetadata.Name = "lblMetadata";
		this.lblMetadata.Size = new System.Drawing.Size(240, 15);
		this.lblMetadata.TabIndex = 6;
		this.lblMetadata.Text = "SONG TITLE";
		this.lblMetadata.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblExtraMetadata.Font = new System.Drawing.Font("Segoe UI", 6.5f);
		this.lblExtraMetadata.Location = new System.Drawing.Point(10, 140);
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
		base.Controls.Add(this.tasksListPanel);
		base.Controls.Add(this.tasksHeaderPanel);
		base.Controls.Add(this.playerFooterPanel);
		base.Controls.Add(this.cassettesHeaderPanel);
		base.Controls.Add(this.pnlGrip);
		base.Controls.Add(this.timerPanel);
		base.Controls.Add(this.btnCloseApp);
		base.Controls.Add(this.picCincross);
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
