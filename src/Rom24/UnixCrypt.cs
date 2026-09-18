using System.Text;
using CryptSharp;

namespace Rom24
{
    /// <summary>
    /// libc crypt(3) traditional DES, as used by nanny.c:
    /// strcmp(crypt(argument, ch->pcdata->pwd), ch->pcdata->pwd)
    /// </summary>
    public static class UnixCrypt
    {
        public static string crypt(string key, string salt)
	{
    if (key == null) key = "";
    if (string.IsNullOrEmpty(salt))
        return key;
    if (salt.Length > 2)
        salt = salt.Substring(0, 2);
    return Crypter.TraditionalDes.Crypt(Encoding.Latin1.GetBytes(key), salt);
	}
    }
}
