//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Persistence.Json
{
    /// <summary>
    /// A Json.Net contract resolver that knows how to instantiate Unity objects
    /// like <see cref="Component"/> and <see cref="MonoBehaviour"/>.
    /// </summary>
    /// <remarks>
    /// This resolver is necessary because in Unity we can't just create behaviors
    /// using their constructors, we must add them to game objects using
    /// <see cref="GameObject.AddComponent(Type)">AddComponent</see>.
    /// </remarks>
    public class UnityContractResolver : DefaultContractResolver
    {
        #region Member Variables
        private GameObject creationContext;
        #endregion // Member Variables

        #region Overridables / Event Triggers
        /// <summary>
        /// Instantiates a Unity <see cref="Component"/> (or <see cref="MonoBehaviour"/>).
        /// </summary>
        /// <param name="objectType">
        /// The type of component to create.
        /// </param>
        /// <returns>
        /// The created component.
        /// </returns>
        protected virtual Component CreateComponent(Type objectType)
        {
            // Use the existing creation context if found
            // Otherwise, create a new GameObject to serve as the parent
            GameObject parent = (creationContext ?? new GameObject());

            // Instantiate the component
            return parent.AddComponent(objectType);
        }
        #endregion // Overridables / Event Triggers

        #region IContractResolver Overrides
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {

            if (typeof(Component).IsAssignableFrom(objectType))
            {
                // Start with base contract
                var contract = base.CreateObjectContract(objectType);

                // Override to instantiate the component
                contract.OverrideCreator = ((args) => CreateComponent(objectType));

                // Return the new contract
                return contract;
            }
            else
            {
                // Just use the base contract.
                return base.CreateObjectContract(objectType);
            }
        }
        #endregion // IContractResolver Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the <see cref="GameObject"/> that will be used as the
        /// creation context.
        /// </summary>
        /// <remarks>
        /// If set, this <see cref="GameObject"/> will be used to instantiate
        /// components. Otherwise a new <see cref="GameObject"/> will be created
        /// for each new component.
        /// </remarks>
        public GameObject CreationContext { get => creationContext; set => creationContext = value; }
        #endregion // Public Properties
    }
}
