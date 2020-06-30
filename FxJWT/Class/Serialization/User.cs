using System;
using System.Collections.Generic;
using System.Text;

namespace FxJWT.Class.Serialization
{
    class User
    {
        public int IdClient { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short MinutesAlive { get; set; }

    }
}
