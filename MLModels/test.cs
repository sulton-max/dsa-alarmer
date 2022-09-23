using GIBS_Api_Model;

namespace MLModels
{
    public class test
    {
        public void Start()
        {

            var filesDirectory = @"D:\PROJECTS\PERSONAL\DSAlarmer\Research materials\Sample Images";
            var dsImageSamples = Path.Combine(filesDirectory, "DS");
            var noDSImageSamples = Path.Combine(filesDirectory, "No DS");

            // Analyzing positive reuslts
            Console.WriteLine("Analyzing dust images");
            var filesPath = Directory.GetFiles(dsImageSamples, "*.jpg").ToList();
            var modelOutputs = filesPath
                .Select(x =>
                {
                    var imageBytes = File.ReadAllBytes(x);
                    var sampleData = new DustAnalyzeModel.ModelInput { ImageSource = imageBytes, Label = "DS" };
                    return DustAnalyzeModel.Predict(sampleData);
                }).ToList();

            modelOutputs.ForEach(x =>
            {
                Console.WriteLine();
                Console.WriteLine($"Predicted label : {x.PredictedLabel}");
                Console.WriteLine($"DS : {x.Score[0]}");
                Console.WriteLine($"No DS : {x.Score[1]}");
            });

            // Analyzing negative reuslts
            Console.WriteLine("Analyzing no dust images");

            filesPath = Directory.GetFiles(noDSImageSamples, "*.jpg").ToList();
            modelOutputs = filesPath
                .Select(x =>
                {
                    var imageBytes = File.ReadAllBytes(x);
                    var sampleData = new DustAnalyzeModel.ModelInput { ImageSource = imageBytes, Label = "DS" };
                    return DustAnalyzeModel.Predict(sampleData);
                }).ToList();

            modelOutputs.ForEach(x =>
            {
                Console.WriteLine();
                Console.WriteLine($"Predicted label : {x.PredictedLabel}");
                Console.WriteLine($"DS : {x.Score[0]}");
                Console.WriteLine($"No DS : {x.Score[1]}");
            });
        }
    }
}
