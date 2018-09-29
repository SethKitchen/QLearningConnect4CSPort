using System;

namespace Connect4QLearning
{
    public enum Player
    {
        Player1 = 1,
        Player2 = 2
    }

    public enum Result
    {
        Invalid,
        Intermediate,
        Win,
        Loss,
        Tie
    }


    public class BoardState
    {
        public int rows, cols;
        public int[,] board;

        public BoardState(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;

            this.board = new int[rows,cols];
            for (int i = 0; i < this.rows; ++i)
            {
                for (int j = 0; j < this.cols; ++j)
                {
                    this.board[i,j] = 0;
                }
            }
        }

        public BoardState(BoardState other)
        {
            this.rows = other.rows;
            this.cols = other.cols;
            this.board = new int[rows, cols];
            for (int i = 0; i < this.rows; ++i)
            {
                for (int j = 0; j < this.cols; ++j)
                {
                    this.board[i, j] = other.board[i, j];
                }
            }
        }

        public void PrettyPrint()
        {
            Console.BackgroundColor = ConsoleColor.White;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if(board[i,j]==0)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("-\t\t");
                    }
                    else if(board[i,j]==1)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("|B|\t\t");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("|R|\t\t");
                    }
                }
                Console.WriteLine();
            }
        }

        public string resultToString(Result r)
        {
            switch (r)
            {
                case Result.Invalid: return "Invalid";
                case Result.Intermediate: return "Intermediate";
                case Result.Win: return "Win";
                case Result.Loss: return "Loss";
                case Result.Tie: return "Tie";
            }
            return "";
        }

        public static bool operator ==(BoardState lhs, BoardState rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(BoardState lhs, BoardState rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override bool Equals(object obj)
        {
            if (obj is BoardState)
            {
                BoardState other = obj as BoardState;
                if (this.rows != other.rows)
                    return false;
                if (this.cols != other.cols)
                    return false;

                for(int i=0; i<other.rows; i++)
                {
                    for(int j=0; j<other.rows; j++)
                    {
                        if(this.board[i,j]!=other.board[i,j])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static BoardState initFromArray(int[] arr, int size, int cols)
        {
            int rows = size / cols;
            BoardState b = new BoardState(rows, cols);
            for (int i = 0; i < size; i++)
            {
                int r = (i / rows);
                int c = i % cols;
                if (arr[i] == 1 || arr[i] == 2)
                {
                    b.board[r,c] = arr[i];
                }
            }
            return b;
        }

        public BoardState move(int a, Player p, ref Result r)
        {
            int changedRow = -1;
            if (this.board[0,a] != 0)
            { // empty
                r = Result.Invalid;
            }

            BoardState newState = new BoardState(this);

            for (int i = newState.rows - 1; i >= 0; --i)
            {
                if (newState.board[i,a] == 0)
                {
                    newState.board[i,a] = (int)p;
                    changedRow = i;
                    r = newState.result(changedRow, a, p);
                    break;
                }
            }
            return newState;
        }

        public static bool validRowCol(int rows, int cols, int r, int c)
        {
            return (r < rows && r >= 0) && (c < cols && c >= 0);
        }
        public static bool match(int[,] board, int r, int c, int p)
        {
            return board[r,c] == p;
        }

        public Result result(int changedRow, int changedCol, Player p)
        {
            // Tie
            bool filled = true;
            for (int i = 0; i < this.cols; ++i)
            {
                filled = filled && (this.board[0,i] == 1 || this.board[0,i] == 2);
            }
            if (filled) return Result.Tie;

            bool end = false;
            int t = this.board[changedRow,changedCol];

            // vertical
            for (int i = 0; i < 4; ++i)
            {
                int[] rows = { changedRow + 0 - i, changedRow + 1 - i, changedRow + 2 - i, changedRow + 3 - i };
                end = end ||
                    ((validRowCol(this.rows, this.cols, rows[0], changedCol)
                        && match(this.board, rows[0], changedCol,(int) p)) &&
                        (validRowCol(this.rows, this.cols, rows[1], changedCol)
                            && match(this.board, rows[1], changedCol,(int) p)) &&
                            (validRowCol(this.rows, this.cols, rows[2], changedCol)
                                && match(this.board, rows[2], changedCol,(int) p)) &&
                                (validRowCol(this.rows, this.cols, rows[3], changedCol)
                                    && match(this.board, rows[3], changedCol,(int) p)));
            }

            //horizontal
            for (int i = 0; i < 4; ++i)
            {
                int[] cols = { changedCol + 0 - i, changedCol + 1 - i, changedCol + 2 - i, changedCol + 3 - i };
                end = end ||
                    ((validRowCol(this.rows, this.cols, changedRow, cols[0])
                        && match(this.board, changedRow, cols[0],(int) p)) &&
                        (validRowCol(this.rows, this.cols, changedRow, cols[1])
                            && match(this.board, changedRow, cols[1],(int) p)) &&
                            (validRowCol(this.rows, this.cols, changedRow, cols[2])
                                && match(this.board, changedRow, cols[2],(int) p)) &&
                                (validRowCol(this.rows, this.cols, changedRow, cols[3])
                                    && match(this.board, changedRow, cols[3],(int) p)));
            }


            // diagonal \
            // 
            for (int i = 0; i < 4; ++i)
            {
                int[] cols = { changedCol + 0 - i, changedCol + 1 - i, changedCol + 2 - i, changedCol + 3 - i };
                int[] rows = { changedRow + 0 - i, changedRow + 1 - i, changedRow + 2 - i, changedRow + 3 - i };

                end = end ||
                    ((validRowCol(this.rows, this.cols, rows[0], cols[0])
                        && match(this.board, rows[0], cols[0],(int) p)) &&
                        (validRowCol(this.rows, this.cols, rows[1], cols[1])
                            && match(this.board, rows[1], cols[1],(int) p)) &&
                            (validRowCol(this.rows, this.cols, rows[2], cols[2])
                                && match(this.board, rows[2], cols[2],(int) p)) &&
                                (validRowCol(this.rows, this.cols, rows[3], cols[3])
                                    && match(this.board, rows[3], cols[3],(int) p)));

            }

            // diagonal /
            for (int i = 0; i < 4; ++i)
            {
                int[] cols = { changedCol - 0 + i, changedCol - 1 + i, changedCol - 2 + i, changedCol - 3 + i };
                int[] rows = { changedRow + 0 - i, changedRow + 1 - i, changedRow + 2 - i, changedRow + 3 - i };

                end = end ||
                    ((validRowCol(this.rows, this.cols, rows[0], cols[0])
                        && match(this.board, rows[0], cols[0],(int) p)) &&
                        (validRowCol(this.rows, this.cols, rows[1], cols[1])
                            && match(this.board, rows[1], cols[1],(int) p)) &&
                            (validRowCol(this.rows, this.cols, rows[2], cols[2])
                                && match(this.board, rows[2], cols[2],(int) p)) &&
                                (validRowCol(this.rows, this.cols, rows[3], cols[3])
                                    && match(this.board, rows[3], cols[3],(int) p)));

            }


            if (end)
            {
                return (t == (int)p) ? Result.Win : Result.Loss;
            }
            return Result.Intermediate;
        }
    }
}