
/*
 * John Wright June 2021.
 * All credit to Zachery Patten for the original code:
 * https://github.com/ZacharyPatten/dotnet-console-games/tree/master/Projects/Snake
 * This program runs a snake game in the console window.
 * Let's try and change the code to improve the game:

DONE:
- stop the food from spawning on the borders - limit its spawn area to the center DONE
- slow the game down if the snake is moving up or down. for some reason this is faster than left or right... :/ DONE
- Create a visual border around the game DONE
- Create unique colours for the different game elements. DONE
- Set the top line of the game to have a scoreboard DONE
- change foodPosition to generate a random amount of food on the board
- Create a unique character to be the head of the snake (this ones giving me trouble but WORKS)(DONE)
- add an AI snake that moves in random directions for random amounts of time - a competitor (DONE)
- stop the ai hitting itself ... DONE!
- create random border blocks to act as obstacles for a more interesting game map DONE
- make the ai snake chase you down or aim for food (WIP - snake prioritises food over empty tiles

TODO:
--> create more types of food - superfood that adds two sections and poison apples that remove a section
--> add a secret button that grows the snake by one cell
--> implement a 'lives' ability that gives you another chance when you hit a wall
--> implement game in OOP, create snake class to add multiple snakes
--> think of more changes !


* Updates: June 20th - 
* improved ai snake code - it wont run into walls or itself, and it prioritises food tiles.
* general code compression
* Update June 21st - AI detects the closest piece of food (or near enough to it)
 */

/// ////////////////////////////////////////////GAME CODE:////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;


char[] DirectionChars = { '^', 'v', '<', '>', };
	TimeSpan sleep = TimeSpan.FromMilliseconds(150);
	int width = Console.WindowWidth;
	int height = Console.WindowHeight;
	Random random = new();
	Tile[,] map = new Tile[width, height];
	Direction? direction = null;
	Direction? aiDirection = null;

	Queue<(int X, int Y)> snake = new();
	Queue<(int A, int B)> ai = new();

	List<(int p, int q)> foodList = new();

	(int X, int Y) = (width / 2, height / 2); //represent player position
(int A, int B) = (0, 0); //represent ai position

bool closeRequested = false;

	(int E, int F) = (0, 0); //represents the closest food position

double distanceToFood = 1000;

try
{
	Console.CursorVisible = false; //set up:
	Console.Clear();
	drawBorder();
	addTerrain();
	displayScore(X, Y);
	spawnEnemy();
	PositionFood(125);
	snake.Enqueue((X, Y));
	map[X, Y] = Tile.Snake;
	Console.SetCursorPosition(X, Y);
	Console.ForegroundColor = ConsoleColor.DarkYellow;
	Console.Write('@');
	Console.ResetColor();


	//set the first item of food to be the closest one


	while (!direction.HasValue && !closeRequested) //wait for player input to start
	{
		GetDirection();
	GetAiDirection();
}

//MAIN LOOP - while no close requested
while (!closeRequested)
{
	switch (direction) //new player snake position depends on result from GetDirection
	{
		case Direction.Up: Y--; break;
		case Direction.Down: Y++; break;
		case Direction.Left: X--; break;
		case Direction.Right: X++; break;
	}

	switch (aiDirection) //new player snake position depends on result from GetDirection
	{
		case Direction.Up: B--; break;
		case Direction.Down: B++; break;
		case Direction.Left: A--; break;
		case Direction.Right: A++; break;
	}

	if (map[X, Y] is Tile.Snake || map[X, Y] is Tile.Border) //if player snake or wall
	{
		Console.Clear();
		Console.Write("Game Over. Score: " + (snake.Count - 1) + ".");
		displayScore(0, 0);
		return;
	}
	else
	{
		moveSnake(snake, X, Y); //player is moved
	}


	moveSnake(ai, A, B); //ai is moved
	findClosestFood();
	displayScore(X, Y);

	if (Console.KeyAvailable) //if player pressed a key
	{
		GetDirection();
	}

	GetAiDirection(); //get new ai direction
	System.Threading.Thread.Sleep(sleep); //pause (affects game speed)
}
Console.Clear();
}
finally
{
	Console.CursorVisible = true;
}

void findClosestFood()
{
	double shortest = 100000;

	foreach ((int p, int q) in foodList) //iterate the foodlist
	{
		double checkdub = getDistance(p, q, A, B); //get the distance of each thing

		if (checkdub < shortest) //if that distance is less than the shortest current distance
		{
			shortest = checkdub;
			(E, F) = (p, q);
		}
	}
	Console.SetCursorPosition(E, F);
	Console.Write('!');
}



