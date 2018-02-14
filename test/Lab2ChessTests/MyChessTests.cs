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
    public class MyChessTests : ChessTest
    {

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

            initialKnightMoves.Should().HaveCount(2, "The knight should be able to move to two different positions" +
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
            board.CurrentAdvantage.Should().Be(Advantage(1, 9), "Player one should have a gain of 9 and a loss of 1, while " +
                "player two should have loss of 5 after losing the rook");
        }

        /// <summary>
        /// The test checks if the king is in checkmate or not before and after the promotion. 
        /// Test : - UndoLastMove
        /// Player: - White, Black
        /// Result: - After promoting the white pawn to a queen, the black king is at checkmate, but after undoing the move the
        /// the king is no more at checkmate.
        /// </summary>
        [Fact]
        private void CheckMateAfterPromotion()
        {
            ChessBoard board = CreateBoardFromPositions(
                Pos("e1"), ChessPieceType.King, 1,
                Pos("a5"), ChessPieceType.Bishop, 1,
                Pos("a6"), ChessPieceType.Queen, 1,
                Pos("a8"), ChessPieceType.Rook, 1,
                Pos("g7"), ChessPieceType.Pawn, 1,
                Pos("d7"), ChessPieceType.King, 2);

            board.IsCheck.Should().BeFalse("The black king can move from d7 to either c7 or e7");

            //White rook move
            Apply(board, "a8,a7");
            board.IsCheck.Should().BeTrue("The black king is being checked by white rook which just moved to a7");
            board.IsCheckmate.Should().BeFalse("The black king can still move to e8");

            //Black king move
            Apply(board, "d7,e8");
            board.IsCheck.Should().BeFalse("The black king is not being checked by any white piece");

            //White pawn move to promote
            Apply(board, Move("(g7,g8,Queen)"));
            board.IsCheckmate.Should().BeTrue("The black king is checked by promoted white queen and cannot move anywhere");

            //check player one advantage
            // board.CurrentAdvantage.Should().Be(Advantage(1,), "The black only has a king whereas white has king, bishop, queen," +
            //    "rook, and the promoted queen");

            board.CurrentPlayer.Should().Be(2, "It is player 2's turn after the white promoted to pawn");

            board.UndoLastMove();

            board.CurrentPlayer.Should().Be(1, "It is player 1's turn after undoing the move");

            board.IsCheckmate.Should().BeFalse("The black king is not at check anymore because the promoted queen is not there");
        }

        /// <summary>
        /// The test checks if castling is possible when the new position of the king will be in check. 
        /// Test : - UndoLastMove
        /// Player: - White
        /// Result: - In the given layout of the board, castling cannot be perfomred because the white king at new position, 
        /// c1 would be in check, but if we moved the knight away from b3 by undoing the move, castling should be possible 
        /// </summary>
        [Fact]
        public void CastlingLeadingToCheck()
        {
            ChessBoard board = CreateBoardFromMoves(
                "b2, b4",
                "b8, c6",
                "c1, a3",
                "c6, d4",
                "b1, c3",
                "f7, f6",
                "e2, e3",
                "f6, f5",
                "d1, e2");

            //Move the black knight to put c1 as a position that can be captured
            Apply(board, "d4, b3");

            var possibleMoves = board.GetPossibleMoves();
            var castlingKingMoves = GetMovesAtPosition(possibleMoves, Pos("e1"));
            castlingKingMoves.Should().HaveCount(1, "The king cannot castle because the black knight would result in check")
              .And.NotContain(Move(Pos("e1"), Pos("c1"), ChessMoveType.CastleQueenSide));
            
            board.UndoLastMove();

            board.CurrentPlayer.Should().Be(2, "It is player 2's turn after undoing the move");

            //move the black knight to a different position
            Apply(board, "d4,b5");

            possibleMoves = board.GetPossibleMoves();
            castlingKingMoves = GetMovesAtPosition(possibleMoves, Pos("e1"));
            castlingKingMoves.Should().HaveCount(2, "The king can castle as it would not lead to check")
                .And.Contain(Move(Pos("e1"), Pos("c1"), ChessMoveType.CastleQueenSide));
        
        }

        /// <summary>
        /// The test checks possible moves for a king at check. 
        /// Test : - KingAtCheck
        /// Player: - Black
        /// Result: - In the given layout of the board, the king cannot capture the pawn that is checking it, as it would 
        /// lead to another check. The king cannot also do castling because it is at check. In addition, the black queen cannot 
        /// capture the white bishop because the king is at check. 
        /// </summary>
        [Fact]
        public void KingCapturePawnAtCheck()
        {
            ChessBoard board = CreateBoardFromPositions(
               Pos("e1"), ChessPieceType.King, 1,
               Pos("a4"), ChessPieceType.Bishop, 1,
               Pos("c6"), ChessPieceType.Pawn, 1,
               Pos("d7"), ChessPieceType.Pawn, 2,
               Pos("e8"), ChessPieceType.King, 2,
               Pos("f4"), ChessPieceType.Queen, 2,
               Pos("h8"), ChessPieceType.Rook, 2);

            //apply move to the white king
            Apply(board, "e1,d1");

            var possibleMoves = board.GetPossibleMoves();

            //all the moves of black king
            var blackKingMoves = GetMovesAtPosition(possibleMoves, Pos("e8"));

            board.IsCheck.Should().BeFalse("The black king has not been checked yet.");
            blackKingMoves.Should().HaveCount(5, "The black king can move to adjacent squares not occupied by friendly piece" +
                "as well as the castling move")
                .And.Contain(Move("e8,d8"))
                .And.Contain(Move("e8,f8"))
                .And.Contain(Move("e8,f7"))
                .And.Contain(Move("e8,e7"))
                .And.Contain(Move(Pos("e8"), Pos("g8"), ChessMoveType.CastleKingSide));

            //Move the white queen
            Apply(board, "f4,e4");
            //Move the white pawn to capture black pawn
            Apply(board, "c6,d7");

            possibleMoves = board.GetPossibleMoves();

            board.IsCheck.Should().BeTrue("The black king is at check by the white pawn");

            //all the moves valid for the black king at check
            var checkedKingMoves = GetMovesAtPosition(possibleMoves, Pos("e8"));
            //all the moves valid for the black queen
            var queenMoves = GetMovesAtPosition(possibleMoves, Pos("f4"));

            queenMoves.Should().HaveCount(0, "The king is at check so the queen should not be able to move unless" +
                "the move removes the check")
                .And.NotContain(Move("f4,a4"));

            checkedKingMoves.Should().HaveCount(4, "The black king can move to adjacent squares but cannot do castling " +
                "or capture the black pawn")
                .And.NotContain(Move(Pos("e8"), Pos("g8"), ChessMoveType.CastleKingSide))
                .And.NotContain(Move("e8,d7"));
        }
    }
}
