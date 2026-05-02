using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace _5CRXmod;

public static class FontHelper
{
	private static PrivateFontCollection _pfc;

	public static FontFamily? CustomFontFamily { get; private set; }

	static FontHelper()
	{
		_pfc = new PrivateFontCollection();
		LoadFont();
	}

	private static void LoadFont()
	{
		try
		{
			string nasaPath = "";
			string startupPath = Path.Combine(Application.StartupPath, "files", "typo", "nasa.otf");
			string baseDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "typo", "nasa.otf");

			if (File.Exists(startupPath))
			{
				nasaPath = startupPath;
			}
			else if (File.Exists(baseDirectoryPath))
			{
				nasaPath = baseDirectoryPath;
			}

			if (string.IsNullOrEmpty(nasaPath))
			{
				return;
			}
			_pfc.AddFontFile(nasaPath);
			if (_pfc.Families.Length != 0)
			{
				FontFamily[] families = _pfc.Families;
				for (int i = 0; i < families.Length; i++)
				{
					_ = families[i];
				}
				CustomFontFamily = _pfc.Families[0];
			}
		}
		catch (Exception)
		{
		}
	}
	public static void ApplyFont(Control parent, float size = 9f, FontStyle style = FontStyle.Regular, string[]? excludeNames = null)
	{
		if (CustomFontFamily == null)
		{
			return;
		}
		bool shouldExclude = false;
		if (excludeNames != null)
		{
			foreach (string name in excludeNames)
			{
				if (parent.Name == name)
				{
					shouldExclude = true;
					break;
				}
			}
		}
		if (!shouldExclude)
		{
			parent.Font = new Font(CustomFontFamily, size, style);
		}
		foreach (Control control in parent.Controls)
		{
			ApplyFont(control, size, style, excludeNames);
		}
	}
}
