public static class SECTOR_CODE
{
    public const byte NONE = 0;
    public const byte REDSECTOR = 1;
    //TODO add other Sectors
}

public static class WORKER_ACTIVITY
{
    public const byte IDLE = 0;
    public const byte CAPTAIN = 1;
    public const byte TACTICIAN = 2;
    //TODO add other worker activities
}

public static class LOCATION_TYPE
{
    public const byte NONE = 0;
    public const byte COLONY = 1;
    public const byte SHIP = 2;
    public const byte ASTEROID = 3;
    //TODO consider & add other SH objects
}

public static class SHIP_ACTIVITY
{
    public const byte NONE = 0;
    public const byte DOCKED = 1;
    public const byte TRAVELING = 2;
    public const byte COMBAT = 3;
    public const byte MINING = 4;
    //TODO: add other ship activities

}

public static class OWNER_TYPE
{
    public const byte NONE = 0;
    public const byte PLAYER = 1;
    public const byte COLONY = 2;
    public const byte NATION = 3;
    public const byte NPC = 4;
    //todo specify npc type?
}