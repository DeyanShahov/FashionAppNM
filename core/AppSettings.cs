namespace FashionApp.core
{
    public static class AppSettings
    {
        private static string userName = "Guest";
        private static string phoneIdentificatoryNumber = "0";
        private static int tokens = 0;

        public static string UserName
        {
            get => userName;
            set => userName = value;
        }
       
        public static string PhoneIdentificatoryNumber
        {
            get => phoneIdentificatoryNumber;
            set => phoneIdentificatoryNumber = value;
        }
   
        public static int Tokens
        {
            get => tokens; 
            set => tokens = value;
        }

    }
}
