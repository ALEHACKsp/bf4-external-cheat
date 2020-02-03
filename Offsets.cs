using System;

namespace BF4_Private_By_Tejisav
{
    struct Offsets
    {
        public static long OFFSET_DXRENDERER = 0x142738080; //0x142572FA0 //0x14256FEA0 //0x142567e00 //0x14255cd50
        public static long OFFSET_GAMECONTEXT = 0x142670d80; //0x1424abd20 //0x1424a0e88 //0x1424a0c00 //0x142495b50
        public static long OFFSET_GAMERENDERER = 0x142672378; //0x1424AD330 //0x1424aa268 //0x1424a21f8 //0x1424970b0
        public static long OFFSET_VIEWANGLES = 0x1423b2ec0; //0x1421caee0 //0x1421c7e20 //0x1421e3400 //0x1421d8360
        public static long OFFSET_MAIN = 0x142364b78; //0x14219FF68 //0x14219cfc8 //0x142194fc8 //0x142189ee8
        public static long OFFSET_CURRENT_WEAPONFIRING = 0x1423b2ec8; //0x1421c7e28 //0x1421e3408 //0x1421d8368 //OFFSET_VIEWANGLES + 0x8 
        public static long OFFSET_SHOTSSTAT = 0x142737A40; //0x142572950 //0x14256F860 //0x1425677C8 //0x14255c718 //48 8B 05 ? ? ? ? 48 85 C0 74 0C F3 0F 11 40 ? C6 80 ? ? ? ? ? F3 C3
        public static long OFFSET_BORDERINPUTNODE = 0x142671fb0; //0x1424ACF70; //0x1424a9ea0 //0x1424a1e30 //0x142496cb8
        public static long OFFSET_WORLDRENDERSETTINGS = 0x1426724a0; //0x1424ad460 //0x1424aa390 //0x1424a2320 //0x1424971f8

        public struct ClientGameContext
        {
            public static long m_pPhysicsManager = 0x28; // HavokPhysicsManager
            public static long m_pPlayerManager = 0x60;  // ClientPlayerManager

            public static long GetInstance()
            {
                return OFFSET_GAMECONTEXT;
            }
        }

        public struct ClientPlayerManager
        {
            public static long m_pLocalPlayer = 0x540; // ClientPlayer
            public static long m_ppPlayer = 0x548;     // ClientPlayer
        }

        public struct ClientPlayer
        {
            public static long szName = 0x40;            // 10 CHARS
            public static long m_isSpectator = 0x13C9;   // BYTE
            public static long m_teamId = 0x13CC;        // INT32
            public static long m_character = 0x14B0;     // ClientSoldierEntity 
            public static long m_ownPlayerView = 0x1510; // ClientPlayerView
            public static long m_PlayerView = 0x1520;    // ClientPlayerView
            public static long m_pAttachedControllable = 0x14C0;   // ClientSoldierEntity (ClientVehicleEntity)
            public static long m_pControlledControllable = 0x14D0; // ClientSoldierEntity
            public static long m_attachedEntryId = 0x14C8; // INT32
            public static long m_pInputState = 0x14E0; // EntryInputState
            public static long m_pExternalInputState = 0x14E8; // EntryInputState
        }

        public struct ClientPlayerView
        {
            public static long m_Owner = 0x00F8; // ClientPlayer
        }

        public struct ClientVehicleEntity
        {
            public static long m_data = 0x0030;           // VehicleEntityData
            public static long m_pPhysicsEntity = 0x0238; // DynamicPhysicsEntity
            public static long m_Velocity = 0x0280;       // D3DXVECTOR3 
            public static long m_prevVelocity = 0x0290;   // D3DXVECTOR3 
            public static long m_Chassis = 0x03E0;        // ClientChassisComponent
            public static long m_childrenAABB = 0x0250;   // AxisAlignedBox
        }

        public struct AxisAlignedBox
        {
            public static long m_Min = 0x00; // D3DXVECTOR3 
            public static long m_Max = 0x10; // D3DXVECTOR3 
        }

        public struct DynamicPhysicsEntity
        {
            public static long m_EntityTransform = 0xA0;  // PhysicsEntityTransform
        }

        public struct PhysicsEntityTransform
        {
            public static long m_Transform = 0x00;       // D3DXMATRIX
        }

        public struct VehicleEntityData
        {
            public static long m_FrontMaxHealth = 0x148; // FLOAT
            public static long m_NameSid = 0x0248;       // char* ID_P_VNAME_9K22
        }

        public struct ClientChassisComponent
        {
            public static long m_Velocity = 0x01C0; // D3DXVECTOR4
        }

