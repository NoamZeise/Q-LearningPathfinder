
using System;
using System.Collections.Generic;
using System.Threading;
namespace RL_project
{
    class environment
    {
        int m_actions = 5; // 1 - up ; 2 - down ; 3 - left ; 4 - right ; 5 - goal
        int m_height = 10;
        int m_width = 10;
        int m_walls = 35;
        int m_currentState;
        char[,] m_board;
        Random rand = new Random(DateTime.Now.Second);
        double m_alpha = 0.5; //learning rate
        double m_gamma = 0.8; // discount factor
        public double m_epsilon = 0.8; //exploration factor

        List<List<List<double>>> RTable;
        /* 
        1st list -> all states possible for enviornment
        2nd list -> all possible moves from state
        3rd list:
         0: Q value
         1: Reward
         2: Next state
         */

        void interpritState() //turns state into characters on board
        {
            for (int i = 0; i < m_height; i++) //fill with T's to check for inaccessable areas with spaceCount()
            {
                for (int j = 0; j < m_width; j++)
                {
                    if (m_board[i, j] == 'A' || m_board[i, j] == 'X')
                        m_board[i, j] = ' ';
                }
            }
            int numSpaces = ((m_width * m_height) - m_walls);
            int goalLocation = m_currentState / numSpaces;
            int agentLocation = m_currentState % numSpaces;

            int currentSpace = -1;
            for (int i = 0; i < m_height; i++) //fill with T's to check for inaccessable areas with spaceCount()
            {
                for (int j = 0; j < m_width; j++)
                {
                    if (m_board[i, j] == ' ')
                    {
                        currentSpace++;

                        if (currentSpace == goalLocation)
                        {
                            m_board[i, j] = 'X';
                        }
                        if (currentSpace == agentLocation)
                        {
                            m_board[i, j] = 'A';
                        }
                    }
                }
            }
        }

        void rTableSetUp()
        {
            Random rand = new Random(DateTime.Now.Second);
            RTable = new List<List<List<double>>>();
            int numSpaces = ((m_width * m_height) - m_walls);
            for (int i = 0;
                i < Convert.ToInt32(Math.Pow(Convert.ToDouble(numSpaces), 2.0))
                ; i++)
            {
                RTable.Add(new List<List<double>>());
                for (int j = 0; j < m_actions; j++)
                {
                    float QValue, Reward, nextState;

                    QValue = 0;
                    Reward = -1;
                    nextState = i;

                    // set next state
                    int stateInSet = i % numSpaces;


                    int[] coords = new int[2];
                    int count = -1;
                    for (int x = 0; x < m_height; x++)
                    {
                        for (int y = 0; y < m_width; y++)
                        {
                            if (m_board[x, y] == ' ')
                                count++;
                            if (count == stateInSet)
                            {
                                coords = new int[2] { x, y };
                                count++;
                            }
                        }
                    }

                    //if(j == 0)
                    //Console.WriteLine(i + ":    " +coords[0] + ", " + coords[1]);
                    switch (j)
                    {
                        case 0: //up
                            if (coords[0] - 1 >= 0)
                            {
                                if (m_board[coords[0] - 1, coords[1]] == ' ') //if legal move, find state of up move
                                {
                                    int stateCount = -1;
                                    for (int x = 0; x < m_height; x++)
                                    {
                                        for (int y = 0; y < m_width; y++)
                                        {
                                            if (m_board[x, y] == ' ')
                                                stateCount++;
                                            if (x == coords[0] - 1 && y == coords[1])
                                                nextState = ((i / numSpaces) * numSpaces) + stateCount;
                                        }
                                    }
                                }
                            }
                            break;
                        case 1: //down
                            if (coords[0] + 1 < m_height)
                            {
                                if (m_board[coords[0] + 1, coords[1]] == ' ')
                                {
                                    int stateCount = -1;
                                    for (int x = 0; x < m_height; x++)
                                    {
                                        for (int y = 0; y < m_width; y++)
                                        {
                                            if (m_board[x, y] == ' ')
                                                stateCount++;
                                            if (x == coords[0] + 1 && y == coords[1])
                                                nextState = ((i / numSpaces) * numSpaces) + stateCount;
                                        }
                                    }
                                }
                            }
                            break;
                        case 2: //left
                            if (coords[1] - 1 >= 0)
                            {
                                if (m_board[coords[0], coords[1] - 1] == ' ')
                                {
                                    nextState = i - 1;
                                }
                            }
                            break;
                        case 3: //right
                            if (coords[1] + 1 < m_width)
                            {
                                if (m_board[coords[0], coords[1] + 1] == ' ')
                                {
                                    nextState = i + 1;
                                }
                            }
                            break;
                        case 4: //identify goal

                            //check for goal to determine reward
                            if (i % numSpaces == i / numSpaces) // on goal
                            {
                                Reward = 20;
                                //pick random next state
                                nextState = rand.Next(numSpaces * numSpaces);
                            }
                            else
                            {
                                Reward = -10;
                            }
                            break;
                    }

                    RTable[i].Add(new List<double>() { QValue, Reward, nextState });
                }
            }
        }


