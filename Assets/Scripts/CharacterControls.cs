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
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Basic character Controls for moving a player around in 3D space.
    /// </summary>
    public class CharacterControls : MonoBehaviour
    {
        /// <summary>
        /// Look action to modify the player camera.
        /// </summary>
        public InputAction lookAction;

        /// <summary>
        /// Move action to move the player around.
        /// </summary>
        public InputAction moveAction;

        /// <summary>
        /// Speed of player movement in units per second.
        /// </summary>
        public float playerSpeed = 5.0f;

        /// <summary>
        /// Speed of player rotation in degrees per second.
        /// </summary>
        public float rotationSpeed = 180.0f;

        /// <summary>
        /// Setup for character controls.
        /// </summary>
        public void Start()
        {

        }

        /// <summary>
        /// Update function for character controls.
        /// </summary>
        public void Update()
        {

        }
    }
}
