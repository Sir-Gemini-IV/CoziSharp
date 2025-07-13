namespace CoziSharp.Helpers
{
    public static class LoginHelpers
    {
        public static async Task<bool> TryLoginAsync(CoziClient client, string username, string password)
        {
            try
            {
                await client.LoginAsync(username, password);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
