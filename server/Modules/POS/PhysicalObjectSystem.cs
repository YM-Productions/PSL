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

    /// <summary>
    /// Represents a physical object within the game world.
    /// </summary>
    /// <remarks>
    /// A <c>PhysicalObject</c> is an entity that can exist in the game environment, such as buildings, items, or other interactive elements.
    /// Each object has a unique identity, a name, and may have a parent object (for hierarchical relationships).
    /// The position of the object is defined by its <c>XPos</c> and <c>YPos</c> coordinates.
    /// The <c>IsStatic</c> flag indicates whether the object is immovable (e.g., a foundation or terrain feature).
    /// </remarks>
    [Table(Name = nameof(PhysicalObject), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_physicalobject_parentid", Columns = new[] { nameof(ParentIdentity) })]
    public partial class PhysicalObject
    {
        /// <summary>
        /// Unique identifier for the physical object.
        /// </summary>
        [PrimaryKey]
        public string identity;

        /// <summary>
        /// The display name of the object.
        /// </summary>
        public string Name;

        /// <summary>
        /// The identity of the parent object, if any.
        /// </summary>
        public string ParentIdentity;

        /// <summary>
        /// The X coordinate of the object's position.
        /// </summary>
        public int XPos;

        /// <summary>
        /// The Y coordinate of the object's position.
        /// </summary>
        public int YPos;

        /// <summary>
        /// Indicates whether the object is static (immovable).
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalObject"/> class with default values.
        /// </summary>
        public PhysicalObject()
        {
            identity = Guid.NewGuid().ToString();
            Name = string.Empty;
            ParentIdentity = string.Empty;
            IsStatic = false;
        }
    }

    /// <summary>
    /// Represents a permission entry for a specific account and physical object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>PhysicalObjectPermission</c> defines the access level an account has to a particular physical object.
    /// The <c>identity</c> field is a unique key composed of the <c>AccountIdentity</c> and <c>PhysicalObjectIdentity</c>.
    /// The <c>Level</c> property specifies the permission type:
    /// 0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner.
    /// </para>
    /// </remarks>
    [Table(Name = nameof(PhysicalObjectPermission), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_poperm_poid", Columns = new[] { nameof(PhysicalObjectIdentity) })]
    [SpacetimeDB.Index.BTree(Name = "idx_poperm_accid_poid", Columns = new[] { nameof(AccountIdentity), nameof(PhysicalObjectIdentity) })]
    public partial class PhysicalObjectPermission
    {
        /// <summary>
        /// Unique identifier for the permission entry (AccountIdentity + PhysicalObjectIdentity).
        /// </summary>
        [PrimaryKey]
        public string identity;

        /// <summary>
        /// The identity of the account to which this permission applies.
        /// </summary>
        public Identity AccountIdentity;

        /// <summary>
        /// The identity of the physical object to which this permission applies.
        /// </summary>
        public string PhysicalObjectIdentity;

        /// <summary>
        /// The permission level (0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner).
        /// </summary>
        public int Level;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalObjectPermission"/> class with the specified account, object, and permission level.
        /// </summary>
        /// <param name="accountIdentity">The account identity.</param>
        /// <param name="physicalObjectIdentity">The physical object identity.</param>
        /// <param name="level">The permission level.</param>
        public PhysicalObjectPermission(Identity accountIdentity, string physicalObjectIdentity, int level)
        {
            AccountIdentity = accountIdentity;
            PhysicalObjectIdentity = physicalObjectIdentity;
            Level = level;

            identity = GetIdentity();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalObjectPermission"/> class with default values.
        /// </summary>
        public PhysicalObjectPermission()
        {
            PhysicalObjectIdentity = string.Empty;
            identity = GetIdentity();
            Level = Permission.Level.None;
        }

        /// <summary>
        /// Returns the unique identity string for this permission entry.
        /// </summary>
        public string GetIdentity()
        {
            return $"{AccountIdentity}-{PhysicalObjectIdentity}";
        }

        /// <summary>
        /// Returns the unique identity string for the given account and object.
        /// </summary>
        /// <param name="accountIdentity">The account identity.</param>
        /// <param name="physicalObjectIdentity">The physical object identity.</param>
        public static string GetIdentity(Identity accountIdentity, string physicalObjectIdentity)
        {
            return $"{accountIdentity}-{physicalObjectIdentity}";
        }
    }

    /// <summary>
    /// Represents a hardpoint, which is an attachment slot or mount point on a physical object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <c>Hardpoint</c> is used to attach items or modules to a physical object, such as equipment slots on a vehicle or structure.
    /// Each hardpoint has a unique <c>identity</c>, references the owning physical object via <c>PhysicalObjectIdentity</c>,
    /// and specifies its <c>Size</c> (capacity or allowed item size).
    /// </para>
    /// </remarks>
    [Table(Name = nameof(Hardpoint), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_hardpoint_physicalobjectidentity", Columns = [nameof(PhysicalObjectIdentity)])]
    public partial class Hardpoint
    {
        /// <summary>
        /// Unique identifier for the hardpoint.
        /// </summary>
        [PrimaryKey]
        public string identity;

        /// <summary>
        /// The identity of the physical object this hardpoint belongs to.
        /// </summary>
        public string PhysicalObjectIdentity;

        /// <summary>
        /// The size or capacity of the hardpoint.
        /// </summary>
        public int Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hardpoint"/> class with default values.
        /// </summary>
        public Hardpoint()
        {
            identity = Guid.NewGuid().ToString();
            PhysicalObjectIdentity = string.Empty;
            Size = 0;
        }
    }

    /// <summary>
    /// Represents a permission entry for a specific account and hardpoint.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>HardpointPermission</c> defines the access level an account has to a particular hardpoint.
    /// The <c>identity</c> field is a unique key composed of the <c>AccountIdentity</c> and <c>HardpointIdentity</c>.
    /// The <c>Level</c> property specifies the permission type:
    /// 0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner.
    /// </para>
    /// </remarks>
    [Table(Name = nameof(HardpointPermission), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_hardpoint_permission_hpid", Columns = new[] { nameof(HardpointIdentity) })]
    [SpacetimeDB.Index.BTree(Name = "idx_hardpoint_permission_accid_hpid", Columns = new[] { nameof(AccountIdentity), nameof(HardpointIdentity) })]
    public partial class HardpointPermission
    {
        /// <summary>
        /// Unique identifier for the permission entry (AccountIdentity + HardpointIdentity).
        /// </summary>
        [PrimaryKey]
        public string identity; // NOTE: AccountIdentity + PhysicalObjectIdentity

        /// <summary>
        /// The identity of the account to which this permission applies.
        /// </summary>
        public Identity AccountIdentity;

        /// <summary>
        /// The identity of the hardpoint to which this permission applies.
        /// </summary>
        public string HardpointIdentity;

        /// <summary>
        /// The permission level (0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner).
        /// </summary>
        public int Level; // NOTE: 0 = No Access, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner

        /// <summary>
        /// Initializes a new instance of the <see cref="HardpointPermission"/> class with the specified account, hardpoint, and permission level.
        /// </summary>
        /// <param name="accountIdentity">The account identity.</param>
        /// <param name="hardpointIdentity">The hardpoint identity.</param>
        /// <param name="level">The permission level.</param>
        public HardpointPermission(Identity accountIdentity, string hardpointIdentity, int level)
        {
            AccountIdentity = accountIdentity;
            HardpointIdentity = hardpointIdentity;
            Level = level;

            identity = GetIdentity();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HardpointPermission"/> class with default values.
        /// </summary>
        public HardpointPermission()
        {
            HardpointIdentity = string.Empty;
            identity = GetIdentity();
            Level = Permission.Level.None;
        }

        /// <summary>
        /// Returns the unique identity string for this permission entry.
        /// </summary>
        public string GetIdentity()
        {
            return $"{AccountIdentity}-{HardpointIdentity}";
        }

        /// <summary>
        /// Returns the unique identity string for the given account and hardpoint.
        /// </summary>
        /// <param name="accountIdentity">The account identity.</param>
        /// <param name="hardpointIdentity">The hardpoint identity.</param>
        public static string GetIdentity(Identity accountIdentity, string hardpointIdentity)
        {
            return $"{accountIdentity}-{hardpointIdentity}";
        }
    }

    #endregion

    #region VisibilityFilters
#pragma warning disable STDB_UNSTABLE

    /// <summary>
    /// A filter that selects all <see cref="PhysicalObjectPermission"/> entries for the current account.
    /// </summary>
    /// <remarks>
    /// This filter is used to restrict visibility so that clients only see their own permissions.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PHYSICALOBJECT_PERMISSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(PhysicalObjectPermission)} WHERE AccountIdentity = :sender"
    );

    /// <summary>
    /// A filter that selects all <see cref="PhysicalObjectPermission"/> entries for the current admin account.
    /// </summary>
    /// <remarks>
    /// This filter is used to restrict visibility so that admin clients only see permissions relevant to their admin identity.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PHYSICALOBJECT_PERMISSION_FILTER_ADMIN = new Filter.Sql(
        $"SELECT {nameof(PhysicalObjectPermission)}.* FROM {nameof(PhysicalObjectPermission)} JOIN {nameof(Admin)} WHERE {nameof(Admin)}.identity = :sender"
    );

    /// <summary>
    /// A filter that selects all <see cref="HardpointPermission"/> entries for the current account.
    /// </summary>
    /// <remarks>
    /// This filter is used to restrict visibility so that clients only see their own hardpoint permissions.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter HARDPOINT_PERMISSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(HardpointPermission)} WHERE AccountIdentity = :sender"
    );

    /// <summary>
    /// A filter that selects all <see cref="HardpointPermission"/> entries for the current admin account.
    /// </summary>
    /// <remarks>
    /// This filter is used to restrict visibility so that admin clients only see hardpoint permissions relevant to their admin identity.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter HARDPOINT_PERMISSION_FILTER_ADMIN = new Filter.Sql(
        $"SELECT {nameof(HardpointPermission)}.* FROM {nameof(HardpointPermission)} JOIN {nameof(Admin)} WHERE {nameof(Admin)}.identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE
    #endregion

    #region Reducers

    /// <summary>
    /// Contains reducer methods for manipulating physical objects and hardpoints within the game world.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>PhysicalObjectReducers</c> static partial class provides methods for creating, modifying, and deleting physical objects and hardpoints,
    /// as well as managing permissions. These reducers are invoked in response to client or server actions to update the game state.
    /// </para>
    /// </remarks>
    public static partial class PhysicalObjectReducers
    {
        /// <summary>
        /// Creates a new static foundation object at the specified position within a plot.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="name">The display name of the foundation.</param>
        /// <param name="plotID">The identity of the plot to which the foundation will belong.</param>
        /// <param name="xPos">The X coordinate for the foundation's position.</param>
        /// <param name="yPos">The Y coordinate for the foundation's position.</param>
        /// <remarks>
        /// This reducer creates a new <see cref="PhysicalObject"/> representing a foundation, assigns it a unique identity,
        /// sets its parent to the specified plot, and marks it as static. It also creates an owner permission for the sender.
        /// </remarks>
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

        /// <summary>
        /// Creates a new vehicle object at the specified position within a plot.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="name">The display name of the vehicle.</param>
        /// <param name="plotID">The identity of the plot to which the vehicle will belong.</param>
        /// <param name="xPos">The X coordinate for the vehicle's position.</param>
        /// <param name="yPos">The Y coordinate for the vehicle's position.</param>
        /// <remarks>
        /// This reducer creates a new <see cref="PhysicalObject"/> representing a vehicle, assigns it a unique identity,
        /// sets its parent to the specified plot, and marks it as non-static. It also creates an owner permission for the sender.
        /// </remarks>
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

        /// <summary>
        /// Sets the permission level for a target identity on a specific physical object.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="targetIdentity">The identity of the user whose permission is being set.</param>
        /// <param name="physicalObjectID">The identity of the physical object.</param>
        /// <param name="level">The permission level to assign (0 = None, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner).</param>
        /// <remarks>
        /// This reducer allows an authorized user to set or remove permissions for another user on a physical object.
        /// The sender must have at least Admin level permission on the object and cannot modify their own permissions.
        /// If the target already has a permission entry, it will be updated or removed based on the specified level.
        /// If the target does not have a permission entry and the level is not None, a new entry will be created.
        /// </remarks>
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

        /// <summary>
        /// Sets the name of a physical object if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="PhysicalObjectID">The identity of the physical object whose name will be set.</param>
        /// <param name="name">The new name to assign to the physical object.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to change the name of a physical object.
        /// If the sender lacks permission or the object does not exist, a warning is logged and no changes are made.
        /// </remarks>
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

        /// <summary>
        /// Sets the parent identity of a physical object to a specified plot, if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="physicalObjectID">The identity of the physical object whose parent will be set.</param>
        /// <param name="plotIdentity">The identity of the plot to set as the new parent.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to change the parent plot of a physical object.
        /// If the sender lacks permission, the physical object, or the plot does not exist, a warning is logged and no changes are made.
        /// </remarks>
        [Reducer]
        public static void SetPhysicalObjectParentIdentity(ReducerContext ctx, string physicalObjectID, string plotIdentity)
        {
            ClientLog.Info(ctx, $"SetPhysicalObjectParentIdentity is still experimental and may not work as expected.");

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

            // TODO: ParentID could also be a Inventory Slot usw
            if (ctx.Db.Plot.identity.Find(plotIdentity) is not Plot plot)
            {
                ClientLog.Warning(ctx, "Plot not found.");
                return;
            }

            physicalObject.ParentIdentity = plotIdentity;
            ctx.Db.PhysicalObject.identity.Update(physicalObject);

            ClientLog.Info(ctx, $"Set parent identity of physical object {physicalObjectID} to plot {plotIdentity}.");
        }

        /// <summary>
        /// Sets the position of a physical object to the specified coordinates, if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="physicalObjectID">The identity of the physical object whose position will be set.</param>
        /// <param name="xPos">The new X coordinate for the physical object.</param>
        /// <param name="yPos">The new Y coordinate for the physical object.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to change the position of a physical object.
        /// If the sender lacks permission or the object does not exist, a warning is logged and no changes are made.
        /// Additional validation for position (e.g., collision checks, plot bounds) should be implemented as needed.
        /// </remarks>
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

        /// <summary>
        /// Destroys a physical object and removes all associated permissions and hardpoints, if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="pysicalObjectID">The identity of the physical object to destroy.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to delete a physical object from the database.
        /// All associated <see cref="PhysicalObjectPermission"/> and <see cref="Hardpoint"/> entries are also removed.
        /// For each hardpoint, <c>HardpointReducers.DestroyHardpoint</c> is called to ensure proper cleanup.
        /// If the sender lacks permission or the object does not exist, a warning is logged and no changes are made.
        /// </remarks>
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

            foreach (PhysicalObjectPermission permission in ctx.Db.PhysicalObjectPermission.idx_poperm_poid.Filter(physicalObject.identity))
            {
                ctx.Db.PhysicalObjectPermission.Delete(permission);
            }

            foreach (Hardpoint hardpoint in ctx.Db.Hardpoint.idx_hardpoint_physicalobjectidentity.Filter(physicalObject.identity))
            {
                // Call the Hardpoint reducer to ensure proper cleanup
                HardpointReducers.DestroyHardpoint(ctx, hardpoint.identity);
            }

            ctx.Db.PhysicalObject.Delete(physicalObject);

            ClientLog.Info(ctx, $"Destroyed physical object {pysicalObjectID}.");
        }
    }

    /// <summary>
    /// Contains reducer methods for manipulating hardpoints within the game world.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>HardpointReducers</c> static partial class provides methods for creating, modifying, and deleting hardpoints,
    /// as well as managing hardpoint permissions. These reducers are invoked in response to client or server actions to update the game state.
    /// </para>
    /// </remarks>
    public static partial class HardpointReducers
    {
        /// <summary>
        /// Creates a new hardpoint on a specified physical object, if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="physicalObjectID">The identity of the physical object to which the hardpoint will be added.</param>
        /// <param name="size">The size or capacity of the new hardpoint.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to create a new <see cref="Hardpoint"/> on a physical object.
        /// The hardpoint is assigned a unique identity and linked to the specified physical object.
        /// If the sender lacks permission, a warning is logged and no changes are made.
        /// </remarks>
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

        /// <summary>
        /// Sets the permission level for a target identity on a specific hardpoint.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="targetIdentity">The identity of the user whose permission is being set.</param>
        /// <param name="hardpointID">The identity of the hardpoint.</param>
        /// <param name="level">The permission level to assign (0 = None, 1 = Read, 2 = Write, 3 = Admin, 4 = Owner).</param>
        /// <remarks>
        /// This reducer allows an authorized user to set or remove permissions for another user on a hardpoint.
        /// The sender must have at least Admin level permission on the hardpoint and cannot modify their own permissions.
        /// If the target already has a permission entry, it will be updated or removed based on the specified level.
        /// If the target does not have a permission entry and the level is not None, a new entry will be created.
        /// </remarks>
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

        /// <summary>
        /// Destroys a hardpoint and removes all associated permissions, if the sender has sufficient permissions.
        /// </summary>
        /// <param name="ctx">The reducer context containing sender and database references.</param>
        /// <param name="hardpointID">The identity of the hardpoint to destroy.</param>
        /// <remarks>
        /// This reducer allows an authorized user (with at least Admin permission) to delete a hardpoint from the database.
        /// All associated <see cref="HardpointPermission"/> entries are also removed.
        /// Destroying items in the hardpoint is not yet implemented and will log an informational message.
        /// If the sender lacks permission or the hardpoint does not exist, a warning is logged and no changes are made.
        /// </remarks>
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

            foreach (HardpointPermission permission in ctx.Db.HardpointPermission.idx_hardpoint_permission_hpid.Filter(hardpoint.identity))
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
