// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Pomelo.Data.Faster
{
    /// <summary>
    /// Type of lock taken by FASTER on Read, Upsert, RMW, or Delete operations, either directly or within concurrent callback operations
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// Shared lock, taken on Read
        /// </summary>
        Shared,

        /// <summary>
        /// Exclusive lock, taken on Upsert, RMW, or Delete
        /// </summary>
        Exclusive
    }
}
