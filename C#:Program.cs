dotnet add package Azure.AI.Language.Conversations --version 1.1.0
  // import namespaces
  using Azure;
using Azure.AI.Language.Conversations;
 // Create a client for the Language service model
 Uri endpoint = new Uri(predictionEndpoint);
 AzureKeyCredential credential = new AzureKeyCredential(predictionKey);

 ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential);
 // Call the Language service model to get intent and entities
 var projectName = "Clock";
 var deploymentName = "production";
 var data = new
 {
     analysisInput = new
     {
         conversationItem = new
         {
             text = userText,
             id = "1",
             participantId = "1",
         }
     },
     parameters = new
     {
         projectName,
         deploymentName,
         // Use Utf16CodeUnit for strings in .NET.
         stringIndexType = "Utf16CodeUnit",
     },
     kind = "Conversation",
 };
 // Send request
 Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data));
 dynamic conversationalTaskResult = response.Content.ToDynamicFromJson(JsonPropertyNames.CamelCase);
 dynamic conversationPrediction = conversationalTaskResult.Result.Prediction;   
 var options = new JsonSerializerOptions { WriteIndented = true };
 Console.WriteLine(JsonSerializer.Serialize(conversationalTaskResult, options));
 Console.WriteLine("--------------------\n");
 Console.WriteLine(userText);
 var topIntent = "";
 if (conversationPrediction.Intents[0].ConfidenceScore > 0.5)
 {
     topIntent = conversationPrediction.TopIntent;
 }
 // Apply the appropriate action
 switch (topIntent)
 {
     case "GetTime":
         var location = "local";           
         // Check for a location entity
         foreach (dynamic entity in conversationPrediction.Entities)
         {
             if (entity.Category == "Location")
             {
                 //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                 location = entity.Text;
             }
         }
         // Get the time for the specified location
         string timeResponse = GetTime(location);
         Console.WriteLine(timeResponse);
         break;
     case "GetDay":
         var date = DateTime.Today.ToShortDateString();            
         // Check for a Date entity
         foreach (dynamic entity in conversationPrediction.Entities)
         {
             if (entity.Category == "Date")
             {
                 //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                 date = entity.Text;
             }
         }            
         // Get the day for the specified date
         string dayResponse = GetDay(date);
         Console.WriteLine(dayResponse);
         break;
     case "GetDate":
         var day = DateTime.Today.DayOfWeek.ToString();
         // Check for entities            
         // Check for a Weekday entity
         foreach (dynamic entity in conversationPrediction.Entities)
         {
             if (entity.Category == "Weekday")
             {
                 //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                 day = entity.Text;
             }
         }          
         // Get the date for the specified day
         string dateResponse = GetDate(day);
         Console.WriteLine(dateResponse);
         break;
     default:
         // Some other intent (for example, "None") was predicted
         Console.WriteLine("Try asking me for the time, the day, or the date.");
         break;
 }
