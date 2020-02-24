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

using Microsoft.SpatialAlignment.Persistence.Json;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Persistence
{
    public class PersistenceExampleManager : MonoBehaviour
    {
        #region Constants
        private const string SampleData = @"[
  {
    ""$id"": ""1"",
    ""alignmentStrategy"": {
      ""$type"": ""Microsoft.SpatialAlignment.Azure.AzureAnchorAlignment, Assembly-CSharp"",
      ""anchorId"": ""4c7c32c0-41e4-429d-a7d3-4a1609dd91d1"",
      ""loadOnStart"": false
    },
    ""id"": ""Parent1""
  },
  {
    ""$id"": ""2"",
    ""alignmentStrategy"": {
      ""$type"": ""Microsoft.SpatialAlignment.MultiParentAlignment, Assembly-CSharp"",
      ""distancePower"": 1.1,
      ""mode"": ""Closest"",
      ""parentOptions"": [
        {
          ""frame"": {
            ""$ref"": ""1""
          },
          ""minimumAccuracy"": {
            ""x"": 0.0,
            ""y"": 0.0,
            ""z"": 0.0
          },
          ""minimumState"": ""Resolved"",
          ""position"": {
            ""x"": 0.0,
            ""y"": 0.0,
            ""z"": 0.3
          },
          ""rotation"": {
            ""x"": 0.0,
            ""y"": 0.0,
            ""z"": 0.0,
            ""w"": 1.0
          },
          ""scale"": {
            ""x"": 0.3,
            ""y"": 0.3,
            ""z"": 0.3
          }
        }
      ],
      ""updateFrequency"": 1.0
    },
    ""id"": ""ModelFrame""
  }
]";
        #endregion // Constants

        #region Member Variables
        private JsonStore store;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        public List<SpatialFrame> Frames = new List<SpatialFrame>();
        #endregion // Unity Inspector Variables

        private async Task LoadAsync()
        {
            Frames = await store.LoadFramesAsync(SampleData);
            Debug.Log($"Loaded {Frames.Count} frames.");
        }

        private async Task SaveAsync()
        {
            string result = await store.SaveFramesAsync(Frames);
            Debug.Log(result);
        }

        // Start is called before the first frame update
        async void Start()
        {
            store = new JsonStore();
            try
            {
                // await SaveAsync();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex.Message}");
            }
        }
    }
}