using SpacetimeDB;
using StdbModule.Utils;

namespace StdbModule.Modules;

public static partial class Module
{
    #region hack

    // HACK: Temporary PlotTable
    [Table(Name = nameof(Plot), Public = true)]
    public partial class Plot
    {
        [PrimaryKey]
        public string identity;

        public Plot()
        {
            identity = Guid.NewGuid().ToString();
        }
    }

    #endregion

    #region Tables

    [Table(Name = nameof(PhysicalObject), Public = true)]
    public partial class PhysicalObject
    {
        [PrimaryKey]
        public string identity;
        public string Name;

        public string ParentIdentity;
        public int XPos;
        public int YPos;

        public bool IsStatic;

        public PhysicalObject()
        {
            identity = Guid.NewGuid().ToString();
            Name = string.Empty;
            ParentIdentity = string.Empty;
            IsStatic = false;
        }
    }

    [Table(Name = nameof(PhysicalObjectPermission), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_pos_accid_poid", Columns = new[] { nameof(AccountIdentity), nameof(PhysicalObjectIdentity) })]
    public partial class PhysicalObjectPermission
    {
        [PrimaryKey]
        public string identity; // NOTE: AccountIdentity + PhysicalObjectIdentity
        public Identity AccountIdentity;
        public string PhysicalObjectIdentity;

        public int Level; // NOTE: 0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner

        public PhysicalObjectPermission(Identity accountIdentity, string physicalObjectIdentity, int level)
        {
            AccountIdentity = accountIdentity;
            PhysicalObjectIdentity = physicalObjectIdentity;
            Level = level;

            identity = GetIdentity();
        }

        public PhysicalObjectPermission()
        {
            PhysicalObjectIdentity = string.Empty;
            identity = GetIdentity();
            Level = Permission.Level.None;
        }

        public string GetIdentity()
        {
            return $"{AccountIdentity}-{PhysicalObjectIdentity}";
        }

        public static string GetIdentity(Identity accountIdentity, string physicalObjectIdentity)
        {
            return $"{accountIdentity}-{physicalObjectIdentity}";
        }
    }

    [Table(Name = nameof(Hardpoint), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_hardpoint_physicalobjectidentity", Columns = [nameof(PhysicalObjectIdentity)])]
    public partial class Hardpoint
    {
        [PrimaryKey]
        public string identity;

        public string PhysicalObjectIdentity;

        public int Size;

        public Hardpoint()
        {
            identity = Guid.NewGuid().ToString();
            PhysicalObjectIdentity = string.Empty;
            Size = 0;
        }
    }

    [Table(Name = nameof(HardpointPermission), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_hardpoint_permission_accid_hpid", Columns = new[] { nameof(AccountIdentity), nameof(HardpointIdentity) })]
    public partial class HardpointPermission
    {
        [PrimaryKey]
        public string identity; // NOTE: AccountIdentity + PhysicalObjectIdentity
        public Identity AccountIdentity;
        public string HardpointIdentity;

        public int Level; // NOTE: 0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner

        public HardpointPermission(Identity accountIdentity, string hardpointIdentity, int level)
        {
            AccountIdentity = accountIdentity;
            HardpointIdentity = hardpointIdentity;
            Level = level;

            identity = GetIdentity();
        }

        public HardpointPermission()
        {
            HardpointIdentity = string.Empty;
            identity = GetIdentity();
            Level = Permission.Level.None;
        }

        public string GetIdentity()
        {
            return $"{AccountIdentity}-{HardpointIdentity}";
        }

        public static string GetIdentity(Identity accountIdentity, string hardpointIdentity)
        {
            return $"{accountIdentity}-{hardpointIdentity}";
        }
    }

    #endregion

    #region VisibilityFilters
#pragma warning disable STDB_UNSTABLE

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PHYSICALOBJECT_PERMISSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(PhysicalObjectPermission)} WHERE AccountIdentity = :sender"
    );

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter HARDPOINT_PERMISSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(HardpointPermission)} WHERE AccountIdentity = :sender"
    );

#pragma warning restore STDB_UNSTABLE
    #endregion

    #region Reducers

    public static partial class PhysicalObjectReducers
    {
        [Reducer]
        public static void CreateFoundation(ReducerContext ctx, string name, string plotID, int xPos, int yPos)
        {
            PhysicalObject foundation = new()
            {
                identity = Guid.NewGuid().ToString(),
                Name = name,
                ParentIdentity = plotID,
                XPos = xPos,
                YPos = yPos,
                IsStatic = true
            };

            PhysicalObjectPermission permission = new(ctx.Sender, foundation.identity, 4);

            ctx.Db.PhysicalObject.Insert(foundation);
            ctx.Db.PhysicalObjectPermission.Insert(permission);
        }

        [Reducer]
        public static void CreateVehicle(ReducerContext ctx, string name, string plotID, int xPos, int yPos)
        {
            PhysicalObject vehicle = new PhysicalObject
            {
                identity = Guid.NewGuid().ToString(),
                Name = name,
                ParentIdentity = plotID,
                XPos = xPos,
                YPos = yPos,
                IsStatic = false
            };

            PhysicalObjectPermission permission = new(ctx.Sender, vehicle.identity, 4);

            ctx.Db.PhysicalObject.Insert(vehicle);
            ctx.Db.PhysicalObjectPermission.Insert(permission);
        }

        [Reducer]
        public static void SetPhysicalObjectPermission(ReducerContext ctx, Identity targetIdentity, string physicalObjectID, int level)
        {
            if (ctx.Sender.Equals(targetIdentity))
            {
                // Prevent users from changing their own permissions
                return;
            }

            string actingPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);
            string targetPermID = PhysicalObjectPermission.GetIdentity(targetIdentity, physicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(actingPermID) is not PhysicalObjectPermission actingPerm)
                return;

            if (actingPerm.Level < Permission.Level.Admin)
                return;

            if (ctx.Db.PhysicalObjectPermission.identity.Find(targetPermID) is PhysicalObjectPermission targetPerm)
            {
                if (targetPerm.Level >= actingPerm.Level)
                {
                    // Cannot lower permission of a higher/equal level permission
                    return;
                }

                if (level == Permission.Level.None)
                {
                    ctx.Db.PhysicalObjectPermission.Delete(targetPerm);
                }
                else
                {
                    if (actingPerm.Level < level)
                    {
                        // Cannot set permission to something higher than your own
                        return;
                    }
                    targetPerm.Level = level;
                    ctx.Db.PhysicalObjectPermission.identity.Update(targetPerm);
                }
            }
            else
            {
                if (level == Permission.Level.None)
                    return;

                if (actingPerm.Level < level)
                {
                    // Cannot set permission to something higher than your own
                    return;
                }
                PhysicalObjectPermission newPoPerm = new(targetIdentity, physicalObjectID, level);
                ctx.Db.PhysicalObjectPermission.Insert(newPoPerm);
            }
        }

        [Reducer]
        public static void SetPhysicalObjectName(ReducerContext ctx, string PhysicalObjectID, string name)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, PhysicalObjectID);
            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is PhysicalObjectPermission poPerm &&
                poPerm.Level >= Permission.Level.Admin &&
                ctx.Db.PhysicalObject.identity.Find(PhysicalObjectID) is PhysicalObject physicalObject)
            {
                physicalObject.Name = name;
                ctx.Db.PhysicalObject.identity.Update(physicalObject);
            }
        }

        [Reducer]
        public static void SetPhysicalObjectParentIdentity(ReducerContext ctx, string plotIdentity)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, plotIdentity);
            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is PhysicalObjectPermission poPerm &&
                poPerm.Level >= Permission.Level.Admin &&
                ctx.Db.PhysicalObject.identity.Find(plotIdentity) is PhysicalObject physicalObject)
            {
                physicalObject.ParentIdentity = plotIdentity;
                ctx.Db.PhysicalObject.identity.Update(physicalObject);
            }
        }

        [Reducer]
        public static void SetPhysicalObjectPosition(ReducerContext ctx, string physicalObjectID, int xPos, int yPos)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);
            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is PhysicalObjectPermission poPerm &&
                poPerm.Level >= Permission.Level.Admin &&
                ctx.Db.PhysicalObject.identity.Find(physicalObjectID) is PhysicalObject physicalObject)
            {
                physicalObject.XPos = xPos;
                physicalObject.YPos = yPos;
                ctx.Db.PhysicalObject.identity.Update(physicalObject);
            }
        }

        [Reducer]
        public static void DestroyPhysicalObject(ReducerContext ctx, string pysicalObjectID)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, pysicalObjectID);
            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is PhysicalObjectPermission poPerm &&
                poPerm.Level >= Permission.Level.Admin &&
                ctx.Db.PhysicalObject.identity.Find(pysicalObjectID) is PhysicalObject physicalObject)
            {
                foreach (PhysicalObjectPermission permission in ctx.Db.PhysicalObjectPermission.Iter()
                         .Where(p => p.PhysicalObjectIdentity == physicalObject.identity))
                {
                    ctx.Db.PhysicalObjectPermission.Delete(permission);
                }

                ctx.Db.PhysicalObject.Delete(physicalObject);
            }
        }
    }

    public static partial class HardpointReducers
    {
        [Reducer]
        public static void SetHardpointPermission(ReducerContext ctx, Identity targetIdentity, string hardpointID, int level)
        {
            if (ctx.Sender.Equals(targetIdentity))
                return;

            string actingPermID = HardpointPermission.GetIdentity(ctx.Sender, hardpointID);
            string targetPermID = HardpointPermission.GetIdentity(targetIdentity, hardpointID);

            if (ctx.Db.HardpointPermission.identity.Find(actingPermID) is not HardpointPermission actingPerm)
                return;

            if (actingPerm.Level < Permission.Level.Admin)
                return;

            if (ctx.Db.HardpointPermission.identity.Find(targetPermID) is HardpointPermission targetPerm)
            {
                if (targetPerm.Level >= actingPerm.Level)
                {
                    return;
                }

                if (level == Permission.Level.None)
                {
                    ctx.Db.HardpointPermission.Delete(targetPerm);
                }
                else
                {
                    if (actingPerm.Level < level)
                    {
                        return;
                    }
                    targetPerm.Level = level;
                    ctx.Db.HardpointPermission.identity.Update(targetPerm);
                }
            }
            else
            {
                if (level == Permission.Level.None)
                    return;

                if (actingPerm.Level < level)
                    return;

                HardpointPermission newPerm = new(targetIdentity, hardpointID, level);
                ctx.Db.HardpointPermission.Insert(newPerm);
            }
        }

        [Reducer]
        public static void DestroyHardpoint(ReducerContext ctx, string hardpointID)
        {
            string permID = HardpointPermission.GetIdentity(ctx.Sender, hardpointID);
            if (ctx.Db.HardpointPermission.identity.Find(permID) is HardpointPermission perm &&
                perm.Level >= Permission.Level.Admin &&
                ctx.Db.Hardpoint.identity.Find(hardpointID) is Hardpoint hardpoint)
            {
                foreach (HardpointPermission permission in ctx.Db.HardpointPermission.Iter()
                         .Where(p => p.HardpointIdentity == hardpoint.identity))
                {
                    ctx.Db.HardpointPermission.Delete(permission);
                }

                ctx.Db.Hardpoint.Delete(hardpoint);
            }
        }
    }

    #endregion
}
