using System;

namespace FxJWT.Class
{
    class Settings
    {

        public static string GetVariable(string variable)
        {
            if (variable != null)
            {
                string value = Environment.GetEnvironmentVariable(variable);
                if (value != null)
                {
                    return value;
                } else
                {
                    throw new NullReferenceException($"Unable to get '{variable}' enviroment variable");
                }
            } else
            {
                throw new NullReferenceException($"Variable can not be null");
            }
        }

    }
}
