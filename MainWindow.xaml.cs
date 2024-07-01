using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;





namespace PacmanGame
{
    public partial class MainWindow : Window
    {
        DispatcherTimer GameTimer = new DispatcherTimer();
        bool goLeft, goRight, goDown, goUp;
        bool noLeft, noRight, noDown, noUp;
        int pacmanSpeed = 7;
        Rect pacmanHitBox;
        int ghostSpeed = 10;
        int ghostMoveStep = 160;
        int CurrentGhostStep;
        int score = 0;

        MediaPlayer gameoverSound;
        MediaPlayer chompSound;
        MediaPlayer deathSound;
        

        public MainWindow()
        {
            InitializeComponent();
            GameSetup();
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left && !noLeft)
            {
                SetDirection(ref goLeft, new RotateTransform(-180, Pacman.Width / 2, Pacman.Height / 2));
            }
            if (e.Key == Key.Right && !noRight)
            {
                SetDirection(ref goRight, new RotateTransform(0, Pacman.Width / 2, Pacman.Height / 2));
            }
            if (e.Key == Key.Up && !noUp)
            {
                SetDirection(ref goUp, new RotateTransform(-90, Pacman.Width / 2, Pacman.Height / 2));
            }
            if (e.Key == Key.Down && !noDown)
            {
                SetDirection(ref goDown, new RotateTransform(90, Pacman.Width / 2, Pacman.Height / 2));
            }
        }

        private void SetDirection(ref bool direction, RotateTransform transform)
        {
            goLeft = goRight = goDown = goUp = false;
            noLeft = noRight = noDown = noUp = false;

            direction = true;
            Pacman.RenderTransform = transform;
        }

        private void GameSetup()
        {
            MyCanvas.Focus();
            GameTimer.Tick += GameLoop;
            GameTimer.Interval = TimeSpan.FromMilliseconds(30);
            GameTimer.Start();
            CurrentGhostStep = ghostMoveStep;

            LoadImage(Pacman, "C:\\Projelerim\\PacmanGame\\Images\\pacman.png");
            LoadImage(GreenBoy, "C:\\Projelerim\\PacmanGame\\Images\\green ghost.jpg");
            LoadImage(OrangeBoy, "C:\\Projelerim\\PacmanGame\\Images\\orange ghost.jpg");
            LoadImage(WhiteBoy, "C:\\Projelerim\\PacmanGame\\Images\\white ghost.jpg");
            chompSound = new MediaPlayer();
            chompSound.Open(new Uri("C:\\Projelerim\\PacmanGame\\sounds\\eatcoin.mp3", UriKind.Absolute));

            deathSound = new MediaPlayer();
            deathSound.Open(new Uri("C:\\Projelerim\\PacmanGame\\sounds\\death.mp3", UriKind.Absolute));

            gameoverSound = new MediaPlayer();
            gameoverSound.Open(new Uri("C:\\Projelerim\\PacmanGame\\sounds\\gameover.mp3", UriKind.Absolute));

        }

        private void LoadImage(Rectangle rect, string path)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(path));
            rect.Fill = brush;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            TxtScore.Content = "Score: " + score;

            MovePacman();

            CheckBoundaries();

            pacmanHitBox = new Rect(Canvas.GetLeft(Pacman), Canvas.GetTop(Pacman), Pacman.Width, Pacman.Height);

            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                if ((string)x.Tag == "Wall") HandleWallCollision(x, hitBox);
                if ((string)x.Tag == "Coin") HandleCoinCollision(x, hitBox);
                if ((string)x.Tag == "ghost") HandleGhostCollision(x, hitBox);
            }

            if (score == 131) GameOver("You Win!!");
        }

        private void MovePacman()
        {
            if (goRight) Canvas.SetLeft(Pacman, Canvas.GetLeft(Pacman) + pacmanSpeed);
            if (goLeft) Canvas.SetLeft(Pacman, Canvas.GetLeft(Pacman) - pacmanSpeed);
            if (goDown) Canvas.SetTop(Pacman, Canvas.GetTop(Pacman) + pacmanSpeed);
            if (goUp) Canvas.SetTop(Pacman, Canvas.GetTop(Pacman) - pacmanSpeed);
        }

        private void CheckBoundaries()
        {
            if (goDown && Canvas.GetTop(Pacman) + 80 > Application.Current.MainWindow.Height)
            {
                noDown = true;
                goDown = false;
            }
            if (goUp && Canvas.GetTop(Pacman) < 1)
            {
                noUp = true;
                goUp = false;
            }
            if (goLeft && Canvas.GetLeft(Pacman) < 1)
            {
                noLeft = true;
                goLeft = false;
            }
            if (goRight && Canvas.GetLeft(Pacman) + 80 > Application.Current.MainWindow.Width)
            {
                noRight = true;
                goRight = false;
            }
        }

        private void HandleWallCollision(Rectangle wall, Rect hitBox)
        {
            if (goLeft && pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetLeft(Pacman, Canvas.GetLeft(Pacman) + 10);
                noLeft = true;
                goLeft = false;
            }
            if (goRight && pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetLeft(Pacman, Canvas.GetLeft(Pacman) - 10);
                noRight = true;
                goRight = false;
            }
            if (goDown && pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetTop(Pacman, Canvas.GetTop(Pacman) - 10);
                noDown = true;
                goDown = false;
            }
            if (goUp && pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetTop(Pacman, Canvas.GetTop(Pacman) + 10);
                noUp = true;
                goUp = false;
            }
        }

        private void HandleCoinCollision(Rectangle coin, Rect hitBox)
        {
            if (pacmanHitBox.IntersectsWith(hitBox) && coin.Visibility == Visibility.Visible)
            {
                coin.Visibility = Visibility.Hidden;
                score++;
                chompSound.Play();
            }
        }

        private void HandleGhostCollision(Rectangle ghost, Rect hitBox)
        {
            if (pacmanHitBox.IntersectsWith(hitBox))
            {
                deathSound.Play();
                GameOver("Casper got you. Click OK to play again.");
            }

            if (ghost.Name.ToString() == "OrangeBoy")
            {
                Canvas.SetLeft(ghost, Canvas.GetLeft(ghost) - ghostSpeed);
            }
            else
            {
                Canvas.SetLeft(ghost, Canvas.GetLeft(ghost) + ghostSpeed);
            }

            CurrentGhostStep--;

            if (CurrentGhostStep < 1)
            {
                CurrentGhostStep = ghostMoveStep;
                ghostSpeed = -ghostSpeed;
            }
        }

        private void GameOver(string message)
        {
            GameTimer.Stop();
            gameoverSound.Play();
            MessageBox.Show(message, "PacmanGame WPF");

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}




