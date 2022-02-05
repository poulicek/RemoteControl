using System.Drawing;
using RemoteControl.Controllers.Grip;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Mouse
{
    public class MouseController : GripController
    {
        private readonly GripButtonMouse grip = new GripButtonMouse();

        public override void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "lb":
                    InputHelper.MouseAction(InputHelper.MouseButton.Left, Point.Empty, context.Request.Query["s"] == "1" ? InputHelper.MouseClickMode.Down : InputHelper.MouseClickMode.Up);
                    break;

                case "rb":
                    InputHelper.MouseAction(InputHelper.MouseButton.Right, Point.Empty, context.Request.Query["s"] == "1" ? InputHelper.MouseClickMode.Down : InputHelper.MouseClickMode.Up);
                    break;

                case "su":
                    InputHelper.MouseScroll(0, 100);
                    break;

                case "sd":
                    InputHelper.MouseScroll(0, -100);
                    break;


                case "grip":
                    // pressing or releasing the button
                    if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                        this.grip.Release();
                    else
                        this.grip.Press(this.parseCoords(context.Request.Query["o"]?.Split(',')));

                    break;
            }
        }
    }
}
