using System;
using RedisTr.models;

namespace RedisTr.utils
{
    public static class Utils
    {
        public static string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789йцукенгшщзхъдлорпавыфячсмить";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return  new string(stringChars);
        }

        public static string ErrorPostfix(MDataIn mDataIn)
        {
            return $"(hotel:{mDataIn.HotelId} PosId:{mDataIn.PosId})";
        }

        public static string ChanelName(MDataIn mDataIn)
        {
            return $"canel:{mDataIn.HotelId}:{mDataIn.PosId}";
        }
    }
}
