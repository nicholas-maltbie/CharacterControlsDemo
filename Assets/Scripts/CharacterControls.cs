﻿// Copyright (C) 2022 Nicholas Maltbie
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
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Player state for state machine, transitions include
    ///
    /// Uninitialized -> Idle
    ///          Idle -> Walking  on press movement key
    ///       Walking -> Idle     on release movement key
    /// </summary>
    public enum PlayerState
    {
        Uninitialized,
        Idle,
        Walking
    }

    /// <summary>
    /// Basic character Controls for moving a player around in 3D space.
    /// </summary>
    public class CharacterControls : MonoBehaviour
    {
        
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
        /// Speed of player rotation in degrees per second.
        /// </summary>
        public float rotationSpeed = 180.0f;

        /// <summary>
        /// Current player attitude.
        /// </summary>
        public Vector2 attitude { get; set; }

        /// <summary>
        /// Current movement state of the player.
        /// </summary>
        public PlayerState playerState { get; private set; }

        /// <summary>
        /// Setup for character controls.
        /// </summary>
        public void Start()
        {
            playerState = PlayerState.Idle;
            attitude = Vector3.zero;

            if (moveActionReference != null)
            {
                this.moveAction = moveActionReference.action;
            }
            if (lookActionReference != null)
            {
                this.lookAction = lookActionReference.action;
            }

            this.moveAction.Enable();
            this.lookAction.Enable();
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
            Vector2 movementInput = moveAction.ReadValue<Vector2>();
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // Move camera with player
            CameraFollow();

            // Is the player moving
            bool moving = movementInput.magnitude > 0.001f;

            // Handle state transition if needed.
            switch (playerState)
            {
                case PlayerState.Uninitialized:
                    // Always transition from Uninitialized to idle
                    playerState = PlayerState.Idle;
                    break;
                case PlayerState.Idle:
                    // Transition to walking state if player is moving
                    if (moving)
                    {
                        this.playerState = PlayerState.Walking;
                    }
                    break;
                case PlayerState.Walking:
                    // Transition o idle state if player is not moving
                    if (!moving)
                    {
                        this.playerState = PlayerState.Idle;
                    }
                    break;
            }

            // Update based on current state.
            switch (this.playerState)
            {
                case PlayerState.Idle:
                    // only rotate camera when idle
                    RotatePlayer(lookInput);
                    break;
                case PlayerState.Walking:
                    // rotate camera and move player when walking
                    RotatePlayer(lookInput);
                    MovePlayer(movementInput);
                    break;
            }
        }

        /// <summary>
        /// Rotate the player by a specified input.
        /// </summary>
        /// <param name="lookInput">Look input value for the player.</param>
        public void RotatePlayer(Vector2 lookInput)
        {
            Vector2 scaledInput = lookInput * Time.deltaTime * rotationSpeed;
            this.attitude += new Vector2(-scaledInput.y, scaledInput.x);
            transform.rotation = Quaternion.Euler(this.attitude.x, this.attitude.y, 0);
        }

        /// <summary>
        /// Move camera to follow main player movement.
        /// </summary>
        private void CameraFollow()
        {
            if (Camera.main != null)
            {
                Camera.main.transform.position = transform.position;
                Camera.main.transform.rotation = Quaternion.Euler(attitude.x, attitude.y, 0);
            }
        }

        /// <summary>
        /// Move input value from the player.
        /// </summary>
        /// <param name="movementInput">Movement input value for the player.</param>
        public void MovePlayer(Vector2 movementInput)
        {
            // Scale movement vector to have max length of 1
            movementInput = movementInput.magnitude > 1 ? movementInput.normalized : movementInput;

            // Compute rotated player movement
            Vector3 rotatedMovement = transform.rotation * new Vector3(movementInput.x, 0, movementInput.y);

            // Scale movement based on speed
            Vector3 movement = rotatedMovement * Time.deltaTime * playerSpeed;
            transform.position += movement;
        }
    }
}
