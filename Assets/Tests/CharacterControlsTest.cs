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

namespace CCDemo.Tests
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.TestTools;

    /// <summary>
    /// Tests for character controls to ensure character controller acts as expected.
    /// </summary>
    public class CharacterControlsTest : InputTestFixture
    {
        /// <summary>
        /// Character controls object.
        /// </summary>
        private CharacterControls character;

        /// <summary>
        /// Gamepad for character input
        /// </summary>
        private Gamepad gamepad;

        /// <summary>
        /// Setup code run before each test.
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            this.character = go.AddComponent<CharacterControls>();
            _ = go.AddComponent<PlayerInput>();

            // setup basic inputs in test
            this.character.moveAction = new InputAction(type: InputActionType.PassThrough, binding: "<Gamepad>/leftStick");
            this.character.lookAction = new InputAction(type: InputActionType.PassThrough, binding: "<Gamepad>/rightStick");

            this.character.moveAction.Enable();
            this.character.lookAction.Enable();

            // Setup gamepad, needs to be done in SetUp method
            this.gamepad = InputSystem.AddDevice<Gamepad>();
        }

        /// <summary>
        /// Tear down code used to cleanup tests.
        /// </summary>
        [TearDown]
        public override void TearDown()
        {
            GameObject.DestroyImmediate(this.character.gameObject);
            base.TearDown();
        }

        /// <summary>
        /// Validate player movement for a give period of time in a given direction.
        /// </summary>
        /// <param name="time">Expected time to pass</param>
        /// <param name="expected">Expected distance for player to move.</param>
        /// <returns>Enumerator of waiting events for player movement.</returns>
        public IEnumerator ValidateMovement(float time, Vector3 expected)
        {
            // Measure start position
            Vector3 start = this.character.transform.position;

            // Wait for a second
            yield return new WaitForSeconds(time);

            // Assert that the player has moved forward
            Vector3 end = this.character.transform.position;
            Vector3 movement = end - start;
            float delta = (expected - movement).magnitude;

            Assert.IsTrue(
                Mathf.Approximately(delta, 0),
                $"Expected player to move {expected.ToString("F3")}, but instead found {movement.ToString("F3")}");

            yield return null;
        }

        /// <summary>
        /// Move forward when the player presses forward
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveForward()
        {
            // Set input action to move forward
            float time = 1.0f;
            this.Set(this.gamepad.leftStick, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.forward * time * this.character.playerSpeed);
        }

        /// <summary>
        /// Stop when the player stops pressing forward
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveAndStop()
        {
            float time = 1.0f;

            // Set input action to move forward
            this.Set(this.gamepad.leftStick, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.forward * time * this.character.playerSpeed);

            // Set input action to not move
            this.Set(this.gamepad.leftStick, Vector2.zero);
            yield return this.ValidateMovement(time, Vector3.zero);
        }

        /// <summary>
        /// Rotate based on the mouse movement
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsRotate()
        {
            // Measure starting attitude
            Quaternion start = this.character.transform.rotation;

            // Set input action to look to the left
            this.Set(this.gamepad.rightStick, Vector2.left);

            // Wait for a second
            float time = 1.0f;
            yield return new WaitForSeconds(time);

            // Assert that the player has moved forward
            Quaternion end = this.character.transform.rotation;
            Quaternion expected = start * Quaternion.Euler(0, time * this.character.rotationSpeed, 0);
            float delta = Quaternion.Angle(end, expected);

            Assert.IsTrue(
                Mathf.Approximately(delta, 0),
                $"Expected player to be at {expected.eulerAngles.ToString("F3")}, but instead found {end.eulerAngles.ToString("F3")}");

            yield return null;
        }

        /// <summary>
        /// Move in the direction the player is looking
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveInDirection()
        {
            // Set the player facing to the right and move forward
            this.character.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            float time = 1.0f;
            this.Set(this.gamepad.leftStick, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.left * time * this.character.playerSpeed);
        }
    }
}