        public struct ClientSoldierEntity
        {
            public static long m_data = 0x0030;         // VehicleEntityData
            public static long m_pPlayer = 0x01E0;          // ClientPlayer
            public static long m_pHealthComponent = 0x0140; // HealthComponent
            public static long m_authorativeYaw = 0x04D8;   // FLOAT
            public static long m_authorativePitch = 0x04DC; // FLOAT 
            public static long m_poseType = 0x04F0;         // INT32
            public static long m_RenderFlags = 0x04F4;      // INT32
            public static long m_pPhysicsEntity = 0x0238;   // VehicleDynamicPhysicsEntity
            public static long m_pPredictedController = 0x0490;    // ClientSoldierPrediction
            public static long m_soldierWeaponsComponent = 0x0570; // ClientSoldierWeaponsComponent
            public static long m_ragdollComponent = 0x0580;        // ClientRagDollComponent 
            public static long m_breathControlHandler = 0x0588;    // BreathControlHandler 
            public static long m_sprinting = 0x5B0;  // BYTE 
            public static long m_occluded = 0x05B1;  // BYTE
            public static long m_clientVaultComponent = 0x0D30;    // ClientSoldierVaultComponent
        }

        public struct HealthComponent
        {
            public static long m_Health = 0x0020;        // FLOAT
            public static long m_MaxHealth = 0x0024;     // FLOAT
            public static long m_vehicleHealth = 0x0038; // FLOAT (pLocalSoldier + 0x1E0 + 0x14C0 + 0x140 + 0x38)
        }

        public struct ClientSoldierPrediction
        {
            public static long m_Position = 0x0030; // D3DXVECTOR3
            public static long m_Velocity = 0x0050; // D3DXVECTOR3
        }

        public struct ClientSoldierWeaponsComponent
        {
            public enum WeaponSlot
            {
                M_PRIMARY = 0,
                M_SECONDARY = 1,
                M_GADGET = 2,
                M_GRENADE = 6,
                M_KNIFE = 7
            };

            public static long m_handler = 0x0890;      // m_handler + m_activeSlot * 0x8 = ClientSoldierWeapon
            public static long m_activeSlot = 0x0A98;   // INT32 (WeaponSlot)
            public static long m_activeHandler = 0x08D0; // ClientActiveWeaponHandler 
            public static long m_zeroingDistanceLevel = 0x0AC8; // INT32 changes in this range -> (-1,0,1,2,3,4) //-1 -> 100, 0->200, 1->300, 2->400, 3->500, 4-> 1000;
        }

        public struct ClientSoldierVaultComponent
        {
            public static long m_data = 0x10;   // Float
            public static long m_startHeightMax = 0x70;   // Float Superjump Default 1.5f Set to 255.0f
        }

        public struct UpdatePoseResultData
        {
            public enum BONES
            {
                BONE_HEAD = 104,
                BONE_NECK = 142,
                BONE_SPINE2 = 7,
                BONE_SPINE1 = 6,
                BONE_SPINE = 5,
                BONE_LEFTSHOULDER = 9,
                BONE_RIGHTSHOULDER = 109,
                BONE_LEFTELBOWROLL = 11,
                BONE_RIGHTELBOWROLL = 111,
                BONE_LEFTHAND = 15,
                BONE_RIGHTHAND = 115,
                BONE_LEFTKNEEROLL = 188,
                BONE_RIGHTKNEEROLL = 197,
                BONE_LEFTFOOT = 184,
                BONE_RIGHTFOOT = 198
            };

            public static long m_ActiveWorldTransforms = 0x0028; // QuatTransform
            public static long m_ValidTransforms = 0x0040;       // BYTE
        }

        public struct ClientRagDollComponent
        {
            public static long m_ragdollTransforms = 0x0088; // UpdatePoseResultData
            public static long m_Transform = 0x05D0;         // D3DXMATRIX
        }

        public struct QuatTransform
        {
            public static long m_TransAndScale = 0x0000; // D3DXVECTOR4
            public static long m_Rotation = 0x0010;      // D3DXVECTOR4
        }

        public struct ClientSoldierWeapon
        {
            public static long m_data = 0x0030;              // WeaponEntityData
            public static long m_authorativeAiming = 0x4988; // ClientSoldierAimingSimulation
            public static long m_pWeapon = 0x49A8;           // ClientWeapon
            public static long m_pPrimary = 0x49C0;          // WeaponFiring
        }

        public struct ClientActiveWeaponHandler
        {
            public static long m_activeWeapon = 0x038; // ClientSoldierWeapon
        }

        public struct WeaponEntityData
        {
            public static long m_name = 0x0130; // char*
        }

