using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private static readonly Color _radioSteelTop = Color.FromArgb(76, 76, 81);
	private static readonly Color _radioSteelBottom = Color.FromArgb(22, 22, 25);
	private static readonly Color _radioBrassLight = Color.FromArgb(224, 186, 108);
	private static readonly Color _radioBrassDark = Color.FromArgb(112, 86, 42);
	private static readonly Color _radioRecess = Color.FromArgb(14, 14, 16);
	private static readonly Color _radioSlot = Color.FromArgb(26, 26, 29);

	private void ApplyRadioSkin()
	{
		cassettesHeaderPanel.Paint += HeaderPlatePaint;
		tasksHeaderPanel.Paint += HeaderPlatePaint;
		toolsHeaderPanel.Paint += HeaderPlatePaint;
		toolsRow.Paint += SectionBodyPaint;
		tasksListPanel.Paint += SectionBodyPaint;
		btnP.Paint += KnobButtonPaint;
		btnS.Paint += KnobButtonPaint;
		btnPrevM3u.Paint += CassetteNavPaint;
		btnNextM3u.Paint += CassetteNavPaint;
		btnCloseApp.Paint += ClosePlatePaint;
		pnlProgressBg.Paint += ScaleSlotPaint;

		cassettesHeaderPanel.Invalidate();
		tasksHeaderPanel.Invalidate();
		toolsHeaderPanel.Invalidate();
		toolsRow.Invalidate();
		tasksListPanel.Invalidate();
		btnP.Invalidate();
		btnS.Invalidate();
		btnPrevM3u.Invalidate();
		btnNextM3u.Invalidate();
		btnCloseApp.Invalidate();
		pnlProgressBg.Invalidate();
	}

	private void SectionBodyPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = c.ClientRectangle;
		using (var brush = MetalBrush(new RectangleF(0, 0, r.Width, r.Height)))
			g.FillRectangle(brush, r);
		DrawBrushed(g, r);
		if (_appColor != Color.Transparent)
		{
			using var tint = new SolidBrush(Color.FromArgb(102, _appColor));
			g.FillRectangle(tint, r);
		}
	}

	private void DrawFooterSkin(Graphics g)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = playerFooterPanel.ClientRectangle;
		using (var brush = MetalBrush(new RectangleF(0, 0, r.Width, r.Height)))
			g.FillRectangle(brush, r);
		DrawBrushed(g, r);
		Rectangle grille = pnlCassetteContainer.Bounds;
		grille.Inflate(-4, -6);
		if (grille.Width > 0 && grille.Height > 0)
			DrawGrille(g, grille);
		using (var trim = new Pen(Color.FromArgb(150, 196, 164, 92), 1f))
			g.DrawLine(trim, 0, 91, r.Width, 91);
		using (var top = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
			g.DrawLine(top, 0, 0, r.Width, 0);
		DrawCornerScrews(g, r);
	}

	private void HeaderPlatePaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using (var brush = MetalBrush(new RectangleF(0, 0, c.Width, c.Height)))
			e.Graphics.FillRectangle(brush, c.ClientRectangle);
		DrawBrushed(e.Graphics, c.ClientRectangle);
		using (var top = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
			e.Graphics.DrawLine(top, 0, 0, c.Width, 0);
		using (var brass = new Pen(Color.FromArgb(150, 196, 164, 92), 1f))
			e.Graphics.DrawLine(brass, 0, c.Height - 1, c.Width, c.Height - 1);
		DrawCornerScrews(e.Graphics, c.ClientRectangle);
	}

	private void KnobButtonPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		float cx = b.Width / 2f;
		float cy = b.Height / 2f;
		float r = Math.Min(b.Width, b.Height) / 2f - 3.5f;
		using (var socket = new SolidBrush(Color.FromArgb(12, 12, 14)))
			e.Graphics.FillEllipse(socket, cx - r - 2f, cy - r - 2f, r * 2f + 4f, r * 2f + 4f);
		using (var rim = new SolidBrush(Color.FromArgb(170, _radioBrassDark)))
			e.Graphics.FillEllipse(rim, cx - r - 1f, cy - r - 1f, r * 2f + 2f, r * 2f + 2f);
		using (var face = MetalBrush(new RectangleF(cx - r, cy - r, r * 2f, r * 2f)))
			e.Graphics.FillEllipse(face, cx - r, cy - r, r * 2f, r * 2f);
		using (var hi = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
			e.Graphics.FillEllipse(hi, cx - r, cy - r, r * 2f, r * 2f * 0.45f);
		using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var brush = new SolidBrush(Color.White))
			e.Graphics.DrawString(b.Text, b.Font, brush, new RectangleF(0, 0, b.Width, b.Height), fmt);
	}

	private void MetalButtonPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		RectangleF rect = new RectangleF(1.5f, 1.5f, b.Width - 3f, b.Height - 3f);
		using (var path = RoundedPath(rect, 7f))
		{
			using (var brush = MetalBrush(rect))
				e.Graphics.FillPath(brush, path);
			using (var brass = new Pen(Color.FromArgb(170, 196, 164, 92), 1.2f))
				e.Graphics.DrawPath(brass, path);
			using (var hi = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
				e.Graphics.DrawLine(hi, rect.X + 3f, rect.Y + 1f, rect.Right - 3f, rect.Y + 1f);
		}
		using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var brush = new SolidBrush(Color.White))
			e.Graphics.DrawString(b.Text, b.Font, brush, new RectangleF(0, 0, b.Width, b.Height), fmt);
	}

	private readonly bool[] _cassetteNavHover = new bool[2];
	private readonly bool[] _cassetteNavPress = new bool[2];

	private void CassetteNavPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int idx = b.Name == "btnPrevM3u" ? 0 : 1;

		RectangleF rect = new RectangleF(0, 0, b.Width, b.Height);
		using (var bg = MetalBrush(rect))
			e.Graphics.FillRectangle(bg, rect);
		DrawBrushed(e.Graphics, b.ClientRectangle);

		bool hover = _cassetteNavHover[idx];
		bool press = _cassetteNavPress[idx];
		float cx = b.Width / 2f;
		float cy = b.Height / 2f;

		if (hover)
		{
			float glowR = Math.Min(b.Width, b.Height) * 0.55f;
			Color glowCenter = press
				? Color.FromArgb(25, 224, 186, 108)
				: Color.FromArgb(14, 224, 186, 108);
			using (var path = new GraphicsPath())
			{
				path.AddEllipse(cx - glowR, cy - glowR, glowR * 2f, glowR * 2f);
				using (var blend = new PathGradientBrush(path))
				{
					blend.CenterColor = glowCenter;
					blend.SurroundColors = [Color.FromArgb(0, 224, 186, 108)];
					e.Graphics.FillPath(blend, path);
				}
			}
		}

		float h = b.Height * 0.32f;
		float w = h * 0.78f;
		PointF[] tri;
		if (idx == 0)
		{
			tri =
			[
				new PointF(cx + w * 0.4f, cy),
				new PointF(cx - w * 0.6f, cy - h),
				new PointF(cx - w * 0.6f, cy + h)
			];
		}
		else
		{
			tri =
			[
				new PointF(cx - w * 0.4f, cy),
				new PointF(cx + w * 0.6f, cy - h),
				new PointF(cx + w * 0.6f, cy + h)
			];
		}

		float penAlpha = press ? 220f : hover ? 180f : 100f;
		using (var triPath = new GraphicsPath())
		{
			triPath.AddPolygon(tri);
			Color brass = Color.FromArgb((int)penAlpha, 196, 164, 92);
			using (var pen = new Pen(brass, 1.4f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
				e.Graphics.DrawPath(pen, triPath);
		}
	}

	private void ClosePlatePaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using (var brush = MetalBrush(new RectangleF(0, 0, b.Width, b.Height)))
			e.Graphics.FillRectangle(brush, b.ClientRectangle);
		DrawBrushed(e.Graphics, b.ClientRectangle);
		using (var brass = new Pen(Color.FromArgb(150, 196, 164, 92), 1f))
			e.Graphics.DrawRectangle(brass, 1, 1, b.Width - 3, b.Height - 3);
		DrawCornerScrews(e.Graphics, b.ClientRectangle);
		using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var brush = new SolidBrush(Color.White))
			e.Graphics.DrawString(b.Text, b.Font, brush, new RectangleF(0, 0, b.Width, b.Height), fmt);
	}

	private void SpeakerCirclePaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		float cx = c.Width / 2f;
		float cy = c.Height / 2f;
		float r = Math.Min(c.Width, c.Height) / 2f - 1f;
		RectangleF rimRect = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
		using (var rim = MetalBrush(rimRect))
			e.Graphics.FillEllipse(rim, rimRect);
		using (var chrome = new Pen(Color.FromArgb(200, 224, 214, 190), 1.5f))
			e.Graphics.DrawEllipse(chrome, rimRect.X + 0.5f, rimRect.Y + 0.5f, rimRect.Width - 1f, rimRect.Height - 1f);
		using (var face = new SolidBrush(Color.FromArgb(22, 22, 25)))
			e.Graphics.FillEllipse(face, cx - r + 3f, cy - r + 3f, r * 2f - 6f, r * 2f - 6f);
		float inset = r * 0.2f;
		RectangleF inner = new RectangleF(cx - r + inset, cy - r + inset, r * 2f - inset * 2f, r * 2f - inset * 2f);
		float hr = inner.Width / 2f;
		float step = inner.Width / 6f;
		float dotR = step * 0.32f;
		using (var hole = new SolidBrush(Color.FromArgb(190, 5, 5, 7)))
		{
			for (float y = inner.Y + step / 2f; y < inner.Bottom; y += step)
			{
				for (float x = inner.X + step / 2f; x < inner.Right; x += step)
				{
					float dx = (x - cx) / hr;
					float dy = (y - cy) / hr;
					if (dx * dx + dy * dy <= 1.0f)
						e.Graphics.FillEllipse(hole, x - dotR, y - dotR, dotR * 2f, dotR * 2f);
				}
			}
		}
		float capR = r * 0.24f;
		using (var groove = new SolidBrush(Color.FromArgb(8, 8, 10)))
			e.Graphics.FillEllipse(groove, cx - capR * 0.88f, cy - capR * 0.88f, capR * 1.76f, capR * 1.76f);
		RectangleF capRect = new RectangleF(cx - capR, cy - capR, capR * 2f, capR * 2f);
		using (var cap = MetalBrush(capRect))
			e.Graphics.FillEllipse(cap, capRect);
		using (var ringPen = new Pen(Color.FromArgb(120, 196, 164, 92), 1f))
		{
			e.Graphics.DrawEllipse(ringPen, cx - capR * 0.6f, cy - capR * 0.6f, capR * 1.2f, capR * 1.2f);
			e.Graphics.DrawEllipse(ringPen, cx - capR * 0.35f, cy - capR * 0.35f, capR * 0.7f, capR * 0.7f);
		}
		using (var hi = new SolidBrush(Color.FromArgb(65, 255, 255, 255)))
			e.Graphics.FillEllipse(hi, capRect.X, capRect.Y, capRect.Width, capRect.Height * 0.4f);
		using (var capRim = new Pen(Color.FromArgb(160, 214, 200, 170), 1f))
			e.Graphics.DrawEllipse(capRim, capRect);
	}

	private void VolumeKnobPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		bool selected = b.Tag is bool sel && sel;
		float cx = b.Width / 2f;
		float cy = b.Height / 2f;
		float r = Math.Min(b.Width, b.Height) / 2f - 1f;
		using (var socket = new SolidBrush(Color.FromArgb(12, 12, 14)))
			e.Graphics.FillEllipse(socket, cx - r - 1.5f, cy - r - 1.5f, r * 2f + 3f, r * 2f + 3f);
		using (var rim = new SolidBrush(selected ? Color.FromArgb(230, 220, 200, 150) : Color.FromArgb(170, _radioBrassDark)))
			e.Graphics.FillEllipse(rim, cx - r, cy - r, r * 2f, r * 2f);
		RectangleF faceRect = new RectangleF(cx - r + 2.5f, cy - r + 2.5f, r * 2f - 5f, r * 2f - 5f);
		using (var face = new LinearGradientBrush(faceRect, Color.FromArgb(30, 30, 36), Color.FromArgb(12, 12, 15), LinearGradientMode.Vertical))
			e.Graphics.FillEllipse(face, faceRect);
		RectangleF domeRect = new RectangleF(faceRect.X, faceRect.Y, faceRect.Width, faceRect.Height * 0.55f);
		using (var dome = new LinearGradientBrush(domeRect, Color.FromArgb(120, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
			e.Graphics.FillEllipse(dome, domeRect);
		DrawKnobDots(e.Graphics, cx, cy, r - 1.25f, selected);
		if (selected)
		{
			using (var ind = new Pen(Color.White, 2f))
			{
				ind.StartCap = LineCap.Round;
				ind.EndCap = LineCap.Round;
				e.Graphics.DrawLine(ind, cx - r * 0.3f, cy + r - r * 0.25f, cx + r * 0.3f, cy + r - r * 0.25f);
			}
		}
		using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var brush = new SolidBrush(selected ? Color.White : Color.FromArgb(200, 200, 200)))
			e.Graphics.DrawString(b.Text, b.Font, brush, new RectangleF(0, 0, b.Width, b.Height), fmt);
	}

	private static void DrawKnobDots(Graphics g, float cx, float cy, float rr, bool selected)
	{
		int count = Math.Max(16, (int)Math.Round(rr * 2f));
		float dotR = rr / count * 1.7f;
		using (var light = new SolidBrush(selected ? Color.FromArgb(230, 220, 200, 150) : _radioBrassLight))
		using (var dark = new SolidBrush(selected ? Color.FromArgb(90, 20, 20, 24) : _radioRecess))
		{
			for (int i = 0; i < count; i++)
			{
				float ang = i * (2f * (float)Math.PI / count);
				float x = cx + (float)Math.Cos(ang) * rr;
				float y = cy + (float)Math.Sin(ang) * rr;
				g.FillEllipse(i % 2 == 0 ? light : dark, x - dotR, y - dotR, dotR * 2f, dotR * 2f);
			}
		}
	}

	private void LiveButtonPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Button b) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		RectangleF rect = new RectangleF(1f, 1f, b.Width - 2f, b.Height - 2f);
		using (var path = RoundedPath(rect, 6f))
		{
			using (var recess = new SolidBrush(_radioRecess))
				e.Graphics.FillPath(recess, path);
			using (var brass = new Pen(Color.FromArgb(160, 196, 164, 92), 1f))
				e.Graphics.DrawPath(brass, path);
		}
		using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var brush = new SolidBrush(b.ForeColor))
			e.Graphics.DrawString(b.Text, b.Font, brush, new RectangleF(0, 0, b.Width, b.Height), fmt);
	}

	private void JackSocketPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Color border = c.Tag is Color col ? col : Color.White;
		float cx = c.Width / 2f;
		float cy = c.Height / 2f;
		float r = Math.Min(c.Width, c.Height) / 2f - 1.5f;
		using (var recess = new SolidBrush(_radioRecess))
			e.Graphics.FillEllipse(recess, cx - r, cy - r, r * 2f, r * 2f);
		using (var pen = new Pen(border, 2f))
			e.Graphics.DrawEllipse(pen, cx - r + 0.5f, cy - r + 0.5f, r * 2f - 1f, r * 2f - 1f);
		using (var pen2 = new Pen(border, 1.5f))
			e.Graphics.DrawEllipse(pen2, cx - r + 3.5f, cy - r + 3.5f, r * 2f - 7f, r * 2f - 7f);
		float hr = 2.8f;
		using (var hole = new SolidBrush(Color.FromArgb(3, 3, 4)))
			e.Graphics.FillEllipse(hole, cx - hr, cy - hr, hr * 2f, hr * 2f);
	}

	private void CassetteGlassPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = c.ClientRectangle;
		using (var sheen = new LinearGradientBrush(r, Color.FromArgb(32, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
			g.FillRectangle(sheen, r);
		PointF[] band =
		[
			new PointF(r.Left + 10f, r.Top - 10f),
			new PointF(r.Left + 90f, r.Top - 10f),
			new PointF(r.Left + 120f, r.Bottom + 10f),
			new PointF(r.Left + 40f, r.Bottom + 10f)
		];
		using (var bandBrush = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
			g.FillPolygon(bandBrush, band);
		using (var edge = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
			g.DrawLine(edge, band[0], band[1]);
	}

	private void ScaleSlotPaint(object? sender, PaintEventArgs e)
	{
		if (sender is not Control c) return;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using (var recess = new SolidBrush(_radioRecess))
			e.Graphics.FillRectangle(recess, c.ClientRectangle);
		using (var top = new Pen(Color.FromArgb(60, 0, 0, 0), 1f))
			e.Graphics.DrawLine(top, 0, 0, c.Width, 0);
		using (var brass = new Pen(Color.FromArgb(150, 196, 164, 92), 1f))
		{
			e.Graphics.DrawLine(brass, 0, 1, c.Width, 1);
			e.Graphics.DrawLine(brass, 0, c.Height - 2, c.Width, c.Height - 2);
		}
		using (var tick = new Pen(Color.FromArgb(110, 170, 145, 85), 1f))
		{
			for (int i = 0; i <= 10; i++)
			{
				int x = i * c.Width / 10;
				int len = i % 5 == 0 ? 6 : 3;
				e.Graphics.DrawLine(tick, x, c.Height - 3 - len, x, c.Height - 3);
			}
		}
	}

	private static LinearGradientBrush MetalBrush(RectangleF r)
	{
		return new LinearGradientBrush(r, _radioSteelTop, _radioSteelBottom, LinearGradientMode.Vertical);
	}

	private static void DrawBrushed(Graphics g, Rectangle r)
	{
		using (var pen = new Pen(Color.FromArgb(18, 255, 255, 255), 1f))
		{
			for (int y = r.Y + 3; y < r.Bottom; y += 4)
				g.DrawLine(pen, r.X, y, r.Right, y);
		}
	}

	private static void DrawGrille(Graphics g, Rectangle r)
	{
		using (var hole = new SolidBrush(Color.FromArgb(90, 8, 8, 10)))
		using (var rim = new Pen(Color.FromArgb(60, 214, 200, 170), 1f))
		{
			for (int y = r.Y + 4; y < r.Bottom; y += 6)
			{
				for (int x = r.X + 4; x < r.Right; x += 6)
				{
					g.FillEllipse(hole, x - 2.2f, y - 2.2f, 4.4f, 4.4f);
					g.DrawEllipse(rim, x - 2.2f, y - 2.2f, 4.4f, 4.4f);
				}
			}
		}
	}

	private static void DrawScrew(Graphics g, float cx, float cy)
	{
		using (var body = new SolidBrush(Color.FromArgb(120, 120, 126)))
			g.FillEllipse(body, cx - 3f, cy - 3f, 6f, 6f);
		using (var shade = new SolidBrush(Color.FromArgb(60, 20, 20, 22)))
			g.FillEllipse(shade, cx - 3f, cy, 6f, 3f);
		using (var rim = new Pen(Color.FromArgb(150, 190, 158, 90), 1f))
			g.DrawEllipse(rim, cx - 3f, cy - 3f, 6f, 6f);
		using (var slot = new Pen(Color.FromArgb(45, 45, 48), 1.2f))
			g.DrawLine(slot, cx - 2f, cy, cx + 2f, cy);
	}

	private static void DrawCornerScrews(Graphics g, Rectangle r, bool tl = true, bool tr = true, bool bl = true, bool br = true)
	{
		float inset = 4f;
		if (tl) DrawScrew(g, r.X + inset, r.Y + inset);
		if (tr) DrawScrew(g, r.Right - inset, r.Y + inset);
		if (bl) DrawScrew(g, r.X + inset, r.Bottom - inset);
		if (br) DrawScrew(g, r.Right - inset, r.Bottom - inset);
	}
}
