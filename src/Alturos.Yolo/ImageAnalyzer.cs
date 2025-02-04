﻿#region

using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Alturos.Yolo
{
    public class ImageAnalyzer
    {
        private readonly Dictionary<string, byte[]> _imageFormats = new Dictionary<string, byte[]>();

        public ImageAnalyzer()
        {
            var bmp = Encoding.ASCII.GetBytes("BM"); //BMP
            var png = new byte[] {137, 80, 78, 71}; //PNG
            var jpeg = new byte[] {255, 216, 255}; //JPEG

            _imageFormats.Add("bmp", bmp);
            _imageFormats.Add("png", png);
            _imageFormats.Add("jpeg", jpeg);
        }

        public bool IsValidImageFormat(byte[] imageData)
        {
            if (imageData == null)
            {
                return false;
            }

            if (imageData.Length <= 3)
            {
                return false;
            }

            foreach (var imageFormat in _imageFormats)
            {
                if (imageData.Take(imageFormat.Value.Length).SequenceEqual(imageFormat.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}