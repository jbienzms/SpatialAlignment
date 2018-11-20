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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Persistence.Json
{
    /// <summary>
    /// A class that can load and save spatial alignment data using json.
    /// </summary>
    public class JsonStore : ISpatialAlignmentStore
    {
        #region Member Variables
        private List<SpatialFrame> frames = new List<SpatialFrame>();
        #endregion // Member Variables

        #region Internal Methods
        private JsonSerializer CreateSerializer()
        {
            // Create serialization settings
            JsonSerializerSettings settings = new JsonSerializerSettings();

            // Make pretty
            settings.Formatting = Formatting.Indented;

            // Preserve references since MultiParent alignment does reference
            // other frames
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;

            // Use custom contract resolver (handles Unity-specific instantiation)
            settings.ContractResolver = new SpatialContractResolver();

            // Add converters
            // settings.Converters.Add(new AlignmentStrategyConverter());
            // settings.Converters.Add(new SpatialFrameConverter());
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new Vector3Converter());

            // Enable automatic type name handling to allow extensions for alignment strategies
            // WARNING: Must implement ISerializationBinder to safely handle instantiation
            // See https://stackoverflow.com/questions/39565954/typenamehandling-caution-in-newtonsoft-json
            settings.TypeNameHandling = TypeNameHandling.Auto;

            // Create the serializer
            return JsonSerializer.Create(settings);
        }
        #endregion // Internal Methods

        /// <summary>
        /// Loads an entire json document into serialization frames.
        /// </summary>
        /// <param name="reader">
        /// The reader used to read the document.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task LoadDocumentAsync(JsonReader reader)
        {
            // Validate
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Unload all existing objects
            foreach (var frame in frames)
            {
                GameObject.DestroyImmediate(frame.gameObject);
            }
            frames.Clear();

            // Create the serializer
            JsonSerializer ser = CreateSerializer();

            // Deserialize a list of frames
            frames = ser.Deserialize<List<SpatialFrame>>(reader);

            // No tasks to await yet. May move portions of this
            // method into a parallel task.
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<SpatialFrame> LoadFrameAsync(string id)
        {
            // Validate
            if (string.IsNullOrEmpty(id)) throw new ArgumentException(nameof(id));

            // Try to get from collection
            SpatialFrame frame = frames.Where(f => f.Id == id).FirstOrDefault();

            // Return as result
            return Task.FromResult(frame);
        }

        public async Task SaveDocumentAsync(JsonWriter writer)
        {
            // Validate
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            // If any of the alignment strategies uses native persistence,
            // save them first.
            foreach (SpatialFrame frame in frames)
            {
                INativePersistence np = frame.AlignmentStrategy as INativePersistence;
                if (np != null)
                {
                    await np.SaveNativeAsync();
                }
            }

            // Create the serializer
            JsonSerializer ser = CreateSerializer();

            // Serialize the list of frames
            ser.Serialize(writer, frames);
        }

        /// <inheritdoc />
        public Task SaveFrameAsync(SpatialFrame frame)
        {
            // Validate
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            // TODO: Verify unique
            // Add it into the list
            frames.Add(frame);

            // Done
            return Task.CompletedTask;
        }
    }
}