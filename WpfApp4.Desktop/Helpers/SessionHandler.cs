namespace WpfApp4.Desktop.Helpers
{
    public static class SessionHandler
    {
        public static int UserId { get; set; }
        public static string UserRole { get; set; }

        public static void SetSession(int id, string role)
        {
            UserId = id;
            UserRole = role;
        }
    }
}
