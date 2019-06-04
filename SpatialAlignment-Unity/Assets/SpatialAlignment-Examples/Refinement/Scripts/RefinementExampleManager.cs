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

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.SpatialAlignment.Persistence.Json;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;

namespace Microsoft.SpatialAlignment.Persistence
{
    /// <summary>
    /// An example manager that shows how to add and edit refinement anchors.
    /// </summary>
    public class RefinementExampleManager : BaseInputHandler, IMixedRealityInputActionHandler
    {
        private enum AddAnchorStep
        {
            New,
            PlacingAnchor,
            ModelRay,
            ModelNudge,
            Finishing,
            Done,
        }

        private enum RefinementExampleMode
        {
            None,
            AddAnchor,
            AddAnchorCancel,
            EditingAnchor,
        }

        #region Member Variables
        private SpatialFrame largeScaleFrame;
        private RefinementBase largeScaleRefinement;
        private RefinementExampleMode mode;
        private MultiParentAlignment multiParent;
        private TapToPlace newAnchor;
        private AddAnchorStep anchorStep;
        private SpatialFrameCollection frames = new SpatialFrameCollection();
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The container for all refining anchors.")]
        private GameObject anchorContainer;

        [SerializeField]
        [Tooltip("The prefab used represent an anchor.")]
        private GameObject anchorPrefab;

        [SerializeField]
        [Tooltip("The large-scale model.")]
        private GameObject largeScaleModel;

        [SerializeField]
        [Tooltip("Used to enable split view for video recording.")]
        private SplitViewManager splitViewManager;

        public MixedRealityInputAction AddAnchorAction;
        public MixedRealityInputAction CancelAnchorAction;
        public MixedRealityInputAction RemoveAnchorAction;
        public MixedRealityInputAction ResetAnchorsAction;
        public MixedRealityInputAction ModeClosestAction;
        public MixedRealityInputAction ModeDistanceWeightedAction;
        public MixedRealityInputAction SplitViewAction;

        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Executes the specified step in a series of steps.
        /// </summary>
        /// <param name="step">
        /// The <see cref="AddAnchorStep"/> step to execute.
        /// </param>
        private void DoStep(AddAnchorStep step)
        {
            // Log
            Debug.Log($"Doing {nameof(AddAnchorStep)}: {step}");

            // Execute actions
            switch (step)
            {
                case AddAnchorStep.PlacingAnchor:

                    // Hide the large-scale model
                    HideModel();

                    // Instantiate the anchor prefab
                    GameObject anchorGO = GameObject.Instantiate(anchorPrefab);

                    // Set the parent so it's in the anchor container
                    anchorGO.transform.SetParent(anchorContainer.transform, worldPositionStays: true);

                    // Get the refining behavior
                    newAnchor = anchorGO.GetComponentInChildren<TapToPlace>();
                    if (newAnchor == null)
                    {
                        throw new InvalidOperationException($"{nameof(AnchorPrefab)} does not have a {nameof(TapToPlace)} component.");
                    }

                    // Subscribe to anchor events
                    SubscribeAnchor(newAnchor);

                    // Put the anchor in placement mode
                    newAnchor.IsBeingPlaced = true;
                    break;


                //case AddAnchorStep.RefiningAnchor:

                //    // Now refining anchor
                //    newAnchor.RefinementMode = TapToPlaceMode.Refining;
                //    break;


                case AddAnchorStep.ModelRay:

                    // Add a native alignment to the anchor so that it stays in place
                    newAnchor.gameObject.AddComponent<NativeAnchorAlignment>();

                    // Done placing anchor
                    newAnchor.IsBeingPlaced = false;

                    // Show the model
                    ShowModel();

                    // Disable MultiParent alignment on large-scale model so
                    // that it can be positioned
                    multiParent.enabled = false;

                    // Detach from any current parent (if there is one)
                    largeScaleFrame.transform.parent = null;

                    // Create and subscribe to RayRefinement
                    largeScaleRefinement = SubscribeRefinement<RayRefinement>();

                    // Start the new refining mode
                    largeScaleRefinement.StartRefinement();
                    break;

                case AddAnchorStep.ModelNudge:

                    // Unsubscribe from RayRefinement
                    UnsubscribeRefinement<RayRefinement>();

                    // Create and subscribe to NudgeRefinement
                    largeScaleRefinement = SubscribeRefinement<NudgeRefinement>();

                    // Start the new refining mode
                    largeScaleRefinement.StartRefinement();
                    break;

                case AddAnchorStep.Finishing:

                    // Finish refinement if still in progress
                    if (largeScaleRefinement != null)
                    {
                        // Finish refinement of the large scale model if in progress
                        if (largeScaleRefinement.IsRefining)
                        {
                            largeScaleRefinement.FinishRefinement();
                        }

                        // Unsubscribe from refinement events
                        UnsubscribeRefinement(largeScaleRefinement);

                        // No current large scale refinement
                        largeScaleRefinement = null;
                    }

                    // Unsubscribe from anchor events
                    UnsubscribeAnchor(newAnchor);

                    // Come up with an ID for this anchor
                    string id = DateTime.Now.ToUniversalTime().Ticks.ToString();

                    // Create a new game object
                    GameObject frameGO = new GameObject(id);

                    // Set position and rotation same as the anchor
                    frameGO.transform.position = newAnchor.transform.position;
                    frameGO.transform.rotation = newAnchor.transform.rotation;

                    // Parent it in the anchor container
                    frameGO.transform.SetParent(anchorContainer.transform, worldPositionStays: true);

                    // Add spatial frame to game object
                    SpatialFrame newFrame = frameGO.AddComponent<SpatialFrame>();

                    // Give the frame an ID
                    newFrame.Id = id;

                    // Add a WorldAnchorAlignment to SpatialFrame
                    NativeAnchorAlignment worldAlignment = frameGO.AddComponent<NativeAnchorAlignment>();

                    // Give the anchor an ID
                    worldAlignment.AnchorId = id;

                    // Temporarily parent the large model to the new frame
                    largeScaleFrame.transform.SetParent(newFrame.transform, worldPositionStays: true);

                    // Add this new frame to MultiParentAlignment as a new
                    // parent option and using the current large-scale offset
                    // from the anchor.
                    multiParent.ParentOptions.Add(new ParentAlignmentOptions()
                    {
                        // Set frame
                        Frame = newFrame,

                        // Set offsets
                        Position = largeScaleFrame.transform.localPosition,
                        Rotation = largeScaleFrame.transform.localRotation,
                        Scale = largeScaleFrame.transform.localScale,
                    });

                    // Unparent the large-scale model
                    largeScaleFrame.transform.parent = null;

                    // Delete the new anchor game object and all children
                    DestroyImmediate(newAnchor.gameObject);
                    newAnchor = null;

                    // Show the model
                    ShowModel();

                    // Re-enable MultiParent alignment
                    multiParent.enabled = true;

                    // Done
                    anchorStep = AddAnchorStep.Done;
                    mode = RefinementExampleMode.None;
                    break;


                default:

                    Debug.LogError($"Unknown step {step}");
                    break;
            }
        }

