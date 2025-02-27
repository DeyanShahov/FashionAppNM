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
            public const string PICK_AN_IMAGE = "Pick an image";
            public const string REPLACE_CONFIRMATION = "Replace Confirmation";
            public const string MESSAGE_FOR_REPLACE = "Are you sure you want to replace the";
            public const string SET_CONFIRMATION = "Set Confirmation";
            public const string MESSAGE_FOR_SAVE_MASK = "You will save a new mask image.";
            public const string YES = "Yes";
            public const string CANCEL = "Cancel";
            public const string OK = "OK";
        }

        /// <summary>
        /// Constants related to error handling
        /// </summary>
        public static class Errors
        {         
            public const string PLEASE_ENTER_SOME_TEXT = "Please enter some text.";
            public const string ERROR = "Error";
            public const string FAILED_TO_SAVE_IMAGE = "Failed to save image";
            public const string SELECT_A_VALID_IMAGE = "Please select a valid image file (jpg or png)";
            public const string SELECT_BOTH_IMAGES = "Please select both images first";
            public const string ERROR_CKECK_MASKS = "Error checking masks";
            public const string ERROR_OCCURRED = "An error occurred";
            public const string MISSING_PATH_TO_FILES = "Missing path to files!";
            public const string ERROR_DELETE_FILE = "Грешка при изтриване на файл";
            public const string FILE_NOT_FOUND = "File not found";
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
            public const string CONFY_FUNCTION_GENERATE_NAME = "generate_image";
            public const string CONFY_FUNCTION_GENERATE_ARG = "bottle";
            public const string CONFY_FUNCTION_COMBINE_ENDPOINT = "combine_images";
            public const string APP_CLOTH_GALLERY = "Cloth Gallery";
            public const string APP_FULLPATH_CAPTURE_SCREEN = "/storage/emulated/0/Pictures/FashionApp/CaptureScreen%";
            public const string APP_FULLPATH_MASKS_IMAGES = "/storage/emulated/0/Pictures/FashionApp/MasksImages/";
            public const string APP_NAME = "FashionApp";
            public const string INPUT_IMAGE_CLOTH = "input_image_cloth.png";
            public const string INPUT_IMAGE_BODY = "input_image_body.png";
        }
    }
}
