using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private dynamic? _wmp;

	private Timer? _metaTimer;

	private Timer? _m3u8WatchTimer;

	private HlsPlayer? _hlsPlayer;

	private bool _isHlsStream;

	private string? _lastHlsUrl;

	private bool _isPlaying;

	private string _currentM3uName = "";
	private string _lastTitle = "";

	private List<string> _m3uFiles = new List<string>();

	private int _currentM3uIndex;

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
					catch (Exception ex) { Logger.Error("Form1._m3u8WatchTimer", ex); }
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
		catch (Exception ex)
		{
			Logger.Error("Form1.InitPlayer.WMP", ex);
		}
		try
		{
			_hlsPlayer = new HlsPlayer();
			_hlsPlayer.MediaChanged += () =>
			{
				if (!_isHlsStream || _hlsPlayer == null) return;
				string? title = _hlsPlayer.CurrentTitle;
				if (!string.IsNullOrEmpty(title) && title != _lastTitle)
				{
					_lastTitle = title;
					if (string.IsNullOrEmpty(_currentCassetteTitle))
						lblM3uTitle.Text = Path.GetFileNameWithoutExtension(_lastHlsUrl ?? "").ToUpper();
					else
						lblM3uTitle.Text = _currentCassetteTitle;
					lblMetadata.Text = title.ToUpper();
					string? artist = _hlsPlayer.CurrentArtist;
					lblExtraMetadata.Text = string.IsNullOrEmpty(artist) ? "" : artist.ToUpper();
				}
			};
			_hlsPlayer.Error += msg =>
			{
				try { lblExtraMetadata.Text = msg.ToUpper(); } catch (Exception innerEx) { Logger.Error("Form1.HlsPlayer.Error", innerEx); }
			};
		}
		catch (Exception ex)
		{
			try { lblExtraMetadata.Text = $"VLC: {ex.Message}".ToUpper(); } catch (Exception innerEx) { Logger.Error("Form1.HlsPlayer.Init", innerEx); }
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
		catch (Exception ex)
		{
			Logger.Error("Form1.UpdateMetadata", ex);
		}
	}

	private async Task PlayM3uAsync(string path)
	{
		_isHlsStream = path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase);
		if (_isHlsStream)
		{
			_metaTimer?.Stop();
			_m3u8WatchTimer?.Stop();
			try { _wmp?.controls.stop(); } catch (Exception ex) { Logger.Error("Form1.PlayM3u.StopWmp", ex); }
			if (_hlsPlayer != null)
				await _hlsPlayer.PlayAsync(path);
			_lastHlsUrl = path;
			_currentM3uName = Path.GetFileNameWithoutExtension(path).ToUpper();
			_lastTitle = "";
			if (string.IsNullOrEmpty(_currentCassetteTitle))
				lblM3uTitle.Text = _currentM3uName;
			lblMetadata.Text = "";
			lblExtraMetadata.Text = "";
			lblM3uTitle.Visible = true;
			lblMetadata.Visible = true;
			lblExtraMetadata.Visible = true;
			_isPlaying = true;
			SetVolumePreset(3);
			return;
		}
		_hlsPlayer?.Stop();
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
			_isPlaying = true;
			SetVolumePreset(3);
		}
		catch
		{
			lblMetadata.Text = "";
			lblExtraMetadata.Text = "";
		}
	}

	private void SetVolumePreset(int percent)
	{
		if (_isHlsStream)
		{
			if (_hlsPlayer != null) _hlsPlayer.Volume = percent * 100 / 15;
		}
		else
		{
			if (_wmp != null) _wmp.settings.volume = percent;
		}
		btnVolLow.Tag = percent == 3;
		btnVolMid.Tag = percent == 9;
		btnVolMax.Tag = percent == 15;
		btnVolLow.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
		btnVolMid.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
		btnVolMax.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
		btnVolLow.Invalidate();
		btnVolMid.Invalidate();
		btnVolMax.Invalidate();
		UpdateVolumeVisual(percent);
	}

	private void UpdateVolumeFromMouse(int mouseX)
	{
		int x = Math.Max(0, Math.Min(mouseX - pnlVolumeLine.Left, pnlVolumeLine.Width));
		int thumbCenter = pnlVolumeThumb.Width / 2;
		pnlVolumeThumb.Left = x - thumbCenter;
		double raw = (double)x / (double)pnlVolumeLine.Width;
		int volume = (int)(Math.Max(0.03, Math.Min(0.15, raw)) * 100.0);
		if (_isHlsStream)
		{
			if (_hlsPlayer != null) _hlsPlayer.Volume = volume * 100 / 15;
		}
		else
		{
			if (_wmp != null) _wmp.settings.volume = volume;
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
		catch (Exception ex)
		{
			Logger.Error("Form1.UpdateVolumeVisual", ex);
		}
	}
	
	private void StopM3u()
	{
		_hlsPlayer?.Stop();
		_metaTimer?.Stop();
		_m3u8WatchTimer?.Stop();
		try
		{
			_wmp?.controls.stop();
		}
		catch (Exception ex)
		{
			Logger.Error("Form1.StopM3u", ex);
		}
		_lastTitle = "";
		lblMetadata.Text = "";
		lblExtraMetadata.Text = "";
		_isPlaying = false;
	}

	private void PlayDoneSound()
	{
		if (_wmp == null)
		{
			return;
		}
		string donePath = Path.Combine(PathHelper.GetFilesDir(), "mp3", "DONE.mp3");
		if (File.Exists(donePath))
		{
			_wmp.URL = donePath;
			_wmp.controls.play();
			lblMetadata.Text = "";
		}
	}
}