        public struct ClientSoldierAimingSimulation
        {
            public static long m_fpsAimer = 0x0010;  // AimAssist
            public static long m_yaw = 0x0018;       // FLOAT
            public static long m_pitch = 0x001C;     // FLOAT
            public static long m_sway = 0x0028;      // D3DXVECTOR2
            public static long m_zoomLevel = 0x0068; // FLOAT
        }

        public struct ClientWeapon
        {
            public static long m_pModifier =  0x0020; // WeaponModifier
            public static long m_shootSpace = 0x0040; // D3DXMATRIX
        }

        public struct WeaponFiring
        {
            public static long m_pSway = 0x0078;                  // WeaponSway
            public static long m_pPrimaryFire = 0x0128;           // PrimaryFire 
            public static long m_projectilesLoaded = 0x01A0;      // INT32 
            public static long m_projectilesInMagazines = 0x01A4; // INT32 
            public static long m_overheatPenaltyTimer = 0x01B0;   // FLOAT
            public static long m_pWeaponModifier = 0x01F0;        // WeaponModifier
        }

        public struct WeaponSway
        {
            public static long m_pSwayData = 0x0008;      // GunSwayData
            public static long m_deviationPitch = 0x0130; // FLOAT 
            public static long m_deviationYaw = 0x0134;   // FLOAT 
        }

        public struct GunSwayData
        {
            public static long m_DeviationScaleFactorZoom = 0x430;           // FLOAT 
            public static long m_GameplayDeviationScaleFactorZoom = 0x434;   // FLOAT 
            public static long m_DeviationScaleFactorNoZoom = 0x438;         // FLOAT 
            public static long m_GameplayDeviationScaleFactorNoZoom = 0x43C; // FLOAT 

            public static long m_ShootingRecoilDecreaseScale = 0x440; // FLOAT 
            public static long m_FirstShotRecoilMultiplier = 0x444;   // FLOAT 
        }

        public struct PrimaryFire
        {
            public static long m_FiringData = 0x0010; // ->FiringFunctionData  also ->ShotConfigData1 That is outside FiringFunctionData
            
        }

        public struct FiringFunctionData
        {
            public static long m_ShotConfigData = 0x0060; // ->ShotConfigData inside FiringFunctionData 
            public static long m_FireLogic = 0x0168;       // FireLogicData
        }

        public struct ShotConfigData // Inside FiringFunctionData
        {
            public static long m_InitialPosition = 0x0000; // D3DXMATRIX
            public static long m_InitialDirection = 0x0010; // D3DXMATRIX
            public static long m_InitialSpeed = 0x0020;   // D3DXMATRIX
            public static long m_NumberOfBulletsPerShell = 0x0078;   // int32
            public static long m_NumberOfBulletsPerShot = 0x007C;   // int32
            public static long m_NumberOfBulletsPerBurst = 0x0080;   // int32
        }

        public struct FireLogicData // FireLogicData --> Is not a pointer inside PrimaryFire, it's inherited at 0x168
        {
            public static long m_TriggerPullWeight = 0x5C;    // m_TriggerPullWeight --> 0.15 FAMAS
            public static long m_RateOfFire = 0x60;           // 1200.000f rpm max
            public static long m_RateOfFireForBurst = 0x64;   // 1200.000f rpm max
        }

        public struct ShotConfigData1 // outside FiringFunctionData
        {
            public static long m_initialSpeed = 0x0088;    // FLOAT 
            public static long m_pProjectileData = 0x00B0; // BulletEntityData
        }

        public struct BulletEntityData
        {
            public static long m_Gravity = 0x0130;     // FLOAT
            public static long m_StartDamage = 0x0154; // FLOAT
            public static long m_EndDamage = 0x0158;   // FLOAT
        }

        public struct WeaponModifier
        {
            public static long m_pWeaponZeroingModifier = 0x00C0; // WeaponZeroingModifier
        }

        public struct WeaponZeroingModifier
        {
            public static long m_pModes = 0x0018; // D3DXVECTOR2 contains distance and angle for modes above 100 i.e. 0,1,2,3,4
        }

        public struct AimAssist
        {
            public static long m_yaw = 0x0014;   // FLOAT
            public static long m_pitch = 0x0018; // FLOAT
        }

        public struct BreathControlHandler
        {
            public static long m_breathControlTimer = 0x0038; // FLOAT
            public static long m_breathControlMultiplier = 0x003C; // FLOAT  
            public static long m_breathControlPenaltyTimer = 0x0040; // FLOAT  
            public static long m_breathControlpenaltyMultiplier = 0x0044; // FLOAT  
            public static long m_breathControlActive = 0x0048; // FLOAT  
            public static long m_breathControlInput = 0x004C; // FLOAT  
            public static long m_breathActive = 0x0050; // FLOAT  
            public static long m_Enabled = 0x0058; // FLOAT  
        }