        int spaceCount(int count, int[] test) //ensures enviornment doesnt generate with inaccesible areas
        {
            if (test[0] < m_height && test[1] < m_width && test[0] >= 0 && test[1] >= 0)
            {
                if (m_board[test[0], test[1]] == 'T')
                {
                    count++;
                    m_board[test[0], test[1]] = ' ';
                    count = spaceCount(count, new int[2] { test[0] + 1, test[1] });
                    count = spaceCount(count, new int[2] { test[0] - 1, test[1] });
                    count = spaceCount(count, new int[2] { test[0], test[1] + 1 });
                    count = spaceCount(count, new int[2] { test[0], test[1] - 1 });
                }
            }

            return count;
        }
        public environment()
        {
            //initialise board with blank chars
            m_board = new char[m_height, m_width];
            
            for (int i = 0; i < m_height; i++) //fill with T's to check for inaccessable areas with spaceCount()
            {
                for (int j = 0; j < m_width; j++)
                {
                    m_board[i, j] = 'T';
                }
            }
            while (true)
            {
                //keep track of wall coords to ensure no duplicate walls
                int[,] wallCoords = new int[m_walls, 2];

                //set to default value to check which parts of the array have yet to be populated
                for (int i = 0; i < m_walls; i++)
                {
                    wallCoords[i, 0] = -1;
                    wallCoords[i, 1] = -1;
                }
                for (int i = 0; i < m_walls; i++)
                {
                    //initalise random wall coords
                    int[] wall = new int[2] { rand.Next(m_height - 1), rand.Next(m_width - 1) };
                    bool duplicate = false;
                    //check for duplicates against wallCoords
                    for (int j = 0; j < m_walls; j++)
                    {
                        if (wallCoords[j, 0] == wall[0] && wallCoords[j, 1] == wall[1])
                        {
                            duplicate = true;
                            break;
                        }

                    }
                    //try to initalise wall againt
                    if (duplicate)
                        i--;
                    //add coords to wallCoords and update boards with wall
                    else
                    {
                        for (int j = 0; j < m_walls; j++)
                        {
                            if (wallCoords[j, 0] == -1 && wallCoords[j, 1] == -1)
                            {
                                wallCoords[j, 0] = wall[0];
                                wallCoords[j, 1] = wall[1];
                                break;
                            }

                        }
                        m_board[wall[0], wall[1]] = '■';
                    }
                }
                //check for impossible board
                int[] test = new int[2];
                while (true)
                {
                    //initalise random wall coords
                    test = new int[2] { rand.Next(m_height - 1), rand.Next(m_width - 1) };
                    bool duplicate = false;
                    //check for duplicates against wallCoords
                    for (int j = 0; j < m_walls; j++)
                    {
                        if (wallCoords[j, 0] == test[0] && wallCoords[j, 1] == test[1])
                        {
                            duplicate = true;
                            break;
                        }

                    }
                    if (!duplicate)
                        break;
                }
                int count = 0;
                if (spaceCount(count, test) == (m_width * m_height) - m_walls)
                    break;
                else
                {
                    for (int i = 0; i < m_height; i++)
                    {
                        for (int j = 0; j < m_width; j++)
                        {
                            m_board[i, j] = 'T';
                        }
                    }
                }
            }
            //setup Reward/state transition table
            rTableSetUp();
            m_currentState = rand.Next(Convert.ToInt32(Math.Pow(Convert.ToDouble((m_height * m_width) - m_walls), 2.0)) - 1);
            interpritState();
        }

