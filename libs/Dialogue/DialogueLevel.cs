using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace libs.Dialogue
{
    public class DialogueLevel
    {
        private List<DialogueNode> dialogueNodes;
        private DialogueNode currentNode;
        private DialogueLevelJson jsonObject;
        private int totalPoints;
        public bool IsLevelComplete { get; private set;}

        public DialogueLevel(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<DialogueLevelJson>(json);
            dialogueNodes = jsonObject.Nodes;
            currentNode = dialogueNodes.FirstOrDefault(node => node.Id == jsonObject.StartNodeId); // Use StartNodeId
            totalPoints = 0;
            IsLevelComplete = false; // Initialize to false
        }

        public void Run()
        {
            IsLevelComplete = false; // Reset at the beginning of Run
            while (currentNode != null)
            {
                Console.Clear();
                Console.WriteLine(currentNode.Text);

                if (!currentNode.Options.Any())
                {
                    Console.WriteLine("End of conversation.");
                    Console.WriteLine($"Total points: {totalPoints}");
                    Console.WriteLine("Press any key to proceed to the next level...");
                    Console.ReadKey(); // Wait for user input before proceeding to the next level
                    IsLevelComplete = true; // Set to true when the level is complete
                    return; // Exit the loop after displaying the ending
                }

                for (int i = 0; i < currentNode.Options.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {currentNode.Options[i].Text}");
                }
                Console.WriteLine("Enter your choice:");

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice) && choice > 0 && choice <= currentNode.Options.Count)
                {
                    var selectedOption = currentNode.Options[choice - 1];
                    totalPoints += selectedOption.Points;
                    
                     // If it's the last question, determine the ending now
                    if (currentNode != null && currentNode.Id == "placeholder")
                    {
                       {
                        string endingNodeId = totalPoints >= 0
                            ? (jsonObject.EndingType == "doctor" ? "doctor_ending_good" : "doctor_ending_good")
                            : (jsonObject.EndingType == "uncle" ? "uncle_ending_bad" : "uncle_ending_bad");

                currentNode = dialogueNodes.FirstOrDefault(node => node.Id == endingNodeId);

                         // Print the ending text once and exit the loop
                        if (currentNode != null)
                        {
                            Console.Clear();
                            Console.WriteLine(currentNode.Text);
                            Console.WriteLine("End of conversation.");
                            Console.WriteLine($"Total points: {totalPoints}");
                            Console.WriteLine("Press any key to proceed to the next level...");
                            Console.ReadKey(); // Wait for user input before proceeding to the next level
                            IsLevelComplete = true; // Set to true when the level is complete
                        }

                        return; // Exit the loop after displaying the ending

                        }
                    }
                    else
                    {
                        currentNode = dialogueNodes.FirstOrDefault(node => node.Id == selectedOption.NextNodeId);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                }
            }
        }
    }

    public class DialogueLevelJson
    {
        public string LevelType { get; set; }
        public string LevelName { get; set; }
         public string StartNodeId { get; set; }
         public string EndingType { get; set; }
        public List<DialogueNode> Nodes { get; set; }
    }

    public class DialogueNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<DialogueOption> Options { get; set; }
    }

    public class DialogueOption
    {
        public string Text { get; set; }
        public int Points { get; set; }
        public string NextNodeId { get; set; }
    }
}
