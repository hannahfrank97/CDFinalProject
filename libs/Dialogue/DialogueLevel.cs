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
        private int totalPoints;

        public DialogueLevel(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<DialogueLevelJson>(json);
            dialogueNodes = jsonObject.Nodes;
            currentNode = dialogueNodes.FirstOrDefault(node => node.Id == "doctor_question1");
            totalPoints = 0;
        }

        public void Run()
        {
            while (currentNode != null)
            {
                Console.Clear();
                Console.WriteLine(currentNode.Text);

                if (!currentNode.Options.Any())
                {
                    string endingNodeId = totalPoints >= 0 ? "doctor_ending_good" : "doctor_ending_bad";
                    currentNode = dialogueNodes.FirstOrDefault(node => node.Id == endingNodeId);

                    if (currentNode != null)
                    {
                        Console.WriteLine(currentNode.Text);
                    }

                    Console.WriteLine("End of conversation.");
                    Console.WriteLine($"Total points: {totalPoints}");
                    return;
                }

                for (int i = 0; i < currentNode.Options.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {currentNode.Options[i].Text}");
                }

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice) && choice > 0 && choice <= currentNode.Options.Count)
                {
                    var selectedOption = currentNode.Options[choice - 1];
                    totalPoints += selectedOption.Points;
                    currentNode = dialogueNodes.FirstOrDefault(node => node.Id == selectedOption.NextNodeId);
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
