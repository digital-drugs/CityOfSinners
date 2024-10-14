using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Vote
    {
        public int count;
        public Vote()
        {

        }

        public void AddVote(int value)
        {
            count += value;
        }
    }
}
