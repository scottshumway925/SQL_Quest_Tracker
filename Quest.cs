namespace QuestTracker.Models
{
   public class Quest
   {
      public int QuestID { get; set; }
      public string Name { get; set; } = string.Empty;
      public int? QuestTypeID { get; set; }
   }
}
