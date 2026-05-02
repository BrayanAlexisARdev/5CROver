using System;
using System.Windows.Forms;

namespace _5CRXmod;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new Form1());
	}
}
