using SharpDX;

namespace BF4_Private_By_Tejisav
{
    class Player
    {
        public string Name;
        public string VehicleName;
        public int Team;
        public Vector3 Origin;
        public Vector3 Velocity;
        public Vector2 Fov;
        public Vector2 Sway;
        public Vector3 ShootSpace;
        public RadDoll Bone;
        public bool BoneCheck;
        public bool InVehicle;
        public bool IsDriver;
        public bool IsMeAiming;
        public bool IsVehicleWeapon;
        public bool IsValidWeapon;
        //public bool IsSpectator;
        //public Int64 pPlayerView;
        //public Int64 pClientPlayer;
        //public Int64 pOwnerPlayerView;

        // Weapon Data
        public int Ammo, AmmoClip;
        public int shotsFired, shotHit;
        public float Heating;
        public int Pose;
        public int IsOccluded;
        public float Yaw;
        public float BulletGravity;
        public float BulletSpeed;
        public float Distance;
        public float DistanceCrosshair;
        public Vector3 BulletInitialPosition;
        public Vector3 BulletInitialSpeed;
        public int zeroingDistanceLevel;
        public Vector2 zeroingModes;

        // Soldier
        public float Health;
        public float MaxHealth;

        // Vehicle
        public float VehicleHealth;
        public float VehicleMaxHealth;
        public AxisAlignedBox VehicleAABB;
        public Matrix VehicleTranfsorm;

        public bool IsValid()
        {
            return (/*Health > 0 && Health <= 100 && */!Origin.IsZero);
        }

        public bool IsVisible()
        {
           return (IsOccluded == 0);
        }

        public AxisAlignedBox GetAABB()
        {
            AxisAlignedBox aabb = new AxisAlignedBox();
            if (Pose == 0) // standing
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 1.700000f, 0.350000f);
            }
            if (Pose == 1) // crouching
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 1.150000f, 0.350000f);
            }
            if (Pose == 2) // prone
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 0.400000f, 0.350000f);
            }
            return aabb;
        }
    }
}