double getDistance(int x, int y, int x1, int y1)
{
	//the distance between two grid points is the hypotenuse
	double x2 = (x - x1) * (x - x1); //i.e(x-x1)^2
	double y2 = (y - y1) * (y - y1);

	double numToRoot = x2 + y2;

	double distance = Math.Sqrt(numToRoot);
	return distance;
}

void GetDirection() //method for getting the direction from player
{
	switch (Console.ReadKey(true).Key)  //get the inuput key from user
	{
		case ConsoleKey.UpArrow: direction = Direction.Up; break;
		case ConsoleKey.DownArrow: direction = Direction.Down; break;
		case ConsoleKey.LeftArrow: direction = Direction.Left; break;
		case ConsoleKey.RightArrow: direction = Direction.Right; break;
		case ConsoleKey.Escape: closeRequested = true; break;
	}
}

void GetAiDirection() //figure out ai danger noodle next best direction
{
	List<String> openList = new(); //list of possible directions to pick from
	List<String> foodList = new(); //list of directions where there is food.

	if (map[A - 1, B] is Tile.Open || map[A - 1, B] is Tile.Food)  //check tile on left. 
	{
		if (map[A - 1, B] is Tile.Food) //if food, add to food list
		{
			foodList.Add("Food_Left");
		}
		openList.Add("Left"); //otherwise if open, add to directions.
	}
	if (map[A + 1, B] is Tile.Open || map[A + 1, B] is Tile.Food) //check the tile on the right. 
	{
		if (map[A + 1, B] is Tile.Food)
		{
			foodList.Add("Food_Right");
		}
		openList.Add("Right");
	}
	if (map[A, B - 1] is Tile.Open || map[A, B - 1] is Tile.Food) //check the tile above. 
	{
		if (map[A, B - 1] is Tile.Food)
		{
			foodList.Add("Food_Up");
		}
		openList.Add("Up");
	}
	if (map[A, B + 1] is Tile.Open || map[A, B + 1] is Tile.Food)   //check the tile below.
	{
		if (map[A, B + 1] is Tile.Food)
		{
			foodList.Add("Food_Down");
		}
		openList.Add("Down");
	}

	if (foodList.Count > 0) //randomly pick a direction from the possible food sources first
	{
		int iFood = random.Next(foodList.Count); //rng
		string foodDir = foodList[iFood];
		switch (foodDir)
		{
			case "Food_Up": aiDirection = Direction.Up; break;
			case "Food_Down": aiDirection = Direction.Down; break;
			case "Food_Left": aiDirection = Direction.Left; break;
			case "Food_Right": aiDirection = Direction.Right; break;
		}
	}
	else if (openList.Count > 0) //if no food sources randomly pick from list of open tiles.
	{
		int iOpen = random.Next(openList.Count); //rng
		string openDir = openList[iOpen];
		switch (openDir)
		{
			case "Up": aiDirection = Direction.Up; break;
			case "Down": aiDirection = Direction.Down; break;
			case "Left": aiDirection = Direction.Left; break;
			case "Right": aiDirection = Direction.Right; break;
		}
	}
	else //no possible moves (ai has trapped itself)
	{
		if (ai.Count >= 1)
		{
			(int a, int b) = ai.Dequeue(); //dequeue and overwrite with blank space.
			map[a, b] = Tile.Open;
			Console.SetCursorPosition(a, b);
			Console.Write(' ');
		}
		spawnEnemy(); //respawn ai
	}
}

void PositionFood(int repeats) //method for placing food takes how many to place as argyment
{
	Console.ForegroundColor = ConsoleColor.Green;
	for (int z = 0; z < repeats; z++)
	{
		List<(int X, int Y)> possibleCoordinates = new(); //list for all open tiles
		for (int i = 2; i < width - 2; i++)
		{
			for (int j = 2; j < height - 2; j++)
			{
				if (map[i, j] is Tile.Open)
				{
					possibleCoordinates.Add((i, j));
				}
			}
		}
		int index = random.Next(possibleCoordinates.Count);
		(int X, int Y) = possibleCoordinates[index];
		map[X, Y] = Tile.Food; //set a random possible tile to be food
		foodList.Add((X, Y));
		Console.SetCursorPosition(X, Y); //write the food to the screen.
		Console.Write('&');
	}
	Console.ResetColor();
}

void displayScore(int x, int y) //method to display the scoreboard. it needs the x y coordinates so it knows where to put the cursor back.
{
	int score = snake.Count - 1; // get the length of the snakes as score
	int aiScore = ai.Count - 1;

	if (aiScore < 0) { aiScore = 0; }
	if (score < 0) { score = 0; } //stop it displaying a negative number at the start.

	string status;
	if (aiScore > score) { status = "loosing"; }
	else if (aiScore < score) { status = "winning"; }
	else { status = "tied"; }
	Console.SetCursorPosition(0, 0);     //set the cursor at the top
	Console.ForegroundColor = ConsoleColor.DarkRed; //formatting - make the score colour red
	Console.Write($"food pieces: {foodList.Count}   You: {score}. AI: {aiScore}. AI pos: {A},{B} food: {E},{F}.");
	Console.ResetColor();               //i might comment this out and see what happens 
	Console.SetCursorPosition(x, y);    //return the cursor to whence it came
}