        public struct EntryInputState
        {
            public enum AnalogInput
            {
                AnalogInput_MoveLR = 0,
                AnalogInput_Roll = 1,
                AnalogInput_Firing = 2,
                AnalogInput_MoveFB = 3,
                AnalogInput_Pitch = 4,
                AnalogInput_Break = 5,
                AnalogInput_6 = 6,
                AnalogInput_7 = 7,
                AnalogInput_Yaw = 8,
                AnalogInput_MouseUD = 9, //unknown Vehicle movement Thing
                AnalogInput_MouseLR = 10, //unknown Vehicle movement Thing
            };

            public static long m_AnalogInput = 0x0100; // + AnalogInput * 0x4
        }

        public struct GameRenderer
        {
            public static long m_pRenderView = 0x60; // RenderView

            public static long GetInstance()
            {
                return OFFSET_GAMERENDERER;
            }
        }

        public struct RenderView
        {
            public static long m_Transform = 0x0040;         // D3DXMATRIX
            public static long m_FovY = 0x00B4;              // FLOAT
            public static long m_fovX = 0x0250;              // FLOAT
            public static long m_ViewProj = 0x0420;          // D3DXMATRIX
            public static long m_ViewMatrixInverse = 0x02E0; // D3DXMATRIX
            public static long m_ViewProjInverse = 0x04A0;   // D3DXMATRIX
        }
        public struct ShotsStat
        {
            public static long m_shotsFired = 0x48;
            public static long m_shotHit = 0x4c;
            public static long m_damageCount = 0x50;
            public static long GetInstance()
            {
                return Offsets.OFFSET_SHOTSSTAT;
            }
        }
        public struct BorderInputNode
        {
            public static long m_inputCache = 0x0008; //InputCache
            public static long m_pKeyboard = 0x0050; // Keyboard
            public static long m_pMouse = 0x0058; // Mouse
            public static long GetInstance()
            {
                return OFFSET_BORDERINPUTNODE;
            }
        }

