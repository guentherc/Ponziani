using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents.Chesslib
{
    internal enum x88Square
    {
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2 = 16, B2, C2, D2, E2, F2, G2, H2,
        A3 = 32, B3, C3, D3, E3, F3, G3, H3,
        A4 = 48, B4, C4, D4, E4, F4, G4, H4,
        A5 = 64, B5, C5, D5, E5, F5, G5, H5,
        A6 = 80, B6, C6, D6, E6, F6, G6, H6,
        A7 = 96, B7, C7, D7, E7, F7, G7, H7,
        A8 = 112, B8, C8, D8, E8, F8, G8, H8, OUTSIDE = 128
    };

    internal class x88
    {
        internal static sbyte[] knightMoves = new sbyte[8] { -33, -31, -18, -14, +14, +18, +31, +33 };
        internal static sbyte[] kingDirections = new sbyte[8] { 17, 16, 15, 1, -1, -15, -16, -17 };
        internal static sbyte[] rookDirections = new sbyte[4] { 16, 1, -1, -16 };
        internal static sbyte[] bishopDirections = new sbyte[4] { 17, 15, -17, -15 };
        /// <summary>
        /// Returns the 0x88-Board Index for a chess board square 
        /// </summary>
        /// <param name="rank">The rank of the squareTo</param>
        /// <param name="file">The file of the squareTo</param>
        /// <returns>The Index of the square defined by rank and file on the 0x88 Board</returns>
        internal static byte x88Index(int rank, int file) { return (byte)(16 * rank + file); }
        /// <summary>
        /// Returns the 0x88-Board Index for a chess board square defined by a 0..63 based index
        /// </summary>
        /// <param name="index">The square index (A1 = 0, H1 = 7, H8 = 63)</param>
        /// <returns>The 0x88 Index</returns>
        public static byte x88Index(int index) { return (byte)(index + (index & ~7)); }
        /// <summary>
        /// Returns the (Standard-)square Index (defined by numbers 0..63)
        /// </summary>
        /// <param name="x88Index">Index in x88 representation</param>
        /// <returns>0-63 based square Index</returns>
        public static byte Index(int x88Index) { return (byte)((x88Index + (x88Index & 7)) >> 1); }
        /// <summary>
        /// Returns the (Standard-)square Index (defined by numbers 0..63)
        /// </summary>
        /// <param name="x88Square">Square in x88 representation</param>
        /// <returns>0-63 based square Index</returns>
        public static byte Index(x88Square x88Square) { return (byte)(((int)x88Square + (((int)x88Square) & 7)) >> 1); }
        /// <summary>
        /// Returns the File Index (0 = A-File, 7 = H-File) for a given squareTo in 0x88-Notation
        /// </summary>
        /// <param name="x88Index">Square index on 0x88-Board</param>
        /// <returns>The File Index</returns>
        public static byte GetFile(int x88Index) { return (byte)(x88Index & 7); }
        /// <summary>
        /// Returns the Rank of a squareTo in 0x88-Notation
        /// </summary>
        /// <param name="x88Index">Square index on 0x88-Board</param>
        /// <returns>The square's rank</returns>
        public static byte GetRank(int x88Index) { return (byte)(x88Index >> 4); }
        /// <summary>
        /// Checks if a square in 0x88-Notation is part of the chess board
        /// </summary>
        /// <param name="x88Index">Square index on 0x88-Board</param>
        /// <returns>true if index is on board</returns>
        public static bool IsOnBoard(int x88Index) { return (x88Index & 0x88) == 0x00; }
        /// <summary>
        /// Determines the stepsize needed to go from Square <paramref name="from"/> to 
        /// Square <paramref name="to"/>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        internal static sbyte GetDirection(x88Square from, x88Square to) { return direction[0x77 + (int)from - (int)to]; }
        internal static sbyte GetDirection(int from, int to) { return direction[0x77 + from - to]; }
        /// <summary>
        /// Checks if a given piece on a square is attacking a target square (if board is empty)
        /// </summary>
        /// <param name="piece">The attacking piece</param>
        /// <param name="from">The piece's location</param>
        /// <param name="target">The target square</param>
        /// <returns>true, of the piece is attacking the target square</returns>
        internal static bool IsAttacking(Piece piece, x88Square from, x88Square target)
        {
            return (attacks[0x77 + (int)from - (int)target] & (1 << (int)piece)) != 0;
        }
        /// <summary>
        /// Helper Array containing the directions (stepsize in x88 indexing for the minimal step). 
        /// So for A1 to H8 the stepsize is 17 as A1 has index 0 and B2 the next field in direction to
        /// H8 has index 17
        /// The directions are stored so that the index is 0x77 + FromSquare - ToSquare
        /// </summary>
        private static sbyte[] direction = new sbyte[240];
        /// <summary>
        /// Helper Array containing the pieces which might attack a square if they are located on a from square
        /// The atack types are stored as bit sets where 1st bit is set if White Queen may attack, 2nd bit if Black Queen may attack,
        /// ... Thes bitsets are stored under the index 0x77 + FromSquare - TargetSquare
        /// </summary>
        private static ushort[] attacks = new ushort[240];
        /// <summary>
        /// Initializes helper arrays
        /// </summary>
        static x88()
        {
            for (int i = 0; i < 240; ++i)
            {
                direction[i] = 0;
                attacks[i] = 0;
            }
            sbyte[] dir = new sbyte[8] { 16, -16, 1, -1, 17, -17, 15, -15 };
            for (Square from = Square.A1; from <= Square.H8; ++from)
            {
                x88Square xfrom = (x88Square)x88Index((int)from);
                foreach (sbyte d in dir)
                {
                    for (int xto = (int)xfrom + d; xto < 120; xto += d)
                    {
                        if (!IsOnBoard(xto)) break;
                        Debug.Assert(direction[0x77 + (int)xfrom - xto] == 0 || direction[0x77 + (int)xfrom - xto] == d);
                        direction[0x77 + (int)xfrom - xto] = (sbyte)d;
                        Debug.Assert(direction[0x77 + xto - (int)xfrom] == 0 || direction[0x77 + xto - (int)xfrom] == -d);
                        direction[0x77 + xto - (int)xfrom] = (sbyte)-d;
                    }
                }
            }
            int[] wpattacks = new int[] { 15, 17 };
            int[] bpattacks = new int[] { -15, -17 };
            for (Square from = Square.A1; from <= Square.H8; ++from)
            {
                x88Square xfrom = (x88Square)x88Index((int)from);
                //King attacks
                foreach (sbyte d in kingDirections)
                {
                    int xto = (int)xfrom + d;
                    if (IsOnBoard(xto))
                    {
                        int indx = 0x77 + (int)xfrom - xto;
                        attacks[indx] |= 1 << ((int)Piece.WKING);
                        attacks[indx] |= 1 << ((int)Piece.BKING);
                    }
                }
                //Knight attacks
                foreach (sbyte d in knightMoves)
                {
                    int xto = (int)xfrom + d;
                    if (IsOnBoard(xto))
                    {
                        int indx = 0x77 + (int)xfrom - xto;
                        attacks[indx] |= 1 << ((int)Piece.WKNIGHT);
                        attacks[indx] |= 1 << ((int)Piece.BKNIGHT);
                    }
                }
                if (xfrom >= x88Square.A2 && xfrom < x88Square.A8)
                {
                    //WPawn attacks
                    foreach (int d in wpattacks)
                    {
                        int xto = (int)xfrom + d;
                        if (IsOnBoard(xto))
                        {
                            int indx = 0x77 + (int)xfrom - xto;
                            attacks[indx] |= 1 << ((int)Piece.WPAWN);
                        }
                    }
                    //BPawn attacks
                    foreach (int d in bpattacks)
                    {
                        int xto = (int)xfrom + d;
                        if (IsOnBoard(xto))
                        {
                            int indx = 0x77 + (int)xfrom - xto;
                            attacks[indx] |= 1 << ((int)Piece.BPAWN);
                        }
                    }
                }
                //Rooks
                foreach (sbyte d in rookDirections)
                {
                    for (int n = 1; n <= 8; ++n)
                    {
                        int xto = (int)xfrom + n * d;
                        if (IsOnBoard(xto))
                        {
                            int indx = 0x77 + (int)xfrom - xto;
                            attacks[indx] |= 1 << ((int)Piece.WROOK);
                            attacks[indx] |= 1 << ((int)Piece.BROOK);
                            attacks[indx] |= 1 << ((int)Piece.WQUEEN);
                            attacks[indx] |= 1 << ((int)Piece.BQUEEN);
                        }
                        else break;
                    }
                }
                //Bishops
                foreach (sbyte d in bishopDirections)
                {
                    for (int n = 1; n <= 8; ++n)
                    {
                        int xto = (int)xfrom + n * d;
                        if (IsOnBoard(xto))
                        {
                            int indx = 0x77 + (int)xfrom - xto;
                            attacks[indx] |= 1 << ((int)Piece.WBISHOP);
                            attacks[indx] |= 1 << ((int)Piece.BBISHOP);
                            attacks[indx] |= 1 << ((int)Piece.WQUEEN);
                            attacks[indx] |= 1 << ((int)Piece.BQUEEN);
                        }
                        else break;
                    }
                }
            }
        }
    }
}
