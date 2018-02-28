using System;

namespace ASFT.Client
{
    public class ImageSize
    {
        public ImageSize(string size)
        {
            var separator = new[] {'x'};
            var strArray = size.Split(separator);

            if (strArray.Length != 2)
                throw new ArgumentException("Invalid size string. Should be in format 200x300. CommitSize = " + size);

            Height = Convert.ToInt32(strArray[0]);
            Width = Convert.ToInt32(strArray[1]);
        }

        public ImageSize(int width, int height)
        {
            Height = height;
            Width = width;
        }

        public int Height { get; }

        public int Width { get; }

        public override string ToString()
        {
            return string.Format("{0}x{1}", Width, Height);
        }
    }
}