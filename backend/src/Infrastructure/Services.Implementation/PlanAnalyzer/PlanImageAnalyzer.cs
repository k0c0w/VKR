using Compunet.YoloSharp;
using Compunet.YoloSharp.Memory;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using ResultMonad;
using Services.Implementation.PlanAnalyzer.Ocr;
using Services.PlanImageAnalyzer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Services.Implementation.PlanAnalyzer;

public class PlanImageAnalyzer(YoloPredictor yoloPredictor, ILogger<IPlanImageAnalyzerService> logger, IOcr ocrSystem)
    : IPlanImageAnalyzerService
{
    public async Task<Result<RoomOnImage[], ErrorMessage>> FindRoomsAtImageAsync(Stream image, CancellationToken ct)
    {
        ImageWithBounds[]? masks = null;
        try
        {
            masks = await SegmentImageAsync(image, ct);
            if (masks.Length == 0)
            {
                return Result.Ok<RoomOnImage[], ErrorMessage>([]);
            }

            image.Position = 0;
            var foundText = await EnrichRoomDetectionsWithTextAsync(image, masks, ct);

            var rooms = new RoomOnImage[masks.Length];
            for (var i = 0; i < masks.Length; i++)
            {
                rooms[i] = new RoomOnImage
                {
                    Base64EncodedMask = ConvertBitmapToBase64(masks[i].Image),
                    Text = foundText[i],
                    BoundingBox = masks[i].Bounds
                };
            }

            return Result.Ok<RoomOnImage[], ErrorMessage>(rooms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during image segmentation: {message}", ex.Message);
            
            return Result.Fail<RoomOnImage[], ErrorMessage>(
                ErrorMessage.SystemError("Произошла ошибка при сегментации изображения."));
        }
        finally
        {
            if (masks != null)
            {
                foreach (var mask in masks)
                {
                    mask.Dispose();
                }
            }
        }
    }

    private async Task<ImageWithBounds[]> SegmentImageAsync(Stream image, CancellationToken ct)
    {
        var segmentationResult = await yoloPredictor.SegmentAsync(image);
        ct.ThrowIfCancellationRequested();

        return segmentationResult
            .Where(x => x.Confidence > 0.5)
            .Select(segmentation =>
            {
                var mask = BitmapToImage(segmentation.Mask);
                var bounds = segmentation.Bounds;
                int[] bbox = [bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height];

                return new ImageWithBounds(mask, bbox);
            })
            .ToArray();
    }

    private async Task<string?[]> EnrichRoomDetectionsWithTextAsync(Stream image, ImageWithBounds[] masks, CancellationToken ct)
    {
        var foundText = new string?[masks.Length];
        using var originalImage = Image.Load<Rgba32>(image);

        for (var i = 0; i < masks.Length; i++)
        {
            ct.ThrowIfCancellationRequested();

            await using var croppedImageStream = CropImageByMask(originalImage, masks[i].Image, masks[i].Bounds);
            if (croppedImageStream is null)
            {
                foundText[i] = default;
                continue;
            }

            try
            {
                var text = await ocrSystem.GetTExtFromImageAsync(croppedImageStream, ct);
                if (text is { Length: > 0 })
                {
                    foundText[i] = string.Join(',', text);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to extract text for room: {message}", ex.Message);
                foundText[i] = null;
            }
        }

        return foundText;
    }

    private static Image<L8> BitmapToImage(BitmapBuffer bitmap)
    {
        const float confidence = 0.6f;
        const byte foregroundMarker = 255;
        const byte backgroundMarker = 0;
        
        var maskImage = new Image<L8>(bitmap.Width, bitmap.Height);
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var isForeground = bitmap[y, x] > confidence;
                maskImage[x, y] = new L8(isForeground ? foregroundMarker : backgroundMarker);
            }
        }

        return maskImage;
    }

    private static string ConvertBitmapToBase64(Image<L8> mask)
    {
        const byte foreground = 255;
        var width = mask.Width;
        var height = mask.Height;
        var byteCount = (width * height + 7) / 8;
        var bytes = new byte[byteCount];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (mask[x, y].PackedValue != foreground)
                {
                    continue;
                }

                var bitIndex = y * width + x;
                var byteIndex = bitIndex / 8;
                var bitOffset = bitIndex % 8;
                bytes[byteIndex] |= (byte)(1 << (7 - bitOffset));
            }
        }

        return Convert.ToBase64String(bytes);
    }

    private Stream? CropImageByMask(Image<Rgba32> originalImage, Image<L8> mask, int[] bounds)
    {
        try
        {
            int x = bounds[0], y = bounds[1], width = bounds[2], height = bounds[3];

            using var croppedImage = new Image<Rgba32>(width, height);
            const byte foreground = 255;
            for (var dy = 0; dy < height; dy++)
            {
                for (var dx = 0; dx < width; dx++)
                {
                    if (mask[dx, dy].PackedValue == foreground)
                    {
                        croppedImage[dx, dy] = originalImage[x + dx, y + dy];
                    }
                }
            }

            var outputStream = new MemoryStream();
            croppedImage.SaveAsPng(outputStream);
            outputStream.Position = 0;
            return outputStream;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to crop image by mask: {message}", ex.Message);
            return null;
        }
    }

    private readonly record struct ImageWithBounds(Image<L8> Image, int[] Bounds) : IDisposable
    {
        public void Dispose()
        {
            Image.Dispose();
        }
    }
}