        public struct InputCache
        {
            public enum InputConceptIdentifiers
            {
                ConceptMoveFB = 0, //0x0000
                ConceptMoveLR = 1, //0x0001
                ConceptMoveForward = 2, //0x0002
                ConceptMoveBackward = 3, //0x0003
                ConceptMoveLeft = 4, //0x0004
                ConceptMoveRight = 5, //0x0005
                ConceptYaw = 6, //0x0006
                ConceptPitch = 7, //0x0007
                ConceptRoll = 8, //0x0008
                ConceptRecenterCamera = 9, //0x0009
                ConceptFire = 10, //0x000A
                ConceptAltFire = 11, //0x000B
                ConceptFireCountermeasure = 12, //0x000C
                ConceptReload = 13, //0x000D
                ConceptZoom = 14, //0x000E
                ConceptToggleCamera = 15, //0x000F
                ConceptSprint = 16, //0x0010
                ConceptCrawl = 17, //0x0011
                ConceptToggleWeaponLight = 18, //0x0012
                ConceptSelectPartyMember0 = 19, //0x0013
                ConceptSelectPartyMember1 = 20, //0x0014
                ConceptSelectPartyMember2 = 21, //0x0015
                ConceptSelectPartyMember3 = 22, //0x0016
                ConceptLockTarget = 23, //0x0017
                ConceptCrosshairMoveX = 24, //0x0018
                ConceptCrosshairMoveY = 25, //0x0019
                ConceptTacticalMenu = 26, //0x001A
                ConceptConversationSelect = 27, //0x001B
                ConceptConversationSkip = 28, //0x001C
                ConceptConversationChangeSelection = 29, //0x001D
                ConceptJump = 30, //0x001E
                ConceptCrouch = 31, //0x001F
                ConceptCrouchOnHold = 32, //0x0020
                ConceptProne = 33, //0x0021
                ConceptInteract = 34, //0x0022
                ConceptPickUp = 35, //0x0023
                ConceptDrop = 36, //0x0024
                ConceptBreathControl = 37, //0x0025
                ConceptParachute = 38, //0x0026
                ConceptSwitchInventoryItem = 39, //0x0027
                ConceptSelectInventoryItem1 = 40, //0x0028
                ConceptSelectInventoryItem2 = 41, //0x0029
                ConceptSelectInventoryItem3 = 42, //0x002A
                ConceptSelectInventoryItem4 = 43, //0x002B
                ConceptSelectInventoryItem5 = 44, //0x002C
                ConceptSelectInventoryItem6 = 45, //0x002D
                ConceptSelectInventoryItem7 = 46, //0x002E
                ConceptSelectInventoryItem8 = 47, //0x002F
                ConceptSelectInventoryItem9 = 48, //0x0030
                ConceptSwitchToPrimaryWeapon = 49, //0x0031
                ConceptSwitchToGrenadeLauncher = 50, //0x0032
                ConceptSwitchToStaticGadget = 51, //0x0033
                ConceptSwitchToDynamicGadget1 = 52, //0x0034
                ConceptSwitchToDynamicGadget2 = 53, //0x0035
                ConceptMeleeAttack = 54, //0x0036
                ConceptThrowGrenade = 55, //0x0037
                ConceptCycleFireMode = 56, //0x0038
                ConceptChangeVehicle = 57, //0x0039
                ConceptBrake = 58, //0x003A
                ConceptHandBrake = 59, //0x003B
                ConceptClutch = 60, //0x003C
                ConceptGearUp = 61, //0x003D
                ConceptGearDown = 62, //0x003E
                ConceptGearSwitch = 63, //0x003F
                ConceptNextPosition = 64, //0x0040
                ConceptSelectPosition1 = 65, //0x0041
                ConceptSelectPosition2 = 66, //0x0042
                ConceptSelectPosition3 = 67, //0x0043
                ConceptSelectPosition4 = 68, //0x0044
                ConceptSelectPosition5 = 69, //0x0045
                ConceptSelectPosition6 = 70, //0x0046
                ConceptSelectPosition7 = 71, //0x0047
                ConceptSelectPosition8 = 72, //0x0048
                ConceptCameraPitch = 73, //0x0049
                ConceptCameraYaw = 74, //0x004A
                ConceptMapZoom = 75, //0x004B
                ConceptMapInnerZoom = 76, //0x004C
                ConceptMapSize = 77, //0x004D
                ConceptMapThreeDimensional = 78, //0x004E
                ConceptScoreboard = 79, //0x004F
                ConceptScoreboardAndMenuCombo = 80, //0x0050
                ConceptMenu = 81, //0x0051
                ConceptSpawnMenu = 82, //0x0052
                ConceptCancel = 83, //0x0053
                ConceptCommMenu1 = 84, //0x0054
                ConceptCommMenu2 = 85, //0x0055
                ConceptCommMenu3 = 86, //0x0056
                ConceptAccept = 87, //0x0057
                ConceptDecline = 88, //0x0058
                ConceptSelect = 89, //0x0059
                ConceptBack = 90, //0x005A
                ConceptActivate = 91, //0x005B
                ConceptDeactivate = 92, //0x005C
                ConceptEdit = 93, //0x005D
                ConceptView = 94, //0x005E
                ConceptParentNavigateLeft = 95, //0x005F
                ConceptParentNavigateRight = 96, //0x0060
                ConceptMenuZoomIn = 97, //0x0061
                ConceptMenuZoomOut = 98, //0x0062
                ConceptPanX = 99, //0x0063
                ConceptPanY = 100, //0x0064
                ConceptBattledashToggle = 101, //0x0065
                ConceptVoiceFunction1 = 102, //0x0066
                ConceptSquadVoice = 103, //0x0067
                ConceptSayAllChat = 104, //0x0068
                ConceptTeamChat = 105, //0x0069
                ConceptSquadChat = 106, //0x006A
                ConceptSquadLeaderChat = 107, //0x006B
                ConceptToggleChat = 108, //0x006C
                ConceptQuicktimeInteractDrag = 109, //0x006D
                ConceptQuicktimeFire = 110, //0x006E
                ConceptQuicktimeBlock = 111, //0x006F
                ConceptQuicktimeFastMelee = 112, //0x0070
                ConceptQuicktimeJumpClimb = 113, //0x0071
                ConceptQuicktimeCrouchDuck = 114, //0x0072
                ConceptFreeCameraMoveUp = 115, //0x0073
                ConceptFreeCameraMoveDown = 116, //0x0074
                ConceptFreeCameraMoveLR = 117, //0x0075
                ConceptFreeCameraMoveFB = 118, //0x0076
                ConceptFreeCameraMoveUD = 119, //0x0077
                ConceptFreeCameraRotateX = 120, //0x0078
                ConceptFreeCameraRotateY = 121, //0x0079
                ConceptFreeCameraRotateZ = 122, //0x007A
                ConceptFreeCameraIncreaseSpeed = 123, //0x007B
                ConceptFreeCameraDecreaseSpeed = 124, //0x007C
                ConceptFreeCameraFOVModifier = 125, //0x007D
                ConceptFreeCameraChangeFOV = 126, //0x007E
                ConceptFreeCameraSwitchSpeed = 127, //0x007F
                ConceptFreeCameraTurboSpeed = 128, //0x0080
                ConceptFreeCameraActivator1 = 129, //0x0081
                ConceptFreeCameraActivator2 = 130, //0x0082
                ConceptFreeCameraActivator3 = 131, //0x0083
                ConceptFreeCameraMayaInputActivator = 132, //0x0084
                ConceptTargetedCameraDistance = 133, //0x0085
                ConceptTargetedCameraRotateX = 134, //0x0086
                ConceptTargetedCameraRotateY = 135, //0x0087
                ConceptTargetedCameraChangeSpeed = 136, //0x0088
                ConceptLThumb = 137, //0x0089
                ConceptRThumb = 138, //0x008A
                ConceptLeftStickX = 139, //0x008B
                ConceptLeftStickY = 140, //0x008C
                ConceptRightStickX = 141, //0x008D
                ConceptRightStickY = 142, //0x008E
                ConceptButtonDPadLeft = 143, //0x008F
                ConceptButtonDPadRight = 144, //0x0090
                ConceptButtonDPadUp = 145, //0x0091
                ConceptButtonDPadDown = 146, //0x0092
                ConceptButtonA = 147, //0x0093
                ConceptButtonB = 148, //0x0094
                ConceptButtonX = 149, //0x0095
                ConceptButtonY = 150, //0x0096
                ConceptButtonSelect = 151, //0x0097
                ConceptButtonStart = 152, //0x0098
                ConceptButtonL1 = 153, //0x0099
                ConceptButtonL2 = 154, //0x009A
                ConceptButtonR1 = 155, //0x009B
                ConceptButtonR2 = 156, //0x009C
                ConceptButtonLeftThumb = 157, //0x009D
                ConceptButtonRightThumb = 158, //0x009E
                ConceptToggleMinimapType = 159, //0x009F
                ConceptDeployZoom = 160, //0x00A0
                ConceptMenuDigitalUp = 161, //0x00A1
                ConceptMenuDigitalDown = 162, //0x00A2
                ConceptMenuDigitalLeft = 163, //0x00A3
                ConceptMenuDigitalRight = 164, //0x00A4
                ConceptRightStickUp = 165, //0x00A5
                ConceptRightStickDown = 166, //0x00A6
                ConceptRightStickLeft = 167, //0x00A7
                ConceptRightStickRight = 168, //0x00A8
                ConceptMultipleSelect = 169, //0x00A9
                ConceptSelectAll = 170, //0x00AA
                ConceptMinimize = 171, //0x00AB
                ConceptMoveCameraUp = 172, //0x00AC
                ConceptMoveCameraDown = 173, //0x00AD
                ConceptMoveCameraLeft = 174, //0x00AE
                ConceptMoveCameraRight = 175, //0x00AF
                ConceptSelectSquad1 = 176, //0x00B0
                ConceptSelectSquad2 = 177, //0x00B1
                ConceptSelectSquad3 = 178, //0x00B2
                ConceptSelectSquad4 = 179, //0x00B3
                ConceptSelectSquad5 = 180, //0x00B4
                ConceptSelectSquad6 = 181, //0x00B5
                ConceptSelectSquad7 = 182, //0x00B6
                ConceptSelectSquad8 = 183, //0x00B7
                ConceptSelectSquad9 = 184, //0x00B8
                ConceptSpectatorViewPrev = 185, //0x00B9
                ConceptSpectatorViewNext = 186, //0x00BA
                ConceptSpectatorTargetPrev = 187, //0x00BB
                ConceptSpectatorTargetNext = 188, //0x00BC
                ConceptSpectatorViewTableTop = 189, //0x00BD
                ConceptSpectatorViewFirstPerson = 190, //0x00BE
                ConceptSpectatorViewThirdPerson = 191, //0x00BF
                ConceptSpectatorViewFreeCam = 192, //0x00C0
                ConceptSpectatorViewPlayer1 = 193, //0x00C1
                ConceptSpectatorViewPlayer2 = 194, //0x00C2
                ConceptSpectatorViewPlayer3 = 195, //0x00C3
                ConceptSpectatorViewPlayer4 = 196, //0x00C4
                ConceptSpectatorViewPlayer5 = 197, //0x00C5
                ConceptSpectatorViewPlayer6 = 198, //0x00C6
                ConceptSpectatorViewPlayer7 = 199, //0x00C7
                ConceptSpectatorViewPlayer8 = 200, //0x00C8
                ConceptSpectatorViewPlayer9 = 201, //0x00C9
                ConceptSpectatorViewPlayer10 = 202, //0x00CA
                ConceptSpectatorViewPlayer11 = 203, //0x00CB
                ConceptSpectatorViewPlayer12 = 204, //0x00CC
                ConceptSpectatorViewPlayer13 = 205, //0x00CD
                ConceptSpectatorViewPlayer14 = 206, //0x00CE
                ConceptSpectatorViewPlayer15 = 207, //0x00CF
                ConceptSpectatorViewPlayer16 = 208, //0x00D0
                ConceptSpectatorViewOptions = 209, //0x00D1
                ConceptSpectatorHudVisibility = 210, //0x00D2
                ConceptAnalogZoom = 211, //0x00D3
                ConceptSpectatorTargetPrevInSquad = 212, //0x00D4
                ConceptSpectatorTargetNextInSquad = 213, //0x00D5
                ConceptSpectatorTargetPrevOnTeam = 214, //0x00D6
                ConceptSpectatorTargetNextOnTeam = 215, //0x00D7
                ConceptSpectatorFreeCameraIncreaseSpeed = 216, //0x00D8
                ConceptSpectatorFreeCameraDecreaseSpeed = 217, //0x00D9
                ConceptSpectatorFreeCameraSwitchSpeed = 218, //0x00DA
                ConceptSpectatorFreeCameraMoveLR = 219, //0x00DB
                ConceptSpectatorFreeCameraMoveUD = 220, //0x00DC
                ConceptSpectatorFreeCameraMoveFB = 221, //0x00DD
                ConceptSpectatorFreeCameraRotateX = 222, //0x00DE
                ConceptSpectatorFreeCameraRotateY = 223, //0x00DF
                ConceptSpectatorFreeCameraRotateZ = 224, //0x00E0
                ConceptSpectatorFreeCameraTurboSpeed = 225, //0x00E1
                ConceptSpectatorFreeCameraFOVModifier = 226, //0x00E2
                ConceptSpectatorFreeCameraChangeFOV = 227, //0x00E3
                ConceptSpectatorSquadLeft = 228, //0x00E4
                ConceptSpectatorSquadRight = 229, //0x00E5
                ConceptSpectatorSquadUp = 230, //0x00E6
                ConceptSpectatorSquadDown = 231, //0x00E7
                ConceptSpectatorSquadActivate = 232, //0x00E8
                ConceptUndefined = 233, //0x00E9
                ConceptSize = 234 //0x00EA
            };

