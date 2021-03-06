// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class ImportEngine
    {
        /// <summary>
        ///     Used to wrap the start and stop of enforcing export changes don't
        ///     break required imports. This context is stored in a AtomicComposition.
        /// </summary>
        private sealed class EngineContext
        {
            private readonly ImportEngine _importEngine;
            private readonly List<PartManager> _addedPartManagers = new List<PartManager>();
            private readonly List<PartManager> _removedPartManagers = new List<PartManager>();
            private readonly EngineContext? _parentEngineContext;

            public EngineContext(ImportEngine importEngine, EngineContext? parentEngineContext)
            {
                _importEngine = importEngine;
                _parentEngineContext = parentEngineContext;
            }

            public void AddPartManager(PartManager part)
            {
                ArgumentNullException.ThrowIfNull(part);

                if (!_removedPartManagers.Remove(part))
                {
                    _addedPartManagers.Add(part);
                }
            }

            public void RemovePartManager(PartManager part)
            {
                ArgumentNullException.ThrowIfNull(part);

                if (!_addedPartManagers.Remove(part))
                {
                    _removedPartManagers.Add(part);
                }
            }

            public IEnumerable<PartManager>? GetAddedPartManagers()
            {
                if (_parentEngineContext != null)
                {
                    return _addedPartManagers.ConcatAllowingNull(_parentEngineContext.GetAddedPartManagers());
                }
                return _addedPartManagers;
            }

            public IEnumerable<PartManager>? GetRemovedPartManagers()
            {
                if (_parentEngineContext != null)
                {
                    return _removedPartManagers.ConcatAllowingNull(_parentEngineContext.GetRemovedPartManagers());
                }
                return _removedPartManagers;
            }

            public void Complete()
            {
                foreach (var partManager in _addedPartManagers)
                {
                    _importEngine.StartSatisfyingImports(partManager, null);
                }

                foreach (var partManager in _removedPartManagers)
                {
                    _importEngine.StopSatisfyingImports(partManager, null);
                }
            }
        }
    }
}
