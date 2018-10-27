using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PokerBN
{
    class Card
    {
        public int Rank { get; set; }
        public char Suit { get; set; }
        char[] suits = new char[] { 'H', 'C', 'D', 'S' };
        public Card() { }

        public Card(int p1, char p2)
        {
            Rank = p1;
            Suit = p2;
            if (!suits.Contains(Suit))
                throw new KeyNotFoundException();
        }
    }

    class Hand
    {
        public List<Card> card = new List<Card>();
        const string pattern = @"\W+";
        int Flush = 0;
        int ThreeOK = 0;
        int TwoOK = 0;

        public Hand() { }

        public Hand(string p1)
        {
            separate(p1);
        }

        public void separate(string s)
        {
            CardRanks cr = new CardRanks();

            foreach (string sub in Regex.Split(s, pattern))
            {
                string[] x = Regex.Split(sub, "(?=[HCDS])");
                Card c = new Card(cr.getRank(x[0]), x[1][0]);
                card.Add(c);
            }
            card.Sort((x, y) => x.Rank.CompareTo(y.Rank));
            if (card.Count != 5)
                throw new Exception("Each player should have 5 cards");
        }
        public void setFlush(int p1) { Flush = p1; }
        public int getFlush() { return Flush; }
        public void set3ofK(int p1) { ThreeOK = p1; }
        public int get3ofK() { return ThreeOK; }
        public void set2ofK(int p1) { TwoOK = p1; }
        public int get2ofK() { return TwoOK; }
    }

    class Player
    {
        string _name;
        public Hand _hand;

        public Player() { }

        public Player(string p1, string p2)
        {
            _name = p1;
            _hand = new Hand(p2);
        }

        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }
    }

    class CardRanks
    {
        Dictionary<string, int> cardRanks = new Dictionary<string, int>();

        public CardRanks() { CreateRankings(); }

        public void addRank(string _rank, int val)
        {
            cardRanks.Add(_rank, val);
        }

        public int getRank(string _rank)
        {
            try
            {
                if (cardRanks.TryGetValue(_rank, out int val))
                    return val;
                else
                    throw new KeyNotFoundException();
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Ranking is wrong/no such card type");
            }
        }

        public void CreateRankings()
        {
            int i = 0;
            addRank("A", 14);
            addRank("K", 13);
            addRank("Q", 12);
            addRank("J", 11);
            for (i = 10; i > 1; i--)
            {
                addRank(i.ToString(), i);
            }
        }
    }
    class Poker
    {
        Dictionary<string, Player> users = new Dictionary<string, Player>();
        string winner = "";
        bool tie = false;
        const int HAND_SIZE = 5;

        public Poker() { }

        public void AddPlayer(string name, string hand)
        {
            Player p = new Player(name, hand);
            users.Add(name, p);
        }

        public bool CheckPlayer(string name)
        {
            return users.ContainsKey(name);
        }

        public Player GetPlayer(string name)
        {
            return users[name];
        }

        private bool Comparator(int src)
        {
            int j = 0, tmp = 0;
            tie = true;

            foreach (KeyValuePair<string, Player> g in users)
            {
                if (src == 1 && g.Value._hand.getFlush() != 1)//come from flush but not flush don't check
                {
                    continue;
                }
                if (winner == "")
                {
                    winner = g.Key; tie = false;
                }
                else
                {
                    tmp = 0;
                    for (j = HAND_SIZE - 1; j >= 0; j--)
                    {
                        if (g.Value._hand.card[j].Rank > users[winner]._hand.card[j].Rank)
                        {
                            winner = g.Key;
                            tie = false;
                            break;
                        }
                        else if (g.Value._hand.card[j].Rank == users[winner]._hand.card[j].Rank)
                        {
                            tmp++;
                            tie = true;
                            if (tmp == HAND_SIZE) winner += ", " + g.Key;
                        }
                        else
                        {
                            tie = false;
                            break;
                        }
                    }
                }
            }
            if (winner == "")
                return false;
            if (src == 0 && tie == true)
                return true;
            if (tie)
                return false;

            return true;
        }

        private bool CheckFlush()
        {
            int i = 1;
            foreach (KeyValuePair<string, Player> g in users)
            {
                i = 1;
                foreach (Card c in g.Value._hand.card)
                {
                    if (g.Value._hand.card[0].Suit != c.Suit)
                    { i = 0; break; }
                }

                if (i == 1)
                    g.Value._hand.setFlush(1);
            }
            return Comparator(1);
        }

        private bool Check3OfAKind()
        {
            foreach (KeyValuePair<string, Player> g in users)
            {
                /*
                    possible triplets - already sorted
                     0 , 1 , 2 , 3 , 4
                     9S, 8D, 8S, 8H, 7C
                     8D, 8S, 8H, 7C, 4C
                     KH, QH, 8D, 8S, 8H
                */
                if (g.Value._hand.card[1].Rank == g.Value._hand.card[3].Rank
                    || g.Value._hand.card[0].Rank == g.Value._hand.card[2].Rank
                    || g.Value._hand.card[2].Rank == g.Value._hand.card[4].Rank)
                {
                    g.Value._hand.set3ofK(g.Value._hand.card[2].Rank);
                    break;
                }
            }
            return CheckKickers(3);
        }

        public bool CheckOnePair()
        {
            int j = 0;
            foreach (KeyValuePair<string, Player> g in users)
            {
                for (j = 4; j >= 1; j--)
                {
                    if (g.Value._hand.card[j].Rank == g.Value._hand.card[j - 1].Rank)
                    {
                        g.Value._hand.set2ofK(g.Value._hand.card[j].Rank);
                        break;
                    }
                }
            }
            return CheckKickers(2);
        }

        public bool CheckHighCard()
        {
            return Comparator(0);
        }

        public bool CheckKickers(int nOfAKind)
        {
            int j = 0;
            tie = true;
            foreach (KeyValuePair<string, Player> g in users)
            {
                if (nOfAKind == 2 && g.Value._hand.get2ofK() == 0)//
                {
                    continue;
                }
                else if (nOfAKind == 3 && g.Value._hand.get3ofK() == 0)
                {
                    continue;
                }

                if (winner == "")
                {
                    winner = g.Key; tie = false;
                }
                else
                {
                    if ((nOfAKind == 2 && users[winner]._hand.get2ofK() < g.Value._hand.get2ofK())
                        || (nOfAKind == 3 && users[winner]._hand.get3ofK() < g.Value._hand.get3ofK()))
                        winner = g.Key;
                    else if ((nOfAKind == 2 && users[winner]._hand.get2ofK() == g.Value._hand.get2ofK())
                        || (nOfAKind == 3 && users[winner]._hand.get3ofK() == g.Value._hand.get3ofK()))
                    {
                        // find greater one
                        for (j = HAND_SIZE - 1; j >= 0; j--)
                        {
                            if (nOfAKind == 2 && (g.Value._hand.card[j].Rank == g.Value._hand.get2ofK()))
                                continue;
                            if (nOfAKind == 3 && (g.Value._hand.card[j].Rank == g.Value._hand.get3ofK()))
                                continue;
                            if (g.Value._hand.card[j].Rank > users[winner]._hand.card[j].Rank)
                            {
                                winner = g.Key;
                                tie = false;
                                break;
                            }
                            else if (g.Value._hand.card[j].Rank == users[winner]._hand.card[j].Rank)
                            {
                                tie = true;
                            }
                            else
                            {
                                tie = false;
                                break;
                            }
                        }
                    }
                }
            }
            if (winner == "")
                return false;
            if (tie)
                return false;

            return true;
        }

        public string FindWinner()
        {
            if (CheckFlush())
                return winner;
            else if (Check3OfAKind())
                return winner;
            else if (CheckOnePair())
                return winner;
            else if (CheckHighCard())
                return winner;
            return winner;
        }
    }
}
