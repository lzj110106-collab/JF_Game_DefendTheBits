using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helper class for binning enemies based on where on the path they are. this
//is to speed up the game on slower devices in endless mode where both
//the enemy count and the tower count is very high, which causes a lot
//of unnecessary targeting tests to occur. 

//NB: this class needs to be created after the PathNetwork class
//has set itself up as this class requires pathing information
//to be assigned during its constructor

public class WorldTargetFinder
{
	class CharacterBin
	{
		//allocate enough to store the maximum number of enemies
		//on a tile at the same time. 32 should be more than enough.
		public Character[] characters = new Character[32]; 
		public int characterCount = 0;
	};

	CharacterBin[] bins;
	int[] binMapping;

	WorldQuery<Character> characterQuery;

	public WorldTargetFinder(WorldQuery<Character> query)
	{
		var landscape = Landscape.instance;
		int binCount = 0;

		binMapping = new int[landscape.w * landscape.h];
		for (var j = 0; j < landscape.h; ++j)
		{
			for (var i = 0; i < landscape.w; ++i)
			{
				if (landscape.HasFlag(i, j, TileFlag.HasPath_RuntimeAssigned))
				{
					binMapping[i + j*landscape.w] = binCount;
					binCount += 1;
				}
				else
				{
					//no path, no mapping.
					binMapping[i + j*landscape.w] = -1;
				}
			}
		}

		bins = new CharacterBin[binCount];
		for (int i = 0; i < bins.Length; ++i)
		{
			bins[i] = new CharacterBin();
			bins[i].characterCount = 0;
		}

		characterQuery = query;
	}

	public void Add(Character character)
	{
		int tileX = 0;
		int tileY = 0;
		Landscape.instance.GetTileIndexFromPosition(character.transform.position.x,
													character.transform.position.z,
													ref tileX,
													ref tileY);

		int mappingIndex = tileX + tileY * Landscape.instance.w;
		int binIndex = binMapping[mappingIndex];

		if (binIndex != -1)
		{
			var bin = bins[binIndex];
			if (bin.characterCount < bin.characters.Length)
			{
				bin.characters[bin.characterCount] = character;
				bin.characterCount += 1;

				return;
			}
		}
	}

	public void Clear()
	{
		for (var i = 0; i < bins.Length; ++i)
			bins[i].characterCount = 0;
	}


#region QUERIES

	//this function just returns every alive enemy that is covered by the plot data. the plot
	//count is passed in because some abilities create the max plots up front and
	//only partially use them.
	public WorldQuery<Character> FindCharactersInTileRange(List<Vector2> plotData, int plotCount, int tileX, int tileY, bool testCentre = true)
	{
		int mappingIndex;
		int binIndex;

		characterQuery.Clear();

		for (var i = 0; i < plotCount; ++i)
		{
			int x = (int)(plotData[i].x + tileX);
			int y = (int)(plotData[i].y + tileY);

			mappingIndex = x + y*Landscape.instance.w;
			binIndex = binMapping[mappingIndex];

			if (binIndex != -1)
				CollectCharacters(bins[binIndex].characters, bins[binIndex].characterCount);
		}

		//plotData doesnt include the tower position. this is ok for towers, but
		//not for abilities, so test the centre tile location manually
		if (testCentre)
		{
			mappingIndex = tileX + tileY * Landscape.instance.w;
			binIndex = binMapping[mappingIndex];

			if (binIndex != -1)
				CollectCharacters(bins[binIndex].characters, bins[binIndex].characterCount);
		}

		return characterQuery;
	}

	//using the plot minimal set defined by the weapon. also performs masking out of
	//enemy types that dont match the passed in weapons targeting attributes etc
	public WorldQuery<Character> FindCharactersInTileRange(Weapon weapon) 
	{
		int mappingIndex;
		int binIndex;

		characterQuery.Clear();

		for (var i = 0; i < weapon.plotDataMinimalSet.Count; ++i)
		{
			//this are already in world space. no need to add tileX/tileY
			int x = (int)(weapon.plotDataMinimalSet[i].x);
			int y = (int)(weapon.plotDataMinimalSet[i].y);

			mappingIndex = x + y*Landscape.instance.w;
			binIndex = binMapping[mappingIndex];

			if (binIndex != -1)
				CollectCharacters(weapon, bins[binIndex].characters, bins[binIndex].characterCount);
		}
			
		return characterQuery;
	}
		
	public WorldQuery<Character> FindCharactersInSplashArea(Weapon weapon, Vector3 position)
	{
		Debug.Assert(weapon != null);
		return FindCharactersInSplashArea(position, weapon.weaponData.projectileSplashRadius, weapon);
	}
		 
	//weapon may be null here. IsEnemyVulnerable checks for it.
	public WorldQuery<Character> FindCharactersInSplashArea(Vector3 position, float radius, Weapon weapon = null)
	{
		characterQuery.Clear();

		var radius2 = radius*radius;

		int tileX = 0;
		int tileY = 0;
		Landscape.instance.GetTileIndexFromPosition(position.x, position.z, ref tileX, ref tileY);

		int tileRadius = (int)(radius/Landscape.instance.tileWidth) + 1;

		//looping on tiles surrounding the focus tile
		for (var j = tileY - tileRadius; j <= tileY + tileRadius; ++j)
		{
			for (var i = tileX - tileRadius; i <= tileX + tileRadius; ++i)
			{
				int mappingIndex = i + j*Landscape.instance.w;
				if (mappingIndex < 0 || mappingIndex >= binMapping.Length)
					continue; //invalid tile index due to adding tileRadius

				int binIndex = binMapping[mappingIndex];
				if (binIndex == -1)
					continue; //not a path.

				var bin = bins[binIndex];

				//need to throw in a radius check here, so dont use CollectCharacters
				for (int k = 0; k < bin.characterCount; ++k)
				{
					var c = bin.characters[k];

					//ignore height for distance check
					var characterPosition = c.transform.position;
					characterPosition.y = position.y;

					if (Vector3.SqrMagnitude(position - characterPosition) <= radius2)
					{
						if (IsEnemyVulnerable(weapon, c))
							characterQuery.Add(c);
					}
				}
			}
		}

		return characterQuery;
	}

#endregion

#region QUERY HELPERS

	void CollectCharacters(Character[] characters, int count)
	{
		for (int i = 0; i < count; ++i)
			if (characters[i].alive)
				characterQuery.Add(characters[i]);
	}

	void CollectCharacters(Weapon weapon, Character[] characters, int count)
	{
		for (int i = 0; i < count; ++i)
			if (IsEnemyVulnerable(weapon, characters[i]))
				characterQuery.Add(characters[i]);
	}

	static bool IsEnemyVulnerable(Weapon weapon, Character c)
	{
		if (!c.alive || !c.isActiveAndEnabled)
			return false;

		if (weapon != null)
		{
			//remove type flags that can always be targeted. this is
			//so that combos like stealth+regen still work
			var alwaysHit = EnemyAttributes.Berserk | EnemyAttributes.Regen;
			var attributes = c.attributes & ~alwaysHit;

			if ((weapon.weaponData.targetingFlags & attributes) != attributes)
				return false;

			//ignore enemies that are immune to the damage type of the weapon
			if ((weapon.category & c.categoryImmunities) != 0)
				return false;
		}

		return true;
	}

#endregion
}
