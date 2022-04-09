// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace CCDemo
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Player state for state machine, transitions include
    ///
    /// Uninitialized -> Idle
    ///          Idle -> Walking  on press movement key
    ///          Idle -> Falling  on leave ground
    ///       Walking -> Idle     on release movement key
    ///       Walking -> Falling  on leave ground
    ///       Falling -> Idle     on grounded and not pressing movement key
    ///       Falling -> Walking  on grounded and pressing movement key
    /// </summary>
    public enum PlayerState
    {
        Uninitialized,
        Idle,
        Walking,
        Falling
    }

    /// <summary>
    /// Basic character Controls for moving a player around in 3D space.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterControls : MonoBehaviour
    {
        /// <summary>
        /// Maximum pitch of player camera rotation.
        /// </summary>
        public const float MaxPitch = 90.0f;

        /// <summary>
        /// Minimum pitch of player camera rotation.
        /// </summary>
        public const float MinPitch = -90.0f;

        /// <summary>
        /// Small epsilon value for avoiding overlapping with objects.
        /// </summary>
        public const float Epsilon = 0.001f;

        /// <summary>
        /// Maximum angle between two colliding objects.
        /// </summary>
        public const float MaxAngleShoveRadians = 90.0f;

        /// <summary>
        /// Look action reference to modify the player camera.
        /// </summary>
        public InputActionReference lookActionReference;

        /// <summary>
        /// Move action reference to move the player around.
        /// </summary>
        public InputActionReference moveActionReference;

        /// <summary>
        /// Look action to modify the player camera.
        /// </summary>
        public InputAction lookAction { get; set; }

        /// <summary>
        /// Move action to move the player around.
        /// </summary>
        public InputAction moveAction { get; set; }

        /// <summary>
        /// Speed of player movement in units per second.
        /// </summary>
        public float playerSpeed = 5.0f;

        /// <summary>
        /// Speed at which the player falls.
        /// </summary>
        public float fallSpeed = 5.0f;

        /// <summary>
        /// Speed of player rotation in degrees per second.
        /// </summary>
        public float rotationSpeed = 180.0f;

        /// <summary>
        /// Max bounces for moving player around.
        /// </summary>
        public int maxBounces = 5;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        public float anglePower = 0.5f;

        /// <summary>
        /// Configuration for capsule collider to compute player collision.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        /// <summary>
        /// Current player attitude.
        /// </summary>
        public Vector2 attitude { get; private set; }

        /// <summary>
        /// Current grounded state of the player.
        /// </summary>
        /// <value></value>
        public bool OnGround { get; private set; }

        /// <summary>
        /// Get the pitch of the player's current view.
        /// </summary>
        public float Pitch => this.attitude.x;

        /// <summary>
        /// Get the yaw of the player's current view.
        /// </summary>
        public float Yaw => this.attitude.y;

        /// <summary>
        /// Current movement state of the player.
        /// </summary>
        public PlayerState playerState { get; private set; }

        /// <summary>
        /// Setup for character controls.
        /// </summary>
        public void Start()
        {
            this.playerState = PlayerState.Idle;
            this.attitude = Vector3.zero;

            if (this.moveActionReference != null)
            {
                this.moveAction = this.moveActionReference.action;
            }

            if (this.lookActionReference != null)
            {
                this.lookAction = this.lookActionReference.action;
            }

            this.moveAction.Enable();
            this.lookAction.Enable();

            this.capsuleCollider = this.GetComponent<CapsuleCollider>();
        }

        /// <summary>
        /// Set the attitude of the player.
        /// </summary>
        /// <param name="attitude">New attitude (pitch, yaw).</param>
        public void SetAttitude(Vector2 attitude)
        {
            this.attitude = attitude;
        }

        /// <summary>
        /// Update function for character controls.
        /// </summary>
        public void Update()
        {
            Vector2 movementInput = this.moveAction.ReadValue<Vector2>();
            Vector2 lookInput = this.lookAction.ReadValue<Vector2>();

            // Update grounded state
            this.OnGround = this.CastSelf(this.transform.position, this.transform.rotation, Vector3.down, 0.01f, out _);

            // Is the player moving
            bool moving = movementInput.magnitude > 0.001f;

            // Handle state transition if needed.
            switch (this.playerState)
            {
                case PlayerState.Uninitialized:
                    // Always transition from Uninitialized to idle
                    this.playerState = PlayerState.Idle;
                    break;
                case PlayerState.Idle:
                    // Transition to falling if not on ground.
                    if (!this.OnGround)
                    {
                        this.playerState = PlayerState.Falling;
                    }
                    // Transition to walking state if player is moving
                    if (moving)
                    {
                        this.playerState = PlayerState.Walking;
                    }

                    break;
                case PlayerState.Walking:
                    // Transition to falling if not on ground.
                    if (!this.OnGround)
                    {
                        this.playerState = PlayerState.Falling;
                    }
                    // Transition to idle state if player is not moving
                    else if (!moving)
                    {
                        this.playerState = PlayerState.Idle;
                    }

                    break;
                case PlayerState.Falling:
                    // Transition to idle state if player is not moving and grounded
                    if (!moving && this.OnGround)
                    {
                        this.playerState = PlayerState.Idle;
                    }
                    // Transition to walking state if player is moving and grounded
                    else if (moving && this.OnGround)
                    {
                        this.playerState = PlayerState.Walking;
                    }

                    break;
            }

            // Update based on current state.
            switch (this.playerState)
            {
                case PlayerState.Idle:
                    // only rotate camera when idle
                    this.RotatePlayer(lookInput);
                    break;
                case PlayerState.Walking:
                    // rotate camera and move player when walking
                    this.RotatePlayer(lookInput);
                    this.MovePlayer(this.GetPlayerMovement(movementInput));
                    break;
                case PlayerState.Falling:
                    // rotate camera and move player when falling
                    this.RotatePlayer(lookInput);
                    this.MovePlayer(this.GetPlayerMovement(movementInput));

                    // As well as fall down
                    this.MovePlayer(Vector3.down * this.fallSpeed * Time.deltaTime);
                    break;
            }

            // Move camera with player
            this.CameraFollow();
        }

        /// <summary>
        /// Rotate the player by a specified input.
        /// </summary>
        /// <param name="lookInput">Look input value for the player.</param>
        public void RotatePlayer(Vector2 lookInput)
        {
            Vector2 scaledInput = lookInput * Time.deltaTime * this.rotationSpeed;
            this.attitude += new Vector2(-scaledInput.y, scaledInput.x);

            // Bound pitch between min and max value
            this.attitude = new Vector2(Mathf.Min(Mathf.Max(this.attitude.x, MinPitch), MaxPitch), this.attitude.y);

            this.transform.rotation = Quaternion.Euler(0, this.attitude.y, 0);
        }

        /// <summary>
        /// Move camera to follow main player movement.
        /// </summary>
        private void CameraFollow()
        {
            if (Camera.main != null)
            {
                Camera.main.transform.position = this.transform.position;
                Camera.main.transform.rotation = Quaternion.Euler(this.attitude.x, this.attitude.y, 0);
            }
        }

        public Vector3 GetPlayerMovement(Vector2 movementInput)
        {
            // Scale movement vector to have max length of 1
            movementInput = movementInput.magnitude > 1 ? movementInput.normalized : movementInput;

            // Compute rotated player movement
            Vector3 rotatedMovement = Quaternion.Euler(0, this.Yaw, 0) * new Vector3(movementInput.x, 0, movementInput.y);

            // Scale movement based on speed
            return rotatedMovement * Time.deltaTime * this.playerSpeed;
        }

        /// <summary>
        /// Cast self in a given direction and get the first object hit.
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Mathf.Infinity if none
        /// is found.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        public bool CastSelf(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out RaycastHit hit)
        {
            // Get Parameters associated with the KCC
            Vector3 center = rot * this.capsuleCollider.center + pos;
            float radius = this.capsuleCollider.radius;
            float height = this.capsuleCollider.height;

            // Get top and bottom points of collider
            Vector3 bottom = center + rot * Vector3.down * (height / 2 - radius);
            Vector3 top = center + rot * Vector3.up * (height / 2 - radius);

            // Check what objects this collider will hit when cast with this configuration excluding itself
            IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll(
                top, bottom, radius, dir, dist, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.transform != this.transform);
            bool didHit = hits.Count() > 0;

            // Find the closest objects hit
            float closestDist = didHit ? Enumerable.Min(hits.Select(hit => hit.distance)) : 0;
            IEnumerable<RaycastHit> closestHit = hits.Where(hit => hit.distance == closestDist);

            // Get the first hit object out of the things the player collides with
            hit = closestHit.FirstOrDefault();

            // Return if any objects were hit
            return didHit;
        }

        /// <summary>
        /// Move input value from the player.
        /// </summary>
        /// <param name="movementInput">Movement input value for the player.</param>
        public void MovePlayer(Vector3 movement)
        {
            Vector3 position = this.transform.position;
            Quaternion rotation = this.transform.rotation;

            Vector3 remaining = movement;

            int bounces = 0;

            while (bounces < this.maxBounces && remaining.magnitude > Epsilon)
            {
                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = remaining.magnitude;
                if (!this.CastSelf(position, rotation, remaining.normalized, distance, out RaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    position += remaining;

                    // Exit as we are done bouncing
                    break;
                }

                float fraction = hit.distance / distance;
                // Set the fraction of remaining movement (minus some small value)
                position += remaining * (fraction);
                // Push slightly along normal to stop from getting caught in walls
                position += hit.normal * Epsilon * 2;
                // Decrease remaining movement by fraction of movement remaining
                remaining *= (1 - fraction);

                // Plane to project rest of movement onto
                Vector3 planeNormal = hit.normal;

                // Only apply angular change if hitting something
                // Get angle between surface normal and remaining movement
                float angleBetween = Vector3.Angle(hit.normal, remaining) - 90.0f;

                // Normalize angle between to be between 0 and 1
                // 0 means no angle, 1 means 90 degree angle
                angleBetween = Mathf.Min(MaxAngleShoveRadians, Mathf.Abs(angleBetween));
                float normalizedAngle = angleBetween / MaxAngleShoveRadians;

                // Reduce the remaining movement by the remaining movement that ocurred
                remaining *= Mathf.Pow(1 - normalizedAngle, this.anglePower) * 0.9f + 0.1f;

                // Rotate the remaining movement to be projected along the plane 
                // of the surface hit (emulate pushing against the object)
                Vector3 projected = Vector3.ProjectOnPlane(remaining, planeNormal).normalized * remaining.magnitude;

                // If projected remaining movement is less than original remaining movement (so if the projection broke
                // due to float operations), then change this to just project along the vertical.
                if (projected.magnitude + Epsilon < remaining.magnitude)
                {
                    remaining = Vector3.ProjectOnPlane(remaining, Vector3.up).normalized * remaining.magnitude;
                }
                else
                {
                    remaining = projected;
                }

                // Track number of times the character has bounced
                bounces++;
            }

            // We're done, player was moved as part of loop
            this.transform.position = position;
        }
    }
}
