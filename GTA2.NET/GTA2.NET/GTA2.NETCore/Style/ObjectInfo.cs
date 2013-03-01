//Created: 20.01.2010

namespace Hiale.GTA2NET.Core.Style
{
    /// <summary>
    /// A very small amount of type information is stored for each type of map object.
    /// Many other properties of objects are required for GTA2, but these are stored in code header files, not in the style file, because they are more likely to be updated by programmers than by artists, and the editor does not need to know about them.
    /// Note that there are two different kinds of object – map objects and code objects. Map objects (e.g. bins and hot dog stands) can be placed by the editor and hence have their information stored here. Code objects (e.g. blood and skidmarks) can only be placed by the code so no information is stored for them here.
    /// </summary>
    public struct ObjectInfo
    {
        /// <summary>
        /// Model is the object model number. Every distinct type of object has a unique model number. Objects placed in the map using the editor are represented by a model number.
        /// </summary>
        public byte Model;
        /// <summary>
        /// Sprites is the number of sprites stored for this model.
        /// </summary>
        public byte Sprites;
    }
}
