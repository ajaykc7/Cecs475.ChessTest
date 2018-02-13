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

         /// <summary>
         /// At the start, the knights should be able to move to two positions jumping over the pawns.
         /// Test : - Initial Starting Board state
         /// Player: - Black
         /// Piece: - Knight
         /// Position: - b8
         /// Desired Positions: - a6, c6
         /// </summary>
         [Fact]
         public void ValidStartingMoveForKnight()
        {
            ChessBoard board = new ChessBoard();

            //Move a white knight so that it is black's turn
            Apply(board, "b1,a3");
            var possibleMoves = board.GetPossibleMoves();
            var initialKnightMoves = GetMovesAtPosition(possibleMoves, Pos("b8"));

            initialKnightMoves.Should().HaveCount(2,"The knight should be able to move to two different positions" +
                "in front of the pawns");

        }


        /// <summary>
        /// The test checks if the advantage is updated once a pawn is promoted to a queen. 
        /// Test : - "Tricky" move - Pawn Promotion
        /// Player: - White
        /// Piece: - Pawn to Queen
        /// Position: - b7
        /// Desired Position: - a8
        /// Result: - After moving from b7 to a8, the pawn should promote to queen.
        /// </summary>
        [Fact]
        public void PawnPromotionAfterCapture()
        {
            ChessBoard board = CreateBoardFromPositions(
                Pos("b7"), ChessPieceType.Pawn, 1,
                Pos("a8"), ChessPieceType.Rook, 2,
                Pos("e1"), ChessPieceType.King, 1,
                Pos("e8"), ChessPieceType.King, 2);

            //Since the two King's point nullify each other, player 2 should have an advantage of 4 (Black Rook's value - White
            //pawn's value)
            board.CurrentAdvantage.Should().Be(Advantage(2, 4), "Player two has a rook and a king, while player one has pawn and king");

            //White pawn captures the Rook and promotes to a queen
            Apply(board, Move("(b7,a8,Queen)"));
            
            board.GetPieceAtPosition(Pos("a8")).Player.Should().Be(1, "Player one captured player two's rook");

            //The advantage should be in favour of player 1 as he has an extra piece i.e. queen on the board.
            board.CurrentAdvantage.Should().Be(Advantage(1, 13), "Player one should have a gain of 9 and a loss of 1, while " +
                "player two should have loss of 5 after losing the rook");
        }



		
	}
}
