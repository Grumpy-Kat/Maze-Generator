using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	private int[,] map;

	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] private int seed;

	private Color[] mapColors;
	private Texture2D mapTexture;
	[SerializeField] private SpriteRenderer mapRenderer;

	[SerializeField] private float wallChance;

	[SerializeField] private int numEnemies;

	private void OnValidate() {
		//width must be greater than 5 and odd to allow for starting room
		if(width < 5) {
			width = 5;
		}
		if(width % 2 == 0) {
			width++;
		}
		//height must be greater than 5 and odd to allow for starting room
		if(height < 5) {
			height = 5;
		}
		if(height % 2 == 0) {
			height++;
		}
		//numEnemies must be greater than 0 and even to allow for starting room
		if(numEnemies < 0) {
			numEnemies = 0;
		}
		if(numEnemies % 2 == 1) {
			numEnemies++;
		}
	}

	public void Start() {
		GenerateMap();
    }

	public void GenerateMap() {
		GenerateMaze();
		GenerateTexture();
	}

	private void GenerateMaze() {
		map = new int[width, height];
		System.Random rand = new System.Random(seed);

		//add walls to every tile
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				map[x, y] = 1;
			}
		}

		//generate starting room for enemies
		GenerateStartingRoom();

		//generate maze using backtracking
		Stack<Vector2> prev = new Stack<Vector2>();
		Vector2 curr = new Vector2(1, 1);
		prev.Push(curr);
		map[(int) curr.x, (int) curr.y] = 0;
		map[width - (int) curr.x - 1, (int) curr.y] = 0;
		while (prev.Count > 0) {
			curr = prev.Peek();
			List<Vector2> neighbors = GetNeighborWalls(curr);
			if(neighbors.Count  == 0) {
				prev.Pop();
			} else {
                Vector2 neighbor = neighbors[rand.Next(0, neighbors.Count)];
				int neighborX = (int) neighbor.x;
				int neighborY = (int) neighbor.y;
				map[neighborX, neighborY] = 0;
				map[width - neighborX - 1, neighborY] = 0;
				map[neighborX, height - neighborY - 1] = 0;
				map[width - neighborX - 1, height - neighborY - 1] = 0;

				int midX = neighborX - (neighborX - (int) curr.x) / 2;
				int midY = neighborY - (neighborY - (int) curr.y) / 2;
				map[midX, midY] = 0;
				map[width - midX - 1, midY] = 0;
				map[midX, height - midY - 1] = 0;
				map[width - midX - 1, height - midY - 1] = 0;

				prev.Push(neighbor);
			}
		}

		//remove extra walls if random chance is higher than wallChance
		for (int x = 1; x < (width - 1) / 2; x++) {
			for (int y = 1; y < (height - 1) / 2; y++) {
				if(map[x, y] == 1 && (float) rand.NextDouble() > wallChance && CanRemoveWall(x, y)) {
					map[x, y] = 0;
					map[width - x - 1, y] = 0;
					map[x, height - y - 1] = 0;
					map[width - x - 1, height - y - 1] = 0;
				}
			}
		}
	}

	private void GenerateStartingRoom() {
		int halfWidth = (width - 1) / 2;
		int halfHeight = (height - 1) / 2;
		int roomHalfWidth = numEnemies / 2;
		//create starting room with space, wall, space, wall, space
		for (int x = -roomHalfWidth - 2; x <= roomHalfWidth + 2; x++) {
			map[halfWidth + x, halfHeight + 2] = 0;
			map[halfWidth + x, halfHeight + 1] = 2;
			map[halfWidth + x, halfHeight] = 0;
			map[halfWidth + x, halfHeight - 1] = 2;
			map[halfWidth + x, halfHeight - 2] = 0;
		}
		//create doorway
		map[halfWidth, halfHeight - 1] = 0;
		//add walls and space around edges
		for (int y = -1; y <= 1; y++) {
			map[halfWidth + roomHalfWidth + 2, halfHeight + y] = 0;
			map[halfWidth + roomHalfWidth + 1, halfHeight + y] = 2;
			map[halfWidth - roomHalfWidth - 1, halfHeight + y] = 2;
			map[halfWidth - roomHalfWidth - 2, halfHeight + y] = 0;
		}
	}

	private List<Vector2> GetNeighborWalls(Vector2 curr) {
		List<Vector2> neighbors = new List<Vector2>();
		int x = (int) curr.x;
		int y = (int) curr.y;
		//check right neighbor
		if(x > 2 && map[x - 2, y] == 1) {
			neighbors.Add(new Vector2(x - 2, y));
		}
		//check up neighbor
		if(y > 2 && map[x, y - 2] == 1) {
			neighbors.Add(new Vector2(x, y - 2));
		}
		int halfWidth = (width - 1) / 2;
		//check left neigbor
		if(x < halfWidth - 1 && map[x + 2, y] == 1) {
			neighbors.Add(new Vector2(x + 2, y));
		}
		int halfHeight = (height - 1) / 2;
		//check down neighbor
		if (y < halfHeight - 1 && map[x, y + 2] == 1) {
			neighbors.Add(new Vector2(x, y + 2));
		}
		return neighbors;
	}

	private bool CanRemoveWall(int x, int y) {
		int halfWidth = (width - 1) / 2;
		int halfHeight = (height - 1) / 2;
		//check if not within bounds
		if (x < 1 || y < 1 || x > halfWidth || y > halfHeight) {
			return false;
		}
		//check if not wall
		if(map[x, y] != 1) {
			return false;
		}
		//check if horizontal wall with no vertical neighbors
		if(map[x + 1, y] == 1 && map[x - 1, y] == 1 && map[x, y + 1] != 1 && map[x, y - 1] != 1) {
			return true;
		}
		//check if vertical wall with no horizontal neighbors
		if(map[x, y + 1] == 1 && map[x, y - 1] == 1 && map[x + 1, y] != 1 && map[x - 1, y] != 1) {
			return true;
		}
		return false;
	}

	private void GenerateTexture() {
		mapColors = new Color[width * height];
		//add pixel: 0 = space/white, 1 = wall/black, 2 = wall/blue highlight
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				//TODO: use actual sprites
				mapColors[y * width + x] = (map[x, y] == 0 ? Color.black : Color.blue);
			}
		}
		//make texture and add pixels
        mapTexture = new Texture2D(width, height) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        mapTexture.SetPixels(mapColors);
		mapTexture.Apply();
		//add to SpriteRenderer and scale map
		mapRenderer.sprite = Sprite.Create(mapTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
		mapRenderer.transform.localScale = new Vector3(width, height, 1);
	}
}
