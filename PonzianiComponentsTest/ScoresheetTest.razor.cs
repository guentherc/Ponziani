using AngleSharp.Dom;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PonzianiComponentsTest
{
    public partial class ScoresheetTest : BunitTestContext
    {
        public string GetId(IRenderedComponent<Scoresheet> scoresheet)
        {
            var nodes = scoresheet.Nodes;
            Assert.AreEqual(1, nodes.Count());
            if (nodes.Count() == 1) return ((IElement)nodes.First()).Id; else return null;
        }
    }

    internal static class Extensions
    {
        public static int NumberOfMovesWithComment(this Game game)
        {
            return game.Moves.Where(m => m.Comment != null && m.Comment.Length > 0).Count();
        }

        public static int NumberOfWhiteMovesWithComment(this Game game)
        {
            return game.Moves.Where(m => m.SideToMove == Side.WHITE && m.Comment != null && m.Comment.Length > 0).Count();
        }

        public static int MaxVariationLevel(this Game game)
        {
            return MaxVariationLevel(game.Moves);
        }

        private static int MaxVariationLevel(List<ExtendedMove> moves)
        {
            int level = 0;
            foreach (ExtendedMove move in moves.Where(m => m.Variations != null && m.Variations.Count > 0))
            {
                foreach (var variation in move.Variations)
                {
                    level = Math.Max(1 + MaxVariationLevel(variation), level);
                }
            }
            return level;
        }
    }
}
