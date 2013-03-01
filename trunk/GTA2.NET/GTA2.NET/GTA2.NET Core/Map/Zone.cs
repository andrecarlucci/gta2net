//Created 17.01.2010

using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    public enum ZoneType : byte
    {
        /// <summary>
        /// General purpose zones are used simply to mark an area so that it can be referred to by that name in mission scripts.
        /// </summary>
        GeneralPurpose = 0,

        /// <summary>
        /// Navigation zones are used to store the correct geographical names of areas within the city. When the player enters a new navigation zone, its name is displayed on screen. When a police radio message reports a crime, the enclosing navigation zone is used to describe the area. Every block in the city must be within some navigation zone. They can overlap, in which case the smaller area zone gets priority. It is not allowed for equally sized zones to overlap. The navigation zone names are not translated in foreign versions of the game – they always have the same name. For large areas, the game appends North, South, etc. to the displayed name, so as to give the player a better idea of where he is.
        /// </summary>
        Navigation = 1,

        /// <summary>
        /// Traffic light zones are used to mark junctions where traffic lights are required. The game will automatically construct a traffic light system at each traffic light zone. It is not allowed to place a traffic light zone on an area which is not a valid junction. Traffic light zones must not be on slopes, and must not be within one block of the end of a slope. Traffic light zones must be at least a screen away from each other.
        /// </summary>
        TrafficLight = 2,

        /// <summary>
        /// Arrow blocker zones are used to mark zones in the map where gang arrows should not be drawn even if within a gang zone.
        /// </summary>
        ArrowBlocker = 5,

        /// <summary>
        /// Railway station zones are used to mark the location of railway stations The platform zone marks where the platform is, i.e. where peds should stand. This must be adjacent to a valid train route. The stop point marks where trains should stop. The entry and exit points mark the points on the track where trains  enter and exit the station.
        /// </summary>
        RailwayStation = 6,

        /// <summary>
        /// Bus stop zones are used to mark the location of bus stops.  The zone is placed on the pavement and it marks where the peds should stand. Buses will stop at the nearest road piece( error if none close ). There are no specific bus routes – buses are simply created at random and stop at bus stops whenever they pass them.
        /// </summary>
        BusStop = 7,

        /// <summary>
        /// General trigger zones are used to mark the position of general purpose triggers. These are used by mission scripts to set off an event when the player enters an area of the map. Normally, some sort of event will be associated with the general trigger zone in the mission script.
        /// </summary>
        GeneralTrigger = 8,

        /// <summary>
        /// Information zones are used to mark an area which has different information values from the area round about it. The actual information is set up using the mission script.
        /// </summary>
        Information = 10,

        RailwayStationEntryPoint = 11,
        RailwayStationExitPoint = 12,
        RailwayStationStopPoint = 13,

        /// <summary>
        /// Gang zones are used to mark the areas used by particular gangs. The name of the zone is the name of the gang. Any one gang may have many zones.
        /// </summary>
        Gang = 14,

        /// <summary>
        /// local navigation zones are used, like navigation zones, to mark the names of places which the player can visit. Local navigation zones will tend to be smaller than main navigation zones. On-screen, the local navigation zone is shown before the main navigation zone – e.g. “Johnny’s Strip Club, Hackenslash” would be displayed if the player is in the local navigation zone “Johnny’s Strip Club” and the main navigation zone “Hackenslash”. The rules for overlapping for local navigation zones are the same as for main navigation zones. However, unlike main navigation zones, local navigation zones do not have to cover the whole city.
        /// </summary>
        LocalNavigation = 15,

        /// <summary>
        /// Restart zones are the places where the player is restarted after he dies . There must be at least one restart zone in a map. They can be any size . When the player dies, he is restarted at the nearest restart zone ( except if it is a mult-player game and another player is too close ) .
        /// </summary>
        Restart = 16,

        /// <summary>
        /// Arrest restart zones mark the places where the player is taken to after he has been arrested. They must be only one block in size.
        /// </summary>
        ArrestRestart = 20
    }

    /// <summary>
    /// Map zones are used in the game to identify named areas for various purposes. 
    /// </summary>
    public class Zone
    {
        public ZoneType Type { get; set; }

        public Rectangle Rectangle { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
