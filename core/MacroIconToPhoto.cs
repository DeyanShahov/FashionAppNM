using FashionApp.Data.Constants;

namespace FashionApp.core
{
    internal class MacroIconToPhoto
    {
        public static readonly Dictionary<string, string> buttonContentToPhotos = new Dictionary<string, string>
        {
            {"dress", AppConstants.ImagesConstants.DRESS_MASK },
            {"dress_full", AppConstants.ImagesConstants.DRESS_FULL_MASK },
            {"jacket", AppConstants.ImagesConstants.JACKET_MASK },
            {"jacket_closed", AppConstants.ImagesConstants.JACKET_CLOSED_MASK },
            {"jacket_open", AppConstants.ImagesConstants.JACKET_OPEN_MASK },
            {"no_set", AppConstants.ImagesConstants.NO_SET_MASK },
            {"pants", AppConstants.ImagesConstants.PANTS_MASK },
            {"pants_short", AppConstants.ImagesConstants.PANTS_SHORT_MASK },
            {"raincoat", AppConstants.ImagesConstants.RAINCOAT_MASK },
            {"shirt", AppConstants.ImagesConstants.SHIRT_MASK },
            {"skirt", AppConstants.ImagesConstants.SKIRT_MASK },
            {"skirt_long", AppConstants.ImagesConstants.SKIRT_LONG_MASK },
            {"tank_top", AppConstants.ImagesConstants.TANK_TOP_MASK }
        };

        public string? Value { get; private set; }

        public MacroIconToPhoto(string buttonContent)
        {
            CheckForPath(buttonContent);
        }

        private void CheckForPath(string buttonContent)
        {
            if (buttonContentToPhotos.TryGetValue(buttonContent, out var value)) Value = value;
            else Value = "Default";
        }
    }
}
