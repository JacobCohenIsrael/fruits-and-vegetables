using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	
	public class BoardManager : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector.
		[Serializable]
		public class Count
		{
			public int minimum; 			//Minimum value for our Count class.
			public int maximum; 			//Maximum value for our Count class.
			
			
			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		
		
		public int columns = 8; 										//Number of columns in our game board.
		public int rows = 8;											//Number of rows in our game board.
		public Count wallCount = new Count (5, 9);						//Lower and upper limit for our random number of walls per level.
		public Count foodCount = new Count (1, 5);						//Lower and upper limit for our random number of food items per level.
		public GameObject exit;											//Prefab to spawn for exit.
		public GameObject[] floorTiles;									//Array of floor prefabs.
		public GameObject[] wallTiles;									//Array of wall prefabs.
		public GameObject[] foodTiles;									//Array of food prefabs.
		public GameObject[] enemyTiles;									//Array of enemy prefabs.
		public GameObject[] outerWallTiles;								//Array of outer tile prefabs.
		
		private Transform boardHolder;									//A variable to store a reference to the transform of our Board object.
		private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.
        private bool[,] walkableGrid = new bool[8, 8];
        private Player player;

        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList ()
		{
			//Clear our list gridPositions.
			gridPositions.Clear ();
			
			//Loop through x axis (columns).
			for(int x = 1; x < columns-1; x++)
			{
				//Within each column, loop through y axis (rows).
				for(int y = 1; y < rows-1; y++)
				{
					//At each index add a new Vector3 to our list with the x and y coordinates of that position.
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}

        void InitializeWalkableGrid()
        {

            //Loop through x axis (columns).
            for (int x = 0; x < columns; x++)
            {
                //Within each column, loop through y axis (rows).
                for (int y = 0; y < rows; y++)
                {
					walkableGrid[x, y] = true;
                }
            }
        }


        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup ()
		{
			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject ("Board").transform;
			
			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for(int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
					
					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
					if(x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
					
					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent (boardHolder);
				}
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}
		
		
		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum, int level)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum + 1);
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
				Vector3 randomPosition = RandomPosition();

				walkableGrid[Mathf.RoundToInt(randomPosition.x), Mathf.RoundToInt(randomPosition.y)] = false;
				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}

        private void LayoutDistinctObjectsAroundObject(GameObject[] foodTiles, GameObject toPlaceAround)
        {
			Vector3 currentPosition = toPlaceAround.transform.position;
            List<GameObject> availableTiles = new List<GameObject>(foodTiles);
			List<MovementButton> movementButtons = new List<MovementButton>(MovementButtonsUiManager.Instance.movementButtons);
            foreach (Vector3 offset in new Vector3[] {Vector3.up, Vector3.down, Vector3.left, Vector3.right})
			{
                Vector3 cloesest = currentPosition + offset;
				if (isWalkable(cloesest))
				{
                    //Choose a random tile from tileArray and assign it to tileChoice
                    int randomIndex = Random.Range(0, availableTiles.Count);
                    GameObject tileChoice = availableTiles[randomIndex];
                    availableTiles.RemoveAt(randomIndex);

                    var randomMovementButtonIndex = Random.Range(0, movementButtons.Count);
                    var movementButton = movementButtons[randomMovementButtonIndex];
                    movementButton.SetFood(tileChoice.GetComponent<Food>());
					movementButton.gameObject.SetActive(true);
                    movementButtons.RemoveAt(randomMovementButtonIndex);

                    //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                    Instantiate(tileChoice, cloesest, Quaternion.identity);
                }
				else
				{
                    var randomMovementButtonIndex = Random.Range(0, movementButtons.Count);
                    var movementButton = movementButtons[randomMovementButtonIndex];
					movementButton.gameObject.SetActive(false);
                    movementButtons.RemoveAt(randomMovementButtonIndex);
                }

            }
        }

        private bool isWalkable(Vector3 cloesest)
        {
			if (cloesest.x < 0 || cloesest.y < 0) return false;
			if (cloesest.x >= columns || cloesest.y >= rows) return false;
			return walkableGrid[Mathf.RoundToInt(cloesest.x), Mathf.RoundToInt(cloesest.y)];
        }


        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene (int level)
		{
			player = GameObject.FindAnyObjectByType<Player>();
			player.OnPlayerEat += OnPlayerEat;
			//Creates the outer walls and floor.
			BoardSetup ();
			
			//Reset our list of gridpositions.
			InitialiseList ();

			InitializeWalkableGrid();
			
			//Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum, level);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutDistinctObjectsAroundObject(foodTiles, player.gameObject);
			
			//Determine number of enemies based on current level number, based on a logarithmic progression
			int enemyCount = (int)Mathf.Log(level, 2f);
			
			//Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
			//LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);
			
			//Instantiate the exit tile in the upper right hand corner of our game board
			Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
		}

        private void OnPlayerEat(object sender, EventArgs e)
        {
			var foodList = GameObject.FindObjectsOfType<Food>();
			foreach (var food in foodList)
			{
				Destroy (food.gameObject);
			}
            LayoutDistinctObjectsAroundObject(foodTiles, player.gameObject);
        }
    }
}
