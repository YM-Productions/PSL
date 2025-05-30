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

            ClientLog.Info(ctx, $"Created foundation {foundation.Name} at ({foundation.XPos}, {foundation.YPos}) in plot {plotID}.");
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

            ClientLog.Info(ctx, $"Created vehicle {vehicle.Name} at ({vehicle.XPos}, {vehicle.YPos}) in plot {plotID}.");
        }

        [Reducer]
        public static void SetPhysicalObjectPermission(ReducerContext ctx, Identity targetIdentity, string physicalObjectID, int level)
        {
            if (ctx.Sender.Equals(targetIdentity))
            {
                ClientLog.Warning(ctx, "Cannot set permission for yourself.");
                return;
            }

            string actingPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);
            string targetPermID = PhysicalObjectPermission.GetIdentity(targetIdentity, physicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(actingPermID) is not PhysicalObjectPermission actingPerm ||
                actingPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to set permissions for this physical object.");
                return;
            }

            if (ctx.Db.PhysicalObjectPermission.identity.Find(targetPermID) is PhysicalObjectPermission targetPerm)
            {
                if (targetPerm.Level >= actingPerm.Level)
                {
                    ClientLog.Warning(ctx, "Cannot lower permission of a higher or equal level permission.");
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
                        ClientLog.Warning(ctx, "Cannot set permission to something higher than your own.");
                        return;
                    }
                    targetPerm.Level = level;
                    ctx.Db.PhysicalObjectPermission.identity.Update(targetPerm);
                }
            }
            else
            {
                if (level == Permission.Level.None)
                {
                    ClientLog.Warning(ctx, "Cannot set permission to None for a user that does not have permission.");
                    return;
                }

                if (actingPerm.Level < level)
                {
                    ClientLog.Warning(ctx, "Cannot set permission to something higher than your own.");
                    return;
                }
                PhysicalObjectPermission newPoPerm = new(targetIdentity, physicalObjectID, level);
                ctx.Db.PhysicalObjectPermission.Insert(newPoPerm);
            }

            ClientLog.Info(ctx, $"Set permission for {targetIdentity} on physical object {physicalObjectID} to {level}.");
        }

        [Reducer]
        public static void SetPhysicalObjectName(ReducerContext ctx, string PhysicalObjectID, string name)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, PhysicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is not PhysicalObjectPermission poPerm ||
                poPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to set the name of this physical object.");
                return;
            }

            if (ctx.Db.PhysicalObject.identity.Find(PhysicalObjectID) is not PhysicalObject physicalObject)
            {
                ClientLog.Warning(ctx, "Physical object not found.");
                return;
            }

            physicalObject.Name = name;
            ctx.Db.PhysicalObject.identity.Update(physicalObject);

            ClientLog.Info(ctx, $"Set name of physical object {PhysicalObjectID} to {name}.");
        }

        [Reducer]
        public static void SetPhysicalObjectParentIdentity(ReducerContext ctx, string physicalObjectID, string plotIdentity)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is not PhysicalObjectPermission poPerm ||
                poPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to set the name of this physical object.");
                return;
            }

            if (ctx.Db.PhysicalObject.identity.Find(physicalObjectID) is not PhysicalObject physicalObject)
            {
                ClientLog.Warning(ctx, "Physical object not found.");
                return;
            }

            if (ctx.Db.Plot.identity.Find(plotIdentity) is not Plot plot)
            {
                ClientLog.Warning(ctx, "Plot not found.");
                return;
            }

            physicalObject.ParentIdentity = plotIdentity;
            ctx.Db.PhysicalObject.identity.Update(physicalObject);

            ClientLog.Info(ctx, $"Set parent identity of physical object {physicalObjectID} to plot {plotIdentity}.");
        }

        [Reducer]
        public static void SetPhysicalObjectPosition(ReducerContext ctx, string physicalObjectID, int xPos, int yPos)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is not PhysicalObjectPermission poPerm ||
                poPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to set the position of this physical object.");
                return;
            }

            if (ctx.Db.PhysicalObject.identity.Find(physicalObjectID) is not PhysicalObject physicalObject)
            {
                ClientLog.Warning(ctx, "Physical object not found.");
                return;
            }

            // TODO: Check if position is valid (e.g. not colliding with other objects, in bounds of the plot, etc.)

            physicalObject.XPos = xPos;
            physicalObject.YPos = yPos;
            ctx.Db.PhysicalObject.identity.Update(physicalObject);

            ClientLog.Info(ctx, $"Set position of physical object {physicalObjectID} to ({xPos}, {yPos}).");
        }

        [Reducer]
        public static void DestroyPhysicalObject(ReducerContext ctx, string pysicalObjectID)
        {
            string poPermID = PhysicalObjectPermission.GetIdentity(ctx.Sender, pysicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(poPermID) is not PhysicalObjectPermission poPerm ||
                poPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to destroy this physical object.");
                return;
            }

            if (ctx.Db.PhysicalObject.identity.Find(pysicalObjectID) is not PhysicalObject physicalObject)
            {
                ClientLog.Warning(ctx, "Physical object not found.");
                return;
            }

            foreach (PhysicalObjectPermission permission in ctx.Db.PhysicalObjectPermission.Iter()
                     .Where(p => p.PhysicalObjectIdentity == physicalObject.identity))
            {
                ctx.Db.PhysicalObjectPermission.Delete(permission);
            }

            foreach (Hardpoint hardpoint in ctx.Db.Hardpoint.Iter()
                     .Where(h => h.PhysicalObjectIdentity == physicalObject.identity))
            {
                HardpointReducers.DestroyHardpoint(ctx, hardpoint.identity);
            }

            ctx.Db.PhysicalObject.Delete(physicalObject);

            ClientLog.Info(ctx, $"Destroyed physical object {pysicalObjectID}.");
        }
    }

    public static partial class HardpointReducers
    {
        [Reducer]
        public static void CreateHardpoint(ReducerContext ctx, string physicalObjectID, int size)
        {
            string permID = PhysicalObjectPermission.GetIdentity(ctx.Sender, physicalObjectID);

            if (ctx.Db.PhysicalObjectPermission.identity.Find(permID) is not PhysicalObjectPermission poPerm ||
                poPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to create a hardpoint on this physical object.");
                return;
            }

            Hardpoint hardpoint = new()
            {
                identity = Guid.NewGuid().ToString(),
                PhysicalObjectIdentity = physicalObjectID,
                Size = size,
            };

            ctx.Db.Hardpoint.Insert(hardpoint);

            ClientLog.Info(ctx, $"Created hardpoint {hardpoint.identity} on physical object {physicalObjectID} with size {size}.");
        }

        [Reducer]
        public static void SetHardpointPermission(ReducerContext ctx, Identity targetIdentity, string hardpointID, int level)
        {
            if (ctx.Sender.Equals(targetIdentity))
            {
                ClientLog.Warning(ctx, "Cannot set permission for yourself.");
                return;
            }

            string actingPermID = HardpointPermission.GetIdentity(ctx.Sender, hardpointID);
            string targetPermID = HardpointPermission.GetIdentity(targetIdentity, hardpointID);

            if (ctx.Db.HardpointPermission.identity.Find(actingPermID) is not HardpointPermission actingPerm)
            {
                ClientLog.Warning(ctx, "You do not have permission to set permissions for this hardpoint.");
                return;
            }

            if (actingPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to set permissions for this hardpoint.");
                return;
            }

            if (ctx.Db.HardpointPermission.identity.Find(targetPermID) is HardpointPermission targetPerm)
            {
                if (targetPerm.Level >= actingPerm.Level)
                {
                    ClientLog.Warning(ctx, "Cannot lower permission of a higher or equal level permission.");
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
                        ClientLog.Warning(ctx, "Cannot set permission to something higher than your own.");
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
                {
                    ClientLog.Warning(ctx, "Cannot set permission to something higher than your own.");
                    return;
                }

                HardpointPermission newPerm = new(targetIdentity, hardpointID, level);
                ctx.Db.HardpointPermission.Insert(newPerm);
            }

            ClientLog.Info(ctx, $"Set permission for {targetIdentity} on hardpoint {hardpointID} to {level}.");
        }

        [Reducer]
        public static void DestroyHardpoint(ReducerContext ctx, string hardpointID)
        {
            string permID = HardpointPermission.GetIdentity(ctx.Sender, hardpointID);

            if (ctx.Db.HardpointPermission.identity.Find(permID) is not HardpointPermission actingPerm ||
                actingPerm.Level < Permission.Level.Admin)
            {
                ClientLog.Warning(ctx, "You do not have permission to destroy this hardpoint.");
                return;
            }

            if (ctx.Db.Hardpoint.identity.Find(hardpointID) is not Hardpoint hardpoint)
            {
                ClientLog.Warning(ctx, "Hardpoint not found.");
                return;
            }

            foreach (HardpointPermission permission in ctx.Db.HardpointPermission.Iter()
                     .Where(p => p.HardpointIdentity == hardpoint.identity))
            {
                ctx.Db.HardpointPermission.Delete(permission);
            }

            // TODO: Destroy all items in hardpoint
            ClientLog.Info(ctx, "Destroying items in hardpoint is not yet implemented.");

            ctx.Db.Hardpoint.Delete(hardpoint);

            ClientLog.Info(ctx, $"Destroyed hardpoint {hardpointID}.");
        }
    }

    #endregion
}