        /// <summary>
        /// Gathers all dependency components and disables the behavior if a
        /// dependency is not found.
        /// </summary>
        private void GatherDependencies()
        {
            // If no anchor container is specified, create one.
            if (anchorContainer == null)
            {
                anchorContainer = new GameObject("Anchor Container");
            }

            // We must have a prefab to generate for anchors.
            if (anchorPrefab == null)
            {
                Debug.LogError($"{nameof(anchorPrefab)} is required but is not set. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }

            // We must have a large-scale model to turn on and off
            if (largeScaleModel == null)
            {
                Debug.LogError($"{nameof(largeScaleModel)} is required but is not set. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }
            else
            {
                // Attempt to get the frame
                largeScaleFrame = largeScaleModel.GetComponent<SpatialFrame>();
            }

            // The large scale model should be sitting on a spatial frame
            if (largeScaleFrame == null)
            {
                Debug.LogError($"{nameof(largeScaleModel)} should have a {nameof(SpatialFrame)} component but none was found. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }
            else
            {
                multiParent = largeScaleFrame.AlignmentStrategy as MultiParentAlignment;
            }

            // The spatial frames alignment strategy should be MultiParent.
            if (multiParent == null)
            {
                Debug.LogError($"The alignment strategy for {nameof(largeScaleModel)} should be {nameof(MultiParentAlignment)} but none was found. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }
        }

        private void SubscribeAnchor(TapToPlace anchor)
        {
            // Subscribe from placing events
            anchor.IsBeingPlacedChaged += Anchor_IsBeingPlacedChanged;
        }

        private TRefinement SubscribeRefinement<TRefinement>() where TRefinement : RefinementBase
        {
            // If refinement doesn't exist, add it
            TRefinement refinement = largeScaleModel.EnsureComponent<TRefinement>();

            // Subscribe to refinement events
            refinement.RefinementCanceled += LargeScaleRefinement_RefinementCanceled;
            refinement.RefinementFinished += LargeScaleRefinement_RefinementFinished;

            // Return the refinement
            return refinement;
        }

        private void UnsubscribeAnchor(TapToPlace anchor)
        {
            // Unsubscribe from placing events
            anchor.IsBeingPlacedChaged -= Anchor_IsBeingPlacedChanged;
        }

        private void UnsubscribeRefinement(RefinementBase refinement)
        {
            if (refinement == null) throw new ArgumentNullException(nameof(refinement));
            refinement.RefinementCanceled -= LargeScaleRefinement_RefinementCanceled;
            refinement.RefinementFinished -= LargeScaleRefinement_RefinementFinished;
        }

        private void UnsubscribeRefinement<TRefinement>() where TRefinement : RefinementBase
        {
            // Attempt to get the refinement
            TRefinement refinement = largeScaleModel.GetComponent<TRefinement>();

            // If found, unsubscribe from refinement events
            if (refinement != null)
            {
                UnsubscribeRefinement(refinement);
            }
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void Anchor_IsBeingPlacedChanged(object sender, EventArgs e)
        {
            // If we are placing an anchor and it's now placed,
            // go on to the next step.
            if ((anchorStep == AddAnchorStep.PlacingAnchor) && (!newAnchor.IsBeingPlaced))
            {
                NextStep();
            }
        }

        private void LargeScaleRefinement_RefinementCanceled(object sender, System.EventArgs e)
        {
            // Cancel adding anchor
            if (mode == RefinementExampleMode.AddAnchor)
            {
                CancelAddAnchor();
            }
        }

        private void LargeScaleRefinement_RefinementFinished(object sender, System.EventArgs e)
        {
            // If adding an anchor, go on to the next step
            if (mode == RefinementExampleMode.AddAnchor)
            {
                NextStep();
            }
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected override void Start()
        {
            base.Start();
            GatherDependencies();
        }
        #endregion // Unity Overrides

        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            var action = eventData.MixedRealityInputAction;

            if (action == AddAnchorAction)
            {
                BeginAddAnchor();
            }
            else if (action == CancelAnchorAction)
            {
                CancelAddAnchor();
            }
            else if (action == RemoveAnchorAction)
            {
                RemoveLastAnchor();
            }
            else if (action == ResetAnchorsAction)
            {
                ResetAnchors();
            }
            else if (action == ModeClosestAction)
            {
                SetModeClosest();
            }
            else if (action == ModeDistanceWeightedAction)
            {
                SetModeDistanceWeighted();
            }
            else if (action == SplitViewAction)
            {
                SplitView();
            }
        }

        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {

        }

        #region Public Methods
        /// <summary>
        /// Begins the process of adding a refinement anchor.
        /// </summary>
        public void BeginAddAnchor()
        {
            // If already in another mode, ignore
            if (mode != RefinementExampleMode.None)
            {
                Debug.LogWarning($"{nameof(BeginAddAnchor)} called but already in {mode}");
                return;
            }

            // Start adding a new anchor
            mode = RefinementExampleMode.AddAnchor;
            anchorStep = AddAnchorStep.New;
            NextStep();
        }

        /// <summary>
        /// Cancels the process of adding a refinement anchor.
        /// </summary>
        public void CancelAddAnchor()
        {
            // If in another mode, ignore
            if (mode != RefinementExampleMode.AddAnchor)
            {
                Debug.LogWarning($"{nameof(CancelAddAnchor)} called but in {mode}");
                return;
            }

            // Now canceling
            mode = RefinementExampleMode.AddAnchorCancel;

            if (largeScaleRefinement != null)
            {
                // Cancel refinement of the large scale model if in progress
                if (largeScaleRefinement.IsRefining)
                {
                    largeScaleRefinement.CancelRefinement();
                }

                // Unsubscribe from refinement events
                UnsubscribeRefinement(largeScaleRefinement);

                // No current large scale refinement
                largeScaleRefinement = null;
            }

            // Unsubscribe from anchor events
            UnsubscribeAnchor(newAnchor);

            // Delete the new anchor game object and all children
            DestroyImmediate(newAnchor.gameObject);
            newAnchor = null;

            // Re-enable MultiParent alignment
            multiParent.enabled = true;

            // Show the model
            ShowModel();

            // Done
            anchorStep = AddAnchorStep.Done;
            mode = RefinementExampleMode.None;
        }

        /// <summary>
        /// Finishes the process of adding a refinement anchor.
        /// </summary>
        public void FinishAddAnchor()
        {
            // If in another mode, ignore
            if (mode != RefinementExampleMode.AddAnchor)
            {
                Debug.LogWarning($"{nameof(FinishAddAnchor)} called but in {mode}");
                return;
            }

            // Do the finishing step
            DoStep(AddAnchorStep.Finishing);
        }

        /// <summary>
        /// Hides all refinement anchors.
        /// </summary>
        public void HideAnchors()
        {

        }

        /// <summary>
        /// Hides the model.
        /// </summary>
        public void HideModel()
        {
            largeScaleModel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Proceeds to the next step in the current mode.
        /// </summary>
        public void NextStep()
        {
            switch (mode)
            {
                case RefinementExampleMode.AddAnchor:
                    DoStep(++anchorStep);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Removes the specified anchor option.
        /// </summary>
        /// <param name="parentOption">
        /// The option to remove.
        /// </param>
        public void RemoveAnchor(ParentAlignmentOptions parentOption)
        {
            // Validate
            if (parentOption == null) throw new ArgumentNullException(nameof(parentOption));

            // Remove
            if (multiParent.ParentOptions.Contains(parentOption))
            {
                // If it's the current parent, unparent so we don't get destroyed too
                if (LargeScaleModel.transform.parent == parentOption.Frame.transform)
                {
                    LargeScaleModel.transform.parent = null;
                }

                // Remove the option
                multiParent.ParentOptions.Remove(parentOption);

                // Destroy the frame (and it's strategy) on the next tick
                Destroy(parentOption.Frame.gameObject);
            }
        }

        /// <summary>
        /// Removes the current active anchor.
        /// </summary>
        public void RemoveCurrentAnchor()
        {
            if (multiParent.CurrentParents.Count > 0)
            {
                RemoveAnchor(multiParent.CurrentParents[0]);
            }
        }

        /// <summary>
        /// Removes the last anchor that was created.
        /// </summary>
        public void RemoveLastAnchor()
        {
            int lastIndex = multiParent.ParentOptions.Count - 1;
            if (lastIndex >= 0)
            {
                RemoveAnchor(multiParent.ParentOptions[lastIndex]);
            }
        }

        /// <summary>
        /// Removes all anchors.
        /// </summary>
        public void ResetAnchors()
        {
            for (int i = multiParent.ParentOptions.Count -1; i >= 0; i--)
            {
                // Remove the anchor option
                RemoveAnchor(multiParent.ParentOptions[i]);
            }
        }

        public void SetMode(MultiParentMode mode)
        {
            multiParent.Mode = mode;
        }

        public void SetModeClosest()
        {
            SetMode(MultiParentMode.Closest);
        }

        public void SetModeDistanceWeighted()
        {
            SetMode(MultiParentMode.DistanceWeighted);
        }

        /// <summary>
        /// Shows all refinement anchors.
        /// </summary>
        public void ShowAnchors()
        {

        }

        /// <summary>
        /// Shows the model.
        /// </summary>
        public void ShowModel()
        {
            largeScaleModel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Toggles on and off split view.
        /// </summary>
        public void SplitView()
        {
            if (splitViewManager == null) { return; }

            if (splitViewManager.Mode == SplitViewMode.Unoccluded)
            {
                splitViewManager.Mode = SplitViewMode.OccludedRight;
            }
            else
            {
                splitViewManager.Mode = SplitViewMode.Unoccluded;
            }
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the container for all refining anchors.
        /// </summary>
        public GameObject AnchorContainer { get { return anchorContainer; } set { anchorContainer = value; } }

        /// <summary>
        /// Gets or sets The prefab used represent an anchor.
        /// </summary>
        public GameObject AnchorPrefab { get { return anchorPrefab; } set { anchorPrefab = value; } }

        /// <summary>
        /// Gets or sets the large-scale model.
        /// </summary>
        public GameObject LargeScaleModel { get { return largeScaleModel; } set { largeScaleModel = value; } }

        /// <summary>
        /// Gets or sets the split view manager.
        /// </summary>
        public SplitViewManager SplitViewManager { get { return splitViewManager; } set { splitViewManager = value; } }
        #endregion // Public Properties
    }
}