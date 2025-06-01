using System;
using Utils;
using Networking.SpacetimeController;
using SpacetimeDB.Types;

namespace POS;

public class PhysicalObjectSystem
{
    private Logger logger;

    public PhysicalObjectSystem()
    {
        if (Logger.LoggerFactory.GetLogger(nameof(PhysicalObjectSystem)) is Logger exLogger)
            logger = exLogger;
        else
            logger = Logger.LoggerFactory.CreateLogger("PhysicalObjectSystem");
        logger.SetLevel(9);
    }

    public void Test()
    {
        if (SpacetimeController.Instance.GetConnection() is not DbConnection connection)
            throw new NotImplementedException("Handle Connection not established");
    }
}
