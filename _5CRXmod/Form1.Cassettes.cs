using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private List<Image> _cassetteImages = new List<Image>();

	private List<CassetteData> _cassettes = new List<CassetteData>();

	private int _currentCassetteIndex;
	private string _currentCassetteTitle = "";

	private Color _cassetteColor = Color.FromArgb(40, 40, 40);

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
							return PathHelper.LoadImage(fullPath);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Form1.GetCassetteImageFromM3u", ex);
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
			ResetCassetteTitle();
			string path = _m3uFiles[_currentM3uIndex];
			_ = PlayM3uAsync(path);
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			Image nextImg = GetCassetteImageFromM3u(path, _currentM3uIndex);
			if (nextImg != null)
			{
				StartFade(nextImg);
			}
		}
	}

	private void LoadCurrentM3u()
	{
		if (_m3uFiles.Count != 0)
		{
			string path = _m3uFiles[_currentM3uIndex];
			_ = PlayM3uAsync(path);
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			Image currentImg = GetCassetteImageFromM3u(path, _currentM3uIndex);
			if (currentImg != null)
			{
				picPlayer.Image = currentImg;
				picPlayer.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayer.Size = new Size(154, 97);
				picPlayer.Left = (pnlCassetteContainer.Width - 154) / 2;
				picPlayer.Top = 1;
				picPlayer.Width = 154;
			}
		}
	}

	private void LoadCassetteMaster()
	{
		string m3uDir = PathHelper.GetM3uDir();
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
					case "TEMA_TV": current.TemaTV = value; break;
				}
			}
		}
		if (current != null) _cassettes.Add(current);
	}

	private static string ResolveImgPath(string fileName) => PathHelper.ResolveImg(fileName);

	private void ApplyCassette(int index)
	{
		if (index < 0 || index >= _cassettes.Count) return;
		CassetteData cass = _cassettes[index];
		_currentCassetteIndex = index;

		lblCassettes.Text = "CASSETTES";
		txtCassetteNum.Text = (index + 1).ToString();
		lblCassetteTotal.Text = $"/{_cassettes.Count}";

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
			catch (Exception ex) { Logger.Error("Form1.ApplyCassette.Color", ex); }
		}
		
		if (!string.IsNullOrEmpty(cass.Imagen))
		{
			string imgPath = ResolveImgPath(cass.Imagen);
			if (File.Exists(imgPath))
			{
				if (picPlayer.Image != null) picPlayer.Image.Dispose();
				picPlayer.Image = PathHelper.LoadImage(imgPath);
				picPlayer.SizeMode = PictureBoxSizeMode.Zoom;
				picPlayer.Size = new Size(154, 97);
				picPlayer.Left = (pnlCassetteContainer.Width - 154) / 2;
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
				Image tvImg = PathHelper.LoadImage(tvPath);
				if (timerPanel.BackgroundImage != null) timerPanel.BackgroundImage.Dispose();
				timerPanel.BackgroundImage = tvImg;
				timerPanel.BackgroundImageLayout = ImageLayout.None;
				timerPanel.Height = tvImg.Height - 4;
				_currentTvPath = tvPath;
			}
		}

		if (!string.IsNullOrEmpty(cass.Contenido))
		{
			_ = PlayM3uAsync(cass.Contenido);
		}
	}

	private void ChangeCassette(int direction)
	{
		if (_cassettes.Count == 0) return;
		Timer? slideTimer = _slideTimer;
		if (slideTimer == null || !slideTimer.Enabled)
		{
			int newIndex = (_currentCassetteIndex + direction + _cassettes.Count) % _cassettes.Count;
			CassetteData nextCass = _cassettes[newIndex];

			Image? nextImg = null;
			if (!string.IsNullOrEmpty(nextCass.Imagen))
			{
				string imgPath = ResolveImgPath(nextCass.Imagen);
				if (File.Exists(imgPath)) nextImg = PathHelper.LoadImage(imgPath);
			}

			if (nextImg != null)
			{
				_pendingCassetteIndex = newIndex;
				StartFade(nextImg);
			}
			else
			{
				ApplyCassette(newIndex);
			}
		}
	}

	private void GoToCassette(int index)
	{
		if (index < 0 || index >= _cassettes.Count) return;
		if (index == _currentCassetteIndex) return;
		Timer? slideTimer = _slideTimer;
		if (slideTimer != null && slideTimer.Enabled) return;

		CassetteData cass = _cassettes[index];
		Image? nextImg = null;
		if (!string.IsNullOrEmpty(cass.Imagen))
		{
			string imgPath = ResolveImgPath(cass.Imagen);
			if (File.Exists(imgPath)) nextImg = PathHelper.LoadImage(imgPath);
		}

		if (nextImg != null)
		{
			_pendingCassetteIndex = index;
			StartFade(nextImg);
		}
		else
		{
			ApplyCassette(index);
		}
	}

	private void ResetCassetteTitle()
	{
		_currentCassetteTitle = "";
	}

	private void txtCassetteNum_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Enter)
		{
			e.SuppressKeyPress = true;
			NavigateToTextBoxCassette();
		}
	}

	private void txtCassetteNum_Leave(object? sender, EventArgs e)
	{
		NavigateToTextBoxCassette();
	}

	private void NavigateToTextBoxCassette()
	{
		if (int.TryParse(txtCassetteNum.Text, out int num) && num >= 1 && num <= _cassettes.Count)
			GoToCassette(num - 1);
		else
			txtCassetteNum.Text = (_currentCassetteIndex + 1).ToString();
	}

	private void btnCassetteList_Click(object? sender, EventArgs e)
	{
		using var form = new CassetteListForm(_cassettes.ToArray(), _currentCassetteIndex);
		form.Location = new Point(Left - form.Width, Top);
		if (form.ShowDialog(this) == DialogResult.OK && form.SelectedIndex >= 0)
			GoToCassette(form.SelectedIndex);
	}

	private void LayoutCassetteHeader()
	{
		lblCassetteTotal.Text = $"/{_cassettes.Count}";
		txtCassetteNum.Width = TextRenderer.MeasureText("888", txtCassetteNum.Font).Width;
		txtCassetteNum.Location = new Point(70, 3);
		lblCassetteTotal.Location = new Point(70 + txtCassetteNum.Width + 2, 6);
	}
}
