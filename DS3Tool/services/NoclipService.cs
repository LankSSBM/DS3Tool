using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace DS3Tool

{

    public class NoClipService : IDisposable
    {

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }


        private readonly DS3Process _process;
        private readonly DispatcherTimer _movementTimer;
        private bool _isActive;

        private POINT _lastMousePos;
        private bool _isRotating = false;
        private const float FORWARD_SPEED = 0.14f;
        private const float STRAFE_SPEED = 0.18f;
        private const float VERTICAL_SPEED = 0.25f;

        private const float ROTATION_SPEED = 0.003f;

        public NoClipService(DS3Process process)
        {
            _process = process;
            _movementTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) 
            };
            _movementTimer.Tick += MovementTimer_Tick;
        }

        public void Toggle(bool enable)
        {
            if (enable && !_isActive)
            {
                EnableNoClipFuncs();
                _process.moveCamToPlayer();
                
                _movementTimer.Start();
                _isActive = true;
            }
            else if (!enable && _isActive)
            {
                _movementTimer.Stop();
                _isActive = false;
            }
        }

        private void EnableNoClipFuncs()
        {
        
            _process.enableOpt(DS3Process.DebugOpts.HIDDEN);
            _process.enableOpt(DS3Process.DebugOpts.SILENT);
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            if (!_isActive) return;

            var currentPos = _process.getSetFreeCamCoords();
            var playerPos = _process.getSetPlayerLocalCoords();
            float speedMultiplier = Keyboard.IsKeyDown(Key.LeftShift) ? 3.0f : 1.0f;

            float x = currentPos.x;
            float y = currentPos.y;
            float z = currentPos.z;

            var camPtr = _process.getFreeCamPtr();
            float camDirX = _process.ReadFloat((IntPtr)(camPtr + 0x10));
            float camDirY = _process.ReadFloat((IntPtr)(camPtr + 0x14));
            float camDirZ = _process.ReadFloat((IntPtr)(camPtr + 0x18));
            Console.WriteLine(camDirX + " " + camDirY + " " + camDirZ);

            if (Keyboard.IsKeyDown(Key.W))
            {
                x += camDirX * FORWARD_SPEED * speedMultiplier;
                z += camDirZ * FORWARD_SPEED * speedMultiplier;
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                x -= camDirX * FORWARD_SPEED * speedMultiplier;
                z -= camDirZ * FORWARD_SPEED * speedMultiplier;
            }

            if (Keyboard.IsKeyDown(Key.A))
            {
                x -= camDirZ * STRAFE_SPEED * speedMultiplier;
                z += camDirX * STRAFE_SPEED * speedMultiplier;
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                x += camDirZ * STRAFE_SPEED * speedMultiplier;
                z -= camDirX * STRAFE_SPEED * speedMultiplier;
            }

            if (Keyboard.IsKeyDown(Key.Space))
                y += VERTICAL_SPEED * speedMultiplier;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                y -= VERTICAL_SPEED * speedMultiplier;


            _process.getSetFreeCamCoords((x, y, z));

            UpdateCameraRotation();
        }


        public void UpdateCameraRotation()
        {
            // Check if right mouse button is pressed
            bool isRightButtonPressed = (GetAsyncKeyState(0x02) & 0x8000) != 0;

            if (isRightButtonPressed)
                Console.WriteLine("Mouse is pressed");
            {
                if (!_isRotating)
                {
                
                    
                    GetCursorPos(out _lastMousePos);
                    _isRotating = true;
                    Console.WriteLine("STarted rotate");


            
                    POINT currentPos;
                    GetCursorPos(out currentPos);

             
                    int deltaX = currentPos.X - _lastMousePos.X;
                    int deltaY = currentPos.Y - _lastMousePos.Y;

              
                    var camPtr = _process.getFreeCamPtr();

           
                    float hRotation = _process.ReadFloat((IntPtr)(camPtr + 0x14));
                    hRotation += deltaX * ROTATION_SPEED;
                    _process.WriteFloat((IntPtr)(camPtr + 0x14), hRotation);

              
                    float vRotation = _process.ReadFloat((IntPtr)(camPtr + 0x10));
                    vRotation += deltaY * ROTATION_SPEED;
             
                    vRotation = Math.Max(-1.5f, Math.Min(1.5f, vRotation));
                    _process.WriteFloat((IntPtr)(camPtr + 0x10), vRotation);

           
                    SetCursorPos(_lastMousePos.X, _lastMousePos.Y);
                }


                else
                {
                    Console.WriteLine("Is not pressed");
                    _isRotating = false;
                }
            }
        }
        
        

        public void Dispose()
        {
            Toggle(false);
            _movementTimer?.Stop();
        }
    }
}