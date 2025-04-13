using System;
using System.Collections.Generic;
using Terraria;

namespace HeavenlyArsenal.common;

// ----- INTERFACES FOR CONDITIONS & REWARDS -----

// Conditions used to further restrict quest requirements
public interface ICondition
{
    bool IsMet(Player player);
}

// Rewards granted upon quest completion
public interface IReward
{
    void GrantReward(Player player);
}

// ----- EXAMPLE PLAYER CLASS -----
// This is a basic Player representation to simulate character data.
public class Player
{
    public string Name;
    public int MaxHealth;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    public Dictionary<string, int> Bestiary = new Dictionary<string, int>(); // Tracks kill counts for enemies

    public Player(string name, int maxHealth)
    {
        Name = name;
        MaxHealth = maxHealth;
    }

    public void AddItem(string item, int count)
    {
        if (Inventory.ContainsKey(item))
            Inventory[item] += count;
        else
            Inventory[item] = count;
    }

    public bool HasItem(string item, int count)
    {
        return Inventory.ContainsKey(item) && Inventory[item] >= count;
    }

    public void LogKill(string enemyName)
    {
        if (Bestiary.ContainsKey(enemyName))
            Bestiary[enemyName]++;
        else
            Bestiary[enemyName] = 1;
    }
}

// ----- QUEST TYPES -----
public enum QuestType
{
    KillEnemy,
    FindItem,
    GoToPos
}

// ----- BASE QUEST CLASS -----
// The Quest class includes common functionality such as storing conditions and rewards.
public abstract class Quest
{
    public string QuestName { get; set; }
    public QuestType Type { get; set; }
    public bool IsCompleted { get; set; }
    public List<ICondition> Conditions { get; set; }
    public List<IReward> Rewards { get; set; }

    public Quest(string questName, QuestType type, IEnumerable<ICondition> conditions, IEnumerable<IReward> rewards)
    {
        QuestName = questName;
        Type = type;
        IsCompleted = false;
        Conditions = new List<ICondition>(conditions);
        Rewards = new List<IReward>(rewards);
    }

    // Each quest type needs to implement how its progress is updated.
    public abstract void UpdateProgress(Player player);

    // Check if every condition assigned to the quest is met.
    public bool CheckConditions(Player player)
    {
        foreach (var cond in Conditions)
        {
            if (!cond.IsMet(player))
                return false;
        }
        return true;
    }

    // Grant all the rewards if the quest is successfully completed.
    public void GrantRewards(Player player)
    {
        foreach (var reward in Rewards)
        {
            reward.GrantReward(player);
        }
    }
}

// ----- SPECIFIC QUEST IMPLEMENTATIONS -----

// Quest for killing enemies. It tracks a target enemy name and a target kill count.
public class KillEnemyQuest : Quest
{
    public string EnemyName { get; set; }
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }

    public KillEnemyQuest(string questName, string enemyName, int targetCount = 1, IEnumerable<ICondition> conditions = null, IEnumerable<IReward> rewards = null)
        : base(questName, QuestType.KillEnemy, conditions ?? new ICondition[0], rewards ?? new IReward[0])
    {
        EnemyName = enemyName;
        TargetCount = targetCount;
        CurrentCount = 0;
    }

    public override void UpdateProgress(Player player)
    {
        // For kill quests we check the bestiary kill count.
        if (player.Bestiary.ContainsKey(EnemyName))
        {
            CurrentCount = player.Bestiary[EnemyName];
            if (CurrentCount >= TargetCount && CheckConditions(player))
            {
                IsCompleted = true;
                GrantRewards(player);
            }
        }
    }
}

// Quest for collecting a specific item. It tracks the item name and how many of it are required.
public class FindItemQuest : Quest
{
    public string ItemName { get; set; }
    public int RequiredCount { get; set; }

    public FindItemQuest(string questName, string itemName, int requiredCount = 1, IEnumerable<ICondition> conditions = null, IEnumerable<IReward> rewards = null)
        : base(questName, QuestType.FindItem, conditions ?? new ICondition[0], rewards ?? new IReward[0])
    {
        ItemName = itemName;
        RequiredCount = requiredCount;
    }

    public override void UpdateProgress(Player player)
    {
        if (player.HasItem(ItemName, RequiredCount) && CheckConditions(player))
        {
            IsCompleted = true;
            GrantRewards(player);
        }
    }
}

// Quest for reaching a specific coordinate in the world.
public class GoToPosQuest : Quest
{
    public (int X, int Y) TargetPosition { get; set; }
    public (int X, int Y) Tolerance { get; set; } // Margin of error

