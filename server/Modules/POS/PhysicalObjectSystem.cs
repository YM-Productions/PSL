using SpacetimeDB;

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

        public string PlotIdentity;

        public bool IsStatic;

        public PhysicalObject()
        {
            identity = Guid.NewGuid().ToString();
            Name = string.Empty;
            PlotIdentity = string.Empty;
            IsStatic = false;
        }
    }

    [Table(Name = nameof(PhysicalObjectPermission), Public = true)]
    [SpacetimeDB.Index.BTree(Name = "idx_pos_accid_poid", Columns = new[] { nameof(AccountIdentity), nameof(PhysicalObjectIdentity) })]
    public partial class PhysicalObjectPermission
    {
        public Identity AccountIdentity;
        public string PhysicalObjectIdentity;

        public PhysicalObjectPermission()
        {
            PhysicalObjectIdentity = string.Empty;
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
        public Identity AccountIdentity;
        public string HardpointIdentity;

        public HardpointPermission()
        {
            HardpointIdentity = string.Empty;
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
        public static void CreateFundament(ReducerContext ctx, string name, Guid plotID)
        {
            PhysicalObject fundament = new PhysicalObject
            {
                identity = Guid.NewGuid().ToString(),
                Name = name,
                PlotIdentity = plotID.ToString(),
                IsStatic = true
            };

            ctx.Db.PhysicalObject.Insert(fundament);
        }

        [Reducer]
        public static void CreateVehicle(ReducerContext ctx, string name, Guid plotID)
        {
            PhysicalObject vehicle = new PhysicalObject
            {
                identity = Guid.NewGuid().ToString(),
                Name = name,
                PlotIdentity = plotID.ToString(),
                IsStatic = false
            };

            ctx.Db.PhysicalObject.Insert(vehicle);
        }
    }

    #endregion
}
