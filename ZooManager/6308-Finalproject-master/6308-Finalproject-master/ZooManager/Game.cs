using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public class Game
    {
        
        public int Size { get; private set; } = 11;
        static public Zone holdingPen = new Zone(-1, -1, null);
        private Mouse mouse;
        private Cat cat;
        private Random random;
        private int playerMoves = 0;
        public bool IsGameOver { get; private set; } = false;
        public string GameMessage { get; private set; } = "";
        public bool IsStarted { get; set; } = false;
        private int catSkipTurns = 0;
        static public List<List<Zone>> animalZones = new List<List<Zone>>();
        private int catMoveTowardsWallTurns = 0;
        private (int x, int y)? targetWallPosition;
        private int catPauseTurns = 0;


        public Game()
        {
            random = new Random();
            InitializeZones();
            //elements = new List<Element>();

        }
        private void InitializeZones()
        {
            animalZones.Clear();
            for (var y = 0; y < Size; y++)
            {
                List<Zone> rowList = new List<Zone>();
                for (var x = 0; x < Size; x++) rowList.Add(new Zone(x, y, null));
                animalZones.Add(rowList);
            }
        }

        public void SetUpGame()
        {
            IsStarted = false;
            IsGameOver = false;
            
            playerMoves = 0;
            InitializeZones();
            animalZones.Clear();
            for (var y = 0; y < Size; y++)
            {
                List<Zone> rowList = new List<Zone>();
                // Note one-line variation of for loop below!
                for (var x = 0; x < Size; x++) rowList.Add(new Zone(x, y, null));
                animalZones.Add(rowList);
            }
            //occupants.Clear();
            //animalzone = new Zone[Size, Size];
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
                animalZones[i][0].occupant = new Wall(); 
                animalZones[i][Size - 1].occupant = new Wall(); 
                animalZones[0][i].occupant = new Wall(); 
                animalZones[Size - 1][i].occupant = new Wall(); 
            }
        }

        private void AddRandomExits()
        {
            List<(int, int)> possibleExits = new List<(int, int)>();

            for (int i = 1; i < Size - 1; i++)
            {
                possibleExits.Add((i, 0)); 
                possibleExits.Add((i, Size - 1)); 
                possibleExits.Add((0, i)); 
                possibleExits.Add((Size - 1, i)); 
            }

            var exit1 = possibleExits[random.Next(possibleExits.Count)];
            possibleExits.Remove(exit1);
            var exit2 = possibleExits[random.Next(possibleExits.Count)];

            animalZones[exit1.Item1][exit1.Item2].occupant = null;
            animalZones[exit2.Item1][exit2.Item2].occupant = null;
        }

        private void AddRandomWalls(int count)
        {
            int wallsAdded = 0;
            
            while (wallsAdded < count)
            {
                int x = random.Next(1, Size - 1);
                int y = random.Next(1, Size - 1);
                if (animalZones[y][x].occupant == null)
                {
                    animalZones[y][x].occupant = new Wall();
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
            } while ((Math.Abs(catPos.x - mousePos.x) < 2 && Math.Abs(catPos.y - mousePos.y) < 2) || animalZones[catPos.x][catPos.y].occupant != null || animalZones[mousePos.x][mousePos.y].occupant != null);

            // Place Cat
            cat = new Cat("Amy");
            animalZones[catPos.x][catPos.y].occupant = cat;
            cat.X = catPos.x;
            cat.Y = catPos.y;

            // Place Mouse
            mouse = new Mouse("May");
            animalZones[mousePos.x][mousePos.y].occupant = mouse;
            mouse.X = mousePos.x;
            mouse.Y = mousePos.y;
        }
        public Occupant SelectedOccupant { get; private set; }

        public void SelectOccupant(Occupant occupant)
        {
            SelectedOccupant = occupant;
        }
        private bool IsOwnerPresent()
        {
            Console.WriteLine("Checking if owner is present");
            return animalZones.Any(row => row.Any(zone => zone.occupant is Owner));
        }
        public void AddElement(Occupant occupant, int x, int y)
        {
            if (IsGameOver || !IsStarted) return;

            if (x >= 0 && x < Size && y >= 0 && y < Size)
            {
                var currentOccupant = animalZones[y][x].occupant;
                Console.WriteLine($"Current occupant at {x},{y}: {currentOccupant?.GetType().Name ?? "None"}");

                // Check if placing Cheese on Wall
                if (currentOccupant is Wall && occupant is Cheese)
                {
                    // Remove the current wall and place an owner
                    Console.WriteLine("Owner");
                    animalZones[y][x].occupant = new Owner();
                    occupant = null; // Reset the selected element
                    return;
                }
                // Check if Owner is placed next to Wall
                if (occupant is Owner && IsNextToWall(x, y))
                {
                    // Remove the adjacent Wall
                    RemoveAdjacentWall(x, y);
                }
                // Check if placing Owner next to CatToy
                if (occupant is Owner && IsNextTo(x, y, typeof(CatToy)))
                {
                    Console.WriteLine("Owner placed next to CatToy");
                    // Generate two random CatToys
                    GenerateRandomCatToys(2);
                }
                // Check if Cheese is next to Owner
                if (occupant is Cheese && IsNextToOwner(x, y))
                {
                    // Increase Cat's life
                    if (cat != null)
                    {
                        cat.IncreaseLife();
                        Console.WriteLine("Cat's life increased by 1");
                    }
                }
                // Check if Cheese and CatToy are adjacent
                if (occupant is Cheese)
                {
                    if (IsNextToAnyCatToy(x, y))
                    {
                        Console.WriteLine("Removing both CatToy and Cheese");
                        animalZones[y][x].occupant = null;
                        RemoveAdjacentCatToy(x, y);
                        return;
                    }

                    if (IsNextToAnyWall(x, y))
                    {
                        Console.WriteLine("Removing both Wall and Cheese, placing Owner");
                        var wallPosition = GetAdjacentWallPosition(x, y);
                        if (wallPosition != null)
                        {
                            animalZones[wallPosition.Value.y][wallPosition.Value.x].occupant = new Owner();
                            animalZones[y][x].occupant = null;
                        }
                        return;
                    }
                }
                else if (occupant is CatToy)
                {
                    if (IsNextToAnyCheese(x, y))
                    {
                        Console.WriteLine("Removing both CatToy and Cheese");
                        animalZones[y][x].occupant = null;
                        RemoveAdjacentCheese(x, y);
                        return;
                    }
                }
                // Check if CatToy is next to Wall
                if (occupant is CatToy && IsNextToWall(x, y))
                {
                    // Cat will pause for two turns
                    catPauseTurns = 2;
                }
                // Place the new occupant if the zone is empty
                if (animalZones[y][x].occupant == null)
                {
                    animalZones[y][x].occupant = occupant;
                    occupant.X = x;
                    occupant.Y = y;
                    playerMoves++;
                    MoveCat();
                    CheckGameOver();
                }

                occupant = null; // Reset the selected element
            }
        }
        private void GenerateRandomCatToys(int count)
        {
            int catToysAdded = 0;

            while (catToysAdded < count)
            {
                int x = random.Next(1, Size - 1);
                int y = random.Next(1, Size - 1);

                if (animalZones[y][x].occupant == null)
                {
                    animalZones[y][x].occupant = new CatToy();
                    catToysAdded++;
                    Console.WriteLine($"CatToy generated at {x}, {y}");
                }
            }
        }
        private bool IsNextTo(int x, int y, Type type)
        {
            List<(int, int)> adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant != null && animalZones[pos.Item2][pos.Item1].occupant.GetType() == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool IsNextToOwner(int x, int y)
        {
            List<(int, int)> adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant is Owner)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool IsNextToWall(int x, int y)
        {
            List<(int, int)> adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            return adjacentPositions.Any(pos =>
                pos.Item1 >= 0 && pos.Item1 < Size &&
                pos.Item2 >= 0 && pos.Item2 < Size &&
                animalZones[pos.Item2][pos.Item1].occupant is Wall);
        }
        private void RemoveAdjacentWall(int x, int y)
        {
            List<(int, int)> adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant is Wall)
                    {
                        Console.WriteLine($"Removing Wall at {pos.Item1}, {pos.Item2}");
                        animalZones[pos.Item2][pos.Item1].occupant = null;
                    }
                }
            }
        }
        private bool IsNextToAnyWall(int x, int y)
        {
            return IsNextToType(x, y, typeof(Wall));
        }
        private (int x, int y)? GetAdjacentWallPosition(int x, int y)
        {
            var adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant is Wall)
                    {
                        return pos;
                    }
                }
            }
            return null;
        }
        private bool IsNextToAnyCatToy(int x, int y)
        {
            return IsNextToType(x, y, typeof(CatToy));
        }

        private bool IsNextToAnyCheese(int x, int y)
        {
            return IsNextToType(x, y, typeof(Cheese));
        }

        private bool IsNextToType(int x, int y, Type type)
        {
            var adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant?.GetType() == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RemoveAdjacentCatToy(int x, int y)
        {
            RemoveAdjacentType(x, y, typeof(CatToy));
        }

        private void RemoveAdjacentCheese(int x, int y)
        {
            RemoveAdjacentType(x, y, typeof(Cheese));
        }

        private void RemoveAdjacentType(int x, int y, Type type)
        {
            var adjacentPositions = new List<(int, int)>
    {
        (x - 1, y),
        (x + 1, y),
        (x, y - 1),
        (x, y + 1)
    };

            foreach (var pos in adjacentPositions)
            {
                if (pos.Item1 >= 0 && pos.Item1 < Size && pos.Item2 >= 0 && pos.Item2 < Size)
                {
                    if (animalZones[pos.Item2][pos.Item1].occupant?.GetType() == type)
                    {
                        animalZones[pos.Item2][pos.Item1].occupant = null;
                    }
                }
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

            // Check if within the map boundaries
            if (newX < 0 || newX >= Size || newY < 0 || newY >= Size)
            {
                Console.WriteLine("Out of bounds");
                return;
            }
            if (animalZones[newY][newX].occupant is CatToy)
            {
                Console.WriteLine("Mouse is blocked by CatToy");
                return; // Mouse is blocked by CatToy
            }
            // Check if the move is to a cell occupied by cheese
            if (animalZones[newY][newX].occupant is Cheese)
            {
                Console.WriteLine("Mouse found cheese");
                mouse.IncreaseLife();
                animalZones[newY][newX].occupant = null; // Remove the cheese
            }

            // Check if the move is to an empty cell
            if (animalZones[newX][newY].occupant == null)
            {
                animalZones[mouse.X][mouse.Y].occupant = null;
                mouse.X = newX;
                mouse.Y = newY;
                animalZones[mouse.X][mouse.Y].occupant = mouse;
                playerMoves++;
                Console.WriteLine($"Mouse moves to {newX}, {newY}");
                MoveCat();
                MoveCat();
                if (IsOwnerPresent())
                {
                    MoveCat();
                }
                CheckGameOver();
            }
        }

        private void MoveCat()
        {
            if (cat == null || IsGameOver) return;
            Console.WriteLine(catSkipTurns);
            if (catSkipTurns > 0)
            {
                catSkipTurns--;
                Console.WriteLine("Cat is skipping a turn.");
                return;
            }
            if (catPauseTurns > 0)
            {
                catPauseTurns--;
                Console.WriteLine("Cat is pausing due to CatToy next to Wall.");
                return;
            }
            int deltaX = mouse.X - cat.X;
            int deltaY = mouse.Y - cat.Y;
            int originalX = cat.X;
            int originalY = cat.Y;

            int newX = cat.X;
            int newY = cat.Y;
            Console.WriteLine($"Cat at {cat.X}, {cat.Y} and mouse at {mouse.X}, {mouse.Y}");


            deltaX = mouse.X - cat.X;
            deltaY = mouse.Y - cat.Y;

            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                newX += deltaX > 0 ? 1 : -1;
            }
            else
            {
                newY += deltaY > 0 ? 1 : -1;
            }
            var occupant = animalZones[newX][newY].occupant;
            Console.WriteLine($"Occupant at new position ({newX}, {newY}): {occupant?.GetType().Name ?? "None"}");

            


            Console.WriteLine(occupant);
            // Check if within the map boundaries
            if (newX < 0 || newX >= Size || newY < 0 || newY >= Size)
            {
                Console.WriteLine("Out of bounds");
                return;
            }

            // Check if the move is to a cell occupied by the mouse
            // Check if the move is to a cell occupied by the mouse
            if (occupant is Mouse)
            {
                Console.WriteLine("Cat caught mouse");
                mouse.DecreaseLife();
                if (mouse.Life > 0)
                {
                    // If mouse is still alive, move cat back to the original position
                    newX = originalX;
                    newY = originalY;
                }
                else
                {
                    // If mouse is dead, remove mouse from the board
                    animalZones[mouse.X][mouse.Y].occupant = null;
                }
                CheckGameOver();
                return;
            }
            if (occupant is Cheese)
            {
                Console.WriteLine("Cat is blocked by Cheese");
                return; // Cat is blocked by Cheese
            }
            if (occupant is CatToy)
            {
                Console.WriteLine("Cat found a toy");
                animalZones[newX][newY].occupant = null; // Remove the CatToy
                catSkipTurns = 4; // Cat skips the next 2 turns
            }
            if (occupant is Wall)
            {
                // 50% chance to be blocked by the wall
                if (random.NextDouble() < 0.6)
                {
                    Console.WriteLine("Cat is blocked by the wall and cannot move");
                    return; // Cat is blocked by the wall
                }
                else
                {
                    // Cat ignores the wall and moves
                    Console.WriteLine("Cat ignores the wall and moves");
                    // We don't need to change newX and newY as the cat will move as if the wall is not there
                    animalZones[cat.X][cat.Y].occupant = null;
                    cat.X = newX;
                    cat.Y = newY;
                    animalZones[cat.X][cat.Y].occupant = cat;
                    Console.WriteLine($"Cat moves to {newX}, {newY}");
                }
            }
            // Check if the move is to an empty cell
            if (animalZones[newX][newY].occupant == null)
            {
                animalZones[cat.X][cat.Y].occupant = null;
                cat.X = newX;
                cat.Y = newY;
                animalZones[cat.X][cat.Y].occupant = cat;
                Console.WriteLine($"Cat moves to {newX}, {newY}");
            }
            else
            {
                Console.WriteLine("Cat cannot move");
            }
            CheckGameOver();
            Console.WriteLine("Cat moved");
        }


        private void CheckGameOver()
        {
            
            Console.WriteLine($"Checking move to {cat.X},{cat.Y}, {mouse.X},{mouse.Y}");
            if (mouse.Life == 0)
            {
                IsGameOver = true;
                GameMessage = "Game Over! The cat caught the mouse!";
                Console.WriteLine(GameMessage);
            }            
            else if (mouse.X == 0 || mouse.X == Size - 1 || mouse.Y == 0 || mouse.Y == Size - 1)
            {
                IsGameOver = true;
                GameMessage = "Congratulations! You have escaped!";
                Console.WriteLine(GameMessage);
            }
            else
            {
                Console.WriteLine("Game continues");
            }
        }

        //private bool CanCatCatchMouseNextTurn()
        //{
          //  int deltaX = Math.Abs(mouse.X - cat.X);
            //int deltaY = Math.Abs(mouse.Y - cat.Y);
            //return (deltaX <= 2 && deltaY == 0) || (deltaY <= 2 && deltaX == 0);
        //}

        public void Update()
        {
            //foreach (var occupant in occupants)
            //{
              //  occupant.Activate(this);
            //}
        }

        public bool Seek(int x, int y, Direction direction, string target)
        {
            return random.NextDouble() > 0.5;
        }

        public bool Retreat(Occupant occupant, Direction direction)
        {
            return true;
        }

        public void Attack(Occupant occupant, Direction direction)
        {
        }
        public void SetSelectedOccupant(Occupant occupant)
        {
            SelectedOccupant = occupant;
        }
        public void ZoneClick(Zone clickedZone)
        {
            Console.Write("Got animal ");
            Console.WriteLine(clickedZone.emoji == "" ? "none" : clickedZone.emoji);
            Console.WriteLine(clickedZone.emoji);
            Console.Write("Held animal is ");
            Console.WriteLine(holdingPen.emoji == "" ? "none" : holdingPen.emoji);
            if (clickedZone.occupant != null) clickedZone.occupant.ReportLocation();
            if (holdingPen.occupant != null && clickedZone.occupant == null)
            {
                // put animal in zone from holding pen
                Console.WriteLine("Placing " + holdingPen.emoji);
                clickedZone.occupant = holdingPen.occupant;
                clickedZone.occupant.location = clickedZone.location;
                holdingPen.occupant = null;
                Console.WriteLine("Empty spot now holds: " + clickedZone.emoji);
                AddElement(clickedZone.occupant, clickedZone.location.x, clickedZone.location.y); // Use AddElement
            }
            else if (holdingPen.occupant != null && clickedZone.occupant != null)
            {
                Console.WriteLine("Could not place animal.");
                // Don't activate animals since user didn't get to do anything
            }
            else if (holdingPen.occupant == null && clickedZone.occupant == null && SelectedOccupant != null)
            {
                // place the selected occupant
                if (SelectedOccupant is Mouse newMouse)
                {
                    // Find the original mouse
                    var originalMouse = animalZones.SelectMany(row => row)
                                                   .FirstOrDefault(z => z.occupant is Mouse)
                                                   ?.occupant as Mouse;

                    if (originalMouse != null)
                    {
                        originalMouse.IncreaseLife(); // Increase the life of the original mouse
                    }

                    SelectedOccupant = null; // Clear the selected occupant after placing
                }
                else if (SelectedOccupant is Cat newCat)
                {
                    // Find the original cat
                    var originalCat = animalZones.SelectMany(row => row)
                                                 .FirstOrDefault(z => z.occupant is Cat)
                                                 ?.occupant as Cat;

                    if (originalCat != null)
                    {
                        originalCat.IncreaseLife(); // Increase the life of the original cat
                    }

                    SelectedOccupant = null; // Clear the selected occupant after placing
                }
                else
                {
                    clickedZone.occupant = SelectedOccupant;
                    clickedZone.occupant.location = clickedZone.location; 
                    AddElement(clickedZone.occupant, clickedZone.location.x, clickedZone.location.y); 
                    SelectedOccupant = null; // Clear the selected occupant after placing
                }

                ActivateAnimals();
            }
        }

        static public void ActivateAnimals()
        {
            
        }
        public void AddToHolding(string occupantType)
        {
            if (holdingPen.occupant != null) return;
            if (occupantType == "cat") holdingPen.occupant = new Cat("Fluffy");
            if (occupantType == "mouse") holdingPen.occupant = new Mouse("Squeaky");
            if (occupantType == "elephant") holdingPen.occupant = new Owner();
            if (occupantType == "grass") holdingPen.occupant = new Cheese();
            if (occupantType == "boulder") holdingPen.occupant = new Wall();
            if (occupantType == "boulder") holdingPen.occupant = new CatToy();
            Console.WriteLine($"Holding pen occupant at {holdingPen.occupant.location.x},{holdingPen.occupant.location.y}");
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