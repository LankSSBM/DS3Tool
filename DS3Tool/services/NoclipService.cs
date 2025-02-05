using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DS3Tool.services
{



    internal class NoClipService

    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);


        private DS3Process _ds3Process;



        private IntPtr REGION_ONE_OFFSET = (IntPtr)0x140000000 + 0x270BC20;
        private IntPtr REGION_TWO_OFFSET = (IntPtr)0x140000000 + 0x2718200;
        private IntPtr ORIGIN_COORD_BASE = (IntPtr)0x140000000 + 0x40A02F;
        private IntPtr ORIGIN_IN_AIR_TIMER = (IntPtr)0x140000000 + 0x9D181A;
        private IntPtr ORIGIN_CAM_H_ROTATE = (IntPtr)0x140000000 + 0x5166DC;
        private IntPtr ORIGIN_KEYS = (IntPtr)0x140000000 + 0x188C6FD;
        private IntPtr ORIGIN_COORDS_UPDATE = (IntPtr)0x140000000 + 0x9D2360;
        private IntPtr END_THREAD;
        private IntPtr FLY_MODE;

        IntPtr KEY_LISTENER_CAVE_START;


        private class HookData
        {
            public string Name { get; set; }
            public IntPtr OriginAddr { get; set; }
            public IntPtr CaveAddr { get; set; }
            public byte[] OriginalBytes { get; set; }
        }

        private List<HookData> hooks = new List<HookData>();


        public NoClipService(DS3Process _ds3process)
        {
            this._ds3Process = _ds3process;
        }

        public void EnableNoClip()
        {
           
            InitCodeCaves();

            InstallHooks();

            SetNoClipFuncs();

        }

        private void SetNoClipFuncs()
        {
            _ds3Process.WriteBytes(FLY_MODE, new byte[] { 1 });
            _ds3Process.enableOpt(DS3Process.DebugOpts.HIDDEN);
            _ds3Process.enableOpt(DS3Process.DebugOpts.SILENT);
        }

        private void InitCodeCaves()
        {


            IntPtr PLAYER_COORD_BASE = REGION_ONE_OFFSET + 0x20;     
            IntPtr PLAYER_CAM_INFO = REGION_ONE_OFFSET + 0x30;       
            IntPtr PLAYER_MOVEMENT_INFO = REGION_ONE_OFFSET + 0x40;  
            IntPtr IX_OFFSET = REGION_ONE_OFFSET + 0x50;            
             FLY_MODE = REGION_ONE_OFFSET + 0x60;
            IntPtr Z_DIRECTION = REGION_ONE_OFFSET + 0x70;
            IntPtr TEMP_Z_DIRECTION = REGION_ONE_OFFSET + 0x80;


            Console.WriteLine($"Testing Address - PLAYER_COORD_BASE: 0x{PLAYER_COORD_BASE.ToInt64():X}");
            Console.WriteLine($"Testing Address - PLAYER_CAM_INFO: 0x{PLAYER_CAM_INFO.ToInt64():X}");
            Console.WriteLine($"Testing Address - PLAYER_MOVEMENT_INFO: 0x{PLAYER_MOVEMENT_INFO.ToInt64():X}");
            Console.WriteLine($"Testing Address - IX_OFFSET: 0x{IX_OFFSET.ToInt64():X}");
            Console.WriteLine($"Testing Address - FLY_MODE: 0x{FLY_MODE.ToInt64():X}");
            Console.WriteLine($"Testing Address - Z_DIRECTION: 0x{Z_DIRECTION.ToInt64():X}");
            Console.WriteLine($"Testing Address - TEMP_Z_DIRECTION: 0x{TEMP_Z_DIRECTION.ToInt64():X}");
            Console.WriteLine($"Testing Address - END_THREAD: 0x{END_THREAD.ToInt64():X}");


            IntPtr PLAYER_COORD_CAVE_START = REGION_ONE_OFFSET + 0x140;
            IntPtr IN_AIR_TIMER_CAVE_START = REGION_ONE_OFFSET + 0x200;
            IntPtr CAM_H_ROTATE_CAVE_START = REGION_ONE_OFFSET + 0x280;
            IntPtr MOVEMENT_CAVE_START = REGION_ONE_OFFSET + 0x340;
            IntPtr COORDS_UPDATE_CAVE_START = REGION_TWO_OFFSET + 0x40;
            IntPtr COORDS_UPDATE_Z_START = REGION_TWO_OFFSET + 0x140;
            IntPtr COORDS_UPDATE_CAVE_EXIT = REGION_TWO_OFFSET + 0x200;


            Console.WriteLine($"Testing Address - PLAYER_COORD_CAVE_START: 0x{PLAYER_COORD_CAVE_START.ToInt64():X}");
            Console.WriteLine($"Testing Address - IN_AIR_TIMER_CAVE_START: 0x{IN_AIR_TIMER_CAVE_START.ToInt64():X}");
            Console.WriteLine($"Testing Address - CAM_H_ROTATE_CAVE_START: 0x{CAM_H_ROTATE_CAVE_START.ToInt64():X}");
            Console.WriteLine($"Testing Address - KEYS_CAVE_START: 0x{MOVEMENT_CAVE_START.ToInt64():X}");
            Console.WriteLine($"Testing Address - COORDS_UPDATE_CAVE_START: 0x{COORDS_UPDATE_CAVE_START.ToInt64():X}");

            var playerCoordCave = new byte[] {

                0x48, 0x8B, 0x48, 0x18,                 // mov rcx,[rax+18]
                0x48, 0x89, 0x0D                        // mov [player coords],rcx
};


            int playerCoordOffset = (int)(PLAYER_COORD_BASE.ToInt64() -
                (PLAYER_COORD_CAVE_START.ToInt64() + playerCoordCave.Length + 4));


            playerCoordCave = playerCoordCave
                .Concat(BitConverter.GetBytes(playerCoordOffset))
                .Concat(new byte[] {
                    0x48, 0x8B, 0x48, 0x18, // mov rcx,[rax+18]
                    0x8D, 0x46, 0x9C,       // lea eax,[rsi-0x64]
                  
                    0xE9                    // jmp return
                }).ToArray();


            int playerCoordReturn = (int)(ORIGIN_COORD_BASE.ToInt64() + 7 -
                (PLAYER_COORD_CAVE_START.ToInt64() + playerCoordCave.Length + 4));
            playerCoordCave = playerCoordCave
                .Concat(BitConverter.GetBytes(playerCoordReturn))
                .ToArray();


            Console.WriteLine($"Cave bytes: {BitConverter.ToString(playerCoordCave)}");
            try
            {
                _ds3Process.WriteBytes(PLAYER_COORD_CAVE_START, playerCoordCave);
                Console.WriteLine("Player Cords written successfully");


                hooks.Add(new HookData
                {
                    Name = "PlayerCoordBase",
                    OriginAddr = ORIGIN_COORD_BASE,
                    CaveAddr = PLAYER_COORD_CAVE_START,
                    OriginalBytes = new byte[] { 0x48, 0x8B, 0x48, 0x18, 0x8D, 0x46, 0x9C }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing cave: {ex.Message}");
                throw;
            }


            var inAirTimerCave = new byte[] {

                 0x50,                         // push rax
                0x48, 0x8B, 0x05                  // mov rax,[player cords]
        };

            playerCoordOffset = (int)(PLAYER_COORD_BASE.ToInt64() -
                (IN_AIR_TIMER_CAVE_START.ToInt64() + inAirTimerCave.Length + 4));


            inAirTimerCave = inAirTimerCave.Concat(BitConverter.GetBytes(playerCoordOffset))
                .Concat(new byte[] {
                    0x48, 0x85, 0xC0,       // test rax,rax
                      0x74, 0x0C,             // jz 
                      0x48, 0x8B, 0x40, 0x28, // mov rax,[rax+28]
                      0x48, 0x3B, 0xC1,       // cmp rax,rcx
                      0x75, 0x03,             // jne 
                      0x0F, 0x57, 0xC0,       // xorps xmm0,xmm0

                      0x58,                         // pop rax
                          0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01, 0x00, 0x00,  // movss [rcx+000001B0],xmm0
                      0xE9                     // jmp return  
                }).ToArray();


            int originAirTimerReturn = (int)((ORIGIN_IN_AIR_TIMER.ToInt64() + 8)
                - (IN_AIR_TIMER_CAVE_START.ToInt64() + inAirTimerCave.Length + 4));


            Console.WriteLine($"Return Offset: 0x{originAirTimerReturn:X}");

            inAirTimerCave = inAirTimerCave.Concat(BitConverter.GetBytes(originAirTimerReturn)).ToArray();

            Console.WriteLine($"Cave bytes: {BitConverter.ToString(inAirTimerCave)}");
            try
            {
                _ds3Process.WriteBytes(IN_AIR_TIMER_CAVE_START, inAirTimerCave);

                // Verify write
                byte[] verification = _ds3Process.ReadBytes(IN_AIR_TIMER_CAVE_START, inAirTimerCave.Length);
                Console.WriteLine("Cave written successfully");
                Console.WriteLine($"Verification: {BitConverter.ToString(verification)}");

                hooks.Add(new HookData
                {
                    Name = "InAirTimer",
                    OriginAddr = ORIGIN_IN_AIR_TIMER,
                    CaveAddr = IN_AIR_TIMER_CAVE_START,
                    OriginalBytes = new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01, 0x00, 0x00 }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing cave: {ex.Message}");
                throw;
            }

            var camHRotateCave = new byte[] {

                 0x48, 0x89, 0x35                        // mov [cam info],rsi
            };

            int camInfoOffset = (int)(PLAYER_CAM_INFO.ToInt64() - (CAM_H_ROTATE_CAVE_START.ToInt64() + camHRotateCave.Length + 4));

            camHRotateCave = camHRotateCave.Concat(BitConverter.GetBytes(camInfoOffset))
                 .Concat(new byte[] {
                     0x66, 0x0F, 0x7F, 0xAE, 0x40, 0x01, 0x00, 0x00,  // movdqa [rsi+00000140],xmm5
           
                     0xE9                                // jmp 
                 }).ToArray();

            int originCamInfoReturn = (int)(ORIGIN_CAM_H_ROTATE.ToInt64() + 8
                - (CAM_H_ROTATE_CAVE_START.ToInt64() + camHRotateCave.Length + 4));

            camHRotateCave = camHRotateCave.Concat(BitConverter.GetBytes(originCamInfoReturn)).ToArray();

            hooks.Add(new HookData
            {
                Name = "CamHRotate",
                OriginAddr = ORIGIN_CAM_H_ROTATE,
                CaveAddr = CAM_H_ROTATE_CAVE_START,
                OriginalBytes = new byte[]
                             { 0x66, 0x0F, 0x7F, 0xAE, 0x40, 0x01, 0x00, 0x00 }
            });

            try
            {
                _ds3Process.WriteBytes(CAM_H_ROTATE_CAVE_START, camHRotateCave);
                Console.WriteLine("Cam H written successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing Cave 3: {ex.Message}");
                throw;
            }

            var movementCave = new byte[] {
                 0x4C, 0x89, 0x05                        // mov [Movement info],r8
                };

            int movementInfoOffset = (int)(PLAYER_MOVEMENT_INFO.ToInt64() - (MOVEMENT_CAVE_START.ToInt64() + movementCave.Length + 4));

            movementCave = movementCave.Concat(BitConverter.GetBytes(movementInfoOffset))
                 .Concat(new byte[] {
                     0x48, 0x85, 0xFF,                   // test rdi,rdi
                     0x75, 0x0E,                         // jnz 
                     0x83, 0x7C, 0x24, 0x54, 0x00,      // cmp dword ptr [rsp+54],0
                     0x75, 0x07,                         // jne 
                     0x48, 0x89, 0x05                    // mov [ix offset],rax
                 }).ToArray();

            int iXOffset = (int)(IX_OFFSET.ToInt64() - (MOVEMENT_CAVE_START.ToInt64() + movementCave.Length + 4));

            movementCave = movementCave.Concat(BitConverter.GetBytes(iXOffset))
                .Concat(new byte[] {
           0xF3, 0x41, 0x0F, 0x11, 0x0C, 0x80, // movss [r8+rax*4],xmm1
           0xE9                                // jmp 
                }).ToArray();

            int keysReturnOffset = (int)((ORIGIN_KEYS.ToInt64() + 6) - (MOVEMENT_CAVE_START.ToInt64() + movementCave.Length + 4));

            movementCave = movementCave.Concat(BitConverter.GetBytes(keysReturnOffset)).ToArray();

            hooks.Add(new HookData
            {
                Name = "Keys",
                OriginAddr = ORIGIN_KEYS,
                CaveAddr = MOVEMENT_CAVE_START,
                OriginalBytes = new byte[]
                             {  0xF3, 0x41, 0x0F, 0x11, 0x0C, 0x80 }
            });


            try
            {
                _ds3Process.WriteBytes(MOVEMENT_CAVE_START, movementCave);
                Console.WriteLine("Keys written successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing Cave 3: {ex.Message}");
                throw;
            }


            var coordsUpdateCave = new byte[] {
                 0x80, 0x3D        // compare fly mode
                };

            int flyModeOffset = (int)(FLY_MODE.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4 + 1));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(flyModeOffset))
                .Concat(new byte[] {
                     0x01,                               // check if enabled
                     0x0F, 0x84, 0x05, 0x00, 0x00, 0x00,                      // je jump to play check
                     0xE9                                // return
                }).ToArray();

            int caveExitOffset = (int)(COORDS_UPDATE_CAVE_EXIT.ToInt64() -
                        (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(caveExitOffset))
                .Concat(new byte[] {
                    0x48, 0x8B, 0x0D                    // mov rcx,[player coords]
                })
                .ToArray();

            playerCoordOffset = (int)(PLAYER_COORD_BASE.ToInt64() -
                 (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(playerCoordOffset))
                 .Concat(new byte[] {
                     0x48, 0x85, 0xC9,                   // test rcx,rcx
                     0x0F, 0x84                          // je 
                 })
                 .ToArray();

            caveExitOffset = (int)(COORDS_UPDATE_CAVE_EXIT.ToInt64() -
                        (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(caveExitOffset))
                 .Concat(new byte[] {
                     0x48, 0x8B, 0x49, 0x28,            // mov rcx,[rcx+28]
                     0x48, 0x3B, 0xCB,                  // cmp rcx,rbx
                     0x0F, 0x85,                         // jne return
                 })
                 .ToArray();

            caveExitOffset = (int)(COORDS_UPDATE_CAVE_EXIT.ToInt64() -
                        (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(caveExitOffset))
                 .Concat(new byte[] 
                {
        0x66, 0x0F, 0x6F, 0xB3, 0x80, 0x00, 0x00, 0x00,  // movdqa [rbx+80], xmm6
        0x50, 0x52,                                    // push rax, push rdx
        0x48, 0x8B, 0x0D                               // mov rcx, [movement info]
                })
                .ToArray();

            movementInfoOffset = (int)(PLAYER_MOVEMENT_INFO.ToInt64() -
                 (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(movementInfoOffset))
                .Concat(new byte[] {
                    0x48, 0x85, 0xC9,                   // test rcx,rcx
                    0x0F, 0x84                          // jz z movement
                }).ToArray();

            int zCaveOffset = (int)(COORDS_UPDATE_Z_START.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(zCaveOffset))
                .Concat(new byte[] {
                    0x48, 0x8B, 0x15                    // mov rdx,[iXOffset]
                })
                .ToArray();

            iXOffset = (int)(IX_OFFSET.ToInt64() -
    (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(iXOffset))
                .Concat(new byte[] {
                   0x48, 0x85, 0xD2,                   // test rdx,rdx
                   0x0F, 0x84                         // jz z movement
                })
                .ToArray();

            zCaveOffset = (int)(COORDS_UPDATE_Z_START.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(zCaveOffset))
                .Concat(new byte[] {
                   0x48, 0xFF, 0xCA,                   // dec rdx
                   0xF3, 0x44, 0x0F, 0x10, 0x3C, 0x91,              // movss xmm15,[rcx+rdx*4]
                   0x45, 0x0F, 0xC6, 0xFF, 0x00,             // shufps xmm15,xmm15,00
                   0xB8, 0x00, 0x00, 0x80, 0x3E,        // float 0.25
                   0x66, 0x44, 0x0F, 0x6E, 0xF0,              // movd xmm14,eax  - 
                   0x45, 0x0F, 0xC6, 0xF6, 0x00,              // shufps xmm14,xmm14,00 
                   0x45, 0x0F, 0x59, 0xFE,                    // mulps xmm15,xmm14    
                   0x48, 0x8B, 0x05                    // mov rax,[pCamInfo]
                })
                .ToArray();

            camInfoOffset = (int)(PLAYER_CAM_INFO.ToInt64() -
            (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(camInfoOffset))
                .Concat(new byte[] {
                   0x48, 0x85, 0xC0,                   // test rax,rax
                   0x0F, 0x84                          // jz movement start
                })
                .ToArray();

            zCaveOffset = (int)(COORDS_UPDATE_Z_START.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(zCaveOffset))
                .Concat(new byte[] {
                   0x44, 0x0F, 0x59, 0xB8, 0xA0, 0x03, 0x00, 0x00,  // mulps xmm15,[rax+3a0]
                   0x41, 0x0F, 0x58, 0xF7,                   // addps xmm6,xmm15
                   0x48, 0xFF, 0xC2,                   // inc rdx
                    0xF3, 0x44, 0x0F, 0x10, 0x3C, 0x91,      // movss xmm15,[rcx+rdx*4]
                    0x45, 0x0F, 0xC6, 0xFF, 0x00,            // shufps xmm15,xmm15,00
                    0xB8, 0x3D, 0x0A, 0x39, 0x3E,      // mov eax,(float)0.18
                    0x66, 0x44, 0x0F, 0x6E, 0xF0,            // movd xmm14,eax
                    0x45, 0x0F, 0xC6, 0xF6, 0x00,             // shufps xmm14,xmm14,00
                    0x45, 0x0F, 0x59, 0xFE,                   // mulps xmm15,xmm14
                     0x48, 0x8B, 0x05,   //cam info
                }).ToArray();

            camInfoOffset = (int)(PLAYER_CAM_INFO.ToInt64() -
           (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));
            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(camInfoOffset))
               .Concat(new byte[] {
                   0x48, 0x85, 0xC0,                   // test rax,rax
                    0x0F, 0x84 //jz movement
                   })
                .ToArray();

            zCaveOffset = (int)(COORDS_UPDATE_Z_START.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));
            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(zCaveOffset))
                .Concat(new byte[] {
                    0x44, 0x0F, 0x59, 0xB8, 0x80, 0x03, 0x00, 0x00,  // mulps xmm15,[rax+380] 
                   0x41, 0x0F, 0x58, 0xF7,                   // addps xmm6,xmm15
                   0x0F, 0x84, //movement
                }).
                ToArray();

            zCaveOffset = (int)(COORDS_UPDATE_Z_START.ToInt64() - (COORDS_UPDATE_CAVE_START.ToInt64() + coordsUpdateCave.Length + 4));

            coordsUpdateCave = coordsUpdateCave.Concat(BitConverter.GetBytes(zCaveOffset)).ToArray();

            Console.WriteLine($"Cave bytes: {BitConverter.ToString(coordsUpdateCave)}");

            hooks.Add(new HookData
            {
                Name = "CoordsUpdate",
                OriginAddr = ORIGIN_COORDS_UPDATE,
                CaveAddr = COORDS_UPDATE_CAVE_START,
                OriginalBytes = new byte[]
                             { 0x66, 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00 }
            });


            try
            {
                _ds3Process.WriteBytes(COORDS_UPDATE_CAVE_START, coordsUpdateCave);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing coords update cave: {ex.Message}");
                throw;
            }


            var zDirectCave = new byte[]
            {
                0xF3, 0x44, 0x0F, 0x10, 0x3D,
            };

            int zDirectOffset = (int)(Z_DIRECTION.ToInt64() - (COORDS_UPDATE_Z_START.ToInt64() + zDirectCave.Length + 4));


            zDirectCave = zDirectCave.Concat(BitConverter.GetBytes(zDirectOffset))
                .Concat(new byte[]
                {
                     0x45, 0x0F, 0xC6, 0xFF, 0xE1,
                    0xB8, 0x29, 0x5C, 0x0F, 0x3E,     // mov eax,(float)0.14
                     0x66, 0x44, 0x0F, 0x6E, 0xF0,             // movd xmm14,eax
                    0x45, 0x0F, 0xC6, 0xF6, 0x00,             // shufps xmm14,xmm14,0x00
                   0x45, 0x0F, 0x59, 0xFE,                   // mulps xmm15,xmm14
                    0x41, 0x0F, 0x58, 0xF7,                   // addps xmm6,xmm15
                    0x0F, 0x29, 0xB3, 0x70, 0x01, 0x00, 0x00,  // movups [rbx+170],xmm6
                    0x5A,                        // pop rdx
                     0x58,                         // pop rax
                        
             0x45, 0x0F, 0x57, 0xF6,
0x45, 0x0F, 0x57, 0xFF,
                    0xE9,                               // return
                }).ToArray();

            caveExitOffset = (int)(COORDS_UPDATE_CAVE_EXIT.ToInt64() -
                        (COORDS_UPDATE_Z_START.ToInt64() + zDirectCave.Length + 4));

            zDirectCave = zDirectCave.Concat(BitConverter.GetBytes(caveExitOffset)).ToArray();

            try
            {
                _ds3Process.WriteBytes(COORDS_UPDATE_Z_START, zDirectCave);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing coords update cave: {ex.Message}");
                throw;
            }


            var updateCoordsCaveExit = new byte[]
            {
                0x66, 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00,  // movdqa [rbx+00000080],xmm6
                  0xE9,                               // return
                 };


            caveExitOffset = (int)(ORIGIN_COORDS_UPDATE.ToInt64() + 8 -
                        (COORDS_UPDATE_CAVE_EXIT.ToInt64() + updateCoordsCaveExit.Length + 4));

            updateCoordsCaveExit = updateCoordsCaveExit.Concat(BitConverter.GetBytes(caveExitOffset))
                .ToArray();


            try
            {
                _ds3Process.WriteBytes(COORDS_UPDATE_CAVE_EXIT, updateCoordsCaveExit);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing coords update cave: {ex.Message}");
                throw;
            }      
        }

        private void InstallHooks()

        {

            foreach (var hook in hooks)
            {
                IntPtr target = hook.OriginAddr;  // Already an IntPtr
                IntPtr source = hook.CaveAddr;  // Already an IntPtr

                _ds3Process.ReadTestFull(target);

                if (!VerifyHookLocation(target, hook.OriginalBytes))
                {
                    Console.WriteLine($"ERROR: Original bytes don't match for {hook.Name}!");
                    continue;
                }

             
                byte[] jumpBytes = new byte[hook.OriginalBytes.Length];
                jumpBytes[0] = 0xE9; 

            
                long delta = source.ToInt64() - (target.ToInt64() + 5);
                if (delta > int.MaxValue || delta < int.MinValue)
                {
                    Console.WriteLine($"ERROR: Jump distance too far for {hook.Name}!");
                    continue;
                }

             
                Buffer.BlockCopy(BitConverter.GetBytes((int)delta), 0, jumpBytes, 1, 4);

        
                for (int i = 5; i < jumpBytes.Length; i++)
                {
                    jumpBytes[i] = 0x90; 
                }


                try
                {
                    _ds3Process.WriteBytes(target, jumpBytes);
                    Console.WriteLine($"Successfully installed hook for {hook.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to install hook for {hook.Name}: {ex.Message}");
                }
            }

        }



        private bool VerifyHookLocation(IntPtr location, byte[] expectedBytes)
        {
            var currentBytes = _ds3Process.ReadBytes(location, expectedBytes.Length);
            return currentBytes.SequenceEqual(expectedBytes);
        }


        public void disableNoClip() {

            RestoreOriginalBytes();

            DisableNoClipFuncs();

            CleanupMemoryRegions();

        }

        private void RestoreOriginalBytes()
        {
            foreach (var hook in hooks)
            {
                try
                {
                    _ds3Process.WriteBytes(hook.OriginAddr, hook.OriginalBytes);
                    FlushInstructionCache(_ds3Process._targetProcessHandle, hook.OriginAddr, (UIntPtr)hook.OriginalBytes.Length);

                    var verificationBytes = _ds3Process.ReadBytes(hook.OriginAddr, hook.OriginalBytes.Length);
                    if (!verificationBytes.SequenceEqual(hook.OriginalBytes))
                    {
                        throw new Exception($"Verification failed for {hook.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error restoring bytes for {hook.Name}: {ex.Message}");
                    throw;
                }
            }
            hooks.Clear();
        }

        private void DisableNoClipFuncs()
        {
            _ds3Process.WriteBytes(FLY_MODE, new byte[] { 0 });
            _ds3Process.disableOpt(DS3Process.DebugOpts.HIDDEN);
            _ds3Process.disableOpt(DS3Process.DebugOpts.SILENT);
        }

        private void CleanupMemoryRegions()
        {
            try
            {

                var regionTwoSize = 3000;
                var zeroBytes = new byte[regionTwoSize];
                _ds3Process.WriteBytes(REGION_TWO_OFFSET, zeroBytes);
                Console.WriteLine("Region Two cleaned");

                var regionOneSize = 1024;
                zeroBytes = new byte[regionOneSize];
                _ds3Process.WriteBytes(REGION_ONE_OFFSET, zeroBytes);
                Console.WriteLine("Region One cleaned");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning memory regions: {ex.Message}");
                throw;
            }
        }
        
    }

}
