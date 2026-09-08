using System.Windows.Forms;

namespace _5CRXmod
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }
    }

    public class BufferedLabel : Label
    {
        public BufferedLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }
    }

    public class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public BufferedFlowLayoutPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }
    }
}
