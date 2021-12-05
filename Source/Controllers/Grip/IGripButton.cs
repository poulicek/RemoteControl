using System.Drawing;

namespace RemoteControl.Controllers.Grip
{
    internal interface IGripButton
    {
        void Release();

        void Press(PointF coords);
    }
}
