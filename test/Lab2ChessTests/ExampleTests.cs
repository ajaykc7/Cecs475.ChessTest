using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Chess.Test;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Lab2ChessTests {
	public class ExampleTests : ChessTest {
		/// <summary>
		/// Moving pawns one or two spaces.
		/// </summary>
		[Fact]
		public void PawnTwoSpaceMove() {
			ChessBoard b = new ChessBoard();

			var possMoves = b.GetPossibleMoves();
			// Each of the pawns in rank 2 should have two move options.
			foreach (var pos in GetPositionsInRank(2)) {
				var movesAtPos = GetMovesAtPosition(possMoves, pos);
				movesAtPos.Should().HaveCount(2)
					.And.BeEquivalentTo(
						Move(pos, pos.Translate(-1, 0)),
						Move(pos, pos.Translate(-2, 0))
				);
			}
			Apply(b, "a2, a3"); // one space move

			// Same, but for pawns in rank 7
			possMoves = b.GetPossibleMoves();
			foreach (var pos in GetPositionsInRank(7)) {
				var movesAtPos = GetMovesAtPosition(possMoves, pos);
				movesAtPos.Should().HaveCount(2)
					.And.BeEquivalentTo(
						Move(pos, pos.Translate(1, 0)),
						Move(pos, pos.Translate(2, 0))
					);
			}
			Apply(b, "a7, a5"); // player 2 response

			possMoves = b.GetPossibleMoves();
			var oneMoveExpected = GetMovesAtPosition(possMoves, Pos("a3"));
			oneMoveExpected.Should().Contain(Move("a3, a4"))
				.And.HaveCount(1, "a pawn not in its original rank can only move one space forward");

			var twoMovesExpected = GetMovesAtPosition(possMoves, Pos("b2"));
			twoMovesExpected.Should().Contain(Move("b2, b3"))
				.And.Contain(Move("b2, b4"))
				.And.HaveCount(2, "a pawn in its original rank can move up to two spaces forward");
		}

		[Fact]
		public void PawnTwoSpaceMove_IfUnblocked() {
			// Move a pawn from each side to in front of the other's starting pawns.
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"b7, b5",
				"a4, a5",
				"b5, b4",
				"a5, a6",
				"b4, b3"
			);

			var possMoves = b.GetPossibleMoves();
			var blockedPawn = GetMovesAtPosition(possMoves, Pos("b2"));
			blockedPawn.Should().BeEmpty("The pawn at b2 is blocked by the enemy at b3");
			Apply(b, Move("c2, c4"));

			possMoves = b.GetPossibleMoves();
			blockedPawn = GetMovesAtPosition(possMoves, Pos("a7"));
			blockedPawn.Should().BeEmpty("The pawn at a7 is blocked by the enemy at a6");
		}

		/// <summary>
		/// A pawn cannot make a two space movement if it is on the enemy's starting rank.
		/// </summary>
		[Fact]
		public void PawnTwoSpaceMove_DirectionMatters() {
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"b7, b5",
				"a4, b5",
				"b8, a6",
				"b5, b6",
				"h7, h5",
				"b6, b7",
				"h5, h4"
			);
			var possMoves = b.GetPossibleMoves();
			var oneMove = GetMovesAtPosition(possMoves, Pos("b7"));
			// The pawn at b7 should have 12 possible moves: 4 promotion moves each, for a move forward,
			// and two capturing moves diagonally.
			oneMove.Should().HaveCount(12)
				.And.NotContain(Move("b7, b9"))
				.And.OnlyContain(m => m.MoveType == ChessMoveType.PawnPromote,
					"the pawn at b7 should have 12 possible moves, all of which are pawn promotions.");
		}

		/// <summary>
		/// Pawn diagonal capture.
		/// </summary>
		[Fact]
		public void PawnCapture() {
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"b7, b5"
			);

			var poss = b.GetPossibleMoves();
			var expected = GetMovesAtPosition(poss, Pos("a4"));
			expected.Should().Contain(Move("a4, b5"))
				.And.Contain(Move("a4, a5"))
				.And.HaveCount(2, "a pawn can capture diagonally ahead or move forward");

			b.CurrentAdvantage.Should().Be(Advantage(0, 0), "no operations have changed the advantage");

			Apply(b, Move("a4, b5"));
			b.GetPieceAtPosition(Pos("b5")).Player.Should().Be(1, "Player 1 captured Player 2's pawn diagonally");
			b.CurrentAdvantage.Should().Be(Advantage(1, 1), "Black lost a single pawn of 1 value");

			b.UndoLastMove();
			b.CurrentAdvantage.Should().Be(Advantage(0, 0), "after undoing the pawn capture, advantage is neutral");
		}

		/// <summary>
		/// Pawn capture even if it can't move forward.
		/// </summary>
		[Fact]
		public void PawnCapture_EvenIfBlocked() {
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"h7, h5",
				"a4, a5",
				"h5, h4",
				"a5, a6",
				"h4, h3"
			);

			var possMoves = b.GetPossibleMoves();
			var threeMoves = GetMovesAtPosition(possMoves, Pos("a6"));
			threeMoves.Should().HaveCount(1, "the pawn at a6 should be able to capture to b7 even if it can't move forward")
				.And.BeEquivalentTo(Move("a6, b7"));
		}

		/// <summary>
		/// Pawn can't capture backwards.
		/// </summary>
		[Fact]
		public void PawnCapture_NoBackwardsCapture() {
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"b7, b5",
				"a4, a5",
				"c7, c6",
				"a5, a6"
			);

			var possMoves = b.GetPossibleMoves();
			GetMovesAtPosition(possMoves, Pos("b5")).Should().HaveCount(1, "pawn at b5 cannot capture backwards to a6")
				.And.Contain(Move("b5, b4"));
		}

		/// <summary>
		/// Pawns cannot capture diagonally off the board, wrapping to another row.
		/// </summary>
		[Fact]
		public void PawnBorderCapture() {
			ChessBoard b = CreateBoardFromMoves(
				Move("a2, a4"),
				Move("h7, h5")
			);

			var possMoves = b.GetPossibleMoves();
			var forwardOnly = GetMovesAtPosition(possMoves, Pos("a4"));
			forwardOnly.Should().HaveCount(1)
				.And.Contain(Move("a4, a5"));

			Apply(b, Move("b2, b4"));
			possMoves = b.GetPossibleMoves();
			forwardOnly = GetMovesAtPosition(possMoves, Pos("h5"));
			forwardOnly.Should().HaveCount(1, "pawn at h5 can move forward but can't capture to a4")
				.And.Contain(Move("h5, h4"));
		}

		/// <summary>
		/// Promote a pawn after reaching the final rank.
		/// </summary>
		[Fact]
		public void PawnPromoteTest() {
			ChessBoard b = CreateBoardFromMoves(
				"b2, b4",
				"a7, a5",
				"b4, b5",
				"a8, a6",
				"b5, a6", // capture rook with pawn
				"b8, c6",
				"a6, a7",
				"c6, d4"
			);
			b.CurrentAdvantage.Should().Be(Advantage(1, 5), "a Black rook was captured");

			// Make sure all possible moves are marked PawnPromote.
			var possMoves = b.GetPossibleMoves();
			var pawnMoves = GetMovesAtPosition(possMoves, Pos("a7"));
			pawnMoves.Should().HaveCount(4, "there are four possible promotion moves")
				.And.OnlyContain(m => m.MoveType == ChessMoveType.PawnPromote);

			// Apply the promotion move
			Apply(b, Move("(a7, a8, Queen)"));
			b.GetPieceAtPosition(Pos("a8")).PieceType.Should().Be(ChessPieceType.Queen, "the pawn was replaced by a queen");
			b.GetPieceAtPosition(Pos("a8")).Player.Should().Be(1, "the queen is controlled by player 1");
			b.CurrentPlayer.Should().Be(2, "choosing the pawn promotion should change the current player");
			b.CurrentAdvantage.Should().Be(Advantage(1, 13), "gained 9 points, lost 1 point from queen promotion");

			b.UndoLastMove();
			b.CurrentPlayer.Should().Be(1, "undoing a pawn promotion should change the current player");
			b.CurrentAdvantage.Should().Be(Advantage(1, 5), "lose value of queen when undoing promotion");
		}

		/// <summary>
		/// Promote a pawn to rook, move the rook, ensure that castling is still allowed.
		/// </summary>
		[Fact]
		public void PawnPromote_Castling() {
			ChessBoard b = CreateBoardFromMoves(
				"b2, b4",
				"a7, a5",
				"b4, b5",
				"a8, a6",
				"b5, a6", // capture rook with pawn
				"b8, c6",
				"a6, a7",
				"c6, d4"
			);
			b.CurrentAdvantage.Should().Be(Advantage(1, 5), "a Black rook was captured");

			// Apply the promotion move
			Apply(b, Move("(a7, a8, Rook)"));
			b.GetPieceAtPosition(Pos("a8")).PieceType.Should().Be(ChessPieceType.Rook, "the pawn was replaced by a rook");
			b.GetPieceAtPosition(Pos("a8")).Player.Should().Be(1, "the rook is controlled by player 1");

			Apply(b,
				"h7, h6",
				"a8, b8", // move promoted rook
				"h6, h5",
				"b1, a3", // move white pieces out of the way for castling
				"h5, h4",
				"c1, b2",
				"h4, h3",
				"c2, c3",
				"g7, g6",
				"d1, c2",
				"g6, g5"
			);

			var possMoves = b.GetPossibleMoves();
			var forKing = GetMovesAtPosition(possMoves, Pos("e1"));
			forKing.Should().HaveCount(2, "king at e1 can castle queenside even after a pawn-promoted rook has moved")
				.And.BeEquivalentTo(Move("e1, d1"), Move("e1, c1"));
		}

		/// <summary>
		/// Promote a pawn and produce check.
		/// </summary>
		[Fact]
		public void PawnPromote_IntoCheckmate() {
			ChessBoard b = CreateBoardFromMoves(
				"b2, b4",
				"a7, a5",
				"b4, b5",
				"a8, a6",
				"b5, a6", // capture rook with pawn
				"b8, c6",
				"a6, a7",
				"b7, b5",
				"c2, c3",
				"c8, b7",
				"c3, c4",
				"d7, d6",
				"c4, c5",
				"d8, d7"
			);

			// Apply the promotion move
			Apply(b, Move("(a7, a8, Rook)"));
			b.GetPieceAtPosition(Pos("a8")).PieceType.Should().Be(ChessPieceType.Rook, "the pawn was replaced by a rook");
			b.GetPieceAtPosition(Pos("a8")).Player.Should().Be(1, "the rook is controlled by player 1");
			b.IsCheck.Should().BeTrue("the king is threatened by the pawn-promoted rook at a8");
			b.IsCheckmate.Should().BeFalse("the king has an escape");
		}

		[Fact]
		public void EnPassantTest() {
			ChessBoard b = CreateBoardFromMoves(
				"a2, a4",
				"h7, h5",
				"a4, a5"
			);

			// Move pawn forward twice, enabling en passant from a5
			Apply(b, "b7, b5");

			var possMoves = b.GetPossibleMoves();
			var enPassantExpected = GetMovesAtPosition(possMoves, Pos("a5"));
			enPassantExpected.Should().HaveCount(2, "pawn can move forward one or en passant")
				.And.Contain(Move("a5, a6"))
				.And.Contain(Move(Pos("a5"), Pos("b6"), ChessMoveType.EnPassant));

			// Apply the en passant
			Apply(b, Move(Pos("a5"), Pos("b6"), ChessMoveType.EnPassant));
			var pawn = b.GetPieceAtPosition(Pos("b6"));
			pawn.Player.Should().Be(1, "pawn performed en passant move");
			pawn.PieceType.Should().Be(ChessPieceType.Pawn);
			var captured = b.GetPieceAtPosition(Pos("b5"));
			captured.Player.Should().Be(0, "the pawn that moved to b5 was captured by en passant");
			b.CurrentAdvantage.Should().Be(Advantage(1, 1));

			// Undo the move and check the board state
			b.UndoLastMove();
			b.CurrentAdvantage.Should().Be(Advantage(0, 0));
			pawn = b.GetPieceAtPosition(Pos("a5"));
			pawn.Player.Should().Be(1);
			captured = b.GetPieceAtPosition(Pos("b5"));
			captured.Player.Should().Be(2);
			var empty = b.GetPieceAtPosition(Pos("b6"));
			empty.Player.Should().Be(0);
		}
		
		/// <summary>
		/// Simple facts about "new" boards.
		/// </summary>
		[Fact]
		public void NewChessBoard() {
			ChessBoard b = new ChessBoard();
			b.GetPieceAtPosition(Pos(7, 0)).Player.Should().Be(1, "Player 1 should be in lower left of board");
			b.GetPieceAtPosition(Pos(0, 0)).Player.Should().Be(2, "Player 2 should be in upper left of board");
			b.GetPieceAtPosition(Pos(4, 0)).Player.Should().Be(0, "Middle left of board should be empty");
			// Test a few select piece locations.
			b.GetPieceAtPosition(Pos(7, 4)).PieceType.Should().Be(ChessPieceType.King, "White's king at position (7,4)");
			b.GetPieceAtPosition(Pos(0, 4)).PieceType.Should().Be(ChessPieceType.King, "Black's king at position (0,4)");
			// Test other properties
			b.CurrentPlayer.Should().Be(1, "Player 1 starts the game");
			b.CurrentAdvantage.Should().Be(Advantage(0, 0), "no operations have changed the advantage");
		}

		[Fact]
		public void ConfirmCheckAndValidMoves() {
			ChessBoard b = CreateBoardFromMoves(
				"d2, d4",
				"e7, e5",
				"d1, d3",
				"d8, e7",
				"d3, e4",
				"e7, a3",
				"b2, a3",
				"a7, a6",
				"e4, e5");

			var possible = b.GetPossibleMoves();
			possible.Count().Should().Be(3, "There should be 3 possible moves to cancel the checking of black's king");
			possible.Should().Contain(Move("e8, d8")).And.Contain(Move("f8, e7")).And.
				Contain(Move("g8, e7"), "the king cannot move into check");
		}


		/// <summary>
		/// Pawns can place enemy king in check.
		/// </summary>
		[Fact]
		public void PawnCausingCheck() {
			ChessBoard b = CreateBoardFromPositions(
				Pos("a7"), ChessPieceType.Pawn, 1,
				Pos("b8"), ChessPieceType.King, 2,
				Pos("a1"), ChessPieceType.King, 1
			);

			// Move so it's black's turn.
			Apply(b, "a1, a2");
			var possMoves = b.GetPossibleMoves();
			b.IsCheck.Should().BeTrue("black's king is in check from pawn at a7");
		}
	}
}
