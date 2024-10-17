using System.Runtime.InteropServices;

namespace Utils
{
    public class JsUtils
    {
        [DllImport("__Internal")]
        public static extern string GetUserInfo();
    }
}