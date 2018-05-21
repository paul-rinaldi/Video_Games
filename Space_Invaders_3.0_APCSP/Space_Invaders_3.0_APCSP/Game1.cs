using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Space_Invaders_3._0_APCSP
{
    public class Enemy
    {
        public Texture2D texture;
        public Rectangle rect;
        public Vector2 velocity;
        public Vector2 position;
        public bool isVisible;

        public Enemy(Texture2D newTexture, Vector2 newPosition, bool newIsVisible)
        {
            texture = newTexture;
            position = newPosition;
            isVisible = newIsVisible;
        }

        public void Update()
        {
            position += velocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }

    public class Missile
    {
        public Texture2D texture;
        public Rectangle rect;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 originMissile;

        public bool isVisible;

        public Missile(Texture2D newTexture)
        {
            texture = newTexture;
            isVisible = false;
        }

        public void Update()
        {
            position += velocity;
            rect = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, originMissile, 1f, SpriteEffects.None, 0);
        }
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        bool playAgain = false; // boolean used to know when to reset variables

        Vector2 newEnemyVect; // Vector for storing randomly generated X and Y values for enemy spawning

        SoundEffect missileSound; // sound effect for shooting
        Song SISong; // song for the entire game
        int songCount = 0; // counter to detect when to restart the song

        SpriteFont Arial; // spritefont to hold our font for displaying statistics

        // Statistics
        int scoreCount = 0; 
        int shotsTaken = 0;
        int enemiesKilled = 0; 
        int newHighscore = 0; 
        int oldHighscore = 0;
        double accuracy = 0.0;
        int Bonus = 0; 

        float enemySpeed = 1f;

        // Vectors for displaying statistics on game over screen
        Vector2 highscoreVect;
        Vector2 scoreVect;
        Vector2 accuracyVect; 
        Vector2 bonusVect; 

        Vector2 gameScoreVect; 

        int currentScreen;

        // Bools for detecting a presses and releases
        bool enterPress;
        bool enterRelease;
        bool enterClick;
        bool spacePress;
        bool spaceRelease;
        bool spaceClick;

        // enemy spawn location 
        int randomRegion = 0;
        int randomLocation = 0;

        // Screen Rectangle and Texture
        Rectangle screenRect;
        Texture2D screenTexture;

        // Textures for different screens
        Texture2D titleScreenTexture;
        Texture2D instructionScreenTexture;
        Texture2D gameplayScreenTexture;
        Texture2D gameOverScreenTexture;

        // Texture for enemies
        Texture2D enemyTexture;

        // Maximum amount of enemies to be shown at a time.
        const int maxEnemies = 15;

        // Constants for enemy height and width
        const int eWidth = 70;
        const int eHeight = 70;

        // Amount in seconds to wait between the creation of enemies.
        const float enemySpawnTimer = 1.5f;

        // Elapsed time since the last creation of an enemy.
        double elapsedTime = 0;

        // Maximum amount of enemies to be shown at a time.
        const int maxMissiles = 15;

        // Constants for enemy height and width
        const int mWidth = 70;
        const int mHeight = 70;

        // Amount in seconds to wait between the creation of enemies.
        const float missileShotTimer = 1.5f;

        // Objects for all missiles
        List<Rectangle> enemyRects = new List<Rectangle>();

        // Rectangles for all enemies
        List<Missile> missiles = new List<Missile>();

        // Objects for all enemies
        List<Enemy> enemies = new List<Enemy>();

        // Texture and Rectangle for player one's ship
        Texture2D playerOneSpriteTexture;
        Rectangle playerOneRectangle;

        // The center of the image
        Vector2 spriteOrigin;

        // x and y coordinates for each player's ship
        Vector2 playerOnePosition;

        // used for the value of the roation... arc length
        float rotation;

        Vector2 spriteVelocity; // velocity
        const float tangetentialVelocity = 5f; // velocity of a straight line
        float friction = .05f; // float for the friction of the user's ship 

        Random rand = new Random();

        Vector2 position;
        Vector2 velocity;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            gameScoreVect = new Vector2(10, 10);
            highscoreVect = new Vector2(100, 300);
            scoreVect = new Vector2(100, highscoreVect.Y + 100);
            accuracyVect = new Vector2(100, highscoreVect.Y + 200);
            bonusVect = new Vector2(100, highscoreVect.Y + 300);

            velocity = Vector2.Zero; // velocity is set to (0,0) coordinates aka not moving
            position = new Vector2(437, 309); // starting location of ship

            currentScreen = 1; // initialized to 1 for first Screen
            screenRect = new Rectangle(0, 0, 1024, 768);

            playerOnePosition = new Vector2(437, 309); // starting location of ship

            enterPress = false;
            enterRelease = false;
            spacePress = false;
            spaceRelease = false;

            enterClick = false;
            spaceClick = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SISong = this.Content.Load<Song>("SI3.0Song");
            missileSound = this.Content.Load<SoundEffect>("shootlaser");
            Arial = this.Content.Load<SpriteFont>("Arial");
            instructionScreenTexture = this.Content.Load<Texture2D>("instructions");
            playerOneSpriteTexture = this.Content.Load<Texture2D>("spaceship2correct");
            enemyTexture = this.Content.Load<Texture2D>("SpaceInvaderEnemy3.0");
            screenTexture = this.Content.Load<Texture2D>("finalprojtitle");

            titleScreenTexture = this.Content.Load<Texture2D>("finalprojtitle");
            gameplayScreenTexture = this.Content.Load<Texture2D>("gameplayScreen");
            gameOverScreenTexture = this.Content.Load<Texture2D>("gameOverScreen");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Saves values from the keyboard into KeyboardState object called "key"
            KeyboardState key = Keyboard.GetState();

            if (key.IsKeyDown(Keys.Escape))
                Exit();


            // checks to see if it is the first time songCount is to be played
            //   (songCount = 0) or its time to replay the song (songCount >= 4260 aka 71 seconds)
            if (songCount >= 4260 || songCount == 0)                        
            {
                songCount = 0; // resets the song counter
                MediaPlayer.Play(SISong); // Plays songs
            }

            songCount++; // increments the song counter while the song is playing

            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds; // Calculates the time (in seconds) between the last update call and the current call
            
            enterClick = false; 
            enterClick = keyClickListener(key, Keys.Enter, enterClick, enterPress, enterRelease);
            
            spaceClick = false; 
            spaceClick = keyClickListener(key, Keys.Space, spaceClick, spacePress, spaceRelease);

            // Separation between each screen's code for updating
            if (currentScreen == 1) // titlescreen
            {
                // Resetting Variables and Objects
                if (playAgain)
                {
                    Reset();
                }
                
                // if user presses enter, advances to next screen
                if (enterClick) 
                    currentScreen = 2;
            }
            else if (currentScreen == 2) // instruction screen
            {
                if (enterClick) 
                    currentScreen = 3;
            }
            else if (currentScreen == 3) // gameplay screen
            {
                // Player
                UpdatePlayer(key);
                keepPlayerInBounds();
                
                // Spawns enemies 2 seconds apart
                if (elapsedTime >= enemySpawnTimer && enemies.Count < maxEnemies) // Makes sure 1.5 seconds have passed and the amount of enemies is less than 15
                {
                    generateEnemy();
                    elapsedTime = 0;
                }
                
                // Shoots a Missile upon Spacebar click
                if (spaceClick) 
                    Shoot();

                // Checks for interactions between enemies, missiles, and player
                Updaters();

                if (checkIfGameOver())
                    currentScreen = 4; 
            }
            else if (currentScreen == 4) // game over screen
            {
                if (enterClick) // If the user chooses to play again by pressing enter
                    currentScreen = 1; // goes to screen 1

                playAgain = true; // resets boolean flag of playing again to true
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Separation between each screen's code for drawing
            if (currentScreen == 1) // titlescreen
            {
                spriteBatch.Draw(titleScreenTexture, screenRect, Color.White); // Displays background
            }
            else if (currentScreen == 2) // instruction screen
            {
                spriteBatch.Draw(instructionScreenTexture, screenRect, Color.White);
            }
            else if (currentScreen == 3) // gameplay screen
            {
                spriteBatch.Draw(gameplayScreenTexture, screenRect, Color.White); // Displays background
                spriteBatch.Draw(playerOneSpriteTexture, playerOnePosition, null, Color.White, rotation, spriteOrigin, 1f, SpriteEffects.None, 0); // Displays player one ship with rotation

                // Draws enemies and missiles
                foreach (Enemy enemy in enemies)
                {
                    if (enemy.isVisible)
                        enemy.Draw(spriteBatch);
                }
                foreach (Missile mis in missiles)
                {
                    if (mis.isVisible)
                        mis.Draw(spriteBatch);
                }

                spriteBatch.DrawString(Arial, "Score: " + scoreCount, gameScoreVect, Color.WhiteSmoke); // Displays the current score
            }
            else if (currentScreen == 4) // game over screen
            {
                spriteBatch.Draw(gameOverScreenTexture, screenRect, Color.White); // Displays background

                // Displays statistics
                spriteBatch.DrawString(Arial, "Highscore: " + newHighscore, highscoreVect, Color.Gold);
                spriteBatch.DrawString(Arial, "Score: " + scoreCount, scoreVect, Color.Gold);
                spriteBatch.DrawString(Arial, "Accuracy: " + accuracy + "%", accuracyVect, Color.Gold);
                spriteBatch.DrawString(Arial, "Bonus: " + Bonus, bonusVect, Color.Gold);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void UpdatePlayer(KeyboardState key)
        {
            // Player rectangle, velocity, and origin points assigned
            playerOneRectangle = new Rectangle((int)playerOnePosition.X, (int)playerOnePosition.Y, // because vector x and y are floats, we have to cast as ints for rectangle properties
                playerOneSpriteTexture.Width, playerOneSpriteTexture.Height);

            playerOnePosition += spriteVelocity;
            
            spriteOrigin = new Vector2(playerOneRectangle.Width / 2, playerOneRectangle.Height / 2);
            
            // Rotates left and right (counterclockwise or clockwise)
            if (key.IsKeyDown(Keys.Right))
                rotation += 0.1f;
            if (key.IsKeyDown(Keys.Left))
                rotation -= 0.1f;

            // moves the Player One ship forwards
            if (key.IsKeyDown(Keys.Up))
            {
                // The cosine retrieves the x coordinate and sin retrieves the y coordinate, the multiplication of the tangential velocity is 
                //  for the velocity heading in a straight line off (tangent to) the circle
                spriteVelocity.X = (float)Math.Cos(rotation) * tangetentialVelocity;
                spriteVelocity.Y = (float)Math.Sin(rotation) * tangetentialVelocity;
            }
            else if (spriteVelocity != Vector2.Zero) // if the Player One sprite is moving and the up button isn't pressed 
            {
                // Slows the ship down
                float i = spriteVelocity.X;
                float j = spriteVelocity.Y;
                spriteVelocity.X = i -= friction * i;
                spriteVelocity.Y = j -= friction * j;
            }
        }
        
        void UpdateEnemies()
        {
            // Updates all enemies rectangles with their position constantly
            foreach (Enemy enem in enemies)
                enem.rect = new Rectangle((int)enem.position.X, (int)enem.position.Y, enemyTexture.Width, enemyTexture.Height); // Updates Rectangle of the enemy

            // Simple AI of enemies 
            foreach (Enemy enem in enemies) // loops through all enemies
            {
                if (enem.position.X < playerOnePosition.X) // If enemy is to the Left of the player, moves toward player
                    enem.position += new Vector2(enemySpeed, 0);

                if (enem.position.X > playerOnePosition.X) // If enemy is to the Right of the player, moves toward player
                    enem.position -= new Vector2(enemySpeed, 0);

                if (enem.position.Y < playerOnePosition.Y) // If enemy is above the player, moves toward player
                    enem.position += new Vector2(0, enemySpeed);

                if (enem.position.Y > playerOnePosition.Y) // If enemy is below the player, moves toward player
                    enem.position -= new Vector2(0, enemySpeed);
            }
            // If enemy Y and X are equal to player Y and X; enemy shouldn't move
        }

        public void generateEnemy()
        {
            // Enemy new coordinates and such generation
            // at 1.5 second intervals,  generates another enemy...with a random location

            // Randomly determines which region and location for spawn of each enemy,
            // there are 4 possible regions for spawning and for each random region there are 3 possible locations
            randomRegion = rand.Next(1, 5); // 1 through 4
            randomLocation = rand.Next(1, 4); // 1 through 3, 

            // decides the coordinates to generate the enemy based on the random integers above
            switch (randomRegion) // Takes in the random region random integer which is either 1, 2, 3, or 4
            {
                case 1:
                    switch (randomLocation) // Each switch takes in the random location random integer which is either 1, 2, or 3; Top Side
                    {
                        case 1:
                            newEnemyVect.X = 1024 / 2 - 140;
                            newEnemyVect.Y = -70;
                            break;
                        case 2:
                            newEnemyVect.X = 1024 / 2;
                            newEnemyVect.Y = -70;
                            break;
                        case 3:
                            newEnemyVect.X = 1024 / 2 + 140;
                            newEnemyVect.Y = -70;
                            break;
                    }
                    break;

                case 2:
                    switch (randomLocation) // Right Side
                    {
                        case 1:
                            newEnemyVect.X = 1024;
                            newEnemyVect.Y = 768 / 2 - 140;
                            break;
                        case 2:
                            newEnemyVect.X = 1024;
                            newEnemyVect.Y = 768 / 2;
                            break;
                        case 3:
                            newEnemyVect.X = 1024;
                            newEnemyVect.Y = 768 / 2 + 140;
                            break;
                    }
                    break;

                case 3:
                    switch (randomLocation) // Bottom Side
                    {
                        case 1:
                            newEnemyVect.X = 1024 / 2 - 140;
                            newEnemyVect.Y = 768;
                            break;
                        case 2:
                            newEnemyVect.X = 1024 / 2;
                            newEnemyVect.Y = 768;
                            break;
                        case 3:
                            newEnemyVect.X = 1024 / 2 + 140;
                            newEnemyVect.Y = 768;
                            break;
                    }
                    break;

                case 4:
                    switch (randomLocation) // Left Side
                    {
                        case 1:
                            newEnemyVect.X = -70;
                            newEnemyVect.Y = 768 / 2 - 140;
                            break;
                        case 2:
                            newEnemyVect.X = -70;
                            newEnemyVect.Y = 768 / 2;
                            break;
                        case 3:
                            newEnemyVect.X = -70;
                            newEnemyVect.Y = 768 / 2 + 140;
                            break;
                    }
                    break;
            }

            // Adds a new enemy with certain random spawn locations determine by the code above
            enemies.Add(new Enemy(enemyTexture, new Vector2(newEnemyVect.X, newEnemyVect.Y), true)); // position of enemies stays at (0,0)!!
        }

        public void UpdateEnemAndMissCollisions()
        {
            foreach (Enemy enem in enemies)
                foreach (Missile mis in missiles)
                    if (enem.rect.Intersects(mis.rect)) // if missile collides with enemy
                    {
                        enem.isVisible = false; // Sets certain enemy to not visible (to be removed)
                        mis.isVisible = false; // Sets certain missile to not visible (to be removed)

                        // Increments the amount of enemies killed and the score
                        enemiesKilled++;
                        scoreCount += 10;
                    }
        }
        
        public void UpdateMissiles()
        {
            foreach (Missile missile in missiles)
            {
                // Updates position of each missile, its position and its corresponding rectangle
                missile.position += missile.velocity;
                missile.rect = new Rectangle((int)missile.position.X, (int)missile.position.Y, missile.texture.Width, missile.texture.Height);
                if (Vector2.Distance(missile.position, playerOnePosition) > 400)
                    missile.isVisible = false;
            }
        }
        
        public void Shoot()
        {
            Missile newMissile = new Missile(Content.Load<Texture2D>("Blip"));

            // gets angle of ship, so bullet goes faster (* 5) than ship and cant hit each other, even when moving ship
            newMissile.velocity = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)) * 5f + spriteVelocity;
            newMissile.position = playerOnePosition + newMissile.velocity * 5; // making position of bullet infront of ship
            newMissile.isVisible = true;

            if ((missiles.Count) < 20) // 20 missiles at a time, tops
            {
                missiles.Add(newMissile); 
                shotsTaken++; 
                missileSound.Play(); 
            }
        }
        
        public void UpdateKillEnemAndMiss()
        {
            for (int i = 0; i < enemies.Count; i++) // loops through the list of enemy objects
            {
                if (!enemies[i].isVisible) // if any of them are not visible (aka shot by missile)
                {
                    enemies.RemoveAt(i); 
                    i--; 
                }
            }
            for (int i = 0; i < missiles.Count; i++)
            {
                if (!missiles[i].isVisible) // if the bulet isn't visible
                {
                    missiles.RemoveAt(i); //  removes the bullet 
                    i--; // makes sure we dont have infinite bullets on screen
                }
            }
        }

        public void keepPlayerInBounds()
        {
            // Detects if the user is attempting to go past the screen boundaries and bounces them backwards
            if (playerOnePosition.X < 0 || playerOnePosition.X > 1024 || playerOnePosition.Y < 0 || playerOnePosition.Y > 768)
                spriteVelocity = -spriteVelocity;

            // Detects if the user has glitched past borders, puts them inside the border the distance traveled past the border
            if (playerOnePosition.X < -10)
                playerOnePosition = new Vector2(-playerOnePosition.X, playerOnePosition.Y);
            if (playerOnePosition.X > 1034)
                playerOnePosition = new Vector2(2048 - playerOnePosition.X, playerOnePosition.Y);
            if (playerOnePosition.Y < -10)
                playerOnePosition = new Vector2(playerOnePosition.X, -playerOnePosition.Y);
            if (playerOnePosition.Y > 778)
                playerOnePosition = new Vector2(playerOnePosition.X, 1536 - playerOnePosition.Y);
        }

        public bool keyClickListener(KeyboardState k, Keys specificKey, bool clicked, bool press, bool release)
        {
            if (k.IsKeyDown(specificKey))
            {
                press = true;
            }
            if (press && k.IsKeyDown(specificKey) == false)
            {
                release = true;
            }
            if (release && press)
            {
                press = false;
                release = false;
                clicked = true;
            }

            return clicked;
        }

        public bool checkIfGameOver()
        {
            // If player intersects an enemy, game is over; statistics are calculated and screen advances
            foreach (Enemy enemy in enemies)
                if (playerOneRectangle.Intersects(enemy.rect)) // Checks for intersection
                {
                    if (shotsTaken != 0) 
                    {
                        accuracy = (double)(Math.Round(((decimal)enemiesKilled / shotsTaken * 100), 2, MidpointRounding.ToEven));

                        // Secret bonus points if the user has greater than 90% accuracy
                        if (accuracy > 90.0)
                        {
                            Bonus = scoreCount / 2; 
                            scoreCount += Bonus;
                        }
                    }
                    else
                    {
                        accuracy = 0.00; // sets accuracy to zero
                    }

                    // Updates the newest highscore
                    if (oldHighscore > scoreCount) 
                        newHighscore = oldHighscore;
                    else 
                        newHighscore = scoreCount;

                    // game is over, advances to next screen
                    return true;
                }
                else
                    return false;
            return false;
        }

        public void Updaters()
        {
            UpdateEnemies();

            UpdateMissiles();

            UpdateKillEnemAndMiss();

            UpdateEnemAndMissCollisions();
        }

        public void Reset()
        {
            // Going backwards through the enemyRect and missiles list, removes all Rectangles and Missiles
            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies.RemoveAt(i);
            for (int i = missiles.Count - 1; i >= 0; i--)
                missiles.RemoveAt(i);

            // Resets player position
            playerOnePosition = new Vector2(437, 309);

            // Resets statistics
            oldHighscore = newHighscore;
            songCount = 0;
            scoreCount = 0;
            enemiesKilled = 0;
            shotsTaken = 0;
            accuracy = 0.0;
            Bonus = 0;

            playAgain = false;
        }
    }
}
