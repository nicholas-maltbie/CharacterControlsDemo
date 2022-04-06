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
    using System;
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
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
        /// Main camera for player view.
        /// </summary>
        private GameObject mainCamera;

        /// <summary>
        /// Vector2Control for look input controls.
        /// </summary>
        private Vector2Control LookInput => this.gamepad.leftStick;

        /// <summary>
        /// Vector2Control for move input controls.
        /// </summary>
        private Vector2Control MoveInput => this.gamepad.rightStick;

        /// <summary>
        /// Are two vectors close enough for a given distance.
        /// </summary>
        /// <param name="a">Vector one.</param>
        /// <param name="b">Vector two.</param>
        /// <param name="dist">Minimum distance to measure for.</param>
        /// <returns>True if the distance between a and b are within a specific distance.</returns>
        public bool WithinBounds(Vector3 a, Vector3 b, float dist)
        {
            return (a - b).magnitude <= dist;
        }

        /// <summary>
        /// Are two angles within a small enough distance to each other.
        /// </summary>
        /// <param name="a">Rotation one.</param>
        /// <param name="b">Rotation two.</param>
        /// <param name="angle">Threshold angle between the two.</param>
        /// <returns>True if the angle between a and b are within a specific amount.</returns>
        public bool WithinBounds(Quaternion a, Quaternion b, float angle)
        {
            return Quaternion.Angle(a, b) <= angle;
        }

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

            // Setup gamepad, needs to be done in SetUp method
            this.gamepad = InputSystem.AddDevice<Gamepad>();

            // setup basic inputs in test
            this.character.moveAction = new InputAction(type: InputActionType.PassThrough, binding: MoveInput.path);
            this.character.lookAction = new InputAction(type: InputActionType.PassThrough, binding: LookInput.path);

            this.character.moveAction.Enable();
            this.character.lookAction.Enable();

            // Setup a main camera
            this.mainCamera = new GameObject();
            this.mainCamera.AddComponent<Camera>();
            this.mainCamera.gameObject.tag = "MainCamera";
        }

        /// <summary>
        /// Tear down code used to cleanup tests.
        /// </summary>
        [TearDown]
        public override void TearDown()
        {
            GameObject.DestroyImmediate(this.character.gameObject);
            GameObject.DestroyImmediate(this.mainCamera);
            base.TearDown();
        }

        /// <summary>
        /// Validate player movement for a give period of time in a given direction.
        /// </summary>
        /// <param name="time">Expected time to pass</param>
        /// <param name="expected">Expected distance for player to move.</param>
        /// <param name="error">Expected error in final player position.</param>
        /// <returns>Enumerator of waiting events for player movement.</returns>
        public IEnumerator ValidateMovement(float time, Vector3 expected, float error = 0.1f)
        {
            // Measure start position
            Vector3 start = this.character.transform.position;

            // Wait for a second
            yield return new WaitForSeconds(time);

            // Assert that the player has moved forward
            Vector3 end = this.character.transform.position;
            Vector3 movement = end - start;

            Assert.IsTrue(
                this.WithinBounds(movement, expected, error),
                $"Expected player to move {expected.ToString("F3")}, but instead found {movement.ToString("F3")}");

            yield return null;
        }

        /// <summary>
        /// Validate player rotation for a give period of time in a given direction.
        /// </summary>
        /// <param name="time">Expected time to pass</param>
        /// <param name="expected">Expected rotation for player in euler angles.</param>
        /// <returns>Enumerator of waiting events for player rotation.</returns>
        public IEnumerator ValidateRotation(float time, Vector3 expected)
        {
            // Measure starting attitude
            Quaternion start = this.character.transform.rotation;

            // Wait for a second
            yield return new WaitForSeconds(time);

            // Assert that the player has moved forward
            Quaternion end = this.character.transform.rotation;
            Quaternion expectedFinal = start * Quaternion.Euler(expected);

            Assert.IsTrue(
                this.WithinBounds(end, expectedFinal, 10.0f),
                $"Expected player to be at {expectedFinal.eulerAngles.ToString("F3")}, but instead found {end.eulerAngles.ToString("F3")}");

            yield return null;
        }

        /// <summary>
        /// Validate that the camera position is within some bounds of the current player position.
        /// </summary>
        public void ValidateCameraFollowingPlayer()
        {
            Vector3 expectedPosition = this.character.transform.position;
            Vector3 actualPosition = Camera.main.gameObject.transform.position;
            Assert.IsTrue(
                this.WithinBounds(expectedPosition, actualPosition, 0.1f),
                $"Expected camera position to be {expectedPosition.ToString("F3")}, but instead found {actualPosition.ToString("F3")}");

            var expectedRotation = Quaternion.Euler(this.character.attitude.x, this.character.attitude.y, 0);
            Quaternion actualRotation = Camera.main.gameObject.transform.rotation;
            Assert.IsTrue(
                this.WithinBounds(expectedRotation, actualRotation, 10f),
                $"Expected camera rotation to be {expectedRotation.ToString("F3")} but instead found {actualRotation.ToString("F3")}");
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
            this.Set(MoveInput, Vector2.up);
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
            this.Set(MoveInput, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.forward * time * this.character.playerSpeed);

            // Set input action to not move
            this.Set(MoveInput, Vector2.zero);
            yield return this.ValidateMovement(time, Vector3.zero);
        }

        /// <summary>
        /// Rotate based on the mouse movement
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsRotate()
        {
            float time = 1.0f;
            // Set input action to look to the left
            this.Set(LookInput, Vector2.left);
            yield return this.ValidateRotation(time, new Vector3(0, 180, 0));
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
            var attitude = Quaternion.LookRotation(Vector3.left, Vector3.up);
            this.character.SetAttitude(new Vector2(attitude.eulerAngles.x, attitude.eulerAngles.y));
            float time = 1.0f;
            this.Set(MoveInput, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.left * time * this.character.playerSpeed);
        }

        /// <summary>
        /// Move and turn the player in the same test.
        /// </summary>
        /// <returns>Enumerator test events.</returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveAndTurn()
        {
            float time = 1.0f;

            // Set input action to move forward
            this.Set(MoveInput, Vector2.up);
            yield return this.ValidateMovement(time, Vector3.forward * time * this.character.playerSpeed);

            // Set input action to not move and turn
            this.Set(MoveInput, Vector2.zero);
            this.Set(LookInput, Vector2.left);
            yield return this.ValidateRotation(time, new Vector3(0, 180, 0));

            // Move again but should be moving backward now
            this.Set(MoveInput, Vector2.up);
            this.Set(LookInput, Vector2.zero);
            yield return this.ValidateMovement(time, Vector3.back * time * this.character.playerSpeed, error: 1.0f);
        }

        /// <summary>
        /// Valdate that if there is a main camera, it moves with the player.
        /// </summary>
        /// <returns>Enumerator of test events.</returns>
        [UnityTest]
        public IEnumerator CameraMoveWithPlayer()
        {
            // Set input action to move and turn
            this.Set(MoveInput, Vector2.up);
            this.Set(LookInput, Vector2.left);

            // Validate camera starts with player
            yield return null;
            this.ValidateCameraFollowingPlayer();

            // Wait for a delay and validate the camera moves with the player
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.5f);
                this.ValidateCameraFollowingPlayer();
            }
        }

        /// <summary>
        /// Validate that the camera rotates within a given set of bounds for a player.
        /// </summary>
        /// <returns>Enumerator of test events.</returns>
        [UnityTest]
        public IEnumerator CameraPitchBoundsValidation()
        {
            float delay = 0.1f;

            // Set input action to rotate camera up
            this.Set(LookInput, Vector2.up);

            // Have the player rotate up, but assert that they never pass 90 degree pitch
            for (int i = 0; i < 360 / this.character.rotationSpeed / delay; i++)
            {
                yield return new WaitForSeconds(delay);

                Assert.IsTrue(
                    -90 <= character.Pitch && character.Pitch <= 90,
                    $"Expected pitch to be between 0 and 90, but instead found {character.Pitch}");
            }

            // Set input action to rotate camera down
            this.Set(LookInput, Vector2.down);

            // Have the player rotate up, but assert that they never pass 90 degree pitch
            for (int i = 0; i < 360 / this.character.rotationSpeed / delay; i++)
            {
                yield return new WaitForSeconds(delay);

                Assert.IsTrue(
                    -90 <= character.Pitch && character.Pitch <= 90,
                    $"Expected pitch to be between 0 and 90, but instead found {character.Pitch}");
            }
        }

        /// <summary>
        /// Ensure that when the player moves, they only move along the horizontal plane.
        /// </summary>
        /// <returns>Enumerator of test events.</returns>
        [UnityTest]
        public IEnumerator OnlyMoveOnHorizontalPlane()
        {
            // Set input action to move and turn
            this.Set(MoveInput, Vector2.up + Vector2.left);
            this.Set(LookInput, Vector2.up + Vector2.left);

            yield return null;
            float startingY = character.transform.position.y;

            // Wait for a delay and validate the player does not move along the vertical plane
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.1f);
                Assert.IsTrue(
                    Mathf.Abs(character.transform.position.y - startingY) < 0.1f,
                    $"Expected player to not move along horizontal plane, had starting y of {startingY} and found y of {character.transform.position.y}");
            }
        }
    }
}
