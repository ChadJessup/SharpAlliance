using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform
{
    public class Faces
    {
        public int InitFace(int v, int nOBODY, FaceFlags fACE_FORCE_SMALL)
        {
            return 0;
        }
    }

    [Flags]
    public enum FaceFlags
    {
        // FLAGS....
        FACE_DESTROY_OVERLAY = 0x00000000,                      // A face may contain a video overlay
        FACE_BIGFACE = 0x00000001,                      // A BIGFACE instead of small face
        FACE_POTENTIAL_KEYWAIT = 0x00000002,                        // If the option is set, will not stop face until key pressed
        FACE_PCTRIGGER_NPC = 0x00000004,                        // This face has to trigger an NPC after being done
        FACE_INACTIVE_HANDLED_ELSEWHERE = 0x00000008,   // This face has been setup and any disable should be done
                                                        // Externally                   
        FACE_TRIGGER_PREBATTLE_INT = 0x00000010,
        FACE_SHOW_WHITE_HILIGHT = 0x00000020,           // Show highlight around face
        FACE_FORCE_SMALL = 0x00000040,                      // force to small face	
        FACE_MODAL = 0x00000080,                        // make game modal
        FACE_MAKEACTIVE_ONCE_DONE = 0x00000100,
        FACE_SHOW_MOVING_HILIGHT = 0x00000200,
        FACE_REDRAW_WHOLE_FACE_NEXT_FRAM = 0x00000400,						// Redraw the complete face next frame
    }
}
