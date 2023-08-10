using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitAPI.Common
{
    public enum ReasonOfDamage : byte
    {
        Server = 0,
        Weapon = 1,
        Bleeding = 2,
        Fall = 3,
        HelicopterBlade = 4,
        VehicleExplosion = 5,
        Explosion = 6,
        vehicleRunOver = 7,
        BuildingCollapsing = 8,
        SledgeHammer = 9,
        TreeFall = 10,
        CountAsKill = 11,
        Suicide = 12,
        HelicopterCrash = 13,
        BarbedWire = 14,
    }
}
