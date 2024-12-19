using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleArkanoid
{
    class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;

            // Перевірка початкового розміру вікна
            if (Console.WindowWidth < 40 || Console.WindowHeight < 20)
            {
                Console.WriteLine("Розмір вікна консолі замалий. Будь ласка, збільште вікно.");
                return;
            }

            // Ініціалізація гри
            Game game = new Game();
            game.Start();
        }
    }

    class Game
    {
        private Player player;
        private Ball ball;
        private List<Block> blocks;
        private int score;
        private int lives;
        private bool isRunning;
        private int gameSpeed; // Затримка між кадрами (мс)

        private bool isMovingLeft;
        private bool isMovingRight;

        public Game()
        {
            player = new Player(Console.WindowWidth / 2 - 3);
            ball = new Ball(Console.WindowWidth / 2, Console.WindowHeight - 4);
            blocks = new List<Block>();
            score = 0;
            lives = 3;
            isRunning = true;
            gameSpeed = 16; // FPS: ~60 кадрів на секунду

            isMovingLeft = false;
            isMovingRight = false;

            GenerateBlocks();
        }

        private void GenerateBlocks()
        {
            for (int y = 2; y < 6; y++)
            {
                for (int x = 2; x < Console.WindowWidth - 2; x += 6)
                {
                    if (x + 3 < Console.WindowWidth) // Перевірка на розміщення блоку
                    {
                        blocks.Add(new Block(x, y, 1));
                    }
                }
            }
        }

        public void Start()
        {
            while (isRunning)
            {
                // Перевірка розміру вікна
                if (Console.WindowWidth < 40 || Console.WindowHeight < 20)
                {
                    Console.Clear();
                    Console.WriteLine("Розмір вікна консолі замалий. Будь ласка, збільште вікно.");
                    Thread.Sleep(1000);
                    continue;
                }

                if (lives <= 0)
                {
                    GameOver();
                    return;
                }

                HandleInput(); // Обробка введення гравця
                Update();      // Оновлюємо стан гри
                Draw();        // Малюємо елементи гри

                Thread.Sleep(gameSpeed); // Обмеження швидкості гри
            }
        }

        private void Update()
        {
            ball.Move(Console.WindowWidth, Console.WindowHeight);
            CheckCollisions();

            if (ball.PositionY >= Console.WindowHeight - 1)
            {
                lives--;
                ResetBall();
            }

            if (isMovingLeft)
                player.MoveLeft(Console.WindowWidth);
            if (isMovingRight)
                player.MoveRight(Console.WindowWidth);
        }

        private void CheckCollisions()
        {
            // Зіткнення м'яча і платформи
            if (ball.PositionY == Console.WindowHeight - 2 &&
                ball.PositionX >= player.Position && ball.PositionX <= player.Position + 9)
            {
                ball.BounceVertical();
            }

            // Додаткова перевірка пропущеного зіткнення
            if (ball.PositionY + ball.DirectionY == Console.WindowHeight - 2 &&
                ball.PositionX >= player.Position && ball.PositionX <= player.Position + 9)
            {
                ball.BounceVertical();
            }

            // Зіткнення м'яча з блоками
            foreach (var block in blocks.ToArray())
            {
                if (block.IsHit(ball.PositionX, ball.PositionY))
                {
                    ball.BounceVertical();
                    blocks.Remove(block);
                    score += 10;
                }
            }
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(new string(' ', Console.WindowWidth * Console.WindowHeight));

            // Оновлення платформи
            player.Draw();

            // Оновлення м'яча
            ball.Draw(Console.WindowWidth, Console.WindowHeight);

            // Оновлення блоків
            foreach (var block in blocks)
            {
                block.Draw(Console.WindowWidth, Console.WindowHeight);
            }

            // Вивід рахунку та життя
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Score: {score}   Lives: {lives}");
        }

        private void HandleInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.LeftArrow)
                {
                    isMovingLeft = true;
                    isMovingRight = false; // Зупиняємо рух вправо
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    isMovingRight = true;
                    isMovingLeft = false; // Зупиняємо рух вліво
                }
                else if (key == ConsoleKey.P)
                    Pause();
                else if (key == ConsoleKey.Escape)
                    isRunning = false;
                else if (key == ConsoleKey.Enter)
                    Restart();
            }
            else
            {
                isMovingLeft = false;
                isMovingRight = false;
            }
        }

        private void Pause()
        {
            Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2);
            Console.Write("PAUSED. Press any key...");
            Console.ReadKey(true);
        }

        private void Restart()
        {
            score = 0;
            lives = 3;
            blocks.Clear();
            GenerateBlocks();
            ResetBall();
        }

        private void ResetBall()
        {
            ball.Reset(Console.WindowWidth / 2, Console.WindowHeight - 4);
        }

        private void GameOver()
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2);
            Console.WriteLine("GAME OVER!!! Press ENTER to restart or ESCAPE to exit.");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    Restart();
                    Start();
                    return;
                }
                else if (key == ConsoleKey.Escape)
                {
                    isRunning = false;
                    return;
                }
            }
        }
    }

    class Player
    {
        public int Position { get; private set; }
        private int moveSpeed; // Швидкість руху платформи

        public Player(int initialPosition)
        {
            Position = initialPosition;
            moveSpeed = 8; // Кількість позицій за один рух
        }

        public void MoveLeft(int windowWidth)
        {
            Position = Math.Max(0, Position - moveSpeed); 
        }

        public void MoveRight(int windowWidth)
        {
            Position = Math.Min(windowWidth - 10, Position + moveSpeed); 
        }

        public void Draw()
        {
            Console.SetCursorPosition(Position, Console.WindowHeight - 2);
            Console.Write("==========");
        }
    }

    class Ball
    {
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        private int directionX = 1;
        private int directionY = -1;

        public int DirectionX => directionX;
        public int DirectionY => directionY;

        public Ball(int startX, int startY)
        {
            PositionX = startX;
            PositionY = startY;
        }

        public void Move(int windowWidth, int windowHeight)
        {
            PositionX += directionX;
            PositionY += directionY;

            if (PositionX <= 0 || PositionX >= windowWidth - 1)
                BounceHorizontal();

            if (PositionY <= 1)
                BounceVertical();
        }

        public void BounceHorizontal()
        {
            directionX = -directionX;
        }

        public void BounceVertical()
        {
            directionY = -directionY;
        }

        public void Reset(int startX, int startY)
        {
            PositionX = startX;
            PositionY = startY;
            directionX = 1;
            directionY = -1;
        }

        public void Draw(int windowWidth, int windowHeight)
        {
            Console.SetCursorPosition(PositionX, PositionY);
            Console.Write("O");
        }
    }

    class Block
    {
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int HitsToBreak { get; private set; }

        public Block(int x, int y, int hits)
        {
            PositionX = x;
            PositionY = y;
            HitsToBreak = hits;
        }

        public bool IsHit(int ballX, int ballY)
        {
            if (ballX == PositionX && ballY == PositionY)
            {
                HitsToBreak--;
                return HitsToBreak <= 0;
            }
            return false;
        }

        public void Draw(int windowWidth, int windowHeight)
        {
            Console.SetCursorPosition(PositionX, PositionY);
            Console.Write("[]");
        }
    }
}