        void updateQVaule(int act)
        {
            double nextState = RTable[m_currentState][act][2];
            double maxQ = RTable[Convert.ToInt32(nextState)][0][0];
            for (int i = 1; i < m_actions; i++)
            {
                if (RTable[Convert.ToInt32(nextState)][i][0] > maxQ)
                    maxQ = RTable[Convert.ToInt32(nextState)][i][0];
            }
            double oldValue = RTable[m_currentState][act][0];
            double reward = RTable[m_currentState][act][1];
            if(reward == 10)
                Console.WriteLine(m_currentState);
            double temporalDifference = reward + (m_gamma * maxQ);
            RTable[m_currentState][act][0] = ((1 - m_alpha) *oldValue) + (m_alpha * (temporalDifference));
        }
        public void update()
        {
            int oldState = m_currentState;
            int maxQAction = 0;
            int act = 4;
            for (int i = 0; i < m_actions; i++)
            {
                if (RTable[m_currentState][i][0] > RTable[m_currentState][maxQAction][0])
                    maxQAction = i;
            }
            if (rand.NextDouble() < m_epsilon) //explore new q vaules?
                act = rand.Next(m_actions);
            else
                act = maxQAction;
            updateQVaule(act);
            m_currentState = Convert.ToInt32(RTable[m_currentState][act][2]);
            if (RTable[oldState][act][1] == 20)
            {
                m_currentState = oldState % ((m_width * m_height) - m_walls);
                m_currentState += ((m_width * m_height) - m_walls) * rand.Next(((m_width * m_height) - m_walls));
            }
            interpritState();
        }
        public void print()//print board
        {
            //Console.WriteLine(m_currentState);
            for (int i = 0; i < m_height; i++)
            {
                Console.Write("|");
                for (int j = 0; j < m_width; j++)
                {
                    if(m_board[i, j] == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(m_board[i, j]);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("|");
                    }
                    else if (m_board[i, j] == 'A')
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(m_board[i, j]);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("|");
                    }
                    else 
                     Console.Write(m_board[i, j] + "|");
                }
                Console.WriteLine();
            }
        }

        public void printRTable()//print reward table
        {
            for (int i = 0; i <
                Math.Pow(Convert.ToDouble((m_height * m_width) - m_walls), 2)
                ; i++)
            {
                Console.Write(i + "/");
                for (int j = 0; j < m_actions; j++)
                {

                    for (int k = 0; k < 3; k++)
                    {
                        Console.Write(RTable[i][j][k]);
                        Console.Write("/");
                    }
                    Console.Write("|");
                }
                Console.WriteLine();
                if (i % ((m_height * m_width) - m_walls) == 0)
                    Console.WriteLine();
            }
        }

        public void printQValues()
        {
            for (int i = 0; i <
                Math.Pow(Convert.ToDouble((m_height * m_width) - m_walls), 2)
                ; i++)
            {
                Console.Write(i + "|    ");
                for (int j = 0; j < m_actions; j++)
                {
                    Console.Write(RTable[i][j][0]);
                }
                Console.WriteLine();
                if (i % ((m_height * m_width) - m_walls) == 0)
                    Console.WriteLine();
            }
        }
    
    }
    class Program
    {
        static void Main(string[] args)
        {

            environment board = new environment();
            Console.WriteLine("before education: ");
            Console.ReadKey();
            Console.Clear();
            for (int i = 0; i < 10; i++) //see uneducated movement
            {
                board.update();
                board.print();
                Thread.Sleep(500);
                Console.Clear();
            }
            Console.WriteLine("educating...");
            for (int i = 0; i < 1000000; i++) //teach for 1M moves
            {
                board.update();
            }
            Console.WriteLine("after education: ");
            Console.ReadKey();
            Console.Clear();
            board.m_epsilon = 0; //set exploration rate to zero
            while (true)
            {
                board.update();
                board.print();
                Thread.Sleep(500);
                Console.Clear();
            }
        }
    }
}
