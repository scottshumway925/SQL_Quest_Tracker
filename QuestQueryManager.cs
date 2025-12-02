using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuestTracker.Database;
using QuestTracker.Models;

namespace QuestTracker.Data
{
   public class QuestQueryManager
   {
      private readonly string _connectionString;
      
      public QuestQueryManager() 
      {
         _connectionString = DatabaseConnection.ConnectionString; 
      }

      private void QueryDatabase(string sql, Action<SqlDataReader> rowHandler, Dictionary<string, object>? parameters = null)
      {
         using (SqlConnection conn = new SqlConnection(_connectionString))
         {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
               if (parameters != null)
               {
                  foreach (var kv in parameters)
                  {
                     cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                  }
               }

               using (SqlDataReader reader = cmd.ExecuteReader())
               {
                  while (reader.Read())
                  {
                     rowHandler(reader);
                  }
               }
            }
         }
      }

      private int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
      {
         using (SqlConnection conn = new SqlConnection(_connectionString))
         {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
               if (parameters != null)
               {
                  foreach (var kv in parameters)
                  {
                     cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                  }
               }

               return cmd.ExecuteNonQuery();
            }
         }
      }

      //
      // Example Action-based usage: prints quests using QueryDatabase
      //
      public void DisplayAllQuests()
      {
         string sql = "SELECT QuestID, Name, QuestTypeID FROM Quests ORDER BY QuestID";

         // Pass a lambda that knows how to handle each SqlDataReader row
         QueryDatabase(sql, reader =>
         {
            int id = reader.GetInt32(reader.GetOrdinal("QuestID"));
            string name = reader.GetString(reader.GetOrdinal("Name"));
            int questTypeID = reader.GetInt32(reader.GetOrdinal("QuestTypeID"));

            Console.WriteLine($"[{id}] {name}  -  Type: {questTypeID}");
         });
      }

      //
      // Example Func-style method: returns a List<Quest>
      //
      public List<Quest> GetAllQuestsAsList()
      {
         var list = new List<Quest>();
         string sql = "SELECT QuestID, Name, QuestTypeID FROM Quests ORDER BY QuestID";

         QueryDatabase(sql, reader =>
         {
            var q = new Quest
            {
               QuestID = reader.GetInt32(reader.GetOrdinal("QuestID")),
               Name = reader.GetString(reader.GetOrdinal("Name")),
               QuestTypeID = reader.GetInt32(reader.GetOrdinal("QuestType"))
            };
            list.Add(q);
         });

         return list;
      }

      public bool AddQuest(string name, string description, int? questTypeID)
      {
         string sql = "INSERT INTO Quests (Name, Description, QuestTypeID) VALUES (@name, @description, @type)";
         var parameters = new Dictionary<string, object>
            {
                { "@name", name },
                { "@description", description},
                { "@type", (object?)questTypeID ?? DBNull.Value }
            };

         int rows = ExecuteNonQuery(sql, parameters);
         return rows > 0;
      }

      public bool UpdateQuestName(int questId, string newName)
      {
         string sql = "UPDATE Quests SET Name = @name WHERE QuestID = @id";
         var parameters = new Dictionary<string, object>
            {
                { "@name", newName },
                { "@id", questId }
            };
         return ExecuteNonQuery(sql, parameters) > 0;
      }

      public bool DeleteQuest(int questId)
      {
         string sql = "DELETE FROM Quests WHERE QuestID = @id";
         var parameters = new Dictionary<string, object>
         {
            { "@id", questId }
         };
         return ExecuteNonQuery(sql, parameters) > 0;
      }

      public bool DeleteReward(int rewardId)
      {
         string sql = "DELETE FROM Rewards WHERE RewardID = @id";
         var parameters = new Dictionary<string, object>
         {
            { "@id", rewardId }
         };
         return ExecuteNonQuery(sql, parameters) > 0;
      }

      public bool AddReward(int questId, string rewardType, int amount, string? additionalEffects)
      {
         string sql = "INSERT INTO Rewards (QuestID, RewardType, Amount, AdditionalEffects) VALUES (@questID, @rewardType, @amount, @additionalEffects)";
         var parameters = new Dictionary<string, object>
         {
            { "@questID", questId },
            { "@rewardType", rewardType },
            { "@amount", amount },
            { "@additionalEffects", (object?)additionalEffects ?? DBNull.Value }
         };
         return ExecuteNonQuery(sql, parameters) > 0;
      }

      public void GetQuestRewards(int questID)
      {
         string sql = "SELECT q.Name, r.RewardID, r.RewardType, r.Amount, r.AdditionalEffects FROM Quests q LEFT JOIN Rewards r ON q.QuestID = r.QuestID WHERE q.QuestID = @questID ORDER BY r.RewardID";

         var parameters = new Dictionary<string, object>
         {
            { "@questID", questID }
         };

         bool printedHeader = false;
         bool foundAnyRewards = false;

         QueryDatabase(sql, reader =>
         {
            if (!printedHeader)
            {
               string name = reader.GetString(reader.GetOrdinal("Name"));
               Console.WriteLine($" ~~~~ {name} ~~~ ");
               printedHeader = true;
            }

            if (reader.IsDBNull(reader.GetOrdinal("RewardID"))) 
               return;
            else 
               foundAnyRewards = true;

            int rewardId = reader.GetInt32(reader.GetOrdinal("RewardID"));
            string rewardType = reader.GetString(reader.GetOrdinal("RewardType"));
            int amount = reader.GetInt32(reader.GetOrdinal("Amount"));
            string additionalEffects = reader.IsDBNull(reader.GetOrdinal("AdditionalEffects")) ? null : reader.GetString(reader.GetOrdinal("AdditionalEffects"));

            Console.Write($" - Reward #{rewardId}: {rewardType} ({amount})");
            if (additionalEffects != null) Console.Write($" | {additionalEffects} |");
            Console.WriteLine();

         }, parameters);

         if (!foundAnyRewards)
            Console.WriteLine(" No rewards for this quest.\n");
      }
   }
}
