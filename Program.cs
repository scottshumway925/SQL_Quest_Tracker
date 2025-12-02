using System;
using System.Collections.Generic;
using QuestTracker.Data;

namespace QuestTracker
{
   class Program
   {
      static void Main(string[] args)
      {
         var qManager = new QuestQueryManager();

         var menu = new Dictionary<int, Action>
            {
                { 1, qManager.DisplayAllQuests },
                { 2, () => AddQuestInteractive(qManager) },
                { 3, () => UpdateQuestInteractive(qManager) },
                { 4, () => DeleteQuestInteractive(qManager) },
                { 5, () => QueryQuestRewards(qManager) },
                { 6, () => AddQuestRewardInteractive(qManager) },
                { 7, () => RemoveRewardInteractive(qManager)}
            };

         while (true)
         {
            Console.WriteLine("\n=== Quest Tracker ===");
            Console.WriteLine("1) View all quests");
            Console.WriteLine("2) Add quest");
            Console.WriteLine("3) Update quest name");
            Console.WriteLine("4) Delete quest");
            Console.WriteLine("5) View quest rewards");
            Console.WriteLine("6) Add reward to quest");
            Console.WriteLine("7) Remove quest reward");
            Console.WriteLine("0) Exit");
            Console.Write("Choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
               Console.WriteLine("Invalid input.");
               continue;
            }

            if (choice == 0) break;

            if (menu.TryGetValue(choice, out Action action))
            {
               action();
            }
            else
            {
               Console.WriteLine("Unknown option.");
            }
         }
      }

      private static void AddQuestInteractive(QuestQueryManager mgr)
      {
         Console.Write("Quest name: ");
         string name = Console.ReadLine() ?? string.Empty;
         Console.Write("Quest type (optional): ");
         int? type = int.Parse(Console.ReadLine());
         Console.Write("Quest Description: ");
         string description = Console.ReadLine() ?? string.Empty;
         bool ok = mgr.AddQuest(name, description, type);
         Console.WriteLine(ok ? "Quest added." : "Failed to add quest.");
      }

      private static void UpdateQuestInteractive(QuestQueryManager mgr)
      {
         mgr.DisplayAllQuests();
         Console.Write("Enter QuestID to update: ");
         if (!int.TryParse(Console.ReadLine(), out int id))
         {
            Console.WriteLine("Invalid id.");
            return;
         }

         Console.Write("New name: ");
         string newName = Console.ReadLine() ?? string.Empty;
         bool ok = mgr.UpdateQuestName(id, newName);
         Console.WriteLine(ok ? "Quest updated." : "Failed to update (check id).");
      }

      private static void DeleteQuestInteractive(QuestQueryManager mgr)
      {
         mgr.DisplayAllQuests();
         Console.Write("Enter QuestID to delete: ");
         if (!int.TryParse(Console.ReadLine(), out int id))
         {
            Console.WriteLine("Invalid id.");
            return;
         }

         bool ok = mgr.DeleteQuest(id);
         Console.WriteLine(ok ? "Quest deleted." : "Failed to delete (maybe FK constraints).");
      }

      private static void AddQuestRewardInteractive(QuestQueryManager mgr)
      {
         mgr.DisplayAllQuests();
         Console.Write("Enter QuestID to add reward to: ");
         if (!int.TryParse(Console.ReadLine(), out int id))
         {
            Console.WriteLine("Invalid id.");
            return;
         }

         Console.Write("What reward type (money, xp, etc.): ");
         string rewardType = Console.ReadLine() ?? string.Empty;
         Console.Write("Enter reward amount: ");
         if (!int.TryParse(Console.ReadLine(), out int rewardAmount))
         {
            Console.WriteLine("Invalid number.");
            return;
         }
         Console.Write("Enter addition effects upon completion (Leave empty for null): ");
         string? additionalEffects = Console.ReadLine();
         if (string.IsNullOrWhiteSpace(additionalEffects)) additionalEffects = null;

         bool ok = mgr.AddReward(id, rewardType, rewardAmount, additionalEffects);
         Console.WriteLine(ok ? "Reward added." : "Failed to add quest (Check Quest ID)");
      }

      private static void QueryQuestRewards(QuestQueryManager mgr)
      {
         mgr.DisplayAllQuests();
         Console.Write("Enter QuestID to view rewards: ");
         if (!int.TryParse(Console.ReadLine(), out int id))
         {
            Console.WriteLine("Invalid id.");
            return;
         }
         mgr.GetQuestRewards(id);
      }

      private static void RemoveRewardInteractive(QuestQueryManager mgr)
      {
         QueryQuestRewards(mgr);
         Console.Write("Enter RewardNumber to remove reward: ");
         if (!int.TryParse(Console.ReadLine(), out int id))
         {
            Console.WriteLine("Invalid id.");
            return;
         }
         bool ok = mgr.DeleteReward(id);
         Console.WriteLine(ok ? "Reward removed successfully." : "Couldn't remove reward (Check id)");
      }
   }
}