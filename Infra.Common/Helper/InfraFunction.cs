using System;

namespace Infra.Common
{
    public static class InfraFunction
    {
        public static int GetAge(DateTime birthdate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthdate.Year;
            if (birthdate > today.AddYears(-age)) age--;
            return age;
        }
        public static object GetPropertyValue(object obj, string name)
        {
            return obj == null ? null : obj.GetType()
                                           .GetProperty(name)
                                           .GetValue(obj, null);
        }
    }
}
