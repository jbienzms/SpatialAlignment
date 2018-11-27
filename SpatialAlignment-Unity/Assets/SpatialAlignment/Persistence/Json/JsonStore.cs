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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Persistence.Json
{
    /// <summary>
    /// A class that can load and save spatial alignment data using json.
    /// </summary>
    public class JsonStore
    {
        #region Internal Methods
        private JsonSerializer CreateSerializer()
        {
            // Create serialization settings
            JsonSerializerSettings settings = new JsonSerializerSettings();

            // Make pretty
            settings.Formatting = Formatting.Indented;

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


        #region Public Methods
        /// <summary>
        /// Loads an entire json document into spatial frames.
        /// </summary>
        /// <param name="reader">
        /// The reader used to read the document.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that yields the loaded frames.
        /// </returns>
        public async Task<List<SpatialFrame>> LoadFramesAsync(JsonReader reader)
        {
            // Validate
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Create the serializer
            JsonSerializer ser = CreateSerializer();

            // Deserialize the list of frames
            List<SpatialFrame> frames = ser.Deserialize<List<SpatialFrame>>(reader);

            // If any of the alignment strategies uses native persistence,
            // load them now
            if (frames != null)
            {
                foreach (SpatialFrame frame in frames)
                {
                    INativePersistence nativePersist = frame.AlignmentStrategy as INativePersistence;
                    if (nativePersist != null)
                    {
                        await nativePersist.LoadNativeAsync();
                    }
                }
            }

            // Return loaded frames
            return frames;
        }

        /// <summary>
        /// Loads a json fragment into spatial frames.
        /// </summary>
        /// <param name="json">
        /// The string json fragment.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that yields the loaded frames.
        /// </returns>
        public Task<List<SpatialFrame>> LoadFramesAsync(string json)
        {
            // Validate
            if (string.IsNullOrEmpty(json)) throw new ArgumentException(nameof(json));

            // Deserialize
            using (StringReader sr = new StringReader(json))
            {
                using (JsonTextReader jr = new JsonTextReader(sr))
                {
                    return LoadFramesAsync(jr);
                }
            }
        }

        /// <summary>
        /// Saves the list of frames using the specified writer.
        /// </summary>
        /// <param name="frames">
        /// The list of frames to save.
        /// </param>
        /// <param name="writer">
        /// The <see cref="JsonWriter"/> used to save the frames.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task SaveFramesAsync(List<SpatialFrame> frames, JsonWriter writer)
        {
            // Validate
            if (frames == null) throw new ArgumentNullException(nameof(frames));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            // If any of the alignment strategies uses native persistence,
            // save them now
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

        /// <summary>
        /// Saves the list of frames to a json string.
        /// </summary>
        /// <param name="frames">
        /// The list of frames to save.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that yields the json string.
        /// </returns>
        public async Task<string> SaveFramesAsync(List<SpatialFrame> frames)
        {
            // Validate
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            // Serialize
            using (StringWriter sw = new StringWriter())
            {
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    await SaveFramesAsync(frames, jw);
                    return sw.ToString();
                }
            }
        }
        #endregion // Public Methods
    }
}