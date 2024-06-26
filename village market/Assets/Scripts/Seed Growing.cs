using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

public class SeedGrowing : MonoBehaviour
{
    public GameObject fruitObjs;
    private readonly List<Seed> seedsGrewInThisFrame = new();
    private readonly List<Fruit> newFruitsInThisFrame = new();

    public Sprite[] newSprites;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        seedsGrewInThisFrame.Clear();
        newFruitsInThisFrame.Clear();

        foreach (var seed in Objects.Things.OfType<Seed>())
        {
            if (!seed.IsPlanted && seed is not AppleTreeSeed || !seed.IsPlantedOnSeedbeds && seed is AppleTreeSeed)
            {
                seed.GrowingFramesCount = 0;
            }

            if ((seed.IsPlanted || seed.IsPlantedOnSeedbeds) && seed.GrowingFramesCount == 0)
            {
                UnGrowSeed(seed);
            }
            if (seed.IsPlanted && seed.Seedbed.IsPoured ||
                seed.IsPlantedOnSeedbeds && seed.Seedbeds.All(seedBed => seedBed.IsPoured) && seed.GrowingFramesCount != AppleTreeSeed.AppleTreeFramesToGrow)
            {
                UpdateGrowingSeed(seed);
            }

            if (seed.Seedbed == null && seed is not AppleTreeSeed || seed.Seedbeds == null && seed is AppleTreeSeed)
            {
                UnplantSeed(seed);
            }
        }

        Objects.Things.AddRange(newFruitsInThisFrame);
        Objects.Things.RemoveAll(seed => seedsGrewInThisFrame.Contains(seed));
    }

    private void UpdateGrowingSeed(Seed seed)
    {
        seed.GrowingFramesCount++;
        UpdateSeedSprite(seed);

        if (seed is AppleTreeSeed)
            seed.FramesToGrow = AppleTreeSeed.AppleTreeFramesToGrow;

        if (seed.GrowingFramesCount < seed.FramesToGrow) return;
        if (seed.GrowingFramesCount == seed.FramesToGrow && seed is AppleTreeSeed)
        {
            var spriteRenderer = seed.ThingObj.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "seedbeds";
            spriteRenderer.sortingOrder = 2;
            
            seed.CanCarried = false;
            foreach (var seedbed in seed.Seedbeds)
            {
                seedbed.CanDestroy = false;
            }
        }

        if (seed is AppleTreeSeed)
        {
            newFruitsInThisFrame.AddRange(CreateNewFruits(seed));
            foreach (var seedbed in seed.Seedbeds)
            {
                ResetSeedbed(seedbed);
                seedbed.IsBusy = true;
            }
        }
        else
        {
            newFruitsInThisFrame.Add(CreateNewFruit(seed, seed.Cords));
            ResetSeedbed(seed.Seedbed);
        }

        if (seed is not AppleTreeSeed)
        {
            seedsGrewInThisFrame.Add(seed);
            Destroy(seed.ThingObj);
        }
        
    }

    private void UpdateSeedSprite(Seed seed)
    {
        var spriteIndex = seed switch
        {
            WheatSeed => GetSpriteIndex(seed, 1, 3.99),
            BeetSeed => GetSpriteIndex(seed, 6, 3.99),
            AppleTreeSeed => GetSpriteIndex(seed, 11, 3),
            _ => throw new ArgumentOutOfRangeException()
        };

        seed.ThingObj.GetComponent<SpriteRenderer>().sprite = newSprites[spriteIndex];
        return;

        int GetSpriteIndex(Seed seed2, int baseIndex, double multiplier)
        {
            return (int)Math.Floor(baseIndex + multiplier * (1 - (seed2.FramesToGrow - seed2.GrowingFramesCount) /
                (double)seed2.FramesToGrow));
        }
    }

    private void UnplantSeed(Seed seed)
    {
        seed.GrowingFramesCount = 0;
        seed.ThingObj.GetComponent<SpriteRenderer>().sprite = seed switch
        {
            WheatSeed => newSprites[0],
            BeetSeed => newSprites[5],
            AppleTreeSeed => newSprites[10],
            _ => seed.ThingObj.GetComponent<SpriteRenderer>().sprite
        };
        
    }
    
    private void UnGrowSeed(Seed seed)
    {
        seed.GrowingFramesCount = 0;
        seed.ThingObj.GetComponent<SpriteRenderer>().sprite = seed switch
        {
            WheatSeed => newSprites[15],
            BeetSeed => newSprites[16],
            AppleTreeSeed => newSprites[17],
            _ => seed.ThingObj.GetComponent<SpriteRenderer>().sprite
        };
        
        //Play(sounds[1]);
    }

    private Fruit CreateNewFruit(Seed seed, Vector2 coords)
    {
        GameObject prefab;
        Fruit fruit;

        switch (seed)
        {
            case BeetSeed:
                prefab = Beet.BeetPrefab;
                fruit = new Beet();
                break;
            case WheatSeed:
                prefab = Wheat.WheatPrefab;
                fruit = new Wheat();
                break;
            case AppleTreeSeed:
                prefab = Apple.ApplePrefab;
                fruit = new Apple();
                break;
            default:
                prefab = Fruit.FruitPrefab;
                fruit = new Fruit();
                break;
        }

        fruit.ThingObj = Instantiate(prefab, coords, Quaternion.identity, fruitObjs.transform);
        return fruit;
    }

    private IEnumerable<Fruit> CreateNewFruits(Seed seed)
    {
        if (seed is AppleTreeSeed)
        {
            var seedbeds = seed.Seedbeds.OrderBy(seedbed => seedbed.Coords.y).ToList();
            var firstTwo = seedbeds.Take(2).ToList();
            var lastTwo = seedbeds.TakeLast(2).ToList();

            foreach (var seedbed in firstTwo.Concat(lastTwo))
            {
                var apple = CreateNewFruit(seed, seedbed.Coords);
                if (lastTwo.Contains(seedbed))
                {
                    var spriteRenderer = apple.ThingObj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingLayerName = "seedbeds";
                    spriteRenderer.sortingOrder = 1;
                }
                yield return apple;
            }
        }
    }


    private void ResetSeedbed(Seedbed seedbed)
    {
        seedbed.IsPoured = false;
        seedbed.IsBusy = false;
        seedbed.SeedbedObj.GetComponent<SpriteRenderer>().color =
            Seedbed.SeedbedPrefab.GetComponent<SpriteRenderer>().color;
    }
}