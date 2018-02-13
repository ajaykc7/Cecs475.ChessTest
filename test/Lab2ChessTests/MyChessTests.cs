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
            var initialKnightMoves = GetMovesAtPosition(possibleMoves, Pos("b1"));
            initialKnightMoves.Should().HaveCount(2);

           // var twoMovesExpected = GetMovesAtPosition(possibleMoves, Pos("b2"));
          //  twoMovesExpected.Should().HaveCount(2); 
        }


		
	}
}