            public static long m_flInputBuffer = 0x0004; // float buffer + InputConceptIdentifiers(multiply with 0x4)
        }

        public struct Keyboard
        {
            public static long m_pDevice = 0x0008; // KeyboardDevice
        }

        public struct KeyboardDevice
        {
            public enum InputDeviceKeys
            {
                IDK_None = 0,
                IDK_Escape = 1,
                IDK_1 = 2,
                IDK_2 = 3,
                IDK_3 = 4,
                IDK_4 = 5,
                IDK_5 = 6,
                IDK_6 = 7,
                IDK_7 = 8,
                IDK_8 = 9,
                IDK_9 = 10,
                IDK_0 = 11,
                IDK_Minus = 12,
                IDK_Equals = 13,
                IDK_Backspace = 14,
                IDK_Tab = 15,
                IDK_Q = 16,
                IDK_W = 17,
                IDK_E = 18,
                IDK_R = 19,
                IDK_T = 20,
                IDK_Y = 21,
                IDK_U = 22,
                IDK_I = 23,
                IDK_O = 24,
                IDK_P = 25,
                IDK_LeftBracket = 26,
                IDK_RightBracket = 27,
                IDK_Enter = 28,
                IDK_LeftCtrl = 29,
                IDK_A = 30,
                IDK_S = 31,
                IDK_D = 32,
                IDK_F = 33,
                IDK_G = 34,
                IDK_H = 35,
                IDK_J = 36,
                IDK_K = 37,
                IDK_L = 38,
                IDK_Semicolon = 39,
                IDK_Apostrophe = 40,
                IDK_Grave = 41,
                IDK_LeftShift = 42,
                IDK_Backslash = 43,
                IDK_Z = 44,
                IDK_X = 45,
                IDK_C = 46,
                IDK_V = 47,
                IDK_B = 48,
                IDK_N = 49,
                IDK_M = 50,
                IDK_Comma = 51,
                IDK_Period = 52,
                IDK_Slash = 53,
                IDK_RightShift = 54,
                IDK_Multiply = 55,
                IDK_LeftAlt = 56,
                IDK_Space = 57,
                IDK_Capital = 58,
                IDK_F1 = 59,
                IDK_F2 = 60,
                IDK_F3 = 61,
                IDK_F4 = 62,
                IDK_F5 = 63,
                IDK_F6 = 64,
                IDK_F7 = 65,
                IDK_F8 = 66,
                IDK_F9 = 67,
                IDK_F10 = 68,
                IDK_Numlock = 69,
                IDK_ScrollLock = 70,
                IDK_Numpad7 = 71,
                IDK_Numpad8 = 72,
                IDK_Numpad9 = 73,
                IDK_Subtract = 74,
                IDK_Numpad4 = 75,
                IDK_Numpad5 = 76,
                IDK_Numpad6 = 77,
                IDK_Add = 78,
                IDK_Numpad1 = 79,
                IDK_Numpad2 = 80,
                IDK_Numpad3 = 81,
                IDK_Numpad0 = 82,
                IDK_Decimal = 83,
                IDK_OEM_102 = 86,
                IDK_F11 = 87,
                IDK_F12 = 88,
                IDK_F13 = 100,
                IDK_F14 = 101,
                IDK_F15 = 102,
                IDK_Kana = 112,
                IDK_PTBRSlash = 115,
                IDK_Convert = 121,
                IDK_NoConvert = 123,
                IDK_Yen = 125,
                IDK_PTBRNUMPADPOINT = 126,
                IDK_NumpadEquals = 141,
                IDK_PrevTrack = 144,
                IDK_At = 145,
                IDK_Colon = 146,
                IDK_Underline = 147,
                IDK_Kanji = 148,
                IDK_Stop = 149,
                IDK_Ax = 150,
                IDK_Unlabeled = 151,
                IDK_NextTrack = 153,
                IDK_NumpadEnter = 156,
                IDK_RightCtrl = 157,
                IDK_Mute = 160,
                IDK_Calculator = 161,
                IDK_PlayPause = 162,
                IDK_MediaStop = 164,
                IDK_VolumeDown = 174,
                IDK_VolumeUp = 176,
                IDK_WebHome = 178,
                IDK_NumpadComma = 179,
                IDK_Divide = 181,
                IDK_PrintScreen = 183,
                IDK_RightAlt = 184,
                IDK_Home = 199,
                IDK_ArrowUp = 200,
                IDK_PageUp = 201,
                IDK_ArrowLeft = 203,
                IDK_ArrowRight = 205,
                IDK_End = 207,
                IDK_ArrowDown = 208,
                IDK_PageDown = 209,
                IDK_Insert = 210,
                IDK_Delete = 211,
                IDK_LeftWin = 219,
                IDK_RightWin = 220,
                IDK_AppMenu = 221,
                IDK_Power = 222,
                IDK_Sleep = 223,
                IDK_Wake = 227,
                IDK_WebSearch = 229,
                IDK_WebFavorites = 230,
                IDK_WebRefresh = 231,
                IDK_WebStop = 232,
                IDK_WebForward = 233,
                IDK_WebBack = 234,
                IDK_MyComputer = 235,
                IDK_Mail = 236,
                IDK_MediaSelect = 237,
                IDK_Pause = 197,
                IDK_Undefined = 255
            };

