using System;
using System.Collections.Generic;

namespace ZooManager
{
    public class Game
    {
        public string[,] Map { get; private set; }
        public int Size { get; private set; } = 11; // 改为 11x11
        private List<Element> elements;
        private Mouse mouse;
        private Cat cat;
        private Random random;
        private int playerMoves = 0;
        public bool IsGameOver { get; private set; } = false;
        public string GameMessage { get; private set; } = "";
        public bool IsStarted { get; set; } = false; // 添加 IsStarted 属性

        public Game()
        {
            random = new Random();
            Map = new string[Size, Size];
            elements = new List<Element>();
            InitializeMap();
        }

        private void InitializeMap()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Map[i, j] = string.Empty;
                }
            }
        }

        public void SetUpGame()
        {
            IsStarted = false;
            IsGameOver = false;
            GameMessage = "";
            playerMoves = 0;
            InitializeMap();
            elements.Clear();
            AddWalls();
            AddRandomExits();
            AddRandomWalls(4);
            AddCatAndMouse();
            IsStarted = true;
        }

        private void AddWalls()
        {
            for (int i = 0; i < Size; i++)
            {
                Map[0, i] = "🧱"; // Top wall
                Map[Size - 1, i] = "🧱"; // Bottom wall
                Map[i, 0] = "🧱"; // Left wall
                Map[i, Size - 1] = "🧱"; // Right wall
            }
        }

        private void AddRandomExits()
        {
            List<(int, int)> possibleExits = new List<(int, int)>();

            // Collect all possible exit positions
            for (int i = 1; i < Size - 1; i++)
            {
                possibleExits.Add((0, i)); // Top row (excluding corners)
                possibleExits.Add((Size - 1, i)); // Bottom row (excluding corners)
                possibleExits.Add((i, 0)); // Left column (excluding corners)
                possibleExits.Add((i, Size - 1)); // Right column (excluding corners)
            }

            // Select two unique random exits
            var exit1 = possibleExits[random.Next(possibleExits.Count)];
            possibleExits.Remove(exit1); // Ensure the same exit isn't chosen twice
            var exit2 = possibleExits[random.Next(possibleExits.Count)];

            // Clear the exit positions on the map
            Map[exit1.Item1, exit1.Item2] = string.Empty;
            Map[exit2.Item1, exit2.Item2] = string.Empty;
        }

        private void AddRandomWalls(int count)
        {
            int wallsAdded = 0;
            while (wallsAdded < count)
            {
                int x = random.Next(1, Size - 1);
                int y = random.Next(1, Size - 1);
                if (Map[x, y] == string.Empty)
                {
                    Map[x, y] = "🧱";
                    wallsAdded++;
                }
            }
        }

        private void AddCatAndMouse()
        {
            (int x, int y) catPos, mousePos;
            do
            {
                catPos = (random.Next(1, Size - 1), random.Next(1, Size - 1));
                mousePos = (random.Next(1, Size - 1), random.Next(1, Size - 1));
            } while (Math.Abs(catPos.x - mousePos.x) + Math.Abs(catPos.y - mousePos.y) < 3 || Map[catPos.x, catPos.y] != string.Empty || Map[mousePos.x, mousePos.y] != string.Empty);

            // Place Cat
            cat = new Cat();
            AddElement(cat, catPos.x, catPos.y);

            // Place Mouse
            mouse = new Mouse();
            AddElement(mouse, mousePos.x, mousePos.y);
        }

        public void AddElement(Element element, int x, int y)
        {
            if (x >= 0 && x < Size && y >= 0 && y < Size && Map[x, y] == string.Empty)
            {
                elements.Add(element);
                Map[x, y] = element.Emoji;
                element.X = x;
                element.Y = y;
            }
        }

        public void MoveMouse(Direction direction)
        {
            if (mouse == null || IsGameOver || !IsStarted) return;

            int newX = mouse.X;
            int newY = mouse.Y;

            switch (direction)
            {
                case Direction.up:
                    newY = mouse.Y - 1;
                    break;
                case Direction.down:
                    newY = mouse.Y + 1;
                    break;
                case Direction.left:
                    newX = mouse.X - 1;
                    break;
                case Direction.right:
                    newX = mouse.X + 1;
                    break;
            }

            if (IsValidMove(newX, newY))
            {
                Map[mouse.X, mouse.Y] = string.Empty;
                mouse.X = newX;
                mouse.Y = newY;
                Map[mouse.X, mouse.Y] = mouse.Emoji;
                playerMoves++;

                if (playerMoves % 2 == 0)
                {
                    MoveCat();
                }

                CheckGameOver();
            }
        }

        private void MoveCat()
        {
            if (cat == null || IsGameOver) return;

            for (int i = 0; i < 2; i++)
            {
                int deltaX = mouse.X - cat.X;
                int deltaY = mouse.Y - cat.Y;

                int newX = cat.X;
                int newY = cat.Y;

                if (Math.Abs(deltaX) > Math.Abs(deltaY))
                {
                    newX += deltaX > 0 ? 1 : -1;
                }
                else
                {
                    newY += deltaY > 0 ? 1 : -1;
                }

                if (IsValidMove(newX, newY))
                {
                    Map[cat.X, cat.Y] = string.Empty;
                    cat.X = newX;
                    cat.Y = newY;
                    Map[cat.X, cat.Y] = cat.Emoji;
                }
            }

            CheckGameOver();
        }

        private bool IsValidMove(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size && Map[x, y] == string.Empty;
        }

        private void CheckGameOver()
        {
            if (cat.X == mouse.X && cat.Y == mouse.Y)
            {
                IsGameOver = true;
                GameMessage = "Game Over: The cat caught the mouse!";
                Console.WriteLine(GameMessage);
            }
            else if (mouse.X == 0 || mouse.X == Size - 1 || mouse.Y == 0 || mouse.Y == Size - 1)
            {
                IsGameOver = true;
                GameMessage = "Congratulations! You have escaped!";
                Console.WriteLine(GameMessage);
            }
        }

        public void Update()
        {
            foreach (var element in elements)
            {
                element.Activate(this);
            }
        }

        public bool Seek(int x, int y, Direction direction, string target)
        {
            // Implement seeking logic to find target in the specified direction
            return random.NextDouble() > 0.5;
        }

        public bool Retreat(Element element, Direction direction)
        {
            // Implement retreat logic to move element in the specified direction
            return true;
        }

        public void Attack(Element element, Direction direction)
        {
            // Implement attack logic for the element in the specified direction
        }
    }

    public enum Direction
    {
        up,
        down,
        left,
        right
    }
}
