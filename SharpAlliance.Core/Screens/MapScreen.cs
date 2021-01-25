using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using static SharpAlliance.Core.Managers.VideoObjectManager;

namespace SharpAlliance.Core.Screens
{
    public class MapScreen : IScreen
    {
        private readonly IVideoObjectManager videoObjects;
        private readonly MapScreenInterfaceMap mapScreenInterface;
        private readonly MessageSubSystem messages;

        public MapScreen(
            IVideoObjectManager videoObjectManager,
            MapScreenInterfaceMap mapScreenInterfaceMap,
            MessageSubSystem messageSubSystem)
        { 
            this.videoObjects = videoObjectManager;
            this.mapScreenInterface = mapScreenInterfaceMap;
            this.messages = messageSubSystem;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {
            return ValueTask.FromResult(ScreenName.MAP_SCREEN);
        }

        public ValueTask<bool> Initialize()
        {
            VOBJECT_DESC VObjectDesc = new();

            this.mapScreenInterface.SetUpBadSectorsList();

            // setup message box system
            this.messages.InitGlobalMessageList();

            // init palettes for big map
            this.mapScreenInterface.InitializePalettesForMap();

            // set up mapscreen fast help text
            this.mapScreenInterface.SetUpMapScreenFastHelpText();

            // set up leave list arrays for dismissed mercs
            this.mapScreenInterface.InitLeaveList();

            VObjectDesc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            VObjectDesc.ImageFile = Utils.FilenameForBPP("INTERFACE\\group_confirm.sti");

            
            this.videoObjects.AddVideoObject(ref VObjectDesc, out var idx1);
            this.mapScreenInterface.guiUpdatePanel = idx1;

            VObjectDesc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            VObjectDesc.ImageFile = Utils.FilenameForBPP("INTERFACE\\group_confirm_tactical.sti");
            this.videoObjects.AddVideoObject(ref VObjectDesc, out var idx2);
            this.mapScreenInterface.guiUpdatePanelTactical = idx2;

            return ValueTask.FromResult(true);
        }
     
        public void HandlePreloadOfMapGraphics()
        {
        }

        public void Dispose()
        {
        }
    }
}
