using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Maksligais_Intelekts_PD_1
{
    public partial class Form1 : Form
    {
        // Define starting variables
        bool ai_Starts = false;

        // Define constants
        const string ai_name = "AI";
        const string player_name = "Player 1";

        // Define a class to represent a node in the game tree
        public class GameTreeNode
        {
            public GameState State { get; set; }
            public List<GameTreeNode> Children { get; set; }
            public int Score { get; set; }
            public GameTreeNode(GameState state)
            {
                State = state;
                Children = new List<GameTreeNode>();
                Score = 0;
            }
        }

        // Define a class to represent the current state of the game
        public class GameState
        {
            public string LineOfNumbers { get; set; }
            public int CurrentPlayer { get; set; } // 1 or 2
            public int Player1Score { get; set; }
            public int Player2Score { get; set; }
            public int Move { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        // Create a line of 8 random numbers from 0 to 2
        private void button2_Click(object sender, EventArgs e)
        {
            String str = "";
            Random rand = new Random();
            for (int i = 0; i < 8; i++)
            {
                str += rand.Next(3);
            }
            Game_Field_Value.Text = str;
        }

        // Add option to choose who starts
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ai_Starts = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ai_Starts = true;
        }

        // Start the game, disable buttons that shouldn't be used
        private void button3_Click(object sender, EventArgs e)
        {
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;

            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            End_Game.Enabled = true;

            if (ai_Starts)
            {
                ai_move();
            }
            else
            {
                Game_Turn.Text = player_name;
            }
        }

        // End the game, reenable the disabled buttons, set the default values
        private void End_Game_Click(object sender, EventArgs e)
        {
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;

            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            End_Game.Enabled = false;
            AI_Points.Text = "20";
            Player_Points.Text = "20";
            Game_Turn.Text = "";
            Winner_name.Text = "";
            String str = "";
            Random rand = new Random();
            for (int i = 0; i < 9; i++)
            {
                str += rand.Next(3);
            }
            Game_Field_Value.Text = str;

        }

        // User chooses the number to remove, then we automatically launche the ai move
        private void button1_Click(object sender, EventArgs e)
        {
            if (Game_Field_Value.Text.Length == 0) 
            {
                game_ends();
            }
            if (Game_Turn.Text == player_name)
            {
                string game_field = Game_Field_Value.Text;
                string value = listBox1.Text;
                int index = game_field.IndexOf(value);
                if (index != -1)
                {
                    game_field = game_field.Remove(index, 1);
                    Game_Field_Value.Text = game_field;
                    Game_Turn.Text = ai_name;
                    int points = int.Parse(Player_Points.Text) - int.Parse(value);
                    Player_Points.Text = points.ToString();

                    if (Game_Field_Value.Text.Length != 0)
                    {
                        ai_move();
                    }
                    else 
                    {
                        game_ends();
                    }
                }
            }

        }

        // Create new State for this move, to generate a tree, max depth is equal to chars left in the line
        public void ai_move() 
        {
            Game_Turn.Text = ai_name;
            GameState state = new GameState
            {
                LineOfNumbers = Game_Field_Value.Text,
                CurrentPlayer = 1,
                Player1Score = int.Parse(AI_Points.Text),
                Player2Score = int.Parse(Player_Points.Text),
            };
            GameTreeNode node = GenerateGameTree(state, Game_Field_Value.Text.Length);

            GameTreeNode nextMove = DetermineBestMove(node);
            Game_Field_Value.Text = nextMove.State.LineOfNumbers;
            Game_Turn.Text = player_name;
            AI_Points.Text = nextMove.State.Player1Score.ToString();
            AI_Move.Text = nextMove.State.Move.ToString();
            if (Game_Field_Value.Text.Length == 0) 
            {
                game_ends();
            }
            Game_Turn.Text = player_name;
        }

        // The game ends, either of players are the Winner
        public void game_ends() 
        {
            int player_points = int.Parse(Player_Points.Text);
            int ai_points = int.Parse(AI_Points.Text);
            button1.Enabled = false;
            if (player_points > ai_points)
            {
                Winner_name.Text = player_name;
            }
            else if (ai_points == player_points) 
            {
                Winner_name.Text = "DRAW";
            }
            else
            {
                Winner_name.Text = ai_name;
            }
        }

        // Define a function to generate the game tree
        public GameTreeNode GenerateGameTree(GameState state, int depth)
        {
            // Create a new node for the current state
            GameTreeNode node = new GameTreeNode(state);

            // If we've reached the maximum depth, return the node
            if (depth == 0)
            {
                node.Score = state.Player1Score - state.Player2Score;
                return node;
            }

            // Generate all possible states from the current state
            List<GameState> possibleStates = GetPossibleStates(state);

            // For each possible state, generate a child node and add it to the current node
            foreach (GameState possibleState in possibleStates)
            {
                GameTreeNode childNode = GenerateGameTree(possibleState, depth - 1);
                node.Children.Add(childNode);
            }

            // If the current player is player 1 (maximizing player), find the child node with the highest score and set it as the node's score
            if (state.CurrentPlayer == 1)
            {
                node.Score = node.Children.Max(child => child.Score);
            }
            // If the current player is player 2 (minimizing player), find the child node with the lowest score and set it as the node's score
            else if (state.CurrentPlayer == 2)
            {
                node.Score = node.Children.Min(child => child.Score);
            }

            // Return the node
            return node;
        }

        // Define a function to get all possible states from the current state
        public List<GameState> GetPossibleStates(GameState state)
        {

            // Create a new state for each possible move and add it to the list of possible states
            List<GameState> possibleStates = new List<GameState>();
            for (int i = 0; i <= state.LineOfNumbers.Length - 1; i++)
            {
                GameState possibleState = GeneratePossibleState(state, i);
                possibleStates.Add(possibleState);
            }

            // Return the list of possible states
            return possibleStates;
        }

        // Define a function to generate a new state based on a given move
        public GameState GeneratePossibleState(GameState state, int move)
        {
            // Get current player and calculate player scores accordingly
            int CurrentPlayer = state.CurrentPlayer;
            int Player1Score;
            int Player2Score;

            if (CurrentPlayer == 1)
            {
                Player1Score = state.Player1Score - (int)Char.GetNumericValue(state.LineOfNumbers.ElementAt(move));
                Player2Score = state.Player2Score;
            }
            else
            {
                Player1Score = state.Player1Score;
                Player2Score = state.Player2Score - (int)Char.GetNumericValue(state.LineOfNumbers.ElementAt(move));
            }
            string LineOfNumbers = state.LineOfNumbers.Remove(move, 1);
            // Create new state and return it with new values
            return new GameState
            {
                LineOfNumbers = LineOfNumbers,
                Player1Score = Player1Score,
                Player2Score = Player2Score,
                CurrentPlayer = CurrentPlayer,
                Move = (int)Char.GetNumericValue(state.LineOfNumbers.ElementAt(move))
            };
        }

        // Define a function to determine the best move for the current player
        public GameTreeNode DetermineBestMove(GameTreeNode node)
        {
            
            // If the node has no children, return null (this should never happen)
            if (node.Children.Count == 0)
            {
                return null;
            }

            // Determine the best child node based on the MinMax algorithm
            GameTreeNode bestChild = null;
            int bestScore = int.MinValue;
            foreach (GameTreeNode childNode in node.Children)
            {
                if (childNode.Score > bestScore)
                {
                    bestScore = childNode.Score;
                    bestChild = childNode;
                }
            }
            return bestChild;
        }
    }
}
