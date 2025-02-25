using System;

namespace FashionApp.Data.Constants
{
    public static class AppConstants
    {
        /// <summary>
        /// Constants related to application messages
        /// </summary>
        public static class Messages
        {
            public const string WELCOME_GUEST = "Welcome Guest!";
            public const string WELCOME_USER = "Welcome User!";
            public const string LOGIN_AS_USER = "Login as User";
            public const string LOGOUT = "Logout";
            public const string CREATED_BY = "Created by RedFox - AI Айляк";
            public const string GUEST_MESSAGE = "The logged-in users have access to additional features.";
            public const string USER_MESSAGE = "Adding a prompt field for creation an image.";
        }

        /// <summary>
        /// Constants related to error handling
        /// </summary>
        public static class Errors
        {         
            public const string PLEASE_ENTER_SOME_TEXT = "Please enter some text.";
            public const string ERROR = "Error";
            public const string FAILED_TO_SAVE_IMAGE = "Failed to save image";
        }

        /// <summary>
        /// General application constants
        /// </summary>
        public static class ImagesConstants
        {
            public const string CLOSED_JACKET_MASK = "closed_jacket_mask.png";
            public const string OPEN_JACKET_MASK = "open_jacket_mask.png";
            public const string IMAGES_DIRECTORY = "Pictures/FashionApp/Images";
            public const string IMAGES_MASKS_DIRECTORY = "FashionApp/MasksImages";
            public const string IMAGES_CAPTURE_SCREEN = "FashionApp/CaptureScreen";
            public const string IMAGES_CREATED_IMAGES = "FashionApp/Images";
        }

        // Add additional constant groups as needed
        public static class Parameters
        {
            public const string CONFY_FUNCTION_NAME = "generate_image";
            public const string CONFY_FUNCTION_ARG = "bottle";
            public const string APP_CLOTH_GALLERY = "Cloth Gallery";
            public const string APP_FULLPATH_CAPTURE_SCREEN = "/storage/emulated/0/Pictures/FashionApp/CaptureScreen%";
            public const string APP_NAME = "FashionApp";
        }
    }
}
