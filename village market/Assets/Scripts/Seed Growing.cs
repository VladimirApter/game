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
        foreach (var seed in Objects.Things.OfType<Seed>())
        {
            if (!seed.IsPlanted) seed.GrowingFramesCount = 0;
        }

        seedsGrewInThisFrame.Clear();
        newFruitsInThisFrame.Clear();

        foreach (var seed in Objects.Things.OfType<Seed>().Where(seed => seed.IsPlanted && seed.Seedbed.IsPoured))
        {
            if (!Objects.Seedbeds.Values.Contains(seed.Seedbed))
            {
                UnplantSeed(seed);
                return;
            }

            UpdateGrowingSeed(seed);
        }

        Objects.Things.AddRange(newFruitsInThisFrame);
        Objects.Things.RemoveAll(seed => seedsGrewInThisFrame.Contains(seed));
    }

    private void UpdateGrowingSeed(Seed seed)
    {
        seed.GrowingFramesCount++;

        UpdateSeedSprite(seed);

        if (seed.GrowingFramesCount < seed.FramesToGrow) return;

        var newFruit = CreateNewFruit(seed);
        newFruit.Cords = seed.Cords;
        newFruitsInThisFrame.Add(newFruit);
        seedsGrewInThisFrame.Add(seed);

        Destroy(seed.ThingObj);
        ResetSeedbed(seed.Seedbed);
    }

    private void UpdateSeedSprite(Seed seed)
    {
        var spriteIndex = seed switch
        {
            WheatSeed => GetSpriteIndex(seed, 1, 3.99),
            BeetSeed => GetSpriteIndex(seed, 5, 3.99),
            _ => throw new ArgumentOutOfRangeException()
        };

        seed.ThingObj.GetComponent<SpriteRenderer>().sprite = newSprites[spriteIndex];
        return;

        int GetSpriteIndex(Seed seed, int baseIndex, double multiplier)
        {
            return (int)Math.Floor(baseIndex + multiplier * (1 - (seed.FramesToGrow - seed.GrowingFramesCount) /
                (double)seed.FramesToGrow));
        }
    }

    private void UnplantSeed(Seed seed)
    {
        seed.IsPlanted = false;
        seed.GrowingFramesCount = 0;

        seed.ThingObj.GetComponent<SpriteRenderer>().sprite = seed switch
        {
            WheatSeed => newSprites[0],
            BeetSeed => newSprites[5],
            _ => seed.ThingObj.GetComponent<SpriteRenderer>().sprite
        };
    }

    private Fruit CreateNewFruit(Seed seed)
    {
        return seed switch
        {
            BeetSeed => new Beet
                { ThingObj = Instantiate(Beet.BeetPrefab, seed.Cords, Quaternion.identity, fruitObjs.transform) },
            WheatSeed => new Wheat
                { ThingObj = Instantiate(Wheat.WheatPrefab, seed.Cords, Quaternion.identity, fruitObjs.transform) },
            _ => new Fruit
                { ThingObj = Instantiate(Fruit.FruitPrefab, seed.Cords, Quaternion.identity, fruitObjs.transform) }
        };
    }

    private void ResetSeedbed(Seedbed seedbed)
    {
        seedbed.IsPoured = false;
        seedbed.SeedbedObj.GetComponent<SpriteRenderer>().color =
            Seedbed.SeedbedPrefab.GetComponent<SpriteRenderer>().color;
    }
}