namespace Connect4QLearning
{
    public class StateAction
    {

        public BoardState state;
        public int action;

        public StateAction(BoardState s, int a)
        {
            this.state = s;
            this.action = a;
        }

        public override int GetHashCode()
        {
            int s = 0;
            int a = action;

            BoardState b = state;

            for (int i = 0; i < b.rows; i++)
            {
                for (int j = 0; j < b.cols; ++j)
                {
                    int k = (i * b.cols + j) % 64;
                    int m = (b.board[i,j] == (int)Player.Player1 || b.board[i,j] == 0) ? 0 : 1;
                    int v = (m << k);
                    s |= v;
                }
            }

            return (s << 4) + a;
        }

        public static bool operator ==(StateAction lhs, StateAction rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(StateAction lhs, StateAction rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override bool Equals(object obj)
        {
            if (obj is StateAction)
            {
                StateAction rhs = obj as StateAction;
                return (action == rhs.action && state == rhs.state);
            }
            return false;
        }


    };
}