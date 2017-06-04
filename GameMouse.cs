using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HelloGame
{
    public class GameMouse
    {
        private MouseState currentState, lastState;
        public Vector2 currentPosition, lastPosition;
        private float currentScrollWheelValue;
        public float deltaScrollWheelValue;

        public GameMouse()
        {

        }

        public void PreUpdate()
        {
            currentState = Mouse.GetState();
            currentPosition = currentState.Position.ToVector2();

            deltaScrollWheelValue = currentState.ScrollWheelValue - currentScrollWheelValue;
            currentScrollWheelValue += deltaScrollWheelValue;
        }

        public void Update()
        {

        }

        public void PostUpdate()
        {
            lastState = currentState;
            lastPosition = currentPosition;
        }

        public Vector2 GetWorldPosition()
        {
            return Camera.ToWorldCoords(currentPosition);
        }

        public float ScrollWheelDirection()
        {
            return deltaScrollWheelValue;
        }

        public bool LeftButtonPressed()
        {
            return currentState.LeftButton == ButtonState.Pressed && lastState.LeftButton == ButtonState.Released;
        }

        public bool LeftButtonHeld()
        {
            return currentState.LeftButton == ButtonState.Pressed;
        }
    }
}
