using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Connect4QLearning
{

    public class Program
    {

        public static bool PLAY_RANDOM = true;
        public static bool PLAY_SELF = true;

        public static int ROW_COUNT = 6;
        public static int COL_COUNT = 7;

        public static double SPLIT = .99;
        public static double LEFTOVER = ((1.0 - SPLIT) / (COL_COUNT - 1));

        public static double WIN_WEIGHT = 1.0;
        public static double TIE_WEIGHT = 0.0;
        public static double LOSS_WEIGHT = -1.0;
        public static double GAMMA = 0.25;
        public static Random random = new Random();

        public static Dictionary<StateAction, double> Q;
        public static Dictionary<StateAction, int> Q_count;

        public static void updateQ(StateAction sa, double ret)
        {
            double val = 0;
            if (!Q.TryGetValue(sa, out val))
            {
                Q[sa] = ret;
                Q_count[sa] = 1;
                return;
            }

            Q[sa] = (Q_count[sa] * Q[sa] + ret) / (Q_count[sa] + 1);
            Q_count[sa] = Q_count[sa] + 1;
        }

        public static double random_start()
        {
            return random.NextDouble() * (0.1);
        }

        public static int Pi(BoardState b)
        {
            int bestCol = -1;
            double bestWeight = -1;
            for (int i = 0; i < COL_COUNT; ++i)
            {
                StateAction sa = new StateAction(b, i);
                double value = 0;
                double weight = (Q.TryGetValue(sa, out value)) ? Q[sa] : random_start();
                if (weight > bestWeight)
                {
                    bestCol = i;
                    bestWeight = weight;
                }
            }

            List<Tuple<int, double>> probs = new List<Tuple<int, double>>();
            for (int i = 0; i < COL_COUNT; ++i)
            {
                probs.Add(new Tuple<int, double>(0, 0));
                if (i == bestCol)
                    probs[i] = new Tuple<int, double>(i, SPLIT);
                else
                    probs[i] = new Tuple<int, double>(i, LEFTOVER);
            }

            return pick_random(probs);
        }

        public static int pick_random(List<Tuple<int, double>> probs)
        {

            // get universal probability 
            double u = probs.Sum(p => p.Item2);

            // pick a random number between 0 and u
            double r = random.NextDouble() * u;

            double sum = 0;
            foreach (var t in probs)
            {
                // loop until the random number is less than our cumulative probability
                if (r <= (sum = sum + t.Item2))
                {
                    return t.Item1;
                }
            }
            return -1; //Should never get here.
        }


        public static int random_action()
        {
            return random.Next(0, COL_COUNT);

        }

        public class Episode
        {
            public List<StateAction> sas = new List<StateAction>();
            public Result result;
        }

        public static Episode genEpisode()
        {
            Episode e = new Episode();
            BoardState b = new BoardState(ROW_COUNT, COL_COUNT);
            Result r = Result.Invalid;
            int i = 0;
            for (; ; )
            {
                int p1Action = Pi(b);
                BoardState bn = b.move(p1Action, Player.Player1, ref r);

                while (r == Result.Invalid)
                {

                    p1Action = Pi(b);
                    bn = b.move(p1Action, Player.Player1, ref r);
                }
                StateAction sa = new StateAction(b, p1Action);
                e.sas.Add(sa);


                if (r == Result.Win || r == Result.Tie || r == Result.Loss)
                {
                    e.result = r;
                    return e;
                }
                int p2Action = -1;

                if (PLAY_RANDOM)
                {
                    p2Action = random_action();
                }
                else
                {
                    p2Action = Pi(bn);
                }
                b = bn.move(p2Action, Player.Player2, ref r);

                while (r == Result.Invalid)
                {

                    if (PLAY_RANDOM)
                    {
                        p2Action = random_action();
                    }
                    else
                    {
                        p2Action = Pi(bn);
                    }
                    p2Action = random_action();
                    b = bn.move(p2Action, Player.Player2, ref r);
                }

                if (r == Result.Win || r == Result.Tie || r == Result.Loss)
                {
                    if (r == Result.Win)
                        e.result = Result.Loss;
                    else if (r == Result.Loss)
                        e.result = Result.Win;
                    else if (r == Result.Tie)
                        e.result = Result.Tie;
                    return e;
                }

            }
        }

        public static void updateQFromEpisode(Episode e)
        {
            int i = 0;
            for (int it = 0; it < e.sas.Count; ++it)
            {
                double weight = 0;
                if (e.result == Result.Win)
                    weight = WIN_WEIGHT;
                else if (e.result == Result.Loss)
                    weight = LOSS_WEIGHT;
                else if (e.result == Result.Tie)
                    weight = TIE_WEIGHT;

                double futureDiscountedReturn = weight * Math.Pow(GAMMA, i);
                updateQ(e.sas[it], futureDiscountedReturn);
                i++;
            }
        }

        public static void Teach()
        {
            double bestRate = 0;
            Dictionary<StateAction, double> bestQ = new Dictionary<StateAction, double>();
            Dictionary<StateAction, int> bestQCount = new Dictionary<StateAction, int>();



            Q = new Dictionary<StateAction, double>();
            Q_count = new Dictionary<StateAction, int>();
            int winCount = 0;

            for (int i = 0; i < 200000; ++i)
            {
                Episode e = genEpisode();
                if (e.result == Result.Win)
                {
                    winCount++;

                }

                updateQFromEpisode(e);

                if (i >= 50000 && i % 50000 == 0)
                {
                    Console.WriteLine("Iterations: " + i + "\t\t WinRate=" + ((double)winCount / (double)(50000)) + "\t\t" + "ActionStates=" + Q.Count);
                    winCount = 0;
                }
            }

        }


        public static void Main()
        {
            Teach();
        }
    }
}