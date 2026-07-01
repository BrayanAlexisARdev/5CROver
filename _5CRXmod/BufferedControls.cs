using System.Windows.Forms;

namespace _5CRXmod
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }
    }

    public class BufferedLabel : Label
    {
        public BufferedLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }
    }
}
