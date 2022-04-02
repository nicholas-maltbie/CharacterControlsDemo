
namespace CCDemo.Tests
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.TestTools;

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

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            this.character = go.AddComponent<CharacterControls>();
            var playerInput = go.AddComponent<PlayerInput>();

            // setup basic inputs in test
            this.character.moveAction = new InputAction(type: InputActionType.PassThrough, binding: "<Gamepad>/leftStick");
            this.character.lookAction = new InputAction(type: InputActionType.PassThrough, binding: "<Gamepad>/rightStick");

            this.character.moveAction.Enable();
            this.character.lookAction.Enable();

            yield return null;
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // Setup gamepad, needs to be done in SetUp method
            this.gamepad = InputSystem.AddDevice<Gamepad>();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            Object.Destroy(this.character.gameObject);
        }

        public IEnumerator ValidateMovement(float time, Vector3 expected)
        {
            // Measure start position
            Vector3 start = character.transform.position;

            // Wait for a second
            yield return new WaitForSeconds(time);
            
            // Assert that the player has moved forward
            Vector3 end = character.transform.position;
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
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveForward()
        {
            // Set input action to move forward
            float time = 1.0f;
            Set<Vector2>(this.gamepad.leftStick, Vector2.up);
            yield return ValidateMovement(time, Vector3.forward * time * character.playerSpeed);
        }

        /// <summary>
        /// Stop when the player stops pressing forward
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveAndStop()
        {
            float time = 1.0f;

            // Set input action to move forward
            Set<Vector2>(this.gamepad.leftStick, Vector2.up);
            yield return ValidateMovement(time, Vector3.forward * time * character.playerSpeed);

            // Set input action to not move
            Set<Vector2>(this.gamepad.leftStick, Vector2.zero);
            yield return ValidateMovement(time, Vector3.zero);
        }

        /// <summary>
        /// Rotate based on the mouse movement
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CharacterControlsRotate()
        {
            // Measure starting attitude
            Quaternion start = character.transform.rotation;

            // Set input action to look to the left
            Set<Vector2>(this.gamepad.rightStick, Vector2.left);

            // Wait for a second
            float time = 1.0f;
            yield return new WaitForSeconds(time);
            
            // Assert that the player has moved forward
            Quaternion end = character.transform.rotation;
            Quaternion expected = start * Quaternion.Euler(0, time * character.rotationSpeed, 0);
            float delta = Quaternion.Angle(end, expected);

            Assert.IsTrue(
                Mathf.Approximately(delta, 0),
                $"Expected player to be at {expected.eulerAngles.ToString("F3")}, but instead found {end.eulerAngles.ToString("F3")}");

            yield return null;
        }

        /// <summary>
        /// Move in the direction the player is looking
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CharacterControlsMoveInDirection()
        {
            // Set the player facing to the right and move forward
            character.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            float time = 1.0f;
            Set<Vector2>(this.gamepad.leftStick, Vector2.up);
            yield return ValidateMovement(time, Vector3.left * time * character.playerSpeed);
        }
    }
}
