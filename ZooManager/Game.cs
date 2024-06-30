using System.Collections.Generic;
using System;

namespace ZooManager
{
    public class Game
    {
        public List<Element> elements = new List<Element>();
        public Element SelectedElement { get; set; }
        public int Size { get; set; } = 11; // Ensure the grid is 11x11
        public string[,] Map { get; set; }
        public bool IsGameOver { get; set; } = false;
        public bool IsStarted { get; set; } = false;
        public string GameMessage { get; set; } = string.Empty;
        private int playerMoves = 0;
        private Element mouse;
        private Element cat;

        public Game()
        {
            SetUpGame();
        }

        public void SetUpGame()
        {
            elements.Clear();
            Map = new string[Size, Size];
            // Add walls around the edges
            for (int i = 0; i < Size; i++)
            {
                Map[i, 0] = "🧱";
                Map[i, Size - 1] = "🧱";
                Map[0, i] = "🧱";
                Map[Size - 1, i] = "🧱";
            }
            // Create two random exits in the walls
            Random rand = new Random();
            int exit1 = rand.Next(1, Size - 1);
            int exit2 = rand.Next(1, Size - 1);
            Map[exit1, 0] = string.Empty;
            Map[exit2, Size - 1] = string.Empty;

            // Add random walls inside the grid
            for (int i = 0; i < 4; i++)
            {
                int x, y;
                do
                {
                    x = rand.Next(1, Size - 1);
                    y = rand.Next(1, Size - 1);
                } while (Map[x, y] == "🧱");
                Map[x, y] = "🧱";
            }

            // Add a cat and a mouse
            PlaceAnimal("🐱", out int catX, out int catY);
            PlaceAnimal("🐭", out int mouseX, out int mouseY, (cx, cy, mx, my) => Math.Abs(cx - mx) >= 2 && Math.Abs(cy - my) >= 2);
            cat = new Element { X = catX, Y = catY, Emoji = "🐱" };
            mouse = new Element { X = mouseX, Y = mouseY, Emoji = "🐭" };
            elements.Add(cat);
            elements.Add(mouse);
        }

        public void AddElement(Element element, int x, int y)
        {
            if (Map[x, y] == string.Empty)
            {
                element.X = x;
                element.Y = y;
                Map[x, y] = element.Emoji;
                elements.Add(element);
            }
        }

        public void SelectElement(Element element)
        {
            SelectedElement = element;
        }

        public void ZoneClick(int x, int y)
        {
            if (IsGameOver || !IsStarted) return;

            if (x < 0 || x >= Size || y < 0 || y >= Size) return; // Boundary check

            if (SelectedElement != null && Map[x, y] == string.Empty)
            {
                elements.Add(SelectedElement);
                Map[x, y] = SelectedElement.Emoji;
                SelectedElement.X = x;
                SelectedElement.Y = y;
                SelectedElement = null; // Reset selected element
                playerMoves++;
                if (playerMoves % 2 == 0)
                {
                    MoveCat();
                }
                CheckGameOver();
            }
        }
        
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public void MoveMouse(Direction direction)
        {
            if (mouse != null)
            {
                int newX = mouse.X;
                int newY = mouse.Y;

                switch (direction)
                {
                    case Direction.Up:
                        newY--;
                        break;
                    case Direction.Down:
                        newY++;
                        break;
                    case Direction.Left:
                        newX--;
                        break;
                    case Direction.Right:
                        newX++;
                        break;
                }

                if (newX >= 0 && newX < Size && newY >= 0 && newY < Size)
                {
                    mouse.X = newX;
                    mouse.Y = newY;
                }

                Update();
            }
        }

        private void PlaceAnimal(string emoji, out int x, out int y, Func<int, int, int, int, bool> condition = null)
        {
            Random rand = new Random();
            do
            {
                x = rand.Next(1, Size - 1);
                y = rand.Next(1, Size - 1);
            } while (Map[x, y] != string.Empty || (condition != null && !condition(x, y, x, y)));
            Map[x, y] = emoji;
        }

        private void MoveCat()
        {
            // Implement cat movement logic here
        }

        private void CheckGameOver()
        {
            // Implement game over check logic here
        }

        private void Update()
        {
            // Update the game state
        }
    }
}