    // targetPosition is provided as a tuple; tolerance is optional (default is 5 units each direction).
    public GoToPosQuest(string questName, (int, int) targetPosition, (int, int)? tolerance = null, IEnumerable<ICondition> conditions = null, IEnumerable<IReward> rewards = null)
        : base(questName, QuestType.GoToPos, conditions ?? new ICondition[0], rewards ?? new IReward[0])
    {
        TargetPosition = targetPosition;
        Tolerance = tolerance ?? (5, 5);
    }

    // In a full mod, this would compare the player's position with TargetPosition.
    public override void UpdateProgress(Player player)
    {
        // Position check logic would be implemented here.
    }
}

// ----- REWARD IMPLEMENTATIONS -----

// Reward that gives an item.
public class ItemReward : IReward
{
    public string ItemName { get; set; }
    public int Count { get; set; }

    public ItemReward(string itemName, int count)
    {
        ItemName = itemName;
        Count = count;
    }

    public void GrantReward(Player player)
    {
        player.AddItem(ItemName, Count);
        Console.WriteLine($"Granted {Count} {ItemName} to {player.Name}");
    }
}

// Reward that spawns an NPC near the player.
public class SpawnNpcReward : IReward
{
    public string NpcName { get; set; }
    public SpawnNpcReward(string npcName)
    {
        NpcName = npcName;
    }

    public void GrantReward(Player player)
    {
        Console.WriteLine($"Spawning NPC {NpcName} around {player.Name}");
        // Insert NPC spawn logic here.
    }
}

// Reward that boosts the player's maximum health.
public class HealthBoostReward : IReward
{
    public int HealthIncrease { get; set; }
    public HealthBoostReward(int increase)
    {
        HealthIncrease = increase;
    }

    public void GrantReward(Player player)
    {
        player.MaxHealth += HealthIncrease;
        Console.WriteLine($"{player.Name}'s max health increased by {HealthIncrease}");
    }
}

// ----- CONDITION IMPLEMENTATIONS -----

// A condition that requires a specific boss to have been defeated.
public class BossDefeatedCondition : ICondition
{
    public string BossName { get; set; }
    public BossDefeatedCondition(string bossName)
    {
        BossName = bossName;
    }

    public bool IsMet(Player player)
    {
        // Replace with actual boss check logic. Here, we simply check if the boss appears in the bestiary.
        return player.Bestiary.ContainsKey(BossName) && player.Bestiary[BossName] > 0;
    }
}

// A condition that verifies if the game is in hardmode.
public class HardmodeCondition : ICondition
{
    public bool IsMet(Player player)
    {
        // For demonstration purposes, simply return false.
        return false;
    }
}

// A condition that requires the player to have a minimum maximum health.
public class MaxHealthCondition : ICondition
{
    public int RequiredHealth { get; set; }
    public MaxHealthCondition(int requiredHealth)
    {
        RequiredHealth = requiredHealth;
    }
    public bool IsMet(Player player)
    {
        return player.MaxHealth >= RequiredHealth;
    }
}

// A condition that checks for a specific item in the inventory.
public class InventoryCondition : ICondition
{
    public string RequiredItem { get; set; }
    public int Count { get; set; }
    public InventoryCondition(string item, int count = 1)
    {
        RequiredItem = item;
        Count = count;
    }

    public bool IsMet(Player player)
    {
        return player.HasItem(RequiredItem, Count);
    }
}

// ----- QUEST GENERATION -----

public static class QuestGenerate
{
    // Creates a quest based on the type and parameters provided.
    // Using default parameters (with null checks) to allow flexible creation.
    public static Quest CreateQuest(QuestType type, string questName, string targetIdentifier, int amount = 1, ICondition[] conditions = null, IReward[] rewards = null)
    {
        // Ensure conditions and rewards arrays are not null.
        conditions = conditions ?? new ICondition[0];
        rewards = rewards ?? new IReward[0];

        switch (type)
        {
            case QuestType.KillEnemy:
                return new KillEnemyQuest(questName, targetIdentifier, amount, conditions, rewards);
            case QuestType.FindItem:
                return new FindItemQuest(questName, targetIdentifier, amount, conditions, rewards);
            case QuestType.GoToPos:
                // For a positional quest, parse the string into coordinates in the format "x,y"
                var coords = targetIdentifier.Split(',');
                if (coords.Length == 2 &&
                    int.TryParse(coords[0], out int x) &&
                    int.TryParse(coords[1], out int y))
                {
                    return new GoToPosQuest(questName, (x, y), null, conditions, rewards);
                }
                else
                {
                    throw new ArgumentException("Invalid position format. Use 'x,y'.");
                }
            default:
                throw new ArgumentException("Invalid quest type");
        }
    }

