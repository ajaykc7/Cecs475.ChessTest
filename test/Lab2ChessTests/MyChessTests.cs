using System;
using System.Collections.Generic;
using System.Text;

using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.Chess.Test;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Chess.View;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Lab2ChessTests {
	public class MyChessTests : ChessTest {

		/* This is where you will write your tests.
		 * Each test must be marked with the [Test] attribute.
		 * 
		 * Double check that you follow these rules:
		 * 
		 * 0. RENAME THIS FILE to YourName.cs, but USE YOUR ACTUAL NAME.
		 * 1. Every test method should have a meaningful name.
		 * 2. Every Should() must include a string description of the expectation.
		 * 3. Your buster test should be LAST in this file, and should be given a meaningful name
		 *		FOLLOWED BY AN UNDERSCORE, followed by the LAST 6 digits of your student ID.
		 *		Example:
		 *		
		 *		If my ID is 012345678 and involves undoing a castling move, my test might be named
		 *		UndoCastleQueenSide_345678
		 *	
		 */
         [Fact]
         public void InitialStart()
        {
            ChessBoard board = new ChessBoard();

            var possibleMoves = board.GetPossibleMoves();
            var initialKnightMoves = GetMovesAtPosition(possibleMoves, Pos("b8"));
            initialKnightMoves.Should().HaveCount(2, "the knight has two move position in the initial stage");

           // var twoMovesExpected = GetMovesAtPosition(possibleMoves, Pos("b2"));
          //  twoMovesExpected.Should().HaveCount(2); 
        }

        [Fact]
        public void PawnPromotionAfterCapture()
        {
            ChessBoard board = CreateBoardFromPositions(
                Pos("b7"), ChessPieceType.Pawn, 1,
                Pos("a8"), ChessPieceType.Rook, 2,
                Pos("e1"), ChessPieceType.King, 1,
                Pos("e8"), ChessPieceType.King, 2);

            board.CurrentAdvantage.Should().Be(Advantage(2, 4), "Player two has a rook and a king, while player one has pawn and king");

            Apply(board, Move("(b7,a8,Queen)"));
            board.GetPieceAtPosition(Pos("a8")).Player.Should().Be(1, "Player one captured player two's rook");
            board.CurrentAdvantage.Should().Be(Advantage(1, 9), "Player one should have a gain of 9 and a loss of 1, while " +
                "player two should have loss of 5 after losing the rook");

        }


		
	}
}