void drawBorder() //draw the walls around the playing area
{
	Console.ForegroundColor = ConsoleColor.Blue;
	for (int i = 1; i < width - 1; i++) //border should be 1 or 2 tiles in from console width/height and just a square.
	{
		for (int j = 1; j < height - 1; j++)
		{
			if ((i == 1 || i == width - 2) || (j == 1 || j == height - 2))
			{
				map[i, j] = Tile.Border;
				Console.SetCursorPosition(i, j);
				Console.Write('X'); //represent with X character - same as terrain
			}
		}
	}
	Console.ResetColor();
}

//method for generating so many random obstacles on the game board!
void addTerrain()
{
	// list of int pairs that are a 2d array of all possible coordinates
	List<(int X, int Y)> possibleCoordinates = new();
	//loop through all possible x and y coordinates to add to the list.
	//  10 to width/height-10 to remove the possibility of adding the food to the very edge. give the player a chance!
	for (int i = 3; i < width - 3; i++)
	{
		for (int j = 3; j < height - 3; j++)
		{
			if (map[i, j] is Tile.Open)
			{
				possibleCoordinates.Add((i, j));
			}
		}
	}
	Console.ForegroundColor = ConsoleColor.Red;
	for (int i = 0; i < 20; i++)
	{
		int index = random.Next(possibleCoordinates.Count);
		(int X, int Y) = possibleCoordinates[index];
		//set that map coordinate to be 'Border'
		map[X, Y] = Tile.Border;
		//set the cursor position and write a plus to indicate food.
		Console.SetCursorPosition(X, Y);
		Console.Write('X');
	}
	Console.ResetColor();
}

void spawnEnemy()
{
	List<(int X, int Y)> possibleCoordinates = new();
	//loop through all possible x and y coordinates to add to the list. with a margin of 3.
	for (int i = 3; i < width - 3; i++)
	{
		for (int j = 3; j < height - 3; j++)
		{
			if (map[i, j] is Tile.Open)
			{
				possibleCoordinates.Add((i, j));
			}
		}
	}
	int index = random.Next(possibleCoordinates.Count);
	(A, B) = possibleCoordinates[index];
	ai.Enqueue((A, B));
	map[A, B] = Tile.Snake;
	Console.SetCursorPosition(A, B);
	Console.ForegroundColor = ConsoleColor.DarkRed;
	Console.Write('@');
	Console.ResetColor();
}

void moveSnake(Queue<(int a, int b)> sn, int x, int y)
{
	char bodyChar = 'a';
	//switch direction part has already changed the x or y value, here the cursor position is updated.
	Console.SetCursorPosition(x, y);
	if (sn == snake) //player
	{
		Console.ForegroundColor = ConsoleColor.DarkYellow;
		Console.Write(DirectionChars[(int)direction]);
		Console.ResetColor();
		bodyChar = 'o';
	}
	else //sn == ai 
	{
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.Write(DirectionChars[(int)aiDirection]);
		Console.ResetColor();
		bodyChar = 'x';
	}

	//add the new position to the snakes queue
	sn.Enqueue((x, y));
	//if the new position is a food tile
	if (map[x, y] == Tile.Food)
	{
		//foodList.Remove((x, y)); //take that food off of the possible food list.
		foodList.Remove((x, y));
		PositionFood(1);
		displayScore(x, y);
	}
	else
	{
		//dequeue the old position
		(int a, int b) = sn.Dequeue();
		//set that tile as open
		map[a, b] = Tile.Open;
		//update the cursor position
		Console.SetCursorPosition(a, b);
		//overwrite the space at the old location
		Console.Write(' ');

		//change the displayed character of the snake depending on the direction also where the colour is set.

		if (sn.Count >= 1)
		{
			for (int i = 1; i < sn.Count - 1; i++) //only works with this combo though
			{
				//loops through the queue kicking out all values and requeuing with 's'
				(int c, int d) = sn.Dequeue();
				Console.SetCursorPosition(c, d);
				sn.Enqueue((c, d));
				Console.Write(bodyChar);
			}
		}
	}
	//new coordinates are now a snake tile.
	map[x, y] = Tile.Snake;
}

enum Direction
{
	Up = 0,
	Down = 1,
	Left = 2,
	Right = 3,
}
enum Tile
{
	Open = 0,//Open tiles are empty space, 
	Snake, //the snake itself
	Food, //any food
	Border, //the edge of the playing field.
}