    // Method to generate several example quests after the game content is set up.
    public static List<Quest> PostSetupContent()
    {
        var quests = new List<Quest>();

        // Example Quest 1:
        // Kill 10 Slime with the reward of 4 iron pickaxes.
        Quest killSlimeQuest = CreateQuest(
            QuestType.KillEnemy,
            "Slime Extermination",
            "Slime",
            10,
            rewards: new IReward[] { new ItemReward("Iron Pickaxe", 4) }
        );
        quests.Add(killSlimeQuest);

        // Example Quest 2:
        // Find the item "Pink Gel" with a reward of 4 gold coins.
        Quest findPinkGelQuest = CreateQuest(
            QuestType.FindItem,
            "Gel Collector",
            "Pink Gel",
            1,
            rewards: new IReward[] { new ItemReward("Gold Coin", 4) }
        );
        quests.Add(findPinkGelQuest);

        // Example Quest 3:
        // Find "LuckyHorseshoe" with a reward that spawns a KingSlime NPC.
        Quest findLuckyHorseshoeQuest = CreateQuest(
            QuestType.FindItem,
            "Lucky Search",
            "LuckyHorseshoe",
            1,
            rewards: new IReward[] { new SpawnNpcReward("KingSlimeNpc") }
        );
        quests.Add(findLuckyHorseshoeQuest);

        return quests;
    }
}

// ----- QUEST TRACKING & COMPLETION -----

// Tracks active quests for a character.
public class QuestTracker
{
    public List<Quest> ActiveQuests { get; set; }

    public QuestTracker()
    {
        ActiveQuests = new List<Quest>();
    }

    public void AddQuest(Quest quest)
    {
        ActiveQuests.Add(quest);
        Console.WriteLine($"Quest Added: {quest.QuestName}");
    }

    // Update progress for each active quest.
    public void UpdateQuests(Player player)
    {
        foreach (var quest in ActiveQuests)
        {
            if (!quest.IsCompleted)
                quest.UpdateProgress(player);
        }
    }
}

// Logs overall quest completions. Each quest generates a corresponding bool entry.
public class HasCompleted
{
    // Using a dictionary so that the quest name maps to its completion status.
    public Dictionary<string, bool> CompletedQuests { get; set; }

    public HasCompleted()
    {
        CompletedQuests = new Dictionary<string, bool>();
    }

    public void LogCompletion(Quest quest)
    {
        if (!CompletedQuests.ContainsKey(quest.QuestName))
            CompletedQuests.Add(quest.QuestName, quest.IsCompleted);
        else
            CompletedQuests[quest.QuestName] = quest.IsCompleted;

        Console.WriteLine($"Quest '{quest.QuestName}' completion logged as: {quest.IsCompleted}");
    }
}

// The main QuestSystem class that tracks quest data per character.
public class QuestSystem
{
    // A dictionary mapping character names to their QuestTracker.
    public Dictionary<string, QuestTracker> CharacterQuestData { get; set; }
    public HasCompleted GlobalCompletionStatus { get; set; }

    public QuestSystem()
    {
        CharacterQuestData = new Dictionary<string, QuestTracker>();
        GlobalCompletionStatus = new HasCompleted();
    }

    // Retrieve or create a QuestTracker for a given character.
    public QuestTracker GetTrackerForCharacter(string characterName)
    {
        if (!CharacterQuestData.ContainsKey(characterName))
        {
            CharacterQuestData[characterName] = new QuestTracker();
        }
        return CharacterQuestData[characterName];
    }

    // Update quests for the given player and log their completions.
    public void UpdatePlayerQuests(Player player)
    {
        QuestTracker tracker = GetTrackerForCharacter(player.Name);
        tracker.UpdateQuests(player);

        foreach (var quest in tracker.ActiveQuests)
        {
            if (quest.IsCompleted)
            {
                GlobalCompletionStatus.LogCompletion(quest);
            }
        }
    }
}

// ----- USAGE EXAMPLE -----
class Program
{
    static void Main(string[] args)
    {
        // Create a quest system instance.
        QuestSystem questSystem = new QuestSystem();

        // Example: create a player.
        Player player = new Player("Hero", 100);

        // Retrieve the player's quest tracker.
        QuestTracker tracker = questSystem.GetTrackerForCharacter(player.Name);

        // Load the preset quests after content setup.
        List<Quest> exampleQuests = QuestGenerate.PostSetupContent();

        // Add the example quests to the player's tracker.
        foreach (Quest q in exampleQuests)
        {
            tracker.AddQuest(q);
        }

        // Simulate some game events:
        // For example, the player kills 10 Slime.
        for (int i = 0; i < 10; i++)
        {
            player.LogKill("Slime");
        }
        // Simulate the player collecting "Pink Gel" and "LuckyHorseshoe"
        player.AddItem("Pink Gel", 1);
        player.AddItem("LuckyHorseshoe", 1);

        // Update quests based on the player's actions.
        questSystem.UpdatePlayerQuests(player);
    }
}