            public static long m_Buffer = 0x01A8; // + InputDeviceKeys
        }

        public struct Mouse
        {
            public static long m_pDevice = 0x0010; //  MouseDevice
        }

        public struct MouseDevice
        {
            public enum InputDeviceMouseButtons
            {
                IDB_Button_0 = 0, // Left Click
                IDB_Button_1 = 1, // Right Click
                IDB_Button_2 = 2, // Middle Mouse Click
                IDB_Button_3 = 3, // Back Click
                IDB_Button_4 = 4, // Forward Click
                IDB_Button_5 = 5,
                IDB_Button_6 = 6,
                IDB_Button_7 = 7,
                IDB_Button_Undefined = 8
            }; 

            public static long m_Buffer = 0x0104; // D3DXVECTOR3 (x = xaxis y = yaxis z = mousewheel) OR + m_buutons
            public static long x = 0x0000;
            public static long y = 0x0004;
            public static long z = 0x0008;
            public static long m_buttons = 0x000C; // + InputDeviceMouseButtons
        }

        public struct VehicleWeapon
        {
            public static long m_pClientCameraComponent = 0x0010; // ClientCameraComponent
            public static long GetInstance()
            {
                return OFFSET_CURRENT_WEAPONFIRING;
            }
        }

        public struct ClientCameraComponent
        {
            public static long m_pChaseorStaticCamera = 0x00B8; // StaticCamera
        }

        public struct StaticCamera
        {
            public static long m_PreCrossMatrix = 0x0010;    // D3DXMATRIX
            public static long m_CrossMatrix = 0x0050;       // D3DXMATRIX
            public static long m_ForwardOffset = 0x01D0;     // D3DXVECTOR3
        }

        public struct WorldRenderSettings
        {
            public static long m_SkyEnable = 0x0135;   // Byte
            public static long m_SkyFogEnable = 0x0136; //Byte
            public static long GetInstance()
            {
                return OFFSET_WORLDRENDERSETTINGS;
            }
        }
    }
}
