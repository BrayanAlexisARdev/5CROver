using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private List<string> _aniFiles = new List<string>();

	private int _currentAniIndex;

	private string? _noiseFile;

	private string? _avatarPath;
	private string? _savedDisplayPath;
	private Bitmap? _tvCharacterComposite;
	private string? _avatarHead;
	private string? _avatarHair;
	private string? _avatarBody;
	private string? _avatarFace;
	private string? _avatarAccessories;
	private string? _avatarBg;
	private string? _avatarFullOutfit;
	private string? _avatarPet;

	private List<Bitmap> _spriteFrames = new List<Bitmap>();

	private int _currentSpriteFrame;

	private Timer? _spriteTimer;

	private bool _isDraggingForm;

	private Point _dragStartOffset;

	private bool _isDraggingVolume;

	private Timer? _slideTimer;
	private List<Bitmap>? _fadeOutFrames;
	private List<Bitmap>? _fadeInFrames;
	private int _fadePhase;
	private int _fadeFrameIndex;
	private Image? _nextCassetteImage;
	private int _pendingCassetteIndex = -1;
	private FavoriteData? _pendingFavOverride;

	private const int PictureBoxWidth = 140;

	private const int PictureBoxHeight = 88;

	private Timer? _eqTimer;

	private float _eqAnimationOffset;

	private static readonly System.Drawing.Imaging.ImageAttributes _alphaAttrs = new System.Drawing.Imaging.ImageAttributes();

	private void pnlEqualizer_Paint(object? sender, PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int w = pnlEqualizer.Width;
		int h = pnlEqualizer.Height;
		Color themeColor = _appColor == Color.Transparent || _appColor == Color.Empty ? Color.White : _appColor;
		float lum = 0.299f * themeColor.R + 0.587f * themeColor.G + 0.114f * themeColor.B;
		bool isDark = lum < 100f;

		if (_isPlaying)
		{
			int barCount = 12;
			int gap = 3;
			int barWidth = (w - gap * (barCount + 1)) / barCount;
			if (barWidth < 2) barWidth = 2;
			for (int i = 0; i < barCount; i++)
			{
				float phase = (float)i / barCount * MathF.PI * 2f + _eqAnimationOffset;
				float normalized = (MathF.Sin(phase) + 1f) * 0.5f;
				int barHeight = (int)(normalized * (h - 6));
				if (barHeight < 2) barHeight = 2;
				int x = gap + i * (barWidth + gap);
				int y = h - 3 - barHeight;
				Color topColor = isDark ? Color.FromArgb(40, Color.White) : Color.FromArgb(40, themeColor);
				Color bottomColor = isDark ? Color.FromArgb(220, themeColor) : Color.FromArgb(220, themeColor);
				using var brush = new LinearGradientBrush(
					new Rectangle(x, y, barWidth, barHeight),
					topColor,
					bottomColor,
					LinearGradientMode.Vertical);
				e.Graphics.FillRectangle(brush, x, y, barWidth, barHeight);
			}
		}
		else
		{
			using Pen pen = new Pen(Color.White, 2f);
			int y = h / 2;
			e.Graphics.DrawLine(pen, 4, y, w - 4, y);
		}
	}

	private void _fadeTimer_Tick(object? sender, EventArgs e)
	{
		var frames = _fadePhase == 0 ? _fadeOutFrames : _fadeInFrames;
		if (frames == null || _fadeFrameIndex >= frames.Count)
		{
			if (_fadePhase == 0)
			{
				if (_pendingCassetteIndex >= 0)
				{
					int idx = _pendingCassetteIndex;
					_pendingCassetteIndex = -1;
					ApplyCassette(idx);
					if (_pendingFavOverride != null)
					{
						var ov = _pendingFavOverride;
						_pendingFavOverride = null;
						ApplyFavOverride(ov);
					}
				}
				_fadePhase = 1;
				_fadeFrameIndex = 0;
				frames = _fadeInFrames;
			}
			else
			{
				if (_nextCassetteImage != null)
				{
					picPlayer.Image = _nextCassetteImage;
					_nextCassetteImage = null;
				}
				ClearFrameList(_fadeOutFrames);
				ClearFrameList(_fadeInFrames);
				_slideTimer?.Stop();
				return;
			}
		}

		picPlayer.Image = frames[_fadeFrameIndex++];
	}

	private static void ClearFrameList(List<Bitmap>? list)
	{
		if (list == null) return;
		for (int i = 0; i < list.Count; i++)
			list[i].Dispose();
		list.Clear();
	}

	private void StartFade(Image newImage)
	{
		if (picPlayer.Image == null)
		{
			picPlayer.Image = newImage;
			return;
		}

		ClearFrameList(_fadeOutFrames);
		ClearFrameList(_fadeInFrames);

		_nextCassetteImage = newImage;
		var oldImg = picPlayer.Image;

		int steps = 10;
		_fadePhase = 0;
		_fadeFrameIndex = 0;
		_fadeOutFrames = new List<Bitmap>(steps + 1);
		_fadeInFrames = new List<Bitmap>(steps);

		float div = 1f / steps;
		for (int i = steps; i >= 0; i--)
			_fadeOutFrames.Add(CreateAlphaCopy(oldImg, i * div));
		for (int i = 1; i <= steps; i++)
			_fadeInFrames.Add(CreateAlphaCopy(newImage, i * div));

		if (!string.IsNullOrEmpty(_noiseFile))
			picMainDisplay.ImageLocation = _noiseFile;

		_slideTimer.Interval = 40;
		_slideTimer?.Start();
	}

	private static Bitmap CreateAlphaCopy(Image img, float alpha)
	{
		var bmp = new Bitmap(PictureBoxWidth, PictureBoxHeight);
		using (var g = Graphics.FromImage(bmp))
		{
			_alphaAttrs.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha });
			g.DrawImage(img, new Rectangle(0, 0, PictureBoxWidth, PictureBoxHeight),
				0, 0, img.Width, img.Height, GraphicsUnit.Pixel, _alphaAttrs);
		}
		return bmp;
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

	private void PicMainDisplay_Paint(object? sender, PaintEventArgs e)
	{
		if (_tvCharacterComposite != null)
			e.Graphics.DrawImage(_tvCharacterComposite, 0, 0,
				_tvCharacterComposite.Width, _tvCharacterComposite.Height);
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
		catch (Exception ex)
		{
			Logger.Error("Form1.LoadSpriteSheet", ex);
		}
	}
}
