using System;
using System.Collections.Generic;
using System.Text;

namespace LocresLib
{
    /// <summary>
    ///     Data versions for LocRes files.
    ///     <para>
    ///         https://github.com/EpicGames/UnrealEngine/blob/master/Engine/Source/Runtime/Core/Public/Internationalization/TextLocalizationResourceVersion.h
    ///     </para>
    /// </summary>
    public enum LocresVersion : byte
    {
        /// <summary>
        ///     Legacy format file - will be missing the magic number.
        /// </summary>
        Legacy = 0,
        /// <summary>
        ///     Compact format file - strings are stored in a LUT to avoid duplication.
        /// </summary>
        Compact,
        /// <summary>
        ///     Optimized format file - namespaces/keys are pre-hashed (CRC32), we know the number of elements up-front, and the number of references for each string in the LUT (to allow stealing).
        /// </summary>
        Optimized,
        /// <summary>
        ///     Optimized format file - namespaces/keys are pre-hashed (CityHash64, UTF-16), we know the number of elements up-front, and the number of references for each string in the LUT (to allow stealing).
        /// </summary>
        Optimized_CityHash64_UTF16,
    }
}
