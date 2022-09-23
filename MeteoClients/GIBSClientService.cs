using GIBS.API.Client.Models;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;

namespace GIBS.API.Client
{
    public class GIBSClientService
    {
        public const string LayerIdentifierToken = "{{LayerIdentifier}}";
        public const string TimeToken = "{{Tiem}}";
        public const string TileMatrixSetToken = "{{TileMatrixSet}}";
        public const string TileMatrixToken = "{{TileMatrix}}";
        public const string TileRowToken = "{{TimeRow}}";
        public const string TileColToken = "{{TileCol}}";
        public const string FormatSetToken = "{{FormatSet}}";

        protected string BaseURL = "https://gibs.earthdata.nasa.gov/wmts/epsg4326/best/{{LayerIdentifier}}/default/{{Tiem}}/{{TileMatrixSet}}/{{TileMatrix}}/{{TimeRow}}/{{TileCol}}.{{FormatSet}}";

        public async Task GetImages()
        {
            await Task.Run(async () =>
            {
                var initialDate = new DateTime(2018, 5, 1);
                var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Sample Images");
                var httpClient = new HttpClient();
                var tasksList = new List<Task<IEnumerable<string>>>();

                // Get images for a month
                for (int index = 0; index < 60; index++)
                {
                    var imageDate = initialDate.AddDays(index);
                    var topImageFilter = GetDefaultFilter(imageDate, true);
                    var bottomImageFilter = GetDefaultFilter(imageDate, false);

                    tasksList.Add(Task.Run(async () =>
                    {
                        // Create image filters

                        // Load images
                        var topImage = LoadImage(httpClient, topImageFilter);
                        var bottomImage = LoadImage(httpClient, bottomImageFilter);

                        // Write images
                        var topImagePath = WriteFile(await topImage, Path.Combine(fileDirectory, Path.ChangeExtension(Guid.NewGuid().ToString(), topImageFilter.FormatSet)));
                        var bottomImagePath = WriteFile(await bottomImage, Path.Combine(fileDirectory, Path.ChangeExtension(Guid.NewGuid().ToString(), topImageFilter.FormatSet)));

                        // Merge images
                        var imageService = new ImageService();
                        using var stream = CreateNewFileStream(Path.Combine(fileDirectory, Path.ChangeExtension(imageDate.ToString("yyyy-MM-dd"), topImageFilter.FormatSet)));
                        if (await imageService.MergeImages(await topImagePath, await bottomImagePath, stream))
                            stream.Flush();

                        // Delete old images
                        return new[]
                        {
                            topImagePath.Result,
                            bottomImagePath.Result
                        }.AsEnumerable();
                    }));
                }

                Task.WaitAll(tasksList.ToArray());
                tasksList
                    .Select(x => x.Result)
                    .ToList()
                    .ForEach(x => x
                        .ToList()
                        .ForEach(y => File.Delete(y)));
            });
        }

        public async Task<byte[]> LoadImage(HttpClient client, GIBSImageFilter filter)
        {
            return await Task.Run(async () =>
            {
                // Send request for image
                var test = GetURL(filter);
                var message = new HttpRequestMessage(HttpMethod.Get, GetURL(filter));
                var response = await client.SendAsync(message);

                // Return image as byte
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync()
                    : new byte[0];
            });
        }

        public async Task<string> WriteFile(byte[] file, string filePath)
        {
            if (file == null || !file.Any() | string.IsNullOrWhiteSpace(filePath))
                throw new InvalidOperationException();

            return await Task.Run(() =>
            {
                var directory = new FileInfo(filePath).Directory;
                if (directory != null && !Directory.Exists(directory.Name))
                    Directory.CreateDirectory(directory.Name);

                using var fw = File.Create(filePath);
                fw.Write(file);
                fw.Flush();

                return filePath;
            });
        }

        public void ClearStream(Stream stream)
        {
            stream.Flush();
            stream.Close();
        }

        public Stream CreateNewFileStream(string filePath)
        {
            return File.Create(filePath);
        }

        public string GetURL(GIBSImageFilter filter)
        {
            return BaseURL
                .Replace(LayerIdentifierToken, filter.LayerIdentifier)
                .Replace(TimeToken, filter.Time.ToString("yyyy-MM-dd"))
                .Replace(TileMatrixSetToken, filter.TileMatrixSet)
                .Replace(TileMatrixToken, filter.TileMatrix.ToString())
                .Replace(TileRowToken, filter.TileRow.ToString())
                .Replace(TileColToken, filter.TileCol.ToString())
                .Replace(FormatSetToken, filter.FormatSet);
        }

        public GIBSImageFilter GetDefaultFilter(DateTime dateTime, bool isTop)
        {
            return new GIBSImageFilter
            (
                "MODIS_Terra_CorrectedReflectance_TrueColor",
                dateTime,
                "250m",
                5,
                isTop ? 4U : 5,
                26,
                "jpg"
            );
        }
    }
}
