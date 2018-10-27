using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerBN
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int numOfPlayers = 0, i = 0;
                Poker p = new Poker();
                Console.Write("How many players? : ");
                numOfPlayers = Int32.Parse(Console.ReadLine());
                while (i < numOfPlayers)
                {
                    Console.Write("{0} Player Name : ", i + 1); string player = Console.ReadLine();
                    if (p.CheckPlayer(player))
                    {
                        Console.WriteLine("This player name exists, please enter a unique name to identify");
                        continue;
                    }
                    Console.Write("{0} Player Hand : ", i + 1); string hand = Console.ReadLine();
                    p.AddPlayer(player, hand);
                    i++;
                }

                string winner = p.FindWinner();
                Console.WriteLine(winner + " wins");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}